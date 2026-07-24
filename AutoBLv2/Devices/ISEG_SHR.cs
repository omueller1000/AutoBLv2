using EPICS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Devices
{
    public class ISEG_SHR
    {
        #region Constants
        //-----------------------------------------------------------
        public const Int32 SUCCESS = 0;
        public const Int32 ERROR = -1;
        //-----------------------------------------------------------
        private const Int32 SLEEP = 100;
        private const Int32 POLL_TIME = 500;
        //-----------------------------------------------------------
        #endregion



        #region Variables
        //-----------------------------------------------------------        
        private string __beamline;
        private Thread[] __threads;
        private static string __staticError;
        private bool[] channelIsDisabled;
        //-----------------------------------------------------------
        #endregion



        #region Properties
        //-----------------------------------------------------------
        public double maxV { get; set; }
        public double deltaV { get; set; }
        public Int32 TimeoutSeconds { get; set; }
        public bool DisableChannel1
        {
            get { return channelIsDisabled[0]; }
            set { channelIsDisabled[0] = value; }
        }
        public bool DisableChannel2
        {
            get { return channelIsDisabled[1]; }
            set { channelIsDisabled[1] = value; }
        }
        public bool DisableChannel3
        {
            get { return channelIsDisabled[2]; }
            set { channelIsDisabled[2] = value; }
        }
        public bool DisableChannel4
        {
            get { return channelIsDisabled[3]; }
            set { channelIsDisabled[3] = value; }
        }
        //-----------------------------------------------------------
        #endregion




        //===========================================================
        public ISEG_SHR(string _beamline)
        {
            __beamline = _beamline;

            __threads = new Thread[4];

            this.maxV = 2000.0;
            this.deltaV = 5.0;
            this.TimeoutSeconds = 60;

            channelIsDisabled = new bool[4];
            this.DisableChannel1 = true;
            this.DisableChannel2 = true;
            this.DisableChannel3 = true;
            this.DisableChannel4 = true;
        }
        //===========================================================



        #region Private Methods
        //-----------------------------------------------------------
        public Int32 Init(ref string _error)
        {
            Int32 rt;
            string epicsStr;

            epicsStr = $"{__beamline}:ISEG:0:0";
            rt = ca.put(epicsStr, ":Control:setKillEnable", 1);
            if (rt != ca.SUCCESS)
            {
                _error = this.GetType().Name + " " + System.Reflection.MethodBase.GetCurrentMethod().Name;
                _error += " ca.put failed";
                return ERROR;
            }

            for (Int32 ch = 0; ch < 4; ch++)
            {
                Thread.Sleep(SLEEP);

                epicsStr = $"{__beamline}:ISEG:0:0:{ch}";

                rt = ca.put(epicsStr, ":CurrentSet", 0.001);
                if (rt != ca.SUCCESS)
                {
                    _error = this.GetType().Name + " " + System.Reflection.MethodBase.GetCurrentMethod().Name;
                    _error += " ca.put failed";
                    return ERROR;
                }
            }

            return SUCCESS;
        }
        private Int32 WaitForVoltage(Int32 _channel, double _voltage, Int32 _timeoutSeconds)
        {
            Int32 rt;
            string epicsStr = "";
            string error = "";
            double currentVoltage;
            double elapsedSeconds = 0;

            DateTime initialDataTime = DateTime.Now;
            DateTime currentDateTime;

            epicsStr = $"{__beamline}:ISEG:0:0:{_channel - 1}";
            while (true)
            {

                rt = GetV(_channel, out currentVoltage, ref error);
                if (rt != SUCCESS)
                {
                    __staticError = error;
                    return ERROR;
                }


                if (Math.Abs(Math.Abs(currentVoltage) - _voltage) < this.deltaV)
                    return SUCCESS;

                Thread.Sleep(POLL_TIME);

                currentDateTime = DateTime.Now;
                elapsedSeconds = (currentDateTime - initialDataTime).TotalSeconds;
                if (elapsedSeconds > _timeoutSeconds)
                {
                    __staticError = "TIMEOUT_ERROR";
                    return ERROR;
                }
            }

            return SUCCESS;
        }
        //-----------------------------------------------------------
        #endregion


        #region Public Methods
        //-----------------------------------------------------------
        public Int32 SetV(Int32[] _channels, double _setVoltage, bool __blocking, ref string _error)
        {
            Int32 rt;
            string epicsStr = "";


            for (Int32 i = 0; i < _channels.Length; i++)
            {
                if (_channels[i] < 1 || _channels[i] > 4)
                {
                    _error = this.GetType().Name + " " + System.Reflection.MethodBase.GetCurrentMethod().Name;
                    _error += " INVALID_HV_CHANNEL";
                    return ERROR;
                }

                if (channelIsDisabled[_channels[i] - 1])
                {
                    _error = this.GetType().Name + " " + System.Reflection.MethodBase.GetCurrentMethod().Name;
                    _error += " HV_CHANNEL_DISABLED";
                    return ERROR;
                }
            }


            if (_setVoltage < 0)
            {
                _error = this.GetType().Name + " " + System.Reflection.MethodBase.GetCurrentMethod().Name;
                _error += " VOLTAGE_TOO_SMALL";
                return ERROR;
            }

            if (_setVoltage > this.maxV)
            {
                _error = this.GetType().Name + " " + System.Reflection.MethodBase.GetCurrentMethod().Name;
                _error += " VOLTAGE_EXCEEDS_MAX";
                return ERROR;
            }


            for (Int32 i = 0; i < _channels.Length; i++)
            {
                epicsStr = $"{__beamline}:ISEG:0:0:{_channels[i] - 1}";
                rt = ca.put(epicsStr, ":VoltageSet", _setVoltage);
                if (rt != ca.SUCCESS)
                {
                    _error = this.GetType().Name + " " + System.Reflection.MethodBase.GetCurrentMethod().Name;
                    _error += " ca.put failed";
                    return ERROR;
                }
            }

            if (!__blocking)
                return SUCCESS;



            __staticError = "";

            for (Int32 i = 0; i < _channels.Length; i++)
            {
                Int32 channel = _channels[i];
                __threads[i] = new Thread(() => WaitForVoltage(channel, _setVoltage, this.TimeoutSeconds));
                __threads[i].IsBackground = true;
                __threads[i].Start();
            }

            Thread.Sleep(100);

            for (Int32 i = 0; i < _channels.Length; i++)
            {
                __threads[i].Join();
            }

            if (__staticError != "")
            {
                _error = this.GetType().Name + " " + System.Reflection.MethodBase.GetCurrentMethod().Name + " " + __staticError;
                return ERROR;
            }

            return SUCCESS;
        }
        public Int32 GetV(Int32 _channel, out double _voltage, ref string _error)
        {
            Int32 rt;
            string epicsStr = "";
            string error = "";

            _voltage = 0;
            epicsStr = $"{__beamline}:ISEG:0:0:{_channel - 1}";

            rt = ca.put(epicsStr, ":VoltageMeasure", 0);
            if (rt != ca.SUCCESS)
            {
                error = this.GetType().Name + " " + System.Reflection.MethodBase.GetCurrentMethod().Name;
                error += " ca.put failed";
                return ERROR;
            }

            Thread.Sleep(SLEEP);

            rt = ca.get(epicsStr, ":VoltageMeasure", out _voltage);
            if (rt != ca.SUCCESS)
            {
                error = this.GetType().Name + " " + System.Reflection.MethodBase.GetCurrentMethod().Name;
                error += " ca.get failed";
                return ERROR;
            }

            return SUCCESS;
        }
        //-----------------------------------------------------------
        #endregion
    }
}
