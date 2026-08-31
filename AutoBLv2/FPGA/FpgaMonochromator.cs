using EPICS;
using FPGA;
using MathNet.Numerics.Integration;
using SharpCompress.Readers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace Devices
{
    public class FpgaMonochromator
    {

        #region Constants
        //-----------------------------------------------------------
        string MONO_DEF_PATH = "C:\\SSRL_LOCAL_OM\\definitions\\mono.def";
        //string MOTOR_DEF_PATH = "C:\\SSRL_LOCAL_OM\\definitions\\motor.def";
        //-----------------------------------------------------------
        public const Int32 SUCCESS = 1;
        public const Int32 ERROR = -1;
        public const Int32 MAX_DEVIATION_ERROR = -2;
        //-----------------------------------------------------------
        #endregion


        #region Variables
        //-----------------------------------------------------------        
        private XasMonochromator __mono;
        private EpicsMotor __crystal;
        private EpicsMotor __tableV1;
        private EpicsMotor __tableV2;
        //-----------------------------------------------------------
        #endregion


        #region Properties
        //-----------------------------------------------------------
        public bool UseTable{ get; set; }
        public bool UseTableEncoders { get; set; }
        //-----------------------------------------------------------
        #endregion




        //===========================================================
        public FpgaMonochromator()
        {
            Int32 rt;
            string error = "";
            string valueStr;

            string monoFpgaName;
            Int32 monoCrystalMotorId;
            Int32 monoTableV1MotorId;
            Int32 monoTableV2MotorId;
            double monoLatticeSpacing;
            double monoCrystalGap;

            this.UseTable = false;
            this.UseTableEncoders = false;


            double monoCrystalEncoderOffset;
            double monoTableV1EncoderOffset;
            double monoTableV2EncoderOffset;
            


            rt = Def.Def.ReadDefinition(MONO_DEF_PATH, "MONO_FPGA_NAME", out valueStr, ref error);
            if (rt != Def.Def.SUCCESS)
            {
                error = this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + " " + error;
                Console.WriteLine(error);
                return;
            }
            monoFpgaName = valueStr;

            rt = Def.Def.ReadDefinition(MONO_DEF_PATH, "MONO_CRYSTAL_MOTOR_ID", out valueStr, ref error);
            if (rt != Def.Def.SUCCESS)
            {
                error = this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + " " + error;
                Console.WriteLine(error);
                return;
            }
            monoCrystalMotorId = Int32.Parse(valueStr);

            rt = Def.Def.ReadDefinition(MONO_DEF_PATH, "MONO_TABLEV1_MOTOR_ID", out valueStr, ref error);
            if (rt != Def.Def.SUCCESS)
            {
                error = this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + " " + error;
                Console.WriteLine(error);
                return;
            }
            monoTableV1MotorId = Int32.Parse(valueStr);


            rt = Def.Def.ReadDefinition(MONO_DEF_PATH, "MONO_TABLEV2_MOTOR_ID", out valueStr, ref error);
            if (rt != Def.Def.SUCCESS)
            {
                error = this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + " " + error;
                Console.WriteLine(error);
                return;
            }
            monoTableV2MotorId = Int32.Parse(valueStr);


            rt = Def.Def.ReadDefinition(MONO_DEF_PATH, "MONO_LATTICE_SPACING", out valueStr, ref error);
            if (rt != Def.Def.SUCCESS)
            {
                error = this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + " " + error;
                Console.WriteLine(error);
                return;
            }
            monoLatticeSpacing = double.Parse(valueStr);


            rt = Def.Def.ReadDefinition(MONO_DEF_PATH, "MONO_CRYSTAL_GAP", out valueStr, ref error);
            if (rt != Def.Def.SUCCESS)
            {
                error = this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + " " + error;
                Console.WriteLine(error);
                return;
            }            
            monoCrystalGap = double.Parse(valueStr);

            rt = Def.Def.ReadDefinition(MONO_DEF_PATH, $"MONO_CRYSTAL_ENCODER_OFFSET", out valueStr, ref error);
            if (rt != Def.Def.SUCCESS)
            {
                error = this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + " " + error;
                Console.WriteLine(error);
                return;
            }
            monoCrystalEncoderOffset = double.Parse(valueStr);


            rt = Def.Def.ReadDefinition(MONO_DEF_PATH, $"MONO_TABLEV1_ENCODER_OFFSET", out valueStr, ref error);
            if (rt != Def.Def.SUCCESS)
            {
                error = this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + " " + error;
                Console.WriteLine(error);
                return;
            }
            monoTableV1EncoderOffset = double.Parse(valueStr);

            rt = Def.Def.ReadDefinition(MONO_DEF_PATH, $"MONO_TABLEV2_ENCODER_OFFSET", out valueStr, ref error);
            if (rt != Def.Def.SUCCESS)
            {
                error = this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + " " + error;
                Console.WriteLine(error);
                return;
            }
            monoTableV2EncoderOffset = double.Parse(valueStr);




            rt = Def.Def.ReadDefinition(MONO_DEF_PATH, "MONO_USE_TABLE", out valueStr, ref error);
            if (rt != Def.Def.SUCCESS)
            {
                error = this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + " " + error;
                Console.WriteLine(error);
                return;
            }
            this.UseTable = Int32.Parse(valueStr) == 0 ? false : true;


            rt = Def.Def.ReadDefinition(MONO_DEF_PATH, "MONO_USE_TABLE_ENCODERS", out valueStr, ref error);
            if (rt != Def.Def.SUCCESS)
            {
                error = this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + " " + error;
                Console.WriteLine(error);
                return;
            }
            this.UseTableEncoders = Int32.Parse(valueStr) == 0 ? false : true;


            __crystal = new EpicsMotor(monoFpgaName, monoCrystalMotorId);            
            __tableV1 = new EpicsMotor(monoFpgaName, monoTableV1MotorId);
            __tableV2 = new EpicsMotor(monoFpgaName, monoTableV2MotorId);

            __crystal.Init();
            __tableV1.Init();
            __tableV2.Init();

            __crystal.EncoderOffset = monoCrystalEncoderOffset;
            __tableV1.EncoderOffset = monoTableV1EncoderOffset;
            __tableV2.EncoderOffset = monoTableV2EncoderOffset;

            __mono = new XasMonochromator();
            __mono.LatticeSpacing = monoLatticeSpacing;
            __mono.CrystalGap = monoCrystalGap;
            __mono.EncoderCountsPerEgu = __crystal.EncoderCountesPerEGgu;
            __mono.EncoderOffset = __crystal.EncoderOffset;
        }
        //===========================================================





        #region Public Methods
        //-----------------------------------------------------------
        public Int32 ConvertEnergyToAngle(double _energy, out double _phi)
        {
            return __mono.ConvertEnergyToAngle(_energy, out _phi) == XasMonochromator.SUCCESS ? SUCCESS : ERROR;
        }
        public Int32 ConvertAngleToEnergy(double _phi, out double _energy)
        {
            return __mono.ConvertAngleToEnergy(_phi, out _energy) == XasMonochromator.SUCCESS ? SUCCESS : ERROR;
        }
        public Int32 GetBeamOffset(double _energy, out double _z)
        {
            return __mono.GetBeamOffset(_energy, out _z) == XasMonochromator.SUCCESS ? SUCCESS : ERROR;
        }
        //-----------------------------------------------------------
        #endregion


        public Int32 GetEnergy(ref double _energy, ref string _error)
        {
            Int32 rt;
            double currentBraggAngle;

            currentBraggAngle = (__crystal.EncoderCounts / __crystal.EncoderCountesPerEGgu) + __crystal.EncoderOffset;
            rt = __mono.ConvertAngleToEnergy(currentBraggAngle, out _energy);
            if (rt != XasMonochromator.SUCCESS)
            {

                return ERROR;
            }
            return SUCCESS;
        }


        public Int32 SetTableToBeamOffset(ref string _error)
        {
            Int32 rt;
            double tableToBeamOffset;
            double beamOffset;
            double currentBraggAngle;
            double currentEnergy;            
            double currentTableV1;
            double currentTableV2;
            double currentTableVert;
           
            currentBraggAngle = (__crystal.EncoderCounts / __crystal.EncoderCountesPerEGgu) + __crystal.EncoderOffset;

            rt = __mono.ConvertAngleToEnergy(currentBraggAngle, out currentEnergy);
            if (rt != XasMonochromator.SUCCESS)
            {
                _error = this.GetType().Name + " " + System.Reflection.MethodBase.GetCurrentMethod().Name;
                _error += " XasMonochromator";
                return ERROR;
            }

            rt = __mono.GetBeamOffset(currentEnergy, out beamOffset);
            if (rt != XasMonochromator.SUCCESS)
            {
                _error = this.GetType().Name + " " + System.Reflection.MethodBase.GetCurrentMethod().Name;
                _error += " XasMonochromator";
                return ERROR;
            }

            currentTableV1 = (__tableV1.EncoderCounts / __tableV1.EncoderCountesPerEGgu) + __tableV1.EncoderOffset;
            currentTableV2 = (__tableV2.EncoderCounts / __tableV2.EncoderCountesPerEGgu) + __tableV2.EncoderOffset;
            currentTableVert = (currentTableV1 + currentTableV2) / 2.0;

            tableToBeamOffset = currentTableVert - beamOffset;


            rt = ca.put("BL22:STORE", ":TABLE_BEAM_OFFSET", tableToBeamOffset);
            if (rt != ca.SUCCESS)
            {
                _error = this.GetType().Name + " " + System.Reflection.MethodBase.GetCurrentMethod().Name;
                _error += " caput";
                return ERROR;
            }

            return SUCCESS;
        }


        public Int32 MoveToEnergyBlocking(double _targetEnergy, out double _achievedEnergy, double _maxEnergyError, double _maxTableError, ref string _error)
        {
            // moves the crystal first
            // then calculates the achieved energy
            // lastly the table is moved using the achieved energy

            Int32 rt;
            Int32 dmov;
            Int32 SHORT_SLEEP = 20;
            Int32 LONG_SLEEP = 100;

            
            double initialBraggAngle;   // derived from encoder, bragg angle before any move
            double initialEnergy;
            double targetBraggAngle;    // target bragg angle, derived from _targetEnergy
            double targetMotorCrystal;       // target motor position
            double achievedBraggAngle;  // derived from encoder, bragg angle after the move
            double energyError;         // deviation between targetEnergy and achievedEnergy

            double tableError;

            _achievedEnergy = 0;



            #region verify motors are not currently running
            //-----------------------------------------------------------
            while (true)
            {
                rt = __crystal.cagetDMOV(out dmov);
                if (rt == EpicsMotor.SUCCESS && dmov == 1)
                    break;

                Thread.Sleep(SHORT_SLEEP);
                Console.Write(".");
            }
            if (rt != EpicsMotor.SUCCESS)
            {
                _error = this.GetType().Name + " " + System.Reflection.MethodBase.GetCurrentMethod().Name;
                _error += " MotorIsMoving_or_MotorIsUnavailable";
                return ERROR;
            }


            if (this.UseTable)
            {                                 
                while (true)
                {
                    rt = __tableV1.cagetDMOV(out dmov);
                    if (rt == EpicsMotor.SUCCESS && dmov == 1)
                        break;

                    Thread.Sleep(LONG_SLEEP);
                    //Console.WriteLine("_DMOV_V1_");
                    Console.Write(".");
                }
                if (rt != EpicsMotor.SUCCESS)
                {
                    _error = this.GetType().Name + " " + System.Reflection.MethodBase.GetCurrentMethod().Name;
                    _error += " MotorIsMoving_or_MotorIsUnavailable";
                    return ERROR;
                }

                while (true)
                {
                    rt = __tableV2.cagetDMOV(out dmov);
                    if (rt == EpicsMotor.SUCCESS && dmov == 1)
                        break;

                    Thread.Sleep(LONG_SLEEP);
                    //Console.WriteLine("_DMOV_V2_");
                    Console.Write(".");
                }
                if (rt != EpicsMotor.SUCCESS)
                {
                    _error = this.GetType().Name + " " + System.Reflection.MethodBase.GetCurrentMethod().Name;
                    _error += " MotorIsMoving_or_MotorIsUnavailable";
                    return ERROR;
                }
            }
            //-----------------------------------------------------------
            #endregion



            // get the current Bragg angle, derived by the encoder
            initialBraggAngle = (__crystal.EncoderCounts / __crystal.EncoderCountesPerEGgu) + __crystal.EncoderOffset;

            rt = __mono.ConvertAngleToEnergy(initialBraggAngle, out initialEnergy);
            if (rt != XasMonochromator.SUCCESS)
            {
                _error = this.GetType().Name + " " + System.Reflection.MethodBase.GetCurrentMethod().Name;
                _error += " XasMonochromator";
                return ERROR;
            }

            // get target BraggAngle
            rt = __mono.ConvertEnergyToAngle(_targetEnergy, out targetBraggAngle);
            if (rt != XasMonochromator.SUCCESS)
            {
                _error = this.GetType().Name + " " + System.Reflection.MethodBase.GetCurrentMethod().Name;
                _error += " XasMonochromator";
                return ERROR;
            }

            // calculate the target motor position 'targetCrystal'
            targetMotorCrystal = targetBraggAngle - initialBraggAngle + __crystal.Position;



            #region move Crystal
            //-----------------------------------------------------------
            rt = __crystal.MoveAbsolute(targetMotorCrystal);
            if (rt != EpicsMotor.SUCCESS)
            {
                _error = System.Reflection.MethodBase.GetCurrentMethod().Name;
                _error += " MoveAbsoluteFailed";
                return ERROR;
            }
            //-----------------------------------------------------------
            #endregion

            Thread.Sleep(LONG_SLEEP);

            #region wait for Crystal
            //-----------------------------------------------------------
            while (true)
            {
                rt = __crystal.cagetDMOV(out dmov);
                if (rt == EpicsMotor.SUCCESS && dmov == 1)
                    break;

                //Console.Write("_DMOV_CRYSTAL_");
                Console.Write(".");
                Thread.Sleep(SHORT_SLEEP);
            }
            //-----------------------------------------------------------
            #endregion

            Thread.Sleep(LONG_SLEEP);

            #region get achievedBraggAngle and calculate deltaEnergy
            //-----------------------------------------------------------
            achievedBraggAngle = (__crystal.EncoderCounts / __crystal.EncoderCountesPerEGgu) + __crystal.EncoderOffset;
            rt = __mono.ConvertAngleToEnergy(achievedBraggAngle, out _achievedEnergy);
            if (rt != XasMonochromator.SUCCESS)
            {
                _error = this.GetType().Name + " " + System.Reflection.MethodBase.GetCurrentMethod().Name;
                _error += " XasMonochromator";
                return ERROR;
            }
            energyError = Math.Abs(_achievedEnergy - _targetEnergy);
            //-----------------------------------------------------------
            #endregion



            if (!this.UseTable)
                return SUCCESS;

            double initialBeamOffset;
            double finalBeamOffset;

            double targetMotorTableV1;
            double targetMotorTableV2;

            double initialTableVert1;
            double initialTableVert2;
            double initialTableVert;

            double targetTableVert1;
            double targetTableVert2;
            double targetTableVert = 0;

            double achievedTableVert1;
            double achievedTableVert2;
            double achievedTableVert;

            double tablePitch = 0;      // there should be a Table device with TableVert And TablePitch properties;
            double tableLength = 1000;  // distance between two table jacks

            double beamOffset;
            double tableToBeamOffset;


            if (!this.UseTableEncoders)
            {
                // if no encoders can be used, we have to assume that the table moves reproducable

                rt = __mono.GetBeamOffset(initialEnergy, out initialBeamOffset);
                if (rt != XasMonochromator.SUCCESS)
                {
                    _error = this.GetType().Name + " " + System.Reflection.MethodBase.GetCurrentMethod().Name;
                    _error += " XasMonochromator";
                    return ERROR;
                }

                rt = __mono.GetBeamOffset(_achievedEnergy, out finalBeamOffset);
                if (rt != XasMonochromator.SUCCESS)
                {
                    _error = this.GetType().Name + " " + System.Reflection.MethodBase.GetCurrentMethod().Name;
                    _error += " XasMonochromator";
                    return ERROR;
                }

                targetMotorTableV1 = initialBeamOffset - finalBeamOffset + __tableV1.Position;
                targetMotorTableV2 = initialBeamOffset - finalBeamOffset + __tableV2.Position;
            }
            else
            {

                rt = __mono.GetBeamOffset(_achievedEnergy, out beamOffset);
                if (rt != XasMonochromator.SUCCESS)
                {
                    _error = this.GetType().Name + " " + System.Reflection.MethodBase.GetCurrentMethod().Name;
                    _error += " XasMonochromator";
                    return ERROR;
                }


                rt = ca.get("BL22:STORE", ":TABLE_BEAM_OFFSET", out tableToBeamOffset);
                if (rt != ca.SUCCESS)
                {
                    _error = this.GetType().Name + " " + System.Reflection.MethodBase.GetCurrentMethod().Name;
                    _error += " caput";
                    return ERROR;
                }
                
                targetTableVert = beamOffset + tableToBeamOffset;


                // get the current table position based on encoder
                initialTableVert1 = (__tableV1.EncoderCounts / __tableV1.EncoderCountesPerEGgu) + __tableV1.EncoderOffset;
                initialTableVert2 = (__tableV2.EncoderCounts / __tableV2.EncoderCountesPerEGgu) + __tableV2.EncoderOffset;
                initialTableVert = (initialTableVert1 + initialTableVert2) / 2.0;

                
                // inverse kinematics for V1 & V2, keeping tablePitch constant
                targetTableVert1 = targetTableVert - tableLength * tablePitch / 2.0;
                targetTableVert2 = targetTableVert + tableLength * tablePitch / 2.0;


                targetMotorTableV1 = targetTableVert1 - initialTableVert1 + __tableV1.Position;
                targetMotorTableV2 = targetTableVert2 - initialTableVert2 + __tableV2.Position;
            }


            
            #region Move Table
            //-----------------------------------------------------------
            rt = __tableV1.MoveAbsolute(targetMotorTableV1);
            if (rt != EpicsMotor.SUCCESS)
            {
                _error = System.Reflection.MethodBase.GetCurrentMethod().Name;
                _error += " MoveAbsoluteFailed";
                return ERROR;
            }

            rt = __tableV2.MoveAbsolute(targetMotorTableV2);
            if (rt != EpicsMotor.SUCCESS)
            {
                _error = System.Reflection.MethodBase.GetCurrentMethod().Name;
                _error += " MoveAbsoluteFailed";
                return ERROR;
            }
            //-----------------------------------------------------------
            #endregion

            Thread.Sleep(LONG_SLEEP);

            #region wait for table
            //-----------------------------------------------------------
            while (true)
            {
                rt = __tableV1.cagetDMOV(out dmov);
                if (rt == EpicsMotor.SUCCESS && dmov == 1)   // checking rt is important here
                    break;

                //Console.Write("_DMOV_TABLE_V1_");
                Console.Write(".");
                Thread.Sleep(SHORT_SLEEP);
            }

            while (true)
            {
                rt = __tableV2.cagetDMOV(out dmov);
                if (rt == EpicsMotor.SUCCESS && dmov == 1)   // checking rt is important here
                    break;

                //Console.Write("_DMOV_TABLE_V2_");
                Console.Write(".");
                Thread.Sleep(SHORT_SLEEP);
            }
            //-----------------------------------------------------------
            #endregion

            Thread.Sleep(LONG_SLEEP);


            if (this.UseTableEncoders)
            {
                #region get achieved table position based on encoder and calculate deltaTable
                //-----------------------------------------------------------
                achievedTableVert1 = (__tableV1.EncoderCounts / __tableV1.EncoderCountesPerEGgu) + __tableV1.EncoderOffset;
                achievedTableVert2 = (__tableV2.EncoderCounts / __tableV2.EncoderCountesPerEGgu) + __tableV2.EncoderOffset;
                achievedTableVert = (achievedTableVert1 + achievedTableVert2) / 2.0;

                tableError = Math.Abs(achievedTableVert - targetTableVert);
                //-----------------------------------------------------------
                #endregion

            }



            return SUCCESS;
        }


     



    }
}
