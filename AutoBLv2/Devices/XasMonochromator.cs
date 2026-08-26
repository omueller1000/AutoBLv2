using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devices
{
    public class XasMonochromator
    {

        #region Constants
        //--------------------------------------------------------------
        public const double CONST_H_PLANCK = 6.62607004e-34;
        public const double CONST_C_LIGHT = 299792458;
        public const double CONST_M_E = 9.10938356e-31;
        public const double CONST_Q_E = 1.60217662e-19;
        //--------------------------------------------------------------
        public const Int32 SUCCESS = 1;
        public const Int32 ERROR = -1;
        //--------------------------------------------------------------
        #endregion

        #region Variables
        //--------------------------------------------------------------
        private double __crystalGap;
        private double __latticeSpacing;
        private double __encoderOffset;
        private double __encoderResolution;
        //--------------------------------------------------------------
        #endregion

        #region Properties
        //-----------------------------------------------------------
        public double LatticeSpacing
        {
            get { return __latticeSpacing; }
            set { __latticeSpacing = value; }
        }
        public double CrystalGap
        {
            get { return __crystalGap; }
            set { __crystalGap = value; }
        }
        public double EncoderCountsPerEgu
        {
            get { return __encoderResolution; }
            set { __encoderResolution = value; }
        }
        public double EncoderOffset
        {
            get { return __encoderOffset; }
            set { __encoderOffset = value; }
        }
        //-----------------------------------------------------------
        #endregion


        //===========================================================
        public XasMonochromator()
        {
            // load some defaults
            this.LatticeSpacing = 0;
            this.CrystalGap = 0;
            this.EncoderCountsPerEgu = 0;
            this.EncoderOffset = 0;
        }
        //===========================================================


        #region Public Methods
        //-----------------------------------------------------------		
        public Int32 ConvertEnergyToAngle(double _energy, out double _phi)
        {
            double low_e_threshold;

            _phi = 0;
            low_e_threshold = CONST_H_PLANCK * CONST_C_LIGHT / (Math.PI * CONST_Q_E * this.LatticeSpacing * 1e-10);

            if (_energy < low_e_threshold)
                return ERROR;

            _phi = Math.Asin(CONST_H_PLANCK * CONST_C_LIGHT / (2.0 * this.LatticeSpacing * 1e-10 * CONST_Q_E * _energy)) * 180.0 / Math.PI;

            return SUCCESS;
        }
        public Int32 ConvertAngleToEnergy(double _phi, out double _energy)
        {
            _energy = 0;
            if (_phi <= 0 || _phi >= 90)
                return ERROR;

            _energy = CONST_H_PLANCK * CONST_C_LIGHT / (2.0 * this.LatticeSpacing * 1e-10 * CONST_Q_E * Math.Sin(_phi * Math.PI / 180.0));

            return SUCCESS;
        }
        public Int32 GetBeamOffset(double _energy, out double _z)
        {
            double phi;
            _z = 0;

            if (ConvertEnergyToAngle(_energy, out phi) < 0)
                return ERROR;

            _z = 2.0 * this.CrystalGap * Math.Cos(phi * Math.PI / 180.0);

            return SUCCESS;
        }
        //-----------------------------------------------------------
        #endregion



    }
}
