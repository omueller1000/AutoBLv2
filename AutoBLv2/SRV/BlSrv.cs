using Automation;
using Devices;
using FPGA;
using static MongoDB.Driver.WriteConcern;
using static System.Runtime.InteropServices.JavaScript.JSType;
//using System.Collections.Generic;

namespace AutoBLv2.SRV
{
    public class BlSrv
    {

        #region Constants
        //-----------------------------------------------------------
        private const string PATH_XAS_OFFSETS = "C:\\SSRL_LOCAL_OM\\definitions\\xasOffsets.def";
        private string BL_DEF_PATH = "C:\\SSRL_LOCAL_OM\\definitions\\bl.def";
        //-----------------------------------------------------------
        public const Int32 ERROR = -1;
        public const Int32 SUCCESS = 0;
        //-----------------------------------------------------------
        private const Int32 OFFSET_COMMAND = 1;
        //-----------------------------------------------------------
        private Int32 REQUEST_LENGTH_BEAM_SHUTTER = 3;
        private Int32 OFFSET_BEAM_SHUTER_EXECUTE = 2;
        //-----------------------------------------------------------
        private Int32 REQUEST_LENGTH_COLLECT_OFFSETS = 4;
        private Int32 OFFSET_COLLECT_OFFSETS_EXECUTE = 2;
        private Int32 OFFSET_COLLECT_OFFSETS_NUM_SAMPLES = 3;
        //-----------------------------------------------------------        
        private Int32 REQUEST_LENGTH_AUTO_GAIN_MIN = 6;
        private Int32 REQUEST_LENGTH_AUTO_GAIN_MAX = 14;
        private Int32 OFFSET_AUTO_GAIN_EXECUTE = 2;
        private Int32 OFFSET_AUTO_GAIN_COLLECT_OFFSETS = 3;
        private Int32 OFFSET_AUTO_GAIN_RETURN = 4;
        //-----------------------------------------------------------
        private Int32 MAX_AMPLIFIER_INDEX = 4;  // beamline specific
        private Int32 MAX_AMPLIFIER_COUNT = 6;
        private Int32 MAX_ENERGY_COUNT = 6;
        private Int32 MIN_MONO_ENERGY = 4500;
        private Int32 MAX_MONO_ENERGY = 40000;
        //-----------------------------------------------------------
        #endregion


        
        public static AutoGain __AutoGain;
        public FpgaMonochromator __FpgaMono;
        public EpicsSlit __Slit;
        public SampleShutter __sampleShutter;





        #region Variables
        //-----------------------------------------------------------
        private static ServerLock __serverLock;
        private FpgaDaq __fpgaDaq;
        
        //-----------------------------------------------------------
        private static readonly object __taskLock = new object();
        private static string __currentTask = "";
        private static string __staticError = "";
        private Thread __WorkerThread;
        private Thread __MonitorThread;
        //-----------------------------------------------------------
        private SRS570 __srsI0;
        private SRS570 __srsI1;
        private SRS570 __srsI2;
        private SRS570 __srsI3;
        private SRS570[] __srsArr;
        //-----------------------------------------------------------
        #endregion



        //===========================================================
        public BlSrv(ref FpgaDaq _fpgaDaq, ref ServerLock _lock) 
        {


            __fpgaDaq = _fpgaDaq;
            __serverLock = _lock;
            __AutoGain = new AutoGain();
            __FpgaMono = new FpgaMonochromator();
            __Slit = new EpicsSlit();


         
            

            // beamline specific configuration
            //__srsI0 = new SRS570("BL22:SRS570_AMP1", ref __fpgaDaq, 0);
            //__srsI1 = new SRS570("BL22:SRS570_AMP2", ref __fpgaDaq, 1);
            //__srsI2 = new SRS570("BL22:SRS570_AMP3", ref __fpgaDaq, 2);
            //__srsI3 = new SRS570("BL22:SRS570_AMP4", ref __fpgaDaq, 3);
            //__srsArr = new SRS570[] { __srsI0, __srsI1, __srsI2, __srsI3 };            

        }
        //===========================================================





        public Int32 Init()
        {
            Int32 rt;
            string error = "";
            string sampleShutterName = "NULL";
            string sampleShutterDo = "NULL";

            SRS570 srs;
            string numAmpsStr;
            Int32 numAmps;
            string ampEpicsName;
            string ampLabel;
            string ampAiStr;
            Int32 ampAi;



            #region Sample Shutter
            //-----------------------------------------------------------
            rt = Def.Def.ReadDefinition(BL_DEF_PATH, "SAMPLE_SHUTTER_EPICS_NAME", out sampleShutterName, ref error);
            if (rt != Def.Def.SUCCESS)
            {
                error = System.Reflection.MethodBase.GetCurrentMethod().Name + " " + error;
                return ERROR;
            }
            rt = Def.Def.ReadDefinition(BL_DEF_PATH, "SAMPLE_SHUTTER_DO", out sampleShutterDo, ref error);
            if (rt != Def.Def.SUCCESS)
            {
                error = System.Reflection.MethodBase.GetCurrentMethod().Name + " " + error;
                return ERROR;
            }
            if (sampleShutterName != "NULL" || sampleShutterDo != "NULL")
            {
                __sampleShutter = new SampleShutter(sampleShutterName, UInt32.Parse(sampleShutterDo));
            }
            //-----------------------------------------------------------
            #endregion



            #region Amplifiers
            //-----------------------------------------------------------
            rt = Def.Def.ReadDefinition(BL_DEF_PATH, "AMP_NUM", out numAmpsStr, ref error);
            numAmps = Int32.Parse(numAmpsStr);

            __srsArr = new SRS570[numAmps];
            
            for (Int32 i = 0; i < numAmps; i++)
            {
                rt = Def.Def.ReadDefinition(BL_DEF_PATH, $"AMP_{i + 1}_EPICS_NAME", out ampEpicsName, ref error);
                if (rt != Def.Def.SUCCESS)
                {
                    error = System.Reflection.MethodBase.GetCurrentMethod().Name + " " + error;
                    return ERROR;
                }
                rt = Def.Def.ReadDefinition(BL_DEF_PATH, $"AMP_{i + 1}_LABEL", out ampLabel, ref error);
                if (rt != Def.Def.SUCCESS)
                {
                    error = System.Reflection.MethodBase.GetCurrentMethod().Name + " " + error;
                    return ERROR;
                }
                rt = Def.Def.ReadDefinition(BL_DEF_PATH, $"AMP_{i + 1}_AI", out ampAiStr, ref error);
                if (rt != Def.Def.SUCCESS)
                {
                    error = System.Reflection.MethodBase.GetCurrentMethod().Name + " " + error;
                    return ERROR;
                }


                rt = Def.Def.ReadDefinition(BL_DEF_PATH, $"AMP_{i + 1}_LABEL", out ampLabel, ref error);
                if (rt != Def.Def.SUCCESS)
                {
                    error = System.Reflection.MethodBase.GetCurrentMethod().Name + " " + error;
                    return ERROR;
                }              
                ampAi = Int32.Parse(ampAiStr);

                if (ampAi < 0 || ampAi >= FpgaDataFrame.numMaxAi)
                {
                    error = System.Reflection.MethodBase.GetCurrentMethod().Name;
                    error += " ARGUMENT_OUT_OF_RANGE";
                    return ERROR;
                }
                



                srs = new SRS570(ampLabel, ampEpicsName, ref __fpgaDaq, ampAi);
                __srsArr[i] = srs;
            }
            //-----------------------------------------------------------
            #endregion


            return SUCCESS;
        }

        





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
        private Int32 CmdSetTableToBeamOffset(ref string _error)
        {
            Int32 rt;

            rt = __FpgaMono.SetTableToBeamOffset(ref _error);
            if (rt != FpgaMonochromator.SUCCESS)
            {
                _error = this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + " " + _error;
                return ERROR;
            }

            return SUCCESS;
        }    
        private Int32 CmdAutoGain(ref string[] _reqArr, ref string _res, ref string _error)
        {
            Int32 rt;
            bool validateOnly = true;
            bool collectOffsets = false;
            bool returnToStart = false;            
            List<double> energyList;
            List<Int32> ampliferIndexList;
            




            if (_reqArr.Length < REQUEST_LENGTH_AUTO_GAIN_MIN || _reqArr.Length > REQUEST_LENGTH_AUTO_GAIN_MAX)
            {
                _error = this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name;
                _error += " INVALID_NUMBER_OF_ARGUMENTS";
                return ERROR;
            }


            
            ampliferIndexList = new List<Int32>();
            energyList = new List<double>();



            // parse aguments
            try
            {
                validateOnly = Int32.Parse(_reqArr[OFFSET_AUTO_GAIN_EXECUTE]) == 0 ? true : false;
                collectOffsets = Int32.Parse(_reqArr[OFFSET_AUTO_GAIN_COLLECT_OFFSETS]) == 0 ? false : true;
                returnToStart = Int32.Parse(_reqArr[OFFSET_AUTO_GAIN_RETURN]) == 0 ? false : true;

                for (Int32 i = OFFSET_AUTO_GAIN_RETURN + 1; i < _reqArr.Length; i++)
                {
                    if (_reqArr[i][0] == 'A' || _reqArr[i][0] == 'a')
                    {
                        ampliferIndexList.Add(Int32.Parse(_reqArr[i].Substring(1)));
                    }
                    else if (_reqArr[i][0] == 'E' || _reqArr[i][0] == 'e')
                    {
                        energyList.Add(double.Parse(_reqArr[i].Substring(1)));
                    }
                    else
                    {
                        _error = this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name;
                        _error += " INVALID_ARGUMENT";
                        return ERROR;
                    }
                }
            }
            catch (Exception ex)
            {
                _error = this.GetType().Name + " " + System.Reflection.MethodBase.GetCurrentMethod().Name + " " + ex.Message;
                return ERROR;
            }


            
            // verify arguments
            try
            {
                if (ampliferIndexList.Count > MAX_AMPLIFIER_COUNT)
                {
                    _error = this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name;
                    _error += " TOO_MANY_AMPLIFIERS";
                    return ERROR;
                }                

                for (Int32 i = 0; i < ampliferIndexList.Count; i++)
                {
                    if (ampliferIndexList[i] < 0 || ampliferIndexList[i] > MAX_AMPLIFIER_INDEX)
                    {
                        _error = this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name;
                        _error += " ARGUMENT_OUT_OF_RANGE";
                        return ERROR;
                    }

                    // search duplicates
                    for (Int32 j = 0; j < i; j++)
                    {
                        if (ampliferIndexList[i] == ampliferIndexList[j])
                        {
                            _error = this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name;
                            _error += " DUPLICATE_ARGUMENT";
                            return ERROR;
                        }
                    }
                }


                if (energyList.Count > MAX_ENERGY_COUNT)
                {
                    _error = this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name;
                    _error += " TOO_MANY_ENERGIES";
                    return ERROR;
                }

                for (Int32 i = 0; i < energyList.Count; i++)
                {
                    if (energyList[i] < MIN_MONO_ENERGY || energyList[i] > MAX_MONO_ENERGY)
                    {
                        _error = this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name;
                        _error += " ARGUMENT_OUT_OF_RANGE";
                        return ERROR;
                    }

                    // search duplicates
                    for (Int32 j = 0; j < i; j++)
                    {
                        if (energyList[i] == energyList[j])
                        {
                            _error = this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name;
                            _error += " DUPLICATE_ARGUMENT";
                            return ERROR;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _error = this.GetType().Name + " " + System.Reflection.MethodBase.GetCurrentMethod().Name + " " + ex.Message;
                return ERROR;
            }


            if (validateOnly)
                return SUCCESS;




            if (__WorkerThread != null && __WorkerThread.IsAlive)
            {
                _error = this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name;
                _error += " THREAD_ALIVE";
                return ERROR;
            }



            //~~~~~~~~~~~~~~~~~~~~~~~~~~~~
            lock (__taskLock)
            {
                __currentTask = $"AUTO_GAIN";
            }
            //~~~~~~~~~~~~~~~~~~~~~~~~~~~~

            XasOffsets xasOffsets;


            __WorkerThread = new Thread(() =>
            {
                AutoGainBlocking(ampliferIndexList, energyList, returnToStart, ref __staticError, __sampleShutter);
                
                if(collectOffsets)
                    CollectOffsetBlocking(__Slit, 1000, out xasOffsets, ref __staticError);
            });
            __WorkerThread.IsBackground = true;
            __WorkerThread.Start();

            __MonitorThread = new Thread(() => MonitorThread(ref __WorkerThread, ""));
            __MonitorThread.IsBackground = true;
            __MonitorThread.Start();







            return SUCCESS;
        }
        private void CmdAutoGainAbort()
        {
            __AutoGain.Abort = true;
        }
        private Int32 CmdGetGains(ref string _res, ref string _error)
        {
            Int32 rt;
            double[] gain;

            gain = new double[__srsArr.Length];

            for (Int32 i = 0; i < __srsArr.Length; i++)
            {
                rt = __srsArr[i].GetGain(out gain[i], ref _error);
                if (rt != SRS570.SUCCESS)                                
                {                    
                    _error = this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + " " +_error;                        
                    return ERROR;                    
                }                
            }

            _res = "\r";
            for (Int32 i = 0; i < __srsArr.Length; i++)
            {
                _res += gain[i].ToString() + " ";
            }
            _res += "\r";

            return SUCCESS;        
        }
        private Int32 CmdCloseBeamShutter(ref string[] _reqArr, ref string _res, ref string _error)
        {
            Int32 rt;
            bool validateOnly = true;

            if (_reqArr.Length != REQUEST_LENGTH_BEAM_SHUTTER)
            {
                _error = this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name;
                _error += " INVALID_NUMBER_OF_ARGUMENTS";
                return ERROR;
            }

            // parse aguments
            try
            {
                validateOnly = Int32.Parse(_reqArr[OFFSET_BEAM_SHUTER_EXECUTE]) == 0 ? true : false;
            }
            catch (Exception ex)
            {
                _error = this.GetType().Name + " " + System.Reflection.MethodBase.GetCurrentMethod().Name + " " + ex.Message;
                return ERROR;
            }


            if (validateOnly)
                return SUCCESS;





            if (__WorkerThread != null && __WorkerThread.IsAlive)
            {
                _error = this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name;
                _error += " THREAD_ALIVE";
                return ERROR;
            }

            //~~~~~~~~~~~~~~~~~~~~~~~~~~~~
            lock (__taskLock)
            {
                __currentTask = $"CLOSE_BEAM_SHUTTER";
            }
            //~~~~~~~~~~~~~~~~~~~~~~~~~~~~





            __WorkerThread = new Thread(() => __Slit.Close(ref __staticError));            
            __WorkerThread.IsBackground = true;
            __WorkerThread.Start();

            __MonitorThread = new Thread(() => MonitorThread(ref __WorkerThread, ""));
            __MonitorThread.IsBackground = true;
            __MonitorThread.Start();


            return SUCCESS;
        }
        private Int32 CmdOpenBeamShutter(ref string[] _reqArr, ref string _res, ref string _error)
        {            
            bool validateOnly = true;

            if (_reqArr.Length != REQUEST_LENGTH_BEAM_SHUTTER)
            {
                _error = this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name;
                _error += " INVALID_NUMBER_OF_ARGUMENTS";
                return ERROR;
            }

            // parse aguments
            try
            {
                validateOnly = Int32.Parse(_reqArr[OFFSET_BEAM_SHUTER_EXECUTE]) == 0 ? true : false;
            }
            catch (Exception ex)
            {
                _error = this.GetType().Name + " " + System.Reflection.MethodBase.GetCurrentMethod().Name + " " + ex.Message;
                return ERROR;
            }

            if (validateOnly)
                return SUCCESS;



            if (__WorkerThread != null && __WorkerThread.IsAlive)
            {
                _error = this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name;
                _error += " THREAD_ALIVE";
                return ERROR;
            }

            //~~~~~~~~~~~~~~~~~~~~~~~~~~~~
            lock (__taskLock)
            {
                __currentTask = $"OPEN_BEAM_SHUTTER";
            }
            //~~~~~~~~~~~~~~~~~~~~~~~~~~~~


            __WorkerThread = new Thread(() => __Slit.Open(ref __staticError));            
            __WorkerThread.IsBackground = true;
            __WorkerThread.Start();

            __MonitorThread = new Thread(() => MonitorThread(ref __WorkerThread, ""));
            __MonitorThread.IsBackground = true;
            __MonitorThread.Start();

            return SUCCESS;
        }
        //-----------------------------------------------------------
        private Int32 CmdCollectOffsets(ref string[] _reqArr, ref string _res, ref string _error)
        {
            Int32 rt;
            XasOffsets xasOffsets;
            bool validateOnly = true;
            Int32 nSamples;

            if (_reqArr.Length != REQUEST_LENGTH_COLLECT_OFFSETS)
            {
                _error = this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name;
                _error += " INVALID_NUMBER_OF_ARGUMENTS";
                return ERROR;
            }

            // parse aguments
            try
            {
                validateOnly = Int32.Parse(_reqArr[OFFSET_COLLECT_OFFSETS_EXECUTE]) == 0 ? true : false;
                nSamples = Int32.Parse(_reqArr[OFFSET_COLLECT_OFFSETS_NUM_SAMPLES]);
            }
            catch (Exception ex)
            {
                _error = this.GetType().Name + " " + System.Reflection.MethodBase.GetCurrentMethod().Name + " " + ex.Message;
                return ERROR;
            }

            // validate
            try
            {
                if (nSamples < 1 || nSamples > 1000000)
                {
                    _error = this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name;
                    _error += " ARGUMENT_OUT_OF_RANGE";
                    return ERROR;
                }
            }
            catch (Exception ex)
            {
                _error = this.GetType().Name + " " + System.Reflection.MethodBase.GetCurrentMethod().Name + " " + ex.Message;
                return ERROR;
            }

            if (validateOnly)
                return SUCCESS;



            if (__WorkerThread != null && __WorkerThread.IsAlive)
            {
                _error = this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name;
                _error += " THREAD_ALIVE";
                return ERROR;
            }

            //~~~~~~~~~~~~~~~~~~~~~~~~~~~~
            lock (__taskLock)
            {
                __currentTask = $"COLLECT_OFFSETS";
            }
            //~~~~~~~~~~~~~~~~~~~~~~~~~~~~


            __WorkerThread = new Thread(() => CollectOffsetBlocking(__Slit, nSamples, out xasOffsets, ref __staticError));
            __WorkerThread.IsBackground = true;
            __WorkerThread.Start();

            __MonitorThread = new Thread(() => MonitorThread(ref __WorkerThread, ""));
            __MonitorThread.IsBackground = true;
            __MonitorThread.Start();



            return SUCCESS;
        }
        private Int32 CmdGetOffsets(ref string _res, ref string _error)
        {
            Int32 rt;
            XasOffsets xasOffsets;


            xasOffsets = new XasOffsets(8, 0);
            rt = xasOffsets.LoadDefinition(PATH_XAS_OFFSETS, ref _error);
            if (rt != XasOffsets.SUCCESS)
            {
                _error = this.GetType().Name + " " + System.Reflection.MethodBase.GetCurrentMethod().Name + " " + _error;
                return ERROR;
            }


            _res = "\r";
            for (Int32 i = 0; i < xasOffsets.numAi; i++)
            {
                _res += xasOffsets.aiOffset[i].ToString() + " ";
            }
            _res += "\r";


            return SUCCESS;
        }
        //-----------------------------------------------------------
        #endregion






        private Int32 AutoGainBlocking(List<Int32> _amplifiers, List<double> _energies, bool _returnToStart, ref string _error, SampleShutter? _sampleShutter = null)
        {
            Int32 rt;
            
            SRS570[] selectedAmplifiers;
            Int32[] sensId;
            List<Int32[]> sensIdList;
            double achievedEnergy;
            double initialEnergy = 0;

            _amplifiers.Sort();
            _energies.Sort();


            selectedAmplifiers = new SRS570[_amplifiers.Count];
            for (Int32 i = 0; i < _amplifiers.Count; i++)
            {
                selectedAmplifiers[i] = __srsArr[_amplifiers[i]];
            }


            rt = __FpgaMono.GetEnergy(ref initialEnergy, ref _error);
            if (rt != FpgaMonochromator.SUCCESS)
            {
                _error = this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + _error;
                return ERROR;
            }


            if (_energies.Count == 0)
            {

                //##################
                if (_sampleShutter != null) _sampleShutter.Open(ref _error);
                //##################

                rt = __AutoGain.FindMaxGain(ref selectedAmplifiers, out sensId, ref _error);
                if (rt != AutoGain.SUCCESS)
                {
                    _error = this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + _error;
                    return ERROR;
                }

                //##################                
                if (_sampleShutter != null) _sampleShutter.Close(ref _error);
                //##################
            }
            else
            {
                sensIdList = new List<int[]>();
                for (Int32 i = 0; i < _energies.Count; i++)
                {
                    rt = __FpgaMono.MoveToEnergyBlocking(_energies[i], out achievedEnergy, 0, 0, ref _error);
                    if (rt != FpgaMonochromator.SUCCESS)
                    {
                        _error = this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + _error;
                        return ERROR;
                    }


                    //##################                    
                    if (_sampleShutter != null) _sampleShutter.Open(ref _error);
                    //##################

                    rt = __AutoGain.FindMaxGain(ref selectedAmplifiers, out sensId, ref _error);
                    if (rt != AutoGain.SUCCESS)
                    {
                        _error = this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + _error;
                        return ERROR;
                    }

                    //##################                    
                    if (_sampleShutter != null) _sampleShutter.Close(ref _error);
                    //##################

                    sensIdList.Add(sensId);
                }

                // find the min gain across all energies            
                sensId = new Int32[_amplifiers.Count];
                for (Int32 amplifierChannel = 0; amplifierChannel < _amplifiers.Count; amplifierChannel++)
                {
                    sensId[amplifierChannel] = sensIdList[0][amplifierChannel];
                }
                for (Int32 i = 1; i < sensIdList.Count; i++)
                {
                    for (Int32 amplifierChannel = 0; amplifierChannel < _amplifiers.Count; amplifierChannel++)
                    {

                        if (sensIdList[i][amplifierChannel] > sensId[amplifierChannel])
                            sensId[amplifierChannel] = sensIdList[i][amplifierChannel];
                    }
                }


                // set amplifier gain
                for (Int32 amplifierChannel = 0; amplifierChannel < _amplifiers.Count; amplifierChannel++)
                {
                    rt = selectedAmplifiers[amplifierChannel].SetGain(sensId[amplifierChannel], ref _error);
                    if (rt != SRS570.SUCCESS)
                    {
                        _error = this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + _error;
                        return ERROR;
                    }
                }
            }

            if (_returnToStart)
            {
                //~~~~~~~~~~~~~~~~~~~~~~~~~~~~
                lock (__taskLock)
                {
                    __currentTask = $"AUTO_GAIN_RETURN_TO_START";
                }
                //~~~~~~~~~~~~~~~~~~~~~~~~~~~~

                rt = __FpgaMono.MoveToEnergyBlocking(initialEnergy, out achievedEnergy, 0, 0, ref _error);
                if (rt != FpgaMonochromator.SUCCESS)
                {
                    _error = this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + _error;
                    return ERROR;
                }
            }

            return SUCCESS;
        }


        private Int32 CollectOffsetBlocking(Shutter _shutter, Int32 nSamples, out XasOffsets _xasOffsets, ref string _error)
        {
            Int32 rt;
            Int32 nSamplesTmp;
            double[] aiAverage;
            double[] aiStdDev;


            //~~~~~~~~~~~~~~~~~~~~~~~~~~~~
            lock (__taskLock)
            {
                __currentTask = $"COLLECT_OFFSETS";
            }
            //~~~~~~~~~~~~~~~~~~~~~~~~~~~~


            _xasOffsets = new XasOffsets(8, 0);
            rt = _xasOffsets.CaGetAOFF(__fpgaDaq.Name, ref _error);
            if (rt != XasOffsets.SUCCESS)
            {
                _error = this.GetType().Name + " " + System.Reflection.MethodBase.GetCurrentMethod().Name + " " + _error;
                return ERROR;
            }
            rt = _xasOffsets.CaGetASLO(__fpgaDaq.Name, ref _error);
            if (rt != XasOffsets.SUCCESS)
            {
                _error = this.GetType().Name + " " + System.Reflection.MethodBase.GetCurrentMethod().Name + " " + _error;
                return ERROR;
            }




            rt = _shutter.Close(ref _error);
            if (rt != Shutter.SUCCESS)
            {
                _error = this.GetType().Name + " " + System.Reflection.MethodBase.GetCurrentMethod().Name + " " + _error;
                return ERROR;
            }


            nSamplesTmp = __fpgaDaq.nSamples;
            __fpgaDaq.nSamples = nSamples;
            rt = __fpgaDaq.CollectDataCalibrated(out aiAverage, out aiStdDev);
            if (rt != FpgaDaq.SUCCESS)
            {
                _error = this.GetType().Name + " " + System.Reflection.MethodBase.GetCurrentMethod().Name;                
                _error += " CollectData";
                return ERROR;
            }
            __fpgaDaq.nSamples = nSamplesTmp;


            rt = _shutter.Open(ref _error);
            if (rt != Shutter.SUCCESS)
            {
                _error = this.GetType().Name + " " + System.Reflection.MethodBase.GetCurrentMethod().Name + " " + _error;
                return ERROR;
            }


            


            for (Int32 i = 0; i < aiAverage.Length; i++)
            {
                _xasOffsets.aiOffset[i] = -1.0 * aiAverage[i];
            }
            
            rt = _xasOffsets.WriteDefinition(PATH_XAS_OFFSETS, ref _error);
            if (rt != XasOffsets.SUCCESS)
            {
                _error = this.GetType().Name + " " + System.Reflection.MethodBase.GetCurrentMethod().Name + " " + _error;
                return ERROR;
            }





            return SUCCESS;
        }






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


                case "SET_TABLE_TO_BEAM_OFFSET":
                    rt = CmdSetTableToBeamOffset(ref error);
                    if (rt < 0)
                    {
                        err = true;
                        _res = System.Reflection.MethodBase.GetCurrentMethod().Name + " " + error;
                        break;
                    }
                    break;


                case "AUTO_GAIN":
                    if (__serverLock.IsLocked)
                    {
                        err = true;
                        _res = "SERVER_IS_LOCKED";
                        return ERROR;
                    }
                    rt = CmdAutoGain(ref _reqArr, ref _res, ref error);
                    if (rt < 0)
                    {
                        err = true;
                        _res = System.Reflection.MethodBase.GetCurrentMethod().Name + " " + error;
                        break;
                    }
                    break;


                case "AUTO_GAIN_ABORT":
                    CmdAutoGainAbort();
                    break;


                case "GET_GAINS":
                    rt = CmdGetGains(ref _res, ref error);
                    if (rt < 0)
                    {
                        err = true;
                        _res = System.Reflection.MethodBase.GetCurrentMethod().Name + " " + error;
                        break;
                    }
                    break;


                case "CLOSE_BEAM_SHUTTER":
                    rt = CmdCloseBeamShutter(ref _reqArr, ref _res, ref error);
                    if (rt < 0)
                    {
                        err = true;
                        _res = System.Reflection.MethodBase.GetCurrentMethod().Name + " " + error;
                        break;
                    }
                    break;


                case "OPEN_BEAM_SHUTTER":
                    rt = CmdOpenBeamShutter(ref _reqArr, ref _res, ref error);
                    if (rt < 0)
                    {
                        err = true;
                        _res = System.Reflection.MethodBase.GetCurrentMethod().Name + " " + error;
                        break;
                    }
                    break;


                case "COLLECT_OFFSETS":
                    if (__serverLock.IsLocked)
                    {
                        err = true;
                        _res = "SERVER_IS_LOCKED";
                        return ERROR;
                    }
                    rt = CmdCollectOffsets(ref _reqArr, ref _res, ref error);
                    if (rt < 0)
                    {
                        err = true;
                        _res = System.Reflection.MethodBase.GetCurrentMethod().Name + " " + error;
                        break;
                    }
                    break;


                case "GET_OFFSETS":
                    rt = CmdGetOffsets(ref _res, ref error);
                    if (rt < 0)
                    {
                        err = true;
                        _res = System.Reflection.MethodBase.GetCurrentMethod().Name + " " + error;
                        break;
                    }
                    break;


                default:
                    err = true;
                    _res = "UNKNOWN_COMMAND";
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
