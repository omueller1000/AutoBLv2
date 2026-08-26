using EPICS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FPGA
{
    public class FpgaDaqMotor
    {
        #region Constants
        //-----------------------------------------------------------
        public const Int32 SUCCESS = 1;
        public const Int32 ERROR = -1;                
        //-----------------------------------------------------------        
        #endregion



        #region Variables
        //-----------------------------------------------------------
        private string __fpgaName;
        private string __motorName;
        private Int32 __idx;
        //-----------------------------------------------------------
        private Int32 __srev = 0;
        private Int32 __erev = 0;
        private double __urev = 1;
        //-----------------------------------------------------------
        #endregion



        #region Properties
        //-----------------------------------------------------------
        public double Position
        {
            get
            {
                double value;
                ca.get(__fpgaName, __motorName + ".RBV", out value);
                return value;
            }
        }
        public Int32 EncoderCounts
        {
            get
            {
                Int32 rt;
                Int32 value;
                rt = ca.get(__fpgaName, __motorName + ".REP", out value);
                return value;
            }
        }
        public double MotorCountesPerEGgu
        {
            get { return __srev / __urev; }
        }
        public double EncoderCountesPerEGgu
        {
            get { return __erev / __urev; }
        }
        //-----------------------------------------------------------
        public double MotorOffset { get; set; }
        public double EncoderOffset { get; set; }
        public int MotorDirection { get; set; }
        //-----------------------------------------------------------
        #endregion



        //===========================================================
        public FpgaDaqMotor(string _fpgaName, Int32 _idx)
        {
            __fpgaName = _fpgaName;
            __motorName = ":MOTOR" + _idx;
            __idx = _idx;
        }
        //===========================================================




        #region Public Methods
        //-----------------------------------------------------------
        public Int32 Init()
        {
            Int32 rt;


            rt = ca.get(__fpgaName, __motorName + ".SREV", out __srev);
            if (rt != ca.SUCCESS) return ERROR;

            rt = ca.get(__fpgaName, __motorName + ".EREV", out __erev);
            if (rt != ca.SUCCESS) return ERROR;

            rt = ca.get(__fpgaName, __motorName + ".UREV", out __urev);
            if (rt != ca.SUCCESS) return ERROR;


            return SUCCESS;
        }

        //-----------------------------------------------------------
        public Int32 cagetDMOV(out Int32 _dmov)
        {
            Int32 rt;
            rt = ca.get(__fpgaName, __motorName + ".DMOV", out _dmov);
            return rt == 0 ? SUCCESS : ERROR;
        }
        public Int32 cagetMOVN(out Int32 _dmov)
        {
            Int32 rt;
            rt = ca.get(__fpgaName, __motorName + ".MOVN", out _dmov);
            return rt == 0 ? SUCCESS : ERROR;
        }
        public Int32 MoveAbsolute(double _position)
        {
            Int32 rt;
            rt = ca.put(__fpgaName, __motorName + ".VAL", _position);
            return rt == 0 ? SUCCESS : ERROR;
        }
        //-----------------------------------------------------------
        #endregion




    }
}
