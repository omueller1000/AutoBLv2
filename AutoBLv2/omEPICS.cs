using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace EPICS
{
    public class ca
    {
        #region Const
        //-----------------------------------------------------------------				
        private const string __omepics_dll_version = "om_epics.dll";
        private const string __path_omepics_dll = "C:\\SSRL_LOCAL_OM\\dll\\" + __omepics_dll_version;
        //-----------------------------------------------------------------
        public const Int32 SUCCESS = 0;
        //-----------------------------------------------------------------
        #endregion




        #region caput / caget with context        
        //-----------------------------------------------------------------
        public static Int32 createContextCtx(out IntPtr _ctx)
        {
            Int32 rt = 0;

            rt = ext_om_epics_create_context_ctx(out _ctx);

            return rt;
        }
        public static Int32 destroyContextCtx(IntPtr _ctx)
        {
            Int32 rt = 0;

            rt = ext_om_epics_destroy_context_ctx(_ctx);

            return rt;
        }
        //-----------------------------------------------------------------
        public static Int32 putCtx(IntPtr _ctx, string _name, string _pv_name, object _val)
        {
            return ext_om_epics_caput_ctx(_ctx, _name, _pv_name, _val.ToString());
        }
        //-----------------------------------------------------------------
        public static Int32 getIntCtx(IntPtr _ctx, string _name, string _pv_name, out Int32 _valInt)
        {
            Int32 rt;


            rt = ext_om_epics_caget_int_ctx(_ctx, _name, _pv_name, out _valInt);
            return rt;
        }
        public static Int32 getDoubleCtx(IntPtr _ctx, string _name, string _pv_name, out double _valDouble)
        {
            Int32 rt;


            rt = ext_om_epics_caget_double_ctx(_ctx, _name, _pv_name, out _valDouble);
            return rt;
        }
        //-----------------------------------------------------------------
        public static Int32 createChannelCtxChid(IntPtr _ctx, string _name, string _pv_name, out UInt64 _chid)
        {
            Int32 rt = 0;

            rt = ext_om_epics_create_channel_ctx_chid(_ctx, _name, _pv_name, out _chid);

            return rt;
        }
        public static Int32 clearChannelCtxChid(IntPtr _ctx, UInt64 _chid)
        {
            Int32 rt = 0;

            rt = ext_om_epics_clear_channel_ctx_chid(_ctx, _chid);

            return rt;
        }
        public static Int32 getDoubleCtxChid(IntPtr _ctx, UInt64 _chid, out double _value)
        {
            Int32 rt = 0;

            rt = ext_om_epics_caget_double_ctx_chid(_ctx, _chid, out _value);

            return rt;
        }
        //-----------------------------------------------------------------
        #endregion







        #region caput / caget        
        //-----------------------------------------------------------------
        public static Int32 put(string _name, string _pv_name, object _val)
        {
            return ext_om_epics_caput(_name, _pv_name, _val.ToString());
        }
        //-----------------------------------------------------------------
        public static Int32 get(string _name, string _pv_name, out string _valStr)
        {
            Int32 rt;
            IntPtr valPtr;

            rt = ext_om_epics_caget(_name, _pv_name, out valPtr);
            _valStr = Marshal.PtrToStringAnsi(valPtr);            

            return rt;
        }
        public static Int32 get(string _name, string _pv_name, out Int32 _valInt)
        {
            Int32 rt;
            string valStr;

            rt = get(_name, _pv_name, out valStr);
            _valInt = Convert.ToInt32(valStr);

            return rt;
        }
        public static Int32 get(string _name, string _pv_name, out Int64 _valInt64)
        {
            Int32 rt;
            string valStr;

            rt = get(_name, _pv_name, out valStr);
            _valInt64 = Convert.ToInt64(valStr);

            return rt;
        }
        public static Int32 get(string _name, string _pv_name, out double _valDouble)
        {
            Int32 rt;
            string valStr;

            rt = get(_name, _pv_name, out valStr);
            _valDouble = Convert.ToDouble(valStr);

            return rt;
        }
        public static Int32 getDouble(string _name, string _pv_name, out double _val)
        {
            Int32 rt;

            rt = ext_om_epics_caget_double(_name, _pv_name, out _val);
            return rt;
        }
        //-----------------------------------------------------------------
        #endregion




        #region caget arrays
        //-----------------------------------------------------------------      
        public static Int32 getArr(string _name, string _pv_name, out Int32[] dbr)
        {
            Int32 rt;
            Int32 count;
            IntPtr arrPtr = IntPtr.Zero;

            rt = ext_om_epics_caget_arr(_name, _pv_name, out count, out arrPtr);

            dbr = new Int32[count];

            //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~			
            unsafe
            {
                var SourcePtr = (Int32*)arrPtr;
                for (int i = 0; i < count; i++)
                {
                    dbr[i] = *SourcePtr;
                    SourcePtr++;
                }
            }
            //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

            return rt;
        }
        public static Int32 getArrInt(string _name, string _pv_name, out Int32[] dbr)
        {
            Int32 rt;
            Int32 count;
            IntPtr arrPtr = IntPtr.Zero;

            rt = ext_om_epics_caget_arr_int(_name, _pv_name, out count, out arrPtr);

            dbr = new Int32[count];

            //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~			
            unsafe
            {
                var SourcePtr = (Int32*)arrPtr;
                for (int i = 0; i < count; i++)
                {
                    dbr[i] = *SourcePtr;
                    SourcePtr++;
                }
            }
            //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

            return rt;
        }
        public static Int32 getDoubleArr(string _name, string _pv_name, out Int32[] dbr)
        {
            Int32 rt;
            Int32 count;
            IntPtr arrPtr = IntPtr.Zero;

            rt = ext_om_epics_caget_arr(_name, _pv_name, out count, out arrPtr);

            dbr = new Int32[count];

            //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~			
            unsafe
            {
                var SourcePtr = (Int32*)arrPtr;
                for (int i = 0; i < count; i++)
                {
                    dbr[i] = *SourcePtr;
                    SourcePtr++;
                }
            }
            //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

            return rt;
        }
        public static Int32 getArrFast(UInt64 chid_ptr, out Int32[] dbr)
        {
            Int32 rt;
            Int32 count;
            IntPtr arrPtr = IntPtr.Zero;

            rt = ext_om_epics_caget_arr_fast(chid_ptr, out count, out arrPtr);

            dbr = new Int32[count];

            //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~			
            unsafe
            {
                var SourcePtr = (Int32*)arrPtr;
                for (int i = 0; i < count; i++)
                {
                    dbr[i] = *SourcePtr;
                    SourcePtr++;
                }
            }
            //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

            return rt;
        }
        //-----------------------------------------------------------------
        #endregion






        [DllImport(__path_omepics_dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int ext_om_epics_create_context_ctx(out IntPtr _ctx);



        [DllImport(__path_omepics_dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int ext_om_epics_destroy_context_ctx(IntPtr _ctx);



        [DllImport(__path_omepics_dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int ext_om_epics_caput_ctx(
           IntPtr _ctx,
           [MarshalAs(UnmanagedType.LPStr)] string _name,
           [MarshalAs(UnmanagedType.LPStr)] string _pv_name,
           [MarshalAs(UnmanagedType.LPStr)] string _value);



        [DllImport(__path_omepics_dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int ext_om_epics_caget_int_ctx(
            IntPtr _ctx,
            [MarshalAs(UnmanagedType.LPStr)] string _name,
            [MarshalAs(UnmanagedType.LPStr)] string _pv_name,
            out Int32 _value);



        [DllImport(__path_omepics_dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int ext_om_epics_caget_double_ctx(
                    IntPtr _ctx,
                    [MarshalAs(UnmanagedType.LPStr)] string _name,
                    [MarshalAs(UnmanagedType.LPStr)] string _pv_name,
                    out double _value);



        [DllImport(__path_omepics_dll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern int ext_om_epics_create_channel_ctx_chid(
           IntPtr _ctx,
           [MarshalAs(UnmanagedType.LPStr)] string _name,
           [MarshalAs(UnmanagedType.LPStr)] string _pv_name,
           out UInt64 _chid);



        [DllImport(__path_omepics_dll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern int ext_om_epics_clear_channel_ctx_chid(
        IntPtr _ctx,
        UInt64 _chid);



        [DllImport(__path_omepics_dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int ext_om_epics_caget_double_ctx_chid(
            IntPtr context,
            UInt64 _chid,
            out double _value);



        [DllImport(__path_omepics_dll)]
        private static extern int ext_om_epics_caput(string _name, string _pv_name, string _value);
        


        [DllImport(__path_omepics_dll)]
        private static extern int ext_om_epics_caget(string _name, string _pv_name, out IntPtr _value);



        [DllImport(__path_omepics_dll)]
        private static extern int ext_om_epics_caget_double(string _name, string _pv_name, out double _value);



        [DllImport(__path_omepics_dll)]
        private static extern int ext_om_epics_caget_arr(string _name, string _pv_name, out Int32 _len, out IntPtr _arrPtr);



        [DllImport(__path_omepics_dll)]
        private static extern int ext_om_epics_caget_arr_int(string _name, string _pv_name, out Int32 _len, out IntPtr _arrPtr);



        [DllImport(__path_omepics_dll)]
        private static extern int ext_om_epics_caget_arr_fast(UInt64 _chid, out Int32 _len, out IntPtr _arrPtr);

    }



    public class camonitor
    {
        #region Const
        //-----------------------------------------------------------------
        private const string __omepics_dll_version = "om_epics.dll";
        private const string __path_omepics_dll = "C:\\SSRL_LOCAL_OM\\dll\\" + __omepics_dll_version;
        //-----------------------------------------------------------------
        public const Int32 SUCCESS = 0;
        public const Int32 ERROR = -1;
        //-----------------------------------------------------------------
        private const int EPICS_STRING = 0;
        private const int EPICS_INT = 1;
        private const int EPICS_SHORT = 1;
        private const int EPICS_FLOAT = 2;
        private const int EPICS_ENUM = 3;
        private const int EPICS_CHAR = 4;
        private const int EPICS_LONG = 5;
        private const int EPICS_DOUBLE = 6;
        //-----------------------------------------------------------------
        #endregion



        #region Public Variables
        //-----------------------------------------------------------------				
        private string __Name;
        private string __PvName;
        private IntPtr __PvPtr;
        //-----------------------------------------------------------------
        #endregion



        #region Private Variables
        //-----------------------------------------------------------------
        private System.Threading.Thread __tt;
        private OmEpicsMonitorCb __cb;
        //-----------------------------------------------------------------
        private cb_string __cb_debug_msg;
        private cb_string __cb_error_msg;
        private cb_string __cb_event_msg;
        //-----------------------------------------------------------------
        #endregion



        #region Properties
        //-----------------------------------------------------------------
        public bool isActive
        {
            get
            {
                if (__tt != null &&
                    __tt.IsAlive)
                    return true;
                else
                    return false;
            }
        }
        public string Name
        {
            get { return __Name; }
        }
        public string PvName
        {
            get { return __PvName; }
        }
        //-----------------------------------------------------------------
        #endregion



        #region Events
        //-----------------------------------------------------------------
        public event OmEpicsMessageEventHandler DebugMessageReceived;
        public event OmEpicsMessageEventHandler ErrorMessageReceived;
        public event OmEpicsMessageEventHandler EventMessageReceived;
        //-----------------------------------------------------------------
        public event OmEpicsMonitorEventHandler MonitorEvent;
        //-----------------------------------------------------------------
        #endregion



        #region Delegates
        //-----------------------------------------------------------------
        private delegate void cb_string(string str);
        //-----------------------------------------------------------------
        #endregion



        //=================================================================
        public camonitor(string _Name, string _pvName)
        {
            __cb_debug_msg = new cb_string(IncomingDebugMessage);
            __cb_error_msg = new cb_string(IncomingErrorMessage);
            __cb_event_msg = new cb_string(IncomingEventMessage);

            ext_debug_msg(__cb_debug_msg);
            ext_error_msg(__cb_error_msg);
            ext_event_msg(__cb_event_msg);

            __Name = _Name;
            __PvName = _pvName;

            __cb = new OmEpicsMonitorCb(InternalCbMonitorEvent);
        }
        ~camonitor()
        {
            StopMonitor();
        }
        //=================================================================


        #region Public Methods
        //-----------------------------------------------------------------
        public void StartMonitor()
        {
            EmitDebugMessage("START MONITOR: " + this.Name + this.PvName);

            if (__tt != null && __tt.IsAlive)
                return;

            __tt = new System.Threading.Thread(() => ext_om_epics_camonitor(this.Name, this.PvName, __cb));
            //__tt.Priority = System.Threading.ThreadPriority.Highest;
            __tt.IsBackground = true;
            __tt.Start();
        }
        public void StopMonitor()
        {

            if (__PvPtr == null || __PvPtr == IntPtr.Zero)
                return;

            if (__tt == null)
                return;

            if (!__tt.IsAlive)
                return;

            ext_om_epics_clear_camonitor(__PvPtr);

            __PvPtr = IntPtr.Zero;
        }
        //-----------------------------------------------------------------
        #endregion




        #region Public Static Methods
        //-----------------------------------------------------------------
        public static Int32 DbrToInt32(Int32[][] _dbr, out Int32[] _val)
        {
            int i, j, k;
            byte[] tmp_byte_arr, byte_arr;


            int count = _dbr.Length;
            int dbr_len = _dbr[0].Length;

            _val = new Int32[count];
            byte_arr = new byte[dbr_len * 4];

            for (i = 0; i < count; i++)
            {
                for (j = 0; j < dbr_len; j++)
                {
                    tmp_byte_arr = BitConverter.GetBytes(_dbr[i][j]);
                    for (k = 0; k < tmp_byte_arr.Length; k++)
                    {
                        byte_arr[k + j * 4] = tmp_byte_arr[k];
                    }
                }

                _val[i] = (Int32)BitConverter.ToInt32(byte_arr, 0);
            }

            return SUCCESS;
        }
        public static Int32 DbrToInt32(OmEpicsMonitorEventArg _arg, out Int32[] _val)
        {
            int i, j, k;
            byte[] tmp_byte_arr, byte_arr;

            int count = _arg.dbr.Length;

            _val = new Int32[count];

            for (i = 0; i < count; i++)
            {
                byte_arr = new byte[_arg.dbr[i].Length * 4];
                for (j = 0; j < _arg.dbr[i].Length; j++)
                {
                    tmp_byte_arr = BitConverter.GetBytes(_arg.dbr[i][j]);
                    for (k = 0; k < tmp_byte_arr.Length; k++)
                    {
                        byte_arr[k + j * 4] = tmp_byte_arr[k];
                    }
                }

                switch (_arg.PvType)
                {
                    case EPICS_STRING:
                        return ERROR;
                        break;

                    case EPICS_INT:
                        if (_arg.dbr[i].Length != 1)
                            return ERROR;
                        _val[i] = (Int32)BitConverter.ToInt32(byte_arr, 0);
                        break;

                    case EPICS_FLOAT:
                        if (_arg.dbr[i].Length != 1)
                            return ERROR;
                        _val[i] = (Int32)BitConverter.ToDouble(byte_arr, 0);
                        break;

                    case EPICS_ENUM:
                        if (_arg.dbr[i].Length != 1)
                            return ERROR;
                        _val[i] = (Int32)BitConverter.ToUInt16(byte_arr, 0);
                        break;

                    case EPICS_CHAR:
                        if (_arg.dbr[i].Length != 1)
                            return ERROR;
                        _val[i] = (Int32)byte_arr[0];
                        break;

                    case EPICS_LONG:
                        if (_arg.dbr[i].Length != 1)
                            return ERROR;
                        _val[i] = (Int32)BitConverter.ToUInt32(byte_arr, 0);
                        break;

                    case EPICS_DOUBLE:
                        if (_arg.dbr[i].Length != 2)
                            return ERROR;
                        _val[i] = (Int32)BitConverter.ToDouble(byte_arr, 0);
                        break;

                    default:
                        return ERROR;
                        break;
                }
            }

            return SUCCESS;
        }
        public static Int32 DbrToDouble(Int32[][] _dbr, out double[] _val)
        {
            int i, j, k;
            byte[] tmp_byte_arr, byte_arr;


            int count = _dbr.Length;
            int dbr_len = _dbr[0].Length;

            _val = new double[count];
            byte_arr = new byte[dbr_len * 4];

            for (i = 0; i < count; i++)
            {
                for (j = 0; j < dbr_len; j++)
                {
                    tmp_byte_arr = BitConverter.GetBytes(_dbr[i][j]);
                    for (k = 0; k < tmp_byte_arr.Length; k++)
                    {
                        byte_arr[k + j * 4] = tmp_byte_arr[k];
                    }
                }

                _val[i] = (double)BitConverter.ToDouble(byte_arr, 0);
            }

            return SUCCESS;
        }
        public static Int32 DbrToDouble(OmEpicsMonitorEventArg _arg, out double[] _val)
        {
            int i, j, k;
            byte[] tmp_byte_arr, byte_arr;

            int count = _arg.dbr.Length;

            _val = new double[count];

            for (i = 0; i < count; i++)
            {
                byte_arr = new byte[_arg.dbr[i].Length * 4];
                for (j = 0; j < _arg.dbr[i].Length; j++)
                {
                    tmp_byte_arr = BitConverter.GetBytes(_arg.dbr[i][j]);
                    for (k = 0; k < tmp_byte_arr.Length; k++)
                    {
                        byte_arr[k + j * 4] = tmp_byte_arr[k];
                    }
                }

                switch (_arg.PvType)
                {
                    case EPICS_STRING:
                        return ERROR;
                        break;

                    case EPICS_INT:
                        if (_arg.dbr[i].Length != 1)
                            return ERROR;
                        _val[i] = (double)BitConverter.ToInt32(byte_arr, 0);
                        break;

                    case EPICS_FLOAT:
                        if (_arg.dbr[i].Length != 1)
                            return ERROR;
                        _val[i] = (double)BitConverter.ToDouble(byte_arr, 0);
                        break;

                    case EPICS_ENUM:
                        if (_arg.dbr[i].Length != 1)
                            return ERROR;
                        _val[i] = (double)BitConverter.ToUInt16(byte_arr, 0);
                        break;

                    case EPICS_CHAR:
                        if (_arg.dbr[i].Length != 1)
                            return ERROR;
                        _val[i] = (double)byte_arr[0];
                        break;

                    case EPICS_LONG:
                        if (_arg.dbr[i].Length != 1)
                            return ERROR;
                        _val[i] = (double)BitConverter.ToUInt32(byte_arr, 0);
                        break;

                    case EPICS_DOUBLE:
                        if (_arg.dbr[i].Length != 2)
                            return ERROR;
                        _val[i] = (double)BitConverter.ToDouble(byte_arr, 0);
                        break;

                    default:
                        return ERROR;
                        break;
                }
            }

            return SUCCESS;
        }
        //-----------------------------------------------------------------
        #endregion



        #region Private Methods
        //-----------------------------------------------------------------
        private void InternalCbMonitorEvent(IntPtr _ptr_dbr, IntPtr _ptr_pv)
        {
            om_epics_pv pv;
            Int32[][] dbr;
            Int32 dbr_len;
            Int32 count;

            __PvPtr = _ptr_pv;
            pv = (om_epics_pv)Marshal.PtrToStructure((IntPtr)_ptr_pv, typeof(om_epics_pv));
            count = (Int32)pv.count;

            switch (pv.type_id)
            {
                case EPICS_STRING:              // char[40]
                    dbr_len = 10;
                    break;
                case EPICS_INT:                 // short		Int16
                    dbr_len = 1;
                    break;
                case EPICS_FLOAT:               // float		Float32
                    dbr_len = 1;
                    break;
                case EPICS_ENUM:                // unsigned short	UInt 16
                    dbr_len = 1;
                    break;
                case EPICS_CHAR:                // unsigned char	UInt8
                    dbr_len = 1;
                    break;
                case EPICS_LONG:                // int			Int32
                    dbr_len = 1;
                    break;
                case EPICS_DOUBLE:              // double		Float64
                    dbr_len = 2;
                    break;
                default:                        // DEFAULT
                    dbr_len = 1;
                    break;
            }

            dbr = new Int32[count][];
            for (int i = 0; i < count; i++)
            {
                dbr[i] = new Int32[dbr_len];
            }


            //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~			
            unsafe
            {
                var SourcePtr = (Int32*)_ptr_dbr;
                for (int i = 0; i < count; i++)
                {
                    for (int j = 0; j < dbr_len; j++)
                    {
                        dbr[i][j] = *SourcePtr;
                        SourcePtr++;
                    }
                }
            }
            //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~			

            EmitMonitorEvent(dbr, (int)pv.type_id);
        }
        //-----------------------------------------------------------------
        private void IncomingDebugMessage(string _msg)
        {
            EmitDebugMessage(_msg);
        }
        private void IncomingErrorMessage(string _msg)
        {
            EmitErrorMessage(_msg);
        }
        private void IncomingEventMessage(string _msg)
        {
            EmitEventMessage(_msg);
        }
        //-----------------------------------------------------------------
        #endregion



        #region Private Methods - Emit Events
        //-----------------------------------------------------------------
        private void EmitMonitorEvent(Int32[][] _dbr, Int32 _pv_type)
        {
            OmEpicsMonitorEventArg arg = new OmEpicsMonitorEventArg();
            arg.Name = this.Name;
            arg.PvName = this.PvName;
            arg.PvType = _pv_type;
            arg.dbr = _dbr;

            eMonitorEvent(arg);
        }
        //-----------------------------------------------------------------
        private void EmitDebugMessage(string _msg)
        {
            OmEpicsMessageEventArg arg = new OmEpicsMessageEventArg();
            arg.Name = this.Name;
            arg.PvName = this.PvName;
            arg.Message = _msg;

            eDebugMessageReceived(arg);
        }
        private void EmitErrorMessage(string _msg)
        {
            OmEpicsMessageEventArg arg = new OmEpicsMessageEventArg();
            arg.Name = this.Name;
            arg.PvName = this.PvName;
            arg.Message = _msg;

            eErrorMessageReceived(arg);
        }
        private void EmitEventMessage(string _msg)
        {
            OmEpicsMessageEventArg arg = new OmEpicsMessageEventArg();
            arg.Name = this.Name;
            arg.PvName = this.PvName;
            arg.Message = _msg;

            eEventMessageReceived(arg);
        }
        //-----------------------------------------------------------------
        #endregion



        #region Outbound Events
        //-----------------------------------------------------------------
        protected void eMonitorEvent(OmEpicsMonitorEventArg arg)
        {
            if (MonitorEvent != null) { MonitorEvent(this, arg); }
        }
        //-----------------------------------------------------------------
        protected void eDebugMessageReceived(OmEpicsMessageEventArg arg)
        {
            if (DebugMessageReceived != null) { DebugMessageReceived(this, arg); }
        }
        protected void eErrorMessageReceived(OmEpicsMessageEventArg arg)
        {
            if (ErrorMessageReceived != null) { ErrorMessageReceived(this, arg); }
        }
        protected void eEventMessageReceived(OmEpicsMessageEventArg arg)
        {
            if (EventMessageReceived != null) { EventMessageReceived(this, arg); }
        }
        //-----------------------------------------------------------------
        #endregion



        #region DLL Import - OM EPICS
        //----------------------------------------------------------------------------------------------------------------------------------------------------------------		
        [DllImport(__path_omepics_dll)]
        private static extern int ext_om_epics_camonitor(string _name, string _pv_name, OmEpicsMonitorCb _cb);
        
        
        
        [DllImport(__path_omepics_dll)]
        private static extern int ext_om_epics_clear_camonitor(IntPtr _pv_ptr);
        



        [DllImport(__path_omepics_dll)]
        private static extern int ext_debug_msg(cb_string CallbackString);
        

        
        [DllImport(__path_omepics_dll)]
        private static extern int ext_event_msg(cb_string CallbackString);
        
        
        
        [DllImport(__path_omepics_dll)]
        private static extern int ext_error_msg(cb_string CallbackString);
        //----------------------------------------------------------------------------------------------------------------------------------------------------------------
        #endregion


    }


    public class OmEpicsMessageEventArg
    {
        public string Name { get; set; }
        public string PvName { get; set; }
        public string Message { get; set; }
    }
    public class OmEpicsMonitorEventArg
    {
        public string Name { get; set; }
        public string PvName { get; set; }
        public Int32 PvType { get; set; }
        public Int32[][] dbr { get; set; }
    }


    public delegate void OmEpicsMessageEventHandler(object sender, OmEpicsMessageEventArg arg);
    public delegate void OmEpicsMonitorEventHandler(object sender, OmEpicsMonitorEventArg arg);
    public delegate void OmEpicsMonitorCb(IntPtr _ptr, IntPtr _ptr_pv);



    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 0)]     // !552
    unsafe public struct om_epics_pv
    {

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 255)]
        public string Name;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 255)]
        public string PvName;

        [MarshalAs(UnmanagedType.U8)]       //chid				// !8		
        public UInt64 channel_id;

        [MarshalAs(UnmanagedType.U8)]           //evid				// !8		
        public UInt64 event_id;

        [MarshalAs(UnmanagedType.U4)]       //chtype			// !4
        public UInt32 type_id;

        [MarshalAs(UnmanagedType.I4)]       // long				// !4
        public Int32 count;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate int event_cb(epics_event_handler_arg arg);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate int user_cb(IntPtr ptr, IntPtr pv_ptr);
    }




    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 0)]     // !40
    public struct epics_event_handler_arg
    {
        [MarshalAs(UnmanagedType.I8)]           //void		
        public Int64 ptr_usr;

        [MarshalAs(UnmanagedType.U8)]       // chanId = chid
        public UInt64 chid;

        [MarshalAs(UnmanagedType.I4)]       // long
        public Int32 type_id;

        [MarshalAs(UnmanagedType.I4)]       // long
        public Int32 count;

        [MarshalAs(UnmanagedType.I8)]           // const void		
        public Int64 ptr_dbr;

        [MarshalAs(UnmanagedType.I4)]       // int
        public int status;
    }


}
