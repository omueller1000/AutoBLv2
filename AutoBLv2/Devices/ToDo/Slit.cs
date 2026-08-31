using EPICS;
using SharpCompress.Compressors.ZStandard.Unsafe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Devices
{
    public class EpicsSlit
    {
        #region Constants
        //-----------------------------------------------------------
        string BL_DEF_PATH = "C:\\SSRL_LOCAL_OM\\definitions\\bl.def";
        //-----------------------------------------------------------
        public const Int32 SUCCESS = 1;
        public const Int32 ERROR = -1;
        //-----------------------------------------------------------
        #endregion


        #region Variables
        //-----------------------------------------------------------
        private double BottomJawDir = -1.0;
        private double TopJawDir = 1.0;
        private double SpearJawDir = 1;
        private double SsrlJawDir = -1.0;
        //-----------------------------------------------------------
        private EpicsMotor __top;
        private EpicsMotor __bottom;
        private EpicsMotor __ssrl;
        private EpicsMotor __spear;
        //-----------------------------------------------------------
        #endregion


        #region Properties                
        //-----------------------------------------------------------
        public double HGap
        {
            get
            {
                double pos_1, pos_2;
                pos_1 = __spear.Position;
                pos_2 = __ssrl.Position;
                return pos_1 + pos_2;
            }
        }
        public double HCenter
        {
            get
            {
                double pos_1, pos_2;
                pos_1 = __spear.Position;
                pos_2 = __ssrl.Position;
                return (SpearJawDir * pos_1 + SsrlJawDir * pos_2) / 2.0;
            }
        }
        public double VGap
        {
            get
            {
                double pos_1, pos_2;
                pos_1 = __top.Position;
                pos_2 = __bottom.Position;                
                return pos_1 + pos_2;
            }
        }
        public double VCenter
        {
            get
            {
                double pos_1, pos_2;
                pos_1 = __top.Position;
                pos_2 = __bottom.Position;
                return (BottomJawDir * pos_1 + TopJawDir * pos_2) / 2.0;
            }
        }
        //-----------------------------------------------------------
        public bool IsMoving
        {
            get
            {
                int dmov;
                bool isMoving = false;

                __bottom.cagetDMOV(out dmov);
                isMoving = dmov == 1 ? isMoving | false : true;

                __top.cagetDMOV(out dmov);
                isMoving = dmov == 1 ? isMoving | false : true;

                __spear.cagetDMOV(out dmov);
                isMoving = dmov == 1 ? isMoving | false : true;

                __ssrl.cagetDMOV(out dmov);
                isMoving = dmov == 1 ? isMoving | false : true;

                return isMoving;
            }
        }
        //-----------------------------------------------------------
        #endregion




        //===========================================================
        public EpicsSlit()
        {
            // these motors are configured in a way that positive direction
            // generally moves away from the beam, i.e. increases intensity
            // onto the sample

            Int32 rt;
            string error = "";
            string valueStr;
            string mcName;
            Int32 bottomMotorId;
            Int32 topMotorId;
            Int32 spearMotorId;
            Int32 ssrlMotorId;


            rt = Def.Def.ReadDefinition(BL_DEF_PATH, "SLIT_MC_NAME", out valueStr, ref error);
            if (rt != Def.Def.SUCCESS)
            {
                error = this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + " " + error;
                Console.WriteLine(error);
                return;
            }
            mcName = valueStr;

            rt = Def.Def.ReadDefinition(BL_DEF_PATH, "SLIT_BOTTOM_MOTOR_ID", out valueStr, ref error);
            if (rt != Def.Def.SUCCESS)
            {
                error = this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + " " + error;
                Console.WriteLine(error);
                return;
            }
            bottomMotorId = Int32.Parse(valueStr);

            rt = Def.Def.ReadDefinition(BL_DEF_PATH, "SLIT_TOP_MOTOR_ID", out valueStr, ref error);
            if (rt != Def.Def.SUCCESS)
            {
                error = this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + " " + error;
                Console.WriteLine(error);
                return;
            }
            topMotorId = Int32.Parse(valueStr);

            rt = Def.Def.ReadDefinition(BL_DEF_PATH, "SPEAR_MOTOR_ID", out valueStr, ref error);
            if (rt != Def.Def.SUCCESS)
            {
                error = this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + " " + error;
                Console.WriteLine(error);
                return;
            }
            spearMotorId = Int32.Parse(valueStr);

            rt = Def.Def.ReadDefinition(BL_DEF_PATH, "SLIT_SSRL_MOTOR_ID", out valueStr, ref error);
            if (rt != Def.Def.SUCCESS)
            {
                error = this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + " " + error;
                Console.WriteLine(error);
                return;
            }
            ssrlMotorId = Int32.Parse(valueStr);            




            __bottom = new EpicsMotor(mcName, bottomMotorId);
            __top = new EpicsMotor(mcName, topMotorId);
            __spear = new EpicsMotor(mcName, spearMotorId);
            __ssrl = new EpicsMotor(mcName, ssrlMotorId);


            __bottom.Init();
            __top.Init();
            __spear.Init();
            __ssrl.Init();
        }
        //===========================================================





        private Int32 MoveJaws(EpicsMotor _jaw1, EpicsMotor _jaw2, double _val1, double _val2, bool _relative, bool _blocking, ref string _error)
        {
            Int32 rt;
            Int32 dmov;
            Int32 SHORT_SLEEP = 20;
            Int32 LONG_SLEEP = 100;

            double jawPos1;
            double jawPos2;

            double targetPosition1;
            double targetPosition2;


            #region verify motors are not currently running
            //-----------------------------------------------------------            
            while (true)
            {
                rt = _jaw1.cagetDMOV(out dmov);
                if (rt == EpicsMotor.SUCCESS && dmov == 1)
                    break;

                Thread.Sleep(SHORT_SLEEP);
                Console.Write(".");
            }
            while (true)
            {
                rt = _jaw2.cagetDMOV(out dmov);
                if (rt == EpicsMotor.SUCCESS && dmov == 1)
                    break;

                Thread.Sleep(SHORT_SLEEP);
                Console.Write(".");
            }
            //-----------------------------------------------------------
            #endregion


            if (_relative)
            {
                targetPosition1 = _jaw1.Position + _val1;
                targetPosition2 = _jaw2.Position + _val2;
            }
            else
            {
                targetPosition1 = _val1;
                targetPosition2 = _val2;
            }

            rt = _jaw1.MoveAbsolute(targetPosition1);
            if (rt != EpicsMotor.SUCCESS)
            {
                _error = System.Reflection.MethodBase.GetCurrentMethod().Name;
                _error += " MoveAbsoluteFailed";
                return ERROR;
            }

            rt = _jaw2.MoveAbsolute(targetPosition2);
            if (rt != EpicsMotor.SUCCESS)
            {
                _error = System.Reflection.MethodBase.GetCurrentMethod().Name;
                _error += " MoveAbsoluteFailed";
                return ERROR;
            }


            if (!_blocking)
                return SUCCESS;


            Thread.Sleep(LONG_SLEEP);

            #region wait for motor motion to complete
            //-----------------------------------------------------------
            while (true)
            {
                rt = _jaw1.cagetDMOV(out dmov);
                if (rt == EpicsMotor.SUCCESS && dmov == 1)
                    break;

                Thread.Sleep(SHORT_SLEEP);
                Console.Write(".");
            }
            while (true)
            {
                rt = _jaw2.cagetDMOV(out dmov);
                if (rt == EpicsMotor.SUCCESS && dmov == 1)
                    break;

                Thread.Sleep(SHORT_SLEEP);
                Console.Write(".");
            }
            //-----------------------------------------------------------
            #endregion


            return SUCCESS;
        }



        public Int32 MoveHGap(double _targetPosition, bool _blocking, ref string _error)
        {
            Int32 rt;

            double delta = (this.HGap - _targetPosition) / 2.0;

            rt = MoveJaws(__spear, __ssrl, -1.0 * delta, -1.0 * delta, true, _blocking, ref _error);
            if (rt != SUCCESS)
            {
                _error = this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + " " + _error;                
                return ERROR;
            }
            
            return SUCCESS;
        }

        public Int32 MoveVGap(double _targetPosition, bool _blocking, ref string _error)
        {
            Int32 rt;

            double delta = (this.VGap - _targetPosition) / 2.0;

            rt = MoveJaws(__top, __bottom, -1.0 * delta, -1.0 * delta, true, _blocking, ref _error);
            if (rt != SUCCESS)
            {
                _error = this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + " " + _error;
                return ERROR;
            }

            return SUCCESS;
        }


        public Int32 MoveHCenter(double _targetPosition, bool _blocking, ref string _error)
        {
            Int32 rt;

            double delta = this.HCenter - _targetPosition;

            rt = MoveJaws(__spear, __ssrl, -1.0 * SpearJawDir * delta, -1.0 * SsrlJawDir * delta, true, _blocking, ref _error);
            if (rt != SUCCESS)
            {
                _error = this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + " " + _error;
                return ERROR;
            }

            return SUCCESS;
        }


        public Int32 MoveVCenter(double _targetPosition, bool _blocking, ref string _error)
        {
            Int32 rt;

            double delta = this.VCenter - _targetPosition;

            rt = MoveJaws(__top, __bottom, -1.0 * TopJawDir * delta, -1.0 * BottomJawDir * delta, true, _blocking, ref _error);
            if (rt != SUCCESS)
            {
                _error = this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + " " + _error;
                return ERROR;
            }

            return SUCCESS;
        }





    }
}
