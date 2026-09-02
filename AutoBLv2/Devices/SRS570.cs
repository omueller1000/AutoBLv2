using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EPICS;
using FPGA;

namespace Devices
{
    public class SRS570
    {
        #region Constants
        //-----------------------------------------------------------  
        public const Int32 SUCCESS = 1;
        public const Int32 ERROR = -1;
        //-----------------------------------------------------------  
        public Dictionary<Int32, string> SENS_DICT;
        public Dictionary<Int32, string> IOLV_DICT;
        public Dictionary<Int32, double> GAIN_DICT;
        //-----------------------------------------------------------  
        #endregion



        #region Private Variables
        //-----------------------------------------------------------        
        private string __deviceName;
        //-----------------------------------------------------------
        #endregion



        #region Public Variables
        //-----------------------------------------------------------
        public FpgaDaq Daq;
        //-----------------------------------------------------------
        #endregion



        #region Properties
        //-----------------------------------------------------------
        public string Name
        {
            get;
            set;
        }
        public Int32 AiChannel
        {
            get;
            set;
        }
        public double SignalLowLimit
        {
            get;
            set;
        }
        public double SignalHighLimit
        {
            get;
            set;
        }
        //-----------------------------------------------------------
        #endregion



        //===========================================================
        public SRS570(string _deviceName, ref FpgaDaq _fpgaDaq, Int32 _aiChannel)
        {
            #region __SENS_DICT            
            //.....................................................
            SENS_DICT = new Dictionary<Int32, string>();

            SENS_DICT.Add(0, "1x1e12_V/A");

            SENS_DICT.Add(1, "5x1e11_V/A");
            SENS_DICT.Add(2, "2x1e11_V/A");
            SENS_DICT.Add(3, "1x1e11_V/A");

            SENS_DICT.Add(4, "5x1e10_V/A");
            SENS_DICT.Add(5, "2x1e10_V/A");
            SENS_DICT.Add(6, "1x1e10_V/A");

            SENS_DICT.Add(7, "5x1e9_V/A");
            SENS_DICT.Add(8, "2x1e9_V/A");
            SENS_DICT.Add(9, "1x1e9_V/A");

            SENS_DICT.Add(10, "5x1e8_V/A");
            SENS_DICT.Add(11, "2x1e8_V/A");
            SENS_DICT.Add(12, "1x1e8_V/A");

            SENS_DICT.Add(13, "5x1e7_V/A");
            SENS_DICT.Add(14, "2x1e7_V/A");
            SENS_DICT.Add(15, "1x1e7_V/A");

            SENS_DICT.Add(16, "5x1e6_V/A");
            SENS_DICT.Add(17, "2x1e6_V/A");
            SENS_DICT.Add(18, "1x1e6_V/A");

            SENS_DICT.Add(19, "5x1e5_V/A");
            SENS_DICT.Add(20, "2x1e5_V/A");
            SENS_DICT.Add(21, "1x1e5_V/A");

            SENS_DICT.Add(22, "5x1e4_V/A");
            SENS_DICT.Add(23, "2x1e4_V/A");
            SENS_DICT.Add(24, "1x1e4_V/A");

            SENS_DICT.Add(25, "5x1e3_V/A");
            SENS_DICT.Add(26, "2x1e3_V/A");
            SENS_DICT.Add(27, "1x1e3_V/A");
            //.....................................................
            #endregion

            #region __IOLV_DICT
            //.....................................................
            IOLV_DICT = new Dictionary<Int32, string>();

            IOLV_DICT.Add(0, "1_pA");
            IOLV_DICT.Add(1, "2_pA");
            IOLV_DICT.Add(2, "5_pA");
            IOLV_DICT.Add(3, "10_pA");
            IOLV_DICT.Add(4, "20_pA");
            IOLV_DICT.Add(5, "50_pA");
            IOLV_DICT.Add(6, "100_pA");
            IOLV_DICT.Add(7, "200_pA");
            IOLV_DICT.Add(8, "500_pA");

            IOLV_DICT.Add(9, "1_nA");
            IOLV_DICT.Add(10, "2_nA");
            IOLV_DICT.Add(11, "5_nA");
            IOLV_DICT.Add(12, "10_nA");
            IOLV_DICT.Add(13, "20_nA");
            IOLV_DICT.Add(14, "50_nA");
            IOLV_DICT.Add(15, "100_nA");
            IOLV_DICT.Add(16, "200_nA");
            IOLV_DICT.Add(17, "500_nA");

            IOLV_DICT.Add(18, "1_uA");
            IOLV_DICT.Add(19, "2_uA");
            IOLV_DICT.Add(20, "5_uA");
            IOLV_DICT.Add(21, "10_uA");
            IOLV_DICT.Add(22, "20_uA");
            IOLV_DICT.Add(23, "50_uA");
            IOLV_DICT.Add(24, "100_uA");
            IOLV_DICT.Add(25, "200_uA");
            IOLV_DICT.Add(26, "500_uA");

            IOLV_DICT.Add(27, "1_mA");
            IOLV_DICT.Add(28, "2_mA");
            IOLV_DICT.Add(29, "5_mA");
            //.....................................................
            #endregion

            #region __GAIN_DICT            
            //.....................................................
            GAIN_DICT = new Dictionary<Int32, double>();

            GAIN_DICT.Add(0, 1e12);

            GAIN_DICT.Add(1, 5e11);
            GAIN_DICT.Add(2, 2e11);
            GAIN_DICT.Add(3, 1e11);

            GAIN_DICT.Add(4, 5e10);
            GAIN_DICT.Add(5, 2e10);
            GAIN_DICT.Add(6, 1e10);

            GAIN_DICT.Add(7, 5e9);
            GAIN_DICT.Add(8, 2e9);
            GAIN_DICT.Add(9, 1e9);

            GAIN_DICT.Add(10, 5e8);
            GAIN_DICT.Add(11, 2e8);
            GAIN_DICT.Add(12, 1e8);

            GAIN_DICT.Add(13, 5e7);
            GAIN_DICT.Add(14, 2e7);
            GAIN_DICT.Add(15, 1e7);

            GAIN_DICT.Add(16, 5e6);
            GAIN_DICT.Add(17, 2e6);
            GAIN_DICT.Add(18, 1e6);

            GAIN_DICT.Add(19, 5e5);
            GAIN_DICT.Add(20, 2e5);
            GAIN_DICT.Add(21, 1e5);

            GAIN_DICT.Add(22, 5e4);
            GAIN_DICT.Add(23, 2e4);
            GAIN_DICT.Add(24, 1e4);

            GAIN_DICT.Add(25, 5e3);
            GAIN_DICT.Add(26, 2e3);
            GAIN_DICT.Add(27, 1e3);
            //.....................................................
            #endregion



            this.Name = _deviceName;
            this.AiChannel = _aiChannel;
            this.SignalLowLimit = 0.0;
            this.SignalHighLimit = 4.0;//4.8;

            Daq = _fpgaDaq;
            __deviceName = _deviceName;
        }
        //===========================================================




        #region Public Methods
        //-----------------------------------------------------------
        public Int32 SetGain(Int32 _value, ref string _error)
        {
            Int32 rt;
            double value;

            // check input
            value = _value;
            if (value >= SENS_DICT.Count)
            {
                value = SENS_DICT.Count - 1;
            }

            rt = ca.put(__deviceName, ":SENSITIVITY", value);
            if (rt < ca.SUCCESS)
            {
                _error = this.GetType().Name + " " + System.Reflection.MethodBase.GetCurrentMethod().Name;
                return ERROR;
            }

            return SUCCESS;
        }
        public Int32 SetOffset(Int32 _value, Int32 _sign, ref string _error)
        {
            Int32 rt;
            double value = 1;
            double polarity;

            //check input
            value = _value;
            if (value >= IOLV_DICT.Count)
            {
                value = IOLV_DICT.Count - 1;
            }

            if (_sign > 0)
                polarity = 1;   // positive
            else
                polarity = 0;   // negative

            rt = ca.put(__deviceName, ":INPUT_OFFSET_SIGN", polarity);
            if (rt < ca.SUCCESS)
            {
                _error = this.GetType().Name + " " + System.Reflection.MethodBase.GetCurrentMethod().Name;
                return ERROR;
            }

            rt = ca.put(__deviceName, ":INPUT_OFFSET_LEVEL", value);
            if (rt < ca.SUCCESS)
            {
                _error = this.GetType().Name + " " + System.Reflection.MethodBase.GetCurrentMethod().Name;
                return ERROR;
            }

            return SUCCESS;
        }
        //-----------------------------------------------------------
        public Int32 GetSensitivity(out double _gain, ref string _error)
        {
            Int32 rt;
            _gain = -1;

            rt = ca.get(__deviceName, ":SENSITIVITY", out _gain);
            if (rt < ca.SUCCESS)
            {
                _error = this.GetType().Name + " " + System.Reflection.MethodBase.GetCurrentMethod().Name;
                return ERROR;
            }

            return SUCCESS;
        }
        public Int32 GetGain(out double _gain, ref string _error)
        {
            Int32 rt;
            _gain = -1;

            rt = ca.get(__deviceName, ":GAIN", out _gain);
            if (rt < ca.SUCCESS)
            {
                _error = this.GetType().Name + " " + System.Reflection.MethodBase.GetCurrentMethod().Name;
                return ERROR;
            }

            return SUCCESS;
        }
        public Int32 GetOffset(out Int32 _offset, out Int32 _sign, ref string _error)
        {
            Int32 rt;
            _offset = -1;
            _sign = 0;


            rt = ca.get(__deviceName, ":INPUT_OFFSET_LEVEL", out _offset);
            if (rt < ca.SUCCESS)
            {
                _error = this.GetType().Name + " " + System.Reflection.MethodBase.GetCurrentMethod().Name;
                return ERROR;
            }

            rt = ca.get(__deviceName, ":INPUT_OFFSET_LEVEL_SIGN", out _sign);
            if (rt < ca.SUCCESS)
            {
                _error = this.GetType().Name + " " + System.Reflection.MethodBase.GetCurrentMethod().Name;
                return ERROR;
            }

            return SUCCESS;
        }
        //-----------------------------------------------------------
        #endregion




    }
}
