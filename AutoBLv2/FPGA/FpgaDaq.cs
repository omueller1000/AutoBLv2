using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using EPICS;

namespace FPGA
{
    public class FpgaDaq
    {
        #region Constants
        //-----------------------------------------------------------
        public const Int32 SUCCESS = 1;
        public const Int32 ERROR = -1;
        //-----------------------------------------------------------
        #endregion



        #region Variables
        //-----------------------------------------------------------
        private string __name;
        //-----------------------------------------------------------
        private static Mutex __dataMutex;
        private static Int32 __dataEventCounter;
        private static camonitor __monitorData;
        private static List<FpgaDataFrame> __fpgaDataFrameList;
        //-----------------------------------------------------------
        private double[] __ASLO;
        private double[] __AOFF;
        //-----------------------------------------------------------
        #endregion



        #region Properties
        //-----------------------------------------------------------
        public string Name { get { return __name; } }
        public Int32 nSamples { get; set; }
        //-----------------------------------------------------------
        #endregion



        //===========================================================
        public FpgaDaq(string _name)
        {
            __name = _name;
            //this.nSamples = 32;
            this.nSamples = 4;

            GetAdcCalibration();

            __dataMutex = new Mutex();
            __fpgaDataFrameList = new List<FpgaDataFrame>();
            __monitorData = new camonitor(__name, ".DATA");
            __monitorData.MonitorEvent += __monitorData_MonitorEvent;
        }
        //===========================================================


        #region Private Methods
        //-----------------------------------------------------------
        private Int32 GetAdcCalibration()
        {
            Int32 rt;
            string pv;

            __ASLO = new double[FpgaDataFrame.numMaxAi];
            __AOFF = new double[FpgaDataFrame.numMaxAi];


            for (Int32 i = 0; i < FpgaDataFrame.numMaxAi; i++)
            {
                pv = ":ADC" + (i + 1) + ".ASLO";
                rt = ca.get(__name, pv, out __ASLO[i]);


                pv = ":ADC" + (i + 1) + ".AOFF";
                rt = ca.get(__name, pv, out __AOFF[i]);
            }

            return SUCCESS;
        }
        private void AverageFpgaData(ref List<FpgaDataFrame> _fpgaDataFrameList, out double[] _aiAverage, out double[] _aiStdDev)
        {
            _aiAverage = new double[FpgaDataFrame.numMaxAi];
            _aiStdDev = new double[FpgaDataFrame.numMaxAi];


            for (int k = 0; k < FpgaDataFrame.numMaxAi; k++)
            {
                _aiAverage[k] = 0;
                for (Int32 i = 0; i < _fpgaDataFrameList.Count; i++)
                {
                    _aiAverage[k] += _fpgaDataFrameList[i].ai[k];
                }
                _aiAverage[k] /= _fpgaDataFrameList.Count;
            }

            for (int k = 0; k < FpgaDataFrame.numMaxAi; k++)
            {
                _aiStdDev[k] = 0;
                for (Int32 i = 0; i < _fpgaDataFrameList.Count; i++)
                {
                    _aiStdDev[k] += (_fpgaDataFrameList[i].ai[k] - _aiAverage[k]) * (_fpgaDataFrameList[i].ai[k] - _aiAverage[k]);
                }
                _aiStdDev[k] = Math.Sqrt(_aiStdDev[k]);
            }
        }
        private void __monitorData_MonitorEvent(object sender, OmEpicsMonitorEventArg arg)
        {
            UInt32[] local_buffer;
            UInt32 data_len = (UInt32)((arg.dbr[0][0] + 4) / sizeof(UInt32));


            // make a local copy of the raw data			
            local_buffer = new UInt32[data_len];
            for (Int32 i = 0; i < local_buffer.Length; i++)
            {
                local_buffer[i] = (UInt32)arg.dbr[i][0];
            }


            FpgaDataFrame[] fpgaDataFrames;
            FpgaData.evalData(ref local_buffer, out fpgaDataFrames);


            //............................
            __dataMutex.WaitOne();
            if (__dataEventCounter > 0)
            {
                __fpgaDataFrameList.AddRange(fpgaDataFrames);
            }
            __dataEventCounter++;
            __dataMutex.ReleaseMutex();
            //............................

        }
        //-----------------------------------------------------------
        #endregion



        #region Public Methods
        //-----------------------------------------------------------
        public Int32 CollectDataFast(out double[] _ai)
        {
            Int32 rt;
            string valStr;

            _ai = new double[FpgaDataFrame.numMaxAi];

            for (Int32 i = 0; i < 4; i++)
            {
                rt = ca.get(__name, $":ADC{i + 1}", out valStr);
                _ai[i] = double.Parse(valStr);
            }

            return SUCCESS;
        }
        public Int32 CollectData(out double[] _aiAverage, out double[] _aiStdDev)
        {
            Int32 rt;
            Int32 fpgaDataFrameCounter;

            rt = ca.put(__name, ".TRIG", 0);    // disable
            rt = ca.put(__name, ".TSRC", 0);    // free run
            rt = ca.put(__name, ".TBRT", 0);    // kHz            
            rt = ca.put(__name, ".TPSK", 0);    // 1 ms

            //~~~~~~~~~~~~~~~~~~~~~~~~~~~
            __dataMutex.WaitOne();
            __dataEventCounter = 0;
            __fpgaDataFrameList.Clear();
            __monitorData.StartMonitor();
            __dataMutex.ReleaseMutex();
            //~~~~~~~~~~~~~~~~~~~~~~~~~~~

            rt = ca.put(__name, ".TRIG", 2);    // enable                        

            while (true)
            {
                Thread.Sleep(10);                

                //~~~~~~~~~~~~~~~~~~~~~~~~~~~
                __dataMutex.WaitOne();
                fpgaDataFrameCounter = __fpgaDataFrameList.Count;
                __dataMutex.ReleaseMutex();
                //~~~~~~~~~~~~~~~~~~~~~~~~~~~

                if (fpgaDataFrameCounter > this.nSamples)
                    break;
            }

            //~~~~~~~~~~~~~~~~~~~~~~~~~~~
            __dataMutex.WaitOne();
            __monitorData.StopMonitor();
            __dataMutex.ReleaseMutex();
            //~~~~~~~~~~~~~~~~~~~~~~~~~~~

            rt = ca.put(__name, ".TRIG", 0);    // disable


            //~~~~~~~~~~~~~~~~~~~~~~~~~~~
            __dataMutex.WaitOne();
            AverageFpgaData(ref __fpgaDataFrameList, out _aiAverage, out _aiStdDev);
            __dataMutex.ReleaseMutex();
            //~~~~~~~~~~~~~~~~~~~~~~~~~~~


            return SUCCESS;
        }
        public Int32 CollectDataCalibrated(out double[] _aiAverage, out double[] _aiStdDev)
        {
            Int32 rt;


            rt = CollectData(out _aiAverage, out _aiStdDev);
            if (rt != SUCCESS)
                return ERROR;


            for (Int32 i = 0; i < _aiAverage.Length; i++)
            {
                _aiAverage[i] -= 0x80000000;
                _aiAverage[i] *= __ASLO[i];
                _aiAverage[i] += __AOFF[i];
            }

            for (Int32 i = 0; i < _aiStdDev.Length; i++)
            {
                _aiStdDev[i] *= __ASLO[i];
            }

            return SUCCESS;
        }
        //-----------------------------------------------------------
        #endregion









    }
}
