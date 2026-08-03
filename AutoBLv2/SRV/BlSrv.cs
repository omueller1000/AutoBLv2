using System;
//using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Automation;
using Devices;
using FPGA;

namespace AutoBLv2.SRV
{
    public class BlSrv
    {

        #region Constants
        //-----------------------------------------------------------
        public const Int32 ERROR = -1;
        public const Int32 SUCCESS = 0;
        //-----------------------------------------------------------
        private const Int32 OFFSET_COMMAND = 1;
        //-----------------------------------------------------------
        #endregion



        #region Variables
        //-----------------------------------------------------------
        private static ServerLock __serverLock;
        private FpgaDaq __fpgaDaq;
        //-----------------------------------------------------------
        private static readonly object __taskLock = new object();
        private static string __currentTask = "";
        private Thread __MonitorThread;
        //-----------------------------------------------------------
        #endregion



        //===========================================================
        public BlSrv(ref FpgaDaq _fpgaDaq, ref ServerLock _lock) 
        {
            __fpgaDaq = _fpgaDaq;
            __serverLock = _lock;

        }
        //===========================================================



        #region Private Methods
        //-----------------------------------------------------------
        private void MonitorThread(ref Thread _thread, string _final)
        {
            if (_thread != null && _thread.IsAlive)
                _thread.Join();

            //~~~~~~~~~~~~~~~~~~~~~~~~~~~~
            lock (__taskLock)
            {
                __currentTask = _final;
            }
            //~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        }
        //-----------------------------------------------------------
        private Int32 CmdGetStatus(ref string _res, ref string _error)
        {
            string task;
            string error;

            task = __currentTask == "" ? "IDLE" : __currentTask;

            _res = "\r" + task;
            return SUCCESS;
        }





        private Int32 REQUEST_LENGTH_AUTO_GAIN_MIN = 3;
        private Int32 REQUEST_LENGTH_AUTO_GAIN_MAX = 8;
        private Int32 OFFSET_AUTO_GAIN_EXECUTE = 2;

        private Int32 MAX_AMPLIFIER_INDEX = 4;  // beamline specific
        private Int32 MIN_MONO_ENERGY = 5000;
        private Int32 MAX_MONO_ENERGY = 40000;


        private Int32 CmdAutoGain(ref string[] _reqArr, ref string _res, ref string _error)
        {
            Int32 rt;
            bool validateOnly = true;
            List<double> energyList;
            List<Int32> ampliferIndexList;
            SRS570[] selectedAmplifiers;




            if (_reqArr.Length < REQUEST_LENGTH_AUTO_GAIN_MIN || _reqArr.Length > REQUEST_LENGTH_AUTO_GAIN_MAX)
            {
                _error = this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name;
                _error += " INVALID_NUMBER_OF_ARGUMENTS";
                return ERROR;
            }


            
            ampliferIndexList = new List<Int32>();
            energyList = new List<double>();

            // parse aguments
            try
            {
                validateOnly = Int32.Parse(_reqArr[OFFSET_AUTO_GAIN_EXECUTE]) == 0 ? true : false;

                for (Int32 i = OFFSET_AUTO_GAIN_EXECUTE + 1; i < _reqArr.Length; i++)
                {
                    if (_reqArr[i][0] == 'A')
                    {
                        ampliferIndexList.Add(Int32.Parse(_reqArr[i].Substring(1)));
                    }
                    else if (_reqArr[i][0] == 'E')
                    {
                        energyList.Add(double.Parse(_reqArr[i].Substring(1)));
                    }
                    else
                    {
                        _error = this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name;
                        _error += " INVALID_ARGUMENT";
                        return ERROR;
                    }
                }


            }
            catch (Exception ex)
            {
                _error = this.GetType().Name + " " + System.Reflection.MethodBase.GetCurrentMethod().Name + " " + ex.Message;
                return ERROR;
            }


            // verify arguments
            try
            {
                for (Int32 i = 0; i < ampliferIndexList.Count; i++)
                {
                    if (ampliferIndexList[i] < 0 || ampliferIndexList[i] > MAX_AMPLIFIER_INDEX)
                    {
                        _error = this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name;
                        _error += " ARGUMENT_OUT_OF_RANGE";
                        return ERROR;
                    }
                }

                for (Int32 i = 0; i < energyList.Count; i++)
                {
                    if (energyList[i] < MIN_MONO_ENERGY || energyList[i] > MAX_MONO_ENERGY)
                    {
                        _error = this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name;
                        _error += " ARGUMENT_OUT_OF_RANGE";
                        return ERROR;
                    }
                }
            }
            catch (Exception ex)
            {
                _error = this.GetType().Name + " " + System.Reflection.MethodBase.GetCurrentMethod().Name + " " + ex.Message;
                return ERROR;
            }


            if (validateOnly)
                return SUCCESS;


            return SUCCESS;
        }
        //-----------------------------------------------------------
        #endregion



        #region Public Methods
        //-----------------------------------------------------------
        public Int32 EvaluateRequest(ref string[] _reqArr, ref string _res)
        {
            Int32 rt;
            string error = "";
            bool err = false;

            switch (_reqArr[OFFSET_COMMAND].ToUpper())
            {

                case "GET_STATUS":
                    rt = CmdGetStatus(ref _res, ref error);
                    if (rt < 0)
                    {
                        err = true;
                        _res = System.Reflection.MethodBase.GetCurrentMethod().Name + " " + error;
                        break;
                    }
                    break;



                case "AUTO_GAIN":
                    if (__serverLock.IsLocked)
                    {
                        err = true;
                        _res = "SERVER_IS_LOCKED";
                        return ERROR;
                    }

                    rt = CmdAutoGain(ref _reqArr, ref _res, ref error);
                    if (rt < 0)
                    {
                        err = true;
                        _res = System.Reflection.MethodBase.GetCurrentMethod().Name + " " + error;
                        break;
                    }
                    break;



                default:
                    err = true;
                    _res = "UNKNOWN_COMMAND";
                    break;

            }

            if (err)
            {
                _res = this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + " " + _res;
                return ERROR;
            }

            _res = _reqArr[OFFSET_COMMAND].ToUpper() + " " + _res;

            return SUCCESS;
        }
        //-----------------------------------------------------------
        #endregion




    }
}
