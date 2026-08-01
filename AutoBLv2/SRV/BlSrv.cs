using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
