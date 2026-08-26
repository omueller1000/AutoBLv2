using AutoBLv2.SRV;
using Devices;
using FPGA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using static MongoDB.Driver.WriteConcern;

namespace AutoBLv2
{

    public class ServerLock
    {
        private bool __isLocked;


        public bool IsLocked
        {
            get { return __isLocked; }
            set
            {
                __isLocked = value;
                if (!value)
                    this.Message = "";
            }
        }
        public string Message { get; set; }

        //===========================================================        
        public ServerLock()
        {
            this.IsLocked = false;
        }
        //===========================================================        

    }




    public class Server
    {
        #region Constants
        //-----------------------------------------------------------
        public const Int32 SUCCESS = 1;
        public const Int32 ERROR = -1;
        //-----------------------------------------------------------
        private const Int32 REQ_MIN_LEN = 1;
        private const Int32 OFFSET_KEYWORD = 0;
        private const Int32 OFFSET_SERVER_COMMAND = 1;
        //-----------------------------------------------------------
        private string BL_DEF_PATH = "C:\\SSRL_LOCAL_OM\\definitions\\bl.def";
        //-----------------------------------------------------------
        #endregion



        #region Variables
        //-----------------------------------------------------------
        private static ServerLock __lock;
        private OmServer __server;
        private string __dbLogName = "LOGS";
        private FpgaDaq __fpgaDaq;
        private string __fpgaDaqName = "FPGA";
        //-----------------------------------------------------------
        private BlSrv __blSrv;
        //-----------------------------------------------------------
        #endregion



        #region Properties
        //-----------------------------------------------------------

        //-----------------------------------------------------------
        #endregion



        //===========================================================        
        public Server(int _port)
        {
            Int32 rt;
            string error = "";
            string value;
            __lock = new ServerLock();



            rt = Def.Def.ReadDefinition(BL_DEF_PATH, "FPGA_NAME", out value, ref error);
            __fpgaDaqName = value;
            __fpgaDaq = new FpgaDaq(__fpgaDaqName);


            


            #region Beamline Server
            //.................................................
            __blSrv = new BlSrv(ref __fpgaDaq, ref __lock);
            Console.WriteLine("BlSrv");
            //.................................................
            #endregion



            // -- START THE SERVER --
            Utils.DbLogging dbLogging = new Utils.DbLogging("mongodb://xasdb1.slac.stanford.edu:27017", __dbLogName);
            __server = new OmServer(_port, (OmServer.ServerCallback)EvaluateRequest, ref dbLogging);
            __server.DisableLogger = true;
        }
        //===========================================================        


        #region Private Methods        
        //-----------------------------------------------------------
        private Int32 EvaluateRequest(object _sender, UInt64 _id, ref string _req, ref string _res)
        {
            Int32 rt;
            bool err = false;
            string[] reqArr = _req.Split(" ", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            byte[] arr = Encoding.ASCII.GetBytes(_req);


            if (reqArr.Length < REQ_MIN_LEN)
            {
                err = true;
                _res = "INVALID_REQUEST_LENGTH";
            }

            if (!err)
            {
                switch (reqArr[OFFSET_KEYWORD].ToUpper())
                {
                    #region SERVER
                    //...........................................................
                    case "SERVER":
                        rt = EvaluateServerRequest(ref _sender, _id, ref reqArr, ref _res);
                        if (rt < 0)
                        {
                            err = true;
                            break;
                        }
                        break;
                    //...........................................................
                    #endregion


                    #region SERVER
                    //...........................................................
                    case "BL":
                        rt = __blSrv.EvaluateRequest(ref reqArr, ref _res);
                        if (rt < 0)
                        {
                            err = true;
                            break;
                        }
                        break;
                    //...........................................................
                    #endregion


                    #region Default
                    //...........................................................
                    default:
                        err = true;
                        _res = "UNKNOWN_KEYWORD";
                        break;
                        //...........................................................
                    #endregion
                }
            }

            if (err)
            {
                _res = this.GetType().Name + " " + System.Reflection.MethodBase.GetCurrentMethod().Name + " " + _res;
                return ERROR;
            }

            _res = reqArr[OFFSET_KEYWORD].ToUpper() + " " + _res;

            return SUCCESS;
        }

        private Int32 EvaluateServerRequest(ref object _sender, UInt64 _id, ref string[] _reqArr, ref string _res)
        {
            bool err = false;

            switch (_reqArr[OFFSET_SERVER_COMMAND].ToUpper())
            {

                #region GET_ID
                //...........................................................
                case "GET_ID":
                    if (_reqArr.Length != OFFSET_SERVER_COMMAND + 1)
                    {
                        err = true;
                        _res = "INCOMPATIBLE_REQUETS_LENGTH";
                        break;
                    }
                    _res = " \r" + _id;
                    break;
                //...........................................................
                #endregion


                #region LIST_IDS
                //...........................................................
                case "LIST_IDS":
                    if (_reqArr.Length != OFFSET_SERVER_COMMAND + 1)
                    {
                        err = true;
                        _res = "INCOMPATIBLE_REQUETS_LENGTH";
                        break;
                    }
                    ((OmServer)_sender).GetClientIds(ref _res);
                    _res = _reqArr[OFFSET_SERVER_COMMAND].ToUpper() + " " + _res;
                    break;
                //...........................................................
                #endregion


                #region Default
                //...........................................................
                default:
                    err = true;
                    _res = "UNKNON_COMMAND";
                    break;
                    //...........................................................
                    #endregion
            }

            if (err)
            {
                _res = this.GetType().Name + " " + System.Reflection.MethodBase.GetCurrentMethod().Name + " " + _res;
                return ERROR;
            }

            _res = _reqArr[OFFSET_SERVER_COMMAND].ToUpper() + " " + _res;

            return SUCCESS;
        }
        //-----------------------------------------------------------
        #endregion

    }
}
