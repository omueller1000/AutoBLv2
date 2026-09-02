using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoBLv2
{
    public class XasOffsets
    {
        #region Constants
        //-----------------------------------------------------------------
        public const Int32 SUCCESS = 1;
        public const Int32 ERROR = -1;
        //-----------------------------------------------------------------
        #endregion


        #region Variables
        //-----------------------------------------------------------------
        private Int32 __numAi;
        private Int32 __numCnt;
        //-----------------------------------------------------------------
        #endregion


        #region Properties
        //-----------------------------------------------------------------
        public Int32 numAi
        {
            get { return __numAi; }
            set
            {
                __numAi = value;
                InitAi();
            }
        }
        public Int32 numCounter
        {
            get { return __numCnt; }
            set
            {
                __numCnt = value;
                InitCnt();
            }
        }
        //-----------------------------------------------------------------
        public double[] aslo { get; set; }
        public double[] aoff { get; set; }
        public double[] aiOffset { get; set; }
        public double[] cntOffset { get; set; }
        //-----------------------------------------------------------------
        #endregion


        //=================================================================
        public XasOffsets(Int32 _numAi, Int32 _numCnt)
        {
            this.numAi = _numAi;
            this.numCounter = _numCnt;
        }
        //=================================================================




        #region  Private Methods        
        //-----------------------------------------------------------------
        private void InitAi()
        {
            aslo = new double[__numAi];
            aoff = new double[__numAi];
            aiOffset = new double[__numAi];

            for (Int32 i = 0; i < __numAi; i++)
            {
                aslo[i] = 1.0;
            }
        }
        private void InitCnt()
        {
            cntOffset = new double[__numCnt];
        }
        //-----------------------------------------------------------------
        #endregion





        #region Public Methods
        //-----------------------------------------------------------------
        public Int32 LoadDefinition(string _path, ref string _error)
        {
            Int32 rt;
            FileStream fs;
            StreamReader reader;
            string line;
            string[] parts;
            Int32 err = 0;

            //Int32 numAi = 0;
            //Int32 numCnt = 0;
            bool numAiRead = false;
            bool numCntRead = false;


            if (!File.Exists(_path))
            {
                _error = this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name;
                _error += " FILE_DOES_NOT_EXIST";
                return ERROR;
            }



            try
            {
                fs = new FileStream(_path, FileMode.Open);
                reader = new StreamReader(fs);


                while (reader.Peek() >= 0)
                {
                    line = reader.ReadLine();
                    if (line[0] == '#')
                        continue;

                    parts = line.Split(" ", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    if (parts.Length != 2)
                    {
                        _error = this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + " ";
                        _error += "FILE_INCONSISTENT";
                        err++;
                        break;
                    }


                    if (!numAiRead && parts[0] == "NUM_AI:")
                    {
                        this.numAi = Int32.Parse(parts[1]);
                        numAiRead = true;
                        continue;
                    }
                    if (!numCntRead && parts[0] == "NUM_CNT:")
                    {
                        this.numCounter = Int32.Parse(parts[1]);
                        numCntRead = true;
                        continue;
                    }


                    // make sure numAi and numCnt are read first
                    if (numAiRead && numCntRead != true)
                    {
                        _error = this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + " ";
                        _error += "FILE_INCONSISTENT";
                        err++;
                        break;
                    }


                    if (parts[0].Contains("ASLO_"))
                    {
                        for (Int32 i = 0; i < this.numAi; i++)
                        {
                            if (parts[0] == $"ASLO_{i}:")
                            {
                                this.aslo[i] = double.Parse(parts[1]);
                                break;
                            }
                        }
                    }
                    else if (parts[0].Contains("AOFF_"))
                    {
                        for (Int32 i = 0; i < this.numAi; i++)
                        {
                            if (parts[0] == $"AOFF_{i}:")
                            {
                                this.aoff[i] = double.Parse(parts[1]);
                                break;
                            }
                        }
                    }
                    else if (parts[0].Contains("AI_OFFSET_"))
                    {
                        for (Int32 i = 0; i < this.numAi; i++)
                        {
                            if (parts[0] == $"AI_OFFSET_{i}:")
                            {
                                this.aiOffset[i] = double.Parse(parts[1]);
                                break;
                            }
                        }
                    }
                    else if (parts[0].Contains("COUNTER_OFFSET_"))
                    {
                        for (Int32 i = 0; i < this.numCounter; i++)
                        {
                            if (parts[0] == $"CNT_OFFSET_{i}:")
                            {
                                this.cntOffset[i] = double.Parse(parts[1]);
                                break;
                            }
                        }
                    }

                }

                reader.Close();
                fs.Close();
            }
            catch (Exception ex)
            {
                _error = this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + " ";
                _error += ex.Message;
                return ERROR;
            }


            if (err > 0)
                return ERROR;

            return SUCCESS;
        }
        public Int32 WriteDefinition(string _path, ref string _error)
        {
            Int32 rt;
            FileStream fs;
            StreamWriter writer;


            try
            {
                if (__numAi != this.aslo.Length)
                {
                    _error = this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + " ";
                    _error += "DATA_STRUCTURE_INCONSISTENT";
                    return ERROR;
                }
                if (__numAi != this.aoff.Length)
                {
                    _error = this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + " ";
                    _error += "DATA_STRUCTURE_INCONSISTENT";
                    return ERROR;
                }
                if (__numAi != this.aiOffset.Length)
                {
                    _error = this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + " ";
                    _error += "DATA_STRUCTURE_INCONSISTENT";
                    return ERROR;
                }
                if (__numCnt != this.cntOffset.Length)
                {
                    _error = this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + " ";
                    _error += "DATA_STRUCTURE_INCONSISTENT";
                    return ERROR;
                }
            }
            catch (Exception ex)
            {
                _error = this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + " ";
                _error += ex.Message;
                return ERROR;
            }
           


            try
            {
                fs = new FileStream(_path, FileMode.Create);
                writer = new StreamWriter(fs);


                writer.WriteLine($"NUM_AI: {this.numAi}");
                writer.WriteLine($"NUM_CNT: {this.numCounter}");
                writer.WriteLine("#");

                for (Int32 i = 0; i < this.numAi; i++)
                {
                    writer.WriteLine($"ASLO_{i}: {this.aslo[i]}");
                }
                writer.WriteLine("#");

                for (Int32 i = 0; i < this.numAi; i++)
                {
                    writer.WriteLine($"AOFF_{i}: {this.aoff[i]}");
                }
                writer.WriteLine("#");

                for (Int32 i = 0; i < this.numAi; i++)
                {
                    writer.WriteLine($"AI_OFFSET_{i}: {this.aiOffset[i]}");
                }
                writer.WriteLine("#");

                for (Int32 i = 0; i < this.numCounter; i++)
                {
                    writer.WriteLine($"CNT_OFFSET_{i}: {this.cntOffset[i]}");
                }
                writer.WriteLine("#");


                writer.Close();
                fs.Close();
            }
            catch (Exception ex)
            {
                _error = this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + " ";
                _error += ex.Message;
                return ERROR;
            }

            return SUCCESS;
        }
        //-----------------------------------------------------------------
        public Int32 CaGetASLO(string _fpganame, ref string _error)
        {
            Int32 rt;
            double val;

            // check if arrays are alread correct size
            // setting numAi will overwrite ASLO, AOFF and aiOffstes
            if (this.numAi != 8)
                this.numAi = 8;


            for (Int32 i = 0; i < this.numAi; i++)
            {

                rt = EPICS.ca.get(_fpganame, $":ADC{i+1}.ASLO", out val);
                if (rt != EPICS.ca.SUCCESS)
                {
                    _error = System.Reflection.MethodBase.GetCurrentMethod().Name;
                    _error += " EPICS_CA_GET_FAILED";
                    return ERROR;
                }

                this.aslo[i] = val;
            }

            return SUCCESS;
        }
        public Int32 CaGetAOFF(string _fpganame, ref string _error)
        {
            Int32 rt;
            double val;

            // check if arrays are alread correct size
            // setting numAi will overwrite ASLO, AOFF and aiOffstes
            if (this.numAi != 8)
                this.numAi = 8;

            for (Int32 i = 0; i < this.numAi; i++)
            {

                rt = EPICS.ca.get(_fpganame, $":ADC{i+1}.AOFF", out val);
                if (rt != EPICS.ca.SUCCESS)
                {
                    _error = System.Reflection.MethodBase.GetCurrentMethod().Name;
                    _error += " EPICS_CA_GET_FAILED";
                    return ERROR;
                }

                this.aoff[i] = val;
            }


            return SUCCESS;
        }
        //-----------------------------------------------------------------
        #endregion



    }
}
