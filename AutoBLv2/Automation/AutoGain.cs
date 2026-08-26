using Devices;
using FPGA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Automation
{
    public class AutoGain
    {
        #region Constants
        //-----------------------------------------------------------
        public const Int32 SUCCESS = 1;
        public const Int32 ERROR = -1;
        public const Int32 ABORT = -2;
        //-----------------------------------------------------------
        private const Int32 SRS_SLEEP = 100;
        //-----------------------------------------------------------
        #endregion



        #region Properties
        //-----------------------------------------------------------
        public bool Abort { get; set; }
        public Int32 MaxSensitivityId { get; set; }
        public Int32 MinSensitivityId { get; set; }
        //-----------------------------------------------------------
        #endregion




        //===========================================================
        public AutoGain()
        {
            this.Abort = false;
            this.MaxSensitivityId = 9;  // 1e9 V/A
            this.MinSensitivityId = 21; // 1e5 V/A
        }
        //===========================================================






        public Int32 FindMaxGain(ref SRS570[] _amplifiers, out Int32[] _sensId, ref string _error)
        {
            Int32 rt;           

            double[] aiAverage;
            double[] aiStdDev;
            
            bool[] findMaxGainDone;
            bool allFindMaxGainDone = false;

            this.Abort = false; // reset abort


            _sensId = new Int32[_amplifiers.Length];
            findMaxGainDone = new bool[_amplifiers.Length];
            for (Int32 amplifierIdx = 0; amplifierIdx < _amplifiers.Length; amplifierIdx++)
            {
                // reset Offset
                rt = _amplifiers[amplifierIdx].SetOffset(0, 1, ref _error);
                if (rt != SRS570.SUCCESS)
                {
                    _error = this.GetType().Name + " " + System.Reflection.MethodBase.GetCurrentMethod().Name + " " + _error;
                    return ERROR;
                }

                findMaxGainDone[amplifierIdx] = false;
                _sensId[amplifierIdx] = this.MinSensitivityId;
            }


            // runs until all amplfiers have been optimized
            while (true)
            {
                // set gain
                for (Int32 amplifierIdx = 0; amplifierIdx < _amplifiers.Length; amplifierIdx++)
                {
                    if (findMaxGainDone[amplifierIdx])
                        continue;
                    
                    rt = _amplifiers[amplifierIdx].SetGain(_sensId[amplifierIdx], ref _error);
                    if (rt != SRS570.SUCCESS)
                    {
                        _error = this.GetType().Name + " " + System.Reflection.MethodBase.GetCurrentMethod().Name + " " + _error;
                        return ERROR;
                    }                    
                }


                Thread.Sleep(SRS_SLEEP); // wait after swiching the gain


                // collect data
                // expects all amplifiers to be connnected to the same DAQ !!!!!!!!
                rt = _amplifiers[0].Daq.CollectDataCalibrated(out aiAverage, out aiStdDev);
                if (rt != FpgaDaq.SUCCESS)
                {
                    _error = this.GetType().Name + " " + System.Reflection.MethodBase.GetCurrentMethod().Name;
                    _error += " CollectDataCalibrated";
                    return ERROR;
                }


                // adjust gains
                for (Int32 amplifierIdx = 0; amplifierIdx < _amplifiers.Length; amplifierIdx++)
                {
                    if (findMaxGainDone[amplifierIdx])
                        continue;
                    
                    if (aiAverage[_amplifiers[amplifierIdx].AiChannel] > _amplifiers[amplifierIdx].SignalHighLimit)
                    {
                        // if gain was too high, reduce gain and mark this amplifier as optimized
                        _sensId[amplifierIdx]++; // decrease gain
                        findMaxGainDone[amplifierIdx] = true;
                    }
                    else if (_sensId[amplifierIdx] == this.MaxSensitivityId)
                    {
                        // if gain is already set to max gain, mark it as optimized
                        findMaxGainDone[amplifierIdx] = true;
                    }
                    else
                    {
                        // if gain is too low increase gain but not above max gain
                        if (_sensId[amplifierIdx] > this.MaxSensitivityId) // make sure not too exceed max gain
                            _sensId[amplifierIdx]--; // increase gain
                    }
                    
                }


                // check if all amplifiers have been optimized
                allFindMaxGainDone = true;
                for (Int32 amplifierIdx = 0; amplifierIdx < _amplifiers.Length; amplifierIdx++)
                    allFindMaxGainDone &= findMaxGainDone[amplifierIdx];
                if (allFindMaxGainDone)
                    break;


                if (this.Abort)
                    break;
            }



            // if exited due to abort
            if (this.Abort)
            {
                Console.WriteLine("FindMaxGain: ABORT");
                this.Abort = false; // reset ABORT
                return ABORT;
            }


            // finally
            for (Int32 amplifierIdx = 0; amplifierIdx < _amplifiers.Length; amplifierIdx++)
            {
                // set found gain
                rt = _amplifiers[amplifierIdx].SetGain(_sensId[amplifierIdx], ref _error);
                if (rt != SRS570.SUCCESS)
                {
                    _error = this.GetType().Name + " " + System.Reflection.MethodBase.GetCurrentMethod().Name + " " + _error;
                    return ERROR;
                }
            }


            Console.WriteLine("FindMaxGain: SUCCESS");

            return SUCCESS;
        }



    }
}
