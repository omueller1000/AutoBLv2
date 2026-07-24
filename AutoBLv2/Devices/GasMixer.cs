using EPICS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Devices
{
    public class GasMixer
    {
        #region Constants
        //-----------------------------------------------------------
        public const Int32 SUCCESS = 0;
        public const Int32 ERROR = -1;
        //-----------------------------------------------------------
        private const Int32 ILLDEFINED = 0;
        private const Int32 IDLE = 1;
        private const Int32 START = 2;
        private const Int32 IN_PROGRESS = 3;
        //-----------------------------------------------------------
        private const Int32 MANIFOLD = 4;
        //-----------------------------------------------------------
        private const Int32 SLEEP = 100;
        private const Int32 SHORT_SLEEP = 10;
        //-----------------------------------------------------------
        #endregion



        #region Enumerations        
        //-----------------------------------------------------------
        public enum Gas
        {
            Helium,
            Nitrogen,
            Argon
        }
        public enum IonChamber
        {
            I0,
            I1,
            I2,
            Manifold
        }
        //-----------------------------------------------------------        
        #endregion



        #region Variables
        //-----------------------------------------------------------        
        private string __name;        
        //-----------------------------------------------------------
        private static Mutex __mutexVent;
        private static Int32 __stateVent;
        private static camonitor __monitorVent;
        //-----------------------------------------------------------
        private static Mutex __mutexFill;
        private static Int32 __stateFill;
        private static camonitor __monitorFill;
        //-----------------------------------------------------------
        #endregion




        #region Properties
        //-----------------------------------------------------------
        public double maxP { get; set; }
        public double minP { get; set; }
        //-----------------------------------------------------------
        #endregion



        //===========================================================
        public GasMixer(string _name)
        {
            __name = _name;
            this.maxP = 3.0;
            this.minP = 0.8;


            __mutexFill = new Mutex();
            __monitorFill = new camonitor(__name, ".Fill");
            __monitorFill.MonitorEvent += __monitorFill_MonitorEvent;


            __mutexVent = new Mutex();
            __monitorVent = new camonitor(__name, ".Vent");
            __monitorVent.MonitorEvent += __monitorVent_MonitorEvent;
        }
        //===========================================================



        #region Private Methods
        //-----------------------------------------------------------
        //-----------------------------------------------------------
        #endregion



        #region Public Methods
        //-----------------------------------------------------------
        public Int32 GetLastIcGas(IonChamber _ionChamber, out IcGas _icGas, ref string _error)
        {
            Int32 rt;
            Int32 ionChamberIndex;
            Int32 gasValue;


            ionChamberIndex = (Int32)_ionChamber + 1;
            _icGas = new IcGas();

            rt = ca.get(__name, $".IC{ionChamberIndex}L1SetLine", out gasValue);
            if (rt != ca.SUCCESS)
            {
                _error = this.GetType().Name + " " + System.Reflection.MethodBase.GetCurrentMethod().Name;
                _error += " ca.put failed";
                return ERROR;
            }
            _icGas.Gas1 = Enum.GetName(typeof(Gas), (Gas)(gasValue + 1));


            rt = ca.get(__name, $".IC{ionChamberIndex}L2SetLine", out gasValue);
            if (rt != ca.SUCCESS)
            {
                _error = this.GetType().Name + " " + System.Reflection.MethodBase.GetCurrentMethod().Name;
                _error += " ca.put failed";
                return ERROR;
            }
            _icGas.Gas2 = Enum.GetName(typeof(Gas), (Gas)(gasValue + 1));


            rt = ca.get(__name, $".IC{ionChamberIndex}L1SetP", out _icGas.Pressure1);
            if (rt != ca.SUCCESS)
            {
                _error = this.GetType().Name + " " + System.Reflection.MethodBase.GetCurrentMethod().Name;
                _error += " ca.put failed";
                return ERROR;
            }


            rt = ca.get(__name, $".IC{ionChamberIndex}L2SetP", out _icGas.Pressure2);
            if (rt != ca.SUCCESS)
            {
                _error = this.GetType().Name + " " + System.Reflection.MethodBase.GetCurrentMethod().Name;
                _error += " ca.put failed";
                return ERROR;
            }


            rt = GetPressure(_ionChamber, out _icGas.CurrentTotalPressure, ref _error);
            if (rt != ca.SUCCESS)
            {
                _error = this.GetType().Name + " " + System.Reflection.MethodBase.GetCurrentMethod().Name + " " + _error;
                return ERROR;
            }


            return SUCCESS;
        }
        public Int32 Vent(IonChamber _ionChamber, ref string _error)
        {
            Int32 rt;
            Int32 stateVent;


            rt = ca.put(__name, ".SetIC", (Int32)_ionChamber + 1);
            if (rt != ca.SUCCESS)
            {
                _error = this.GetType().Name + " " + System.Reflection.MethodBase.GetCurrentMethod().Name;
                _error += " ca.put failed";
                return ERROR;
            }
            Thread.Sleep(SHORT_SLEEP);


            stateVent = ILLDEFINED;
            __stateVent = ILLDEFINED;
            rt = ca.put(__name, ".Vent", START);
            if (rt != ca.SUCCESS)
            {
                _error = this.GetType().Name + " " + System.Reflection.MethodBase.GetCurrentMethod().Name;
                _error += " ca.put failed";
                return ERROR;
            }
            Thread.Sleep(SHORT_SLEEP);


            __monitorVent.StartMonitor();


            while (true)
            {
                Thread.Sleep(SLEEP);

                //............................
                __mutexVent.WaitOne();
                stateVent = __stateVent;
                __mutexVent.ReleaseMutex();
                //............................

                if (stateVent == IDLE)
                    break;
            }


            __monitorVent.StopMonitor();


            return SUCCESS;
        }
        public Int32 Fill(IonChamber _ionChamber, Gas _gas, double _p, ref string _error)
        {
            Int32 rt;
            Int32 stateFill;


            if (_p < this.minP)
            {
                _error = this.GetType().Name + " " + System.Reflection.MethodBase.GetCurrentMethod().Name;
                _error += " P_TOO_SMALL";
                return ERROR;
            }

            if (_p > this.maxP)
            {
                _error = this.GetType().Name + " " + System.Reflection.MethodBase.GetCurrentMethod().Name;
                _error += " P_EXCEEDS_MAX";
                return ERROR;
            }

            rt = ca.put(__name, ".SetIC", (Int32)_ionChamber + 1);
            if (rt != ca.SUCCESS)
            {
                _error = this.GetType().Name + " " + System.Reflection.MethodBase.GetCurrentMethod().Name;
                _error += " ca.put failed";
                return ERROR;
            }
            Thread.Sleep(SHORT_SLEEP);

            rt = ca.put(__name, ".SetLine", (Int32)_gas + 1);
            if (rt != ca.SUCCESS)
            {
                _error = this.GetType().Name + " " + System.Reflection.MethodBase.GetCurrentMethod().Name;
                _error += " ca.put failed";
                return ERROR;
            }
            Thread.Sleep(SHORT_SLEEP);

            rt = ca.put(__name, ".SetP", _p);
            if (rt != ca.SUCCESS)
            {
                _error = this.GetType().Name + " " + System.Reflection.MethodBase.GetCurrentMethod().Name;
                _error += " ca.put failed";
                return ERROR;
            }
            Thread.Sleep(SHORT_SLEEP);



            stateFill = ILLDEFINED;
            __stateFill = ILLDEFINED;
            rt = ca.put(__name, ".Fill", START);
            if (rt != ca.SUCCESS)
            {
                _error = this.GetType().Name + " " + System.Reflection.MethodBase.GetCurrentMethod().Name;
                _error += " ca.put failed";
                return ERROR;
            }
            Thread.Sleep(SHORT_SLEEP);


            __monitorFill.StartMonitor();


            while (true)
            {
                Thread.Sleep(SLEEP);

                //............................
                __mutexFill.WaitOne();
                stateFill = __stateFill;
                __mutexFill.ReleaseMutex();
                //............................

                if (stateFill == IDLE)
                    break;
            }


            __monitorFill.StopMonitor();


            return SUCCESS;
        }
        public Int32 Fill(IonChamber _ionChamber, Gas _gas1, double _p1, Gas _gas2, double _p2, ref string _error)
        {
            Int32 rt;
            double lowP, highP;
            Gas lowPGas, highPGas;


            if (Math.Min(_p1, _p2) < this.minP)
            {
                _error = this.GetType().Name + " " + System.Reflection.MethodBase.GetCurrentMethod().Name;
                _error += " P_TOO_SMALL";
                return ERROR;
            }


            if (_p1 + _p2 > this.maxP)
            {
                _error = this.GetType().Name + " " + System.Reflection.MethodBase.GetCurrentMethod().Name;
                _error += " P_EXCEEDS_MAX";
                return ERROR;
            }


            if (_p1 < _p2)
            {
                lowP = _p1;
                highP = _p1 + _p2;
                lowPGas = _gas1;
                highPGas = _gas2;
            }
            else
            {
                lowP = _p2;
                highP = _p1 + _p2;
                lowPGas = _gas2;
                highPGas = _gas1;
            }


            rt = Fill(_ionChamber, lowPGas, lowP, ref _error);
            if (rt != SUCCESS)
            {
                _error = this.GetType().Name + " " + System.Reflection.MethodBase.GetCurrentMethod().Name + " " + _error;
                return ERROR;
            }
            Thread.Sleep(SLEEP);


            rt = Vent(IonChamber.Manifold, ref _error);
            if (rt != SUCCESS)
            {
                _error = this.GetType().Name + " " + System.Reflection.MethodBase.GetCurrentMethod().Name + " " + _error;
                return ERROR;
            }
            Thread.Sleep(SLEEP);


            rt = Fill(IonChamber.Manifold, highPGas, highP, ref _error);
            if (rt != SUCCESS)
            {
                _error = this.GetType().Name + " " + System.Reflection.MethodBase.GetCurrentMethod().Name + " " + _error;
                return ERROR;
            }
            Thread.Sleep(SLEEP);


            rt = Fill(_ionChamber, highPGas, highP, ref _error);
            if (rt != SUCCESS)
            {
                _error = this.GetType().Name + " " + System.Reflection.MethodBase.GetCurrentMethod().Name + " " + _error;
                return ERROR;
            }


            return SUCCESS;
        }
        //-----------------------------------------------------------
        public Int32 GetPressure(IonChamber _ionChamber, out double _pressure, ref string _error)
        {
            Int32 rt;

            _pressure = 0;
            rt = ca.get(__name, ".IC" + ((Int32)_ionChamber + 1) + "P", out _pressure);
            if (rt != ca.SUCCESS)
            {
                _error = this.GetType().Name + " " + System.Reflection.MethodBase.GetCurrentMethod().Name;
                _error += " ca.get failed";
                return ERROR;
            }


            return SUCCESS;
        }
        //-----------------------------------------------------------
        #endregion



        private void __monitorVent_MonitorEvent(object sender, OmEpicsMonitorEventArg arg)
        {
            Int32[] state;
            camonitor.DbrToInt32(arg.dbr, out state);

            //............................
            __mutexVent.WaitOne();
            __stateVent = state[0];
            __mutexVent.ReleaseMutex();
            //............................
        }
        private void __monitorFill_MonitorEvent(object sender, OmEpicsMonitorEventArg arg)
        {
            Int32[] state;
            camonitor.DbrToInt32(arg.dbr, out state);

            //............................
            __mutexFill.WaitOne();
            __stateFill = state[0];
            __mutexFill.ReleaseMutex();
            //............................
        }
    }



    public class IcGas
    {
        public string Gas1 = "";
        public string Gas2 = "";
        public double Pressure1 = 0;
        public double Pressure2 = 0;
        public double CurrentTotalPressure = 0;


        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("GAS_1: " + Gas1);
            sb.AppendLine("GAS_2: " + Gas2);
            sb.AppendLine("P_1: " + Pressure1.ToString("F1") + "_Bar");
            sb.AppendLine("P_2: " + Pressure2.ToString("F1") + "_Bar");
            sb.AppendLine("TOTAL_P: " + CurrentTotalPressure.ToString("F1") + "_Bar");

            return sb.ToString();
        }

    }


}
