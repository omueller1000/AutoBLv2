using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPGA
{
    public class FpgaData
    {
        public static Int32 evalData(ref UInt32[] _buffer, out FpgaDataFrame[] _dataFrames)
        {
            _dataFrames = new FpgaDataFrame[0];

            #region Variables
            //----------------------------------------------------
            int n;
            int i = 0;
            int j = 0;

            UInt32 word;

            UInt32 data_num_bytes = 0;
            UInt32 user_data_per_frame = 0;
            UInt32 num_frames = 0;

            UInt32 frame_size;                // frame size in words (4 bytes), not the first frame
            UInt32 frame_offset;

            UInt32 config_config;
            UInt32 config_adc;
            UInt32 config_counter;
            UInt32 config_encoder;
            UInt32 config_motor;

            UInt32 num_trigger = 2;
            UInt32 num_config = 0;
            UInt32 num_counters = 0;
            UInt32 num_adcs = 0;
            UInt32 num_motors = 0;
            UInt32 num_encoders = 0;

            UInt32 offset_trigger;
            UInt32 offset_config;
            UInt32 offset_counter;
            UInt32 offset_adc;
            UInt32 offset_encoder;
            UInt32 offset_motor;
            UInt32 offset_ios;

            UInt32 status_motor_1;
            UInt32 status_motor_2;
            UInt32 status_motor_3;
            UInt32 status_motor_4;
            UInt32 status_digital_in;
            UInt32 status_dac;
            //----------------------------------------------------
            #endregion

            #region Definitions
            //----------------------------------------------------
            UInt32 OFFSET_DATA_LEN = 0;
            UInt32 OFFSET_TRIG_REG_1 = 1;
            UInt32 OFFSET_TRIG_REG_2 = 2;
            UInt32 OFFSET_CONF_REG_1 = 3;
            UInt32 OFFSET_CONF_REG_2 = 4;

            UInt32 MASK_TRIGGER_TIME_MSB = 0xFC000000;
            UInt32 MASK_TRIGGER_TIME_LSB = 0x7FFFFFFF;
            UInt32 MASK_TRIGGER_WIDTH = 0x3FFFFFF;

            UInt32 MASK_CONFIG = 0x3F;
            UInt32 MASK_ADC = 0x3FC00;
            UInt32 MASK_COUNTER = 0xFFFFFFFF;
            UInt32 MASK_MOTOR_ENCODER = 0x3C0000;

            UInt32 MASK_STATUS_MOTOR_1 = 0x1F;
            UInt32 MASK_STATUS_MOTOR_2 = 0x3E0;
            UInt32 MASK_STATUS_MOTOR_3 = 0x7C00;
            UInt32 MASK_STATUS_MOTOR_4 = 0xF8000;

            UInt32 MASK_MOTOR_CW_LIMIT = 0x01;
            UInt32 MASK_MOTOR_CCW_LIMIT = 0x02;
            UInt32 MASK_MOTOR_INDEX_HIT = 0x04;
            UInt32 MASK_MOTOR_CW_MOVING = 0x08;
            UInt32 MASK_MOTOR_CCW_MOVING = 0x10;
            UInt32 MASK_DIGITAL_IN = 0xFF00000;
            UInt32 MASK_DAC = 0x80000000;

            UInt32 SHIFT_STATUS_MOTOR_1 = 0;
            UInt32 SHIFT_STATUS_MOTOR_2 = 5;
            UInt32 SHIFT_STATUS_MOTOR_3 = 10;
            UInt32 SHIFT_STATUS_MOTOR_4 = 15;
            UInt32 SHIFT_STATUS_DIGITAL_IN = 20;
            UInt32 SHIFT_STATUS_DAC = 32;


            UInt32 SHIFT_CONFIG_CONFIG = 0;
            UInt32 SHIFT_CONFIG_ADC = 10;
            UInt32 SHIFT_CONFIG_MOTOR_ENCODER = 18;
            UInt32 SHIFT_CONFIG_COUNTER = 0;
            //----------------------------------------------------
            #endregion

            //----------------------------------------------------------------------
            // The ZERO Header
            //----------------------------------------------------------------------
            // the very first 5 words (20 bytes) are conatin additional information
            // which is not repeated in subsequent headers
            //
            //	0	Total lentgh in Bytes
            //	1	Trigger 1
            //	2	Trigger 2
            //	3	Config 1
            //	4	Config 2
            //----------------------------------------------------------------------

            //--- LENGTH -----------------------------------------------------------
            word = _buffer[0];
            data_num_bytes = word;
            if (data_num_bytes < 20)                // The firts frame must have at least 5 words (20 bytes)				
                return -1;
            //----------------------------------------------------------------------

            //--- DAQ DATA TYPE ----------------------------------------------------
            //word = _buffer[3];
            word = _buffer[1];
            if ((word & 0x80000000) != 0)           // check if daq data or automatic status report			
                return -2;
            //----------------------------------------------------------------------

            //--- CONFIG -----------------------------------------------------------
            // Config 1
            word = _buffer[3];
            config_config = (UInt32)((Int32)(word & MASK_CONFIG) >> (Int32)SHIFT_CONFIG_CONFIG);
            config_adc = (UInt32)((Int32)(word & MASK_ADC) >> (Int32)SHIFT_CONFIG_ADC);
            config_encoder = (UInt32)((Int32)(word & MASK_MOTOR_ENCODER) >> (Int32)SHIFT_CONFIG_MOTOR_ENCODER);
            config_motor = (UInt32)((Int32)(word & MASK_MOTOR_ENCODER) >> (Int32)SHIFT_CONFIG_MOTOR_ENCODER);

            // Config 1
            word = _buffer[4];
            config_counter = (UInt32)((Int32)(word & MASK_COUNTER) >> (Int32)SHIFT_CONFIG_COUNTER);
            //----------------------------------------------------------------------


            //----------------------------------------------------------------------
            num_config = (UInt32)count_high_bits(config_config);
            num_adcs = (UInt32)count_high_bits(config_adc);
            num_encoders = (UInt32)count_high_bits(config_encoder);
            num_motors = (UInt32)count_high_bits(config_motor);
            num_counters = (UInt32)count_high_bits(config_counter);
            //----------------------------------------------------------------------


            //----------------------------------------------------------------------
            // verify the validity of data_num_bytes and get num_frames
            frame_size = num_config + num_adcs + num_encoders + num_motors + num_counters + 3;
            user_data_per_frame = 4 * frame_size;
            if ((data_num_bytes - 8) % user_data_per_frame != 0)
                return -1;

            num_frames = (data_num_bytes - 8) / user_data_per_frame;
            //----------------------------------------------------------------------

            _dataFrames = new FpgaDataFrame[num_frames];
            for (i = 0; i < num_frames; i++)
            {
                _dataFrames[i] = new FpgaDataFrame();
            }


            //----------------------------------------------------------------------
            // Read data frames
            //----------------------------------------------------------------------						
            for (n = 0; n < num_frames; n++)
            {
                // determine correct offsets
                // the 1st frame (n == 0) differs from the others
                if (n == 0)
                {
                    frame_offset = 1;

                    offset_trigger = frame_offset;
                    offset_config = offset_trigger + num_trigger + 2;
                    offset_counter = offset_config + num_config;
                    offset_adc = offset_counter + num_counters;
                    offset_encoder = offset_adc + num_adcs;
                    offset_motor = offset_encoder + num_encoders;
                    offset_ios = offset_motor + num_motors;
                }
                else
                {
                    frame_offset = (UInt32)(frame_size * n) + 3;

                    offset_trigger = frame_offset;
                    offset_config = offset_trigger + num_trigger;
                    offset_counter = offset_config + num_config;
                    offset_adc = offset_counter + num_counters;
                    offset_encoder = offset_adc + num_adcs;
                    offset_motor = offset_encoder + num_encoders;
                    offset_ios = offset_motor + num_motors;
                }


                //--- read TRIGGER --- 2 words ---
                word = _buffer[offset_trigger];
                _dataFrames[n].time = word & MASK_TRIGGER_TIME_LSB;

                word = _buffer[offset_trigger + 1];
                _dataFrames[n].time |= (word & MASK_TRIGGER_TIME_MSB) << 5;
                _dataFrames[n].gate = word & MASK_TRIGGER_WIDTH;


                //--- read COUNTERs --- 0 to 32 words ---
                for (i = 0; i < num_counters; i++)
                {
                    _dataFrames[n].counter[i] = _buffer[offset_counter + i];
                }


                //--- read ADCs --- 0 to 8 words ---
                for (i = 0; i < num_adcs; i++)
                {
                    _dataFrames[n].ai[i] = _buffer[offset_adc + i];
                }


                //--- read ENCODERs --- 0 to 4 words ---
                for (i = 0; i < num_encoders; i++)
                {
                    UInt32 encoder_uint = _buffer[offset_encoder + i];
                    Int32 encoder_int = (Int32)(encoder_uint - 0x7FFFFFFF);
                    _dataFrames[n].encoder[i] = encoder_int;
                }


                //--- read MOTORs --- 0 to 4 words ---
                for (i = 0; i < num_motors; i++)
                {
                    _dataFrames[n].motor[i] = _buffer[offset_motor + i];
                }
            }
            //----------------------------------------------------------------------

            return 0;
        }
        private static Int32 count_high_bits(UInt32 _input)
        {
            UInt32 value = _input;
            UInt32 num = 0;

            while (value > 0)
            {
                num += value & 0x01;
                value >>= 1;
            }

            return (Int32)num;
        }
    }


    public class FpgaDataFrame
    {
        #region Variables
        //-----------------------------------------------------------------
        public static Int32 numMaxCounter = 32;
        public static Int32 numMaxAi = 8;
        public static Int32 numMaxEncoder = 4;
        public static Int32 numMaxMotor = 4;
        //-----------------------------------------------------------------
        public UInt64 time;
        public UInt32 gate;
        public UInt32[] ai;
        public UInt32[] counter;
        public Int32[] encoder;
        public UInt32[] motor;
        //-----------------------------------------------------------------
        #endregion


        //=================================================================
        public FpgaDataFrame()
        {
            ai = new UInt32[8];
            counter = new UInt32[32];
            encoder = new Int32[4];
            motor = new UInt32[4];
        }
        //=================================================================		
    }


    public class FpgaOffsets
    {
        #region Constants
        //-----------------------------------------------------------------
        public const Int32 SUCCESS = 1;
        public const Int32 ERROR = -1;
        //-----------------------------------------------------------------
        #endregion


        #region Private Variables
        //-------------------------------------------------------------------------------
        private Int32 __numAi = 8;
        private double[] __aiOffset;
        private double[] __aiSlope;

        private Int32 __numCounter = 32;
        private double[] __counterOffset;
        //-------------------------------------------------------------------------------
        #endregion


        #region Public Properties
        //-------------------------------------------------------------------------------s
        public Int32 numAi
        {
            get { return __numAi; }
        }
        public double[] aiOffset
        {
            get { return __aiOffset; }
            set { __aiOffset = value; }
        }
        public double[] aiSlope
        {
            get { return __aiSlope; }
            set { __aiSlope = value; }
        }
        public Int32 numCounter
        {
            get { return __numCounter; }
        }
        public double[] counterOffset
        {
            get { return __counterOffset; }
            set { __counterOffset = value; }
        }
        //-------------------------------------------------------------------------------
        #endregion



        //=================================================================
        public FpgaOffsets()
        {
            __aiOffset = new double[__numAi];
            __aiSlope = new double[__numAi];

            __counterOffset = new double[__numCounter];

            for (int i = 0; i < __numAi; i++)
            {
                __aiOffset[i] = -2147483648; // 2^31
                __aiSlope[i] = 1.0;
            }

            for (int i = 0; i < __numCounter; i++)
            {
                __counterOffset[i] = 0.0;
            }
        }
        //=================================================================



        #region Public Methods
        //-------------------------------------------------------------------------------
        public int LoadDefinition(string _path, ref string _error)
        {
            FileStream stream;
            StreamReader reader;
            string line;
            string[] parts;
            Int32 lineCounter = 0;
            Int32 error = 0;


            bool[] aiOffsetRead = new bool[__numAi];
            bool[] aiSlopeRead = new bool[__numAi];
            for (int i = 0; i < __numAi; i++)
            {
                aiOffsetRead[i] = false;
                aiSlopeRead[i] = false;
            }

            bool[] counterOffsetRead = new bool[__numCounter];
            for (int i = 0; i < __numCounter; i++)
            {
                counterOffsetRead[i] = false;
            }




            if (!File.Exists(_path))
            {
                _error = this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name;
                _error += " FILE_DOES_NOT_EXIST";
                return ERROR;
            }

            #region Read File
            //............................................................................
            stream = new FileStream(_path, FileMode.Open);
            reader = new StreamReader(stream);

            while (reader.Peek() >= 0)
            {
                line = reader.ReadLine();
                lineCounter++;

                if (line[0] == '#')
                    continue;

                parts = line.Split('\t');
                if (parts.Length != 2)
                {
                    _error = this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name;
                    _error += " FILE_INCOSISTENT";

                    reader.Close();
                    stream.Close();
                    return ERROR;
                }


                if (parts[0].Contains("AI_OFFSET_"))
                {
                    for (int i = 0; i < __numAi; i++)
                    {
                        if (parts[0] == "AI_OFFSET_" + i.ToString() + ":")
                        {
                            if (aiOffsetRead[i])
                            {
                                _error = this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name;
                                _error += " FILE_INCOSISTENT";
                                error++;
                            }
                            aiOffsetRead[i] = true;
                            __aiOffset[i] = Convert.ToDouble(parts[1]);
                            break;
                        }
                    }
                }
                else if (parts[0].Contains("AI_SLOPE_"))
                {
                    for (int i = 0; i < __numAi; i++)
                    {
                        if (parts[0] == "AI_SLOPE_" + i.ToString() + ":")
                        {
                            if (aiSlopeRead[i])
                            {
                                _error = this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name;
                                _error += " FILE_INCOSISTENT";
                                error++;
                            }
                            aiSlopeRead[i] = true;
                            __aiSlope[i] = Convert.ToDouble(parts[1]);
                            break;
                        }
                    }
                }
                else if (parts[0].Contains("COUNTER_OFFSET_"))
                {
                    for (int i = 0; i < __numCounter; i++)
                    {
                        if (parts[0] == "COUNTER_OFFSET_" + i.ToString() + ":")
                        {
                            if (counterOffsetRead[i])
                            {
                                _error = this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name;
                                _error += " FILE_INCOSISTENT";
                                error++;
                            }
                            counterOffsetRead[i] = true;
                            counterOffset[i] = Convert.ToDouble(parts[1]);
                            break;
                        }
                    }
                }
                else
                {
                    _error = this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name;
                    _error += " UNKNOWN_PROPERTY";
                    error++;
                }
            }

            reader.Close();
            stream.Close();
            //............................................................................
            #endregion


            if (error > 0)
            {
                _error = this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name;
                _error += " ERRORS_OCCURRED";
                return ERROR;
            }

            return SUCCESS;
        }
        public int WriteDefintion(string _path, ref string _error)
        {

            FileStream stream;
            StreamWriter writer;

            try
            {
                stream = new FileStream(_path, FileMode.Create);
                writer = new StreamWriter(stream);

                for (int i = 0; i < __numAi; i++)
                {
                    writer.WriteLine("AI_OFFSET_" + i.ToString() + ":\t" + __aiOffset[i].ToString());
                }

                writer.WriteLine("#");

                for (int i = 0; i < __numAi; i++)
                {
                    writer.WriteLine("AI_SLOPE_" + i.ToString() + ":\t1.0");
                }

                writer.WriteLine("#");

                for (int i = 0; i < __numCounter; i++)
                {
                    writer.WriteLine("COUNTER_OFFSET_" + i.ToString() + ":\t" + __counterOffset[i].ToString());
                }

                writer.Close();
                stream.Close();
            }
            catch (Exception ex)
            {
                _error = this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + " ";
                _error += ex.Message;

                return ERROR;
            }

            return SUCCESS;
        }
        //-------------------------------------------------------------------------------
        #endregion



    }

}
