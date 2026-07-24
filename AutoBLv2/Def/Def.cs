using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Def
{
    public class Def
    {
        #region Constants
        //-----------------------------------------------------------
        public const Int32 SUCCESS = 1;
        public const Int32 ERROR = -1;
        //-----------------------------------------------------------
        #endregion



        #region Public Methods
        //-----------------------------------------------------------        
        public static Int32 UpdateDefinition(string _path, string _key, string _value, ref string _error)
        {
            FileStream stream;
            StreamReader reader;
            Int32 lineCounter = 0;
            Int32 lineToUpdate = -1;
            Int32 keyCounter = 0;
            string line;
            string[] lines;
            string[] parts;


            #region tests
            //---------------------------------------------------------------------------
            if (!File.Exists(_path))
            {
                _error = System.Reflection.MethodBase.GetCurrentMethod().Name + " ";
                _error += "UpdateDefinition: specified file doesn't exist: " + _path;
                return ERROR;
            }

            if (_key == "")
            {
                _error = System.Reflection.MethodBase.GetCurrentMethod().Name + " ";
                _error += "UpdateDefinition: specified key is empty: " + _path;
                return ERROR;
            }
            //---------------------------------------------------------------------------
            #endregion


            #region find the line to update
            //---------------------------------------------------------------------------
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
                    reader.Close();
                    stream.Close();

                    _error = System.Reflection.MethodBase.GetCurrentMethod().Name + " ";
                    _error = "There is an error at line " + lineCounter.ToString() + ", " + _path;
                    return ERROR;
                }

                if (parts[0] == _key + ":")
                {
                    lineToUpdate = lineCounter;
                    keyCounter++;
                }
            }

            reader.Close();
            stream.Close();

            if (keyCounter == 0)
            {
                _error = System.Reflection.MethodBase.GetCurrentMethod().Name + " ";
                _error = "The specified key was not found: " + _key + ", " + _path;
                return ERROR;
            }
            else if (keyCounter > 1)
            {
                _error = System.Reflection.MethodBase.GetCurrentMethod().Name + " ";
                _error += "The key was found at multiple locations: " + _key + ", " + _path;
                return ERROR;
            }
            //---------------------------------------------------------------------------
            #endregion


            #region update actual line
            //---------------------------------------------------------------------------
            // read all lines into memory			
            try
            {
                lines = File.ReadAllLines(_path);
            }
            catch (Exception ex)
            {
                _error = System.Reflection.MethodBase.GetCurrentMethod().Name + " ";
                _error += "File.ReadAllLines failed!\n" + ex.ToString();
                return ERROR;
            }


            // edit specific line
            if (lines == null ||
                lines.Length < lineToUpdate - 2)
            {
                _error = System.Reflection.MethodBase.GetCurrentMethod().Name + " ";
                _error += "Reading file failed: " + _path;
                return ERROR;
            }

            lines[lineToUpdate - 1] = _key + ":\t" + _value;

            // write all lines
            try
            {
                File.WriteAllLines(_path, lines);
            }
            catch (Exception ex)
            {
                _error = System.Reflection.MethodBase.GetCurrentMethod().Name + " ";
                _error += "File.WriteAllLines failed!\n" + ex.ToString();
                return ERROR;
            }
            //---------------------------------------------------------------------------
            #endregion


            return SUCCESS;
        }
        public static Int32 ReadDefinition(string _path, string _key, out string _value, ref string _error)
        {
            _value = "";
            FileStream stream;
            StreamReader reader;
            Int32 lineCounter = 0;
            Int32 keyCounter = 0;
            string line;
            string[] parts;


            #region tests
            //---------------------------------------------------------------------------
            if (!File.Exists(_path))
            {
                _error = System.Reflection.MethodBase.GetCurrentMethod().Name + " ";
                _error += "UpdateDefinition: specified file doesn't exist: " + _path;
                return ERROR;
            }

            if (_key == "")
            {
                _error = System.Reflection.MethodBase.GetCurrentMethod().Name + " ";
                _error += "UpdateDefinition: specified key is empty: " + _path;
                return ERROR;
            }
            //---------------------------------------------------------------------------
            #endregion

            #region find the line to update
            //---------------------------------------------------------------------------
            stream = new FileStream(_path, FileMode.Open);
            reader = new StreamReader(stream);

            while (reader.Peek() >= 0)
            {
                line = reader.ReadLine();

                if (line[0] == '#')
                    continue;

                parts = line.Split('\t');
                if (parts.Length != 2)
                {
                    reader.Close();
                    stream.Close();

                    _error = System.Reflection.MethodBase.GetCurrentMethod().Name + " ";
                    _error = "There is an error at line " + lineCounter.ToString() + ", " + _path;
                    return ERROR;
                }

                if (parts[0] == _key + ":")
                {
                    keyCounter++;
                    _value = parts[1];
                }
            }

            reader.Close();
            stream.Close();

            if (keyCounter == 0)
            {
                _error = System.Reflection.MethodBase.GetCurrentMethod().Name + " ";
                _error = "The specified key was not found: " + _key + ", " + _path;
                return ERROR;
            }
            else if (keyCounter > 1)
            {
                _error = System.Reflection.MethodBase.GetCurrentMethod().Name + " ";
                _error += "The key was found at multiple locations: " + _key + ", " + _path;
                return ERROR;
            }
            //---------------------------------------------------------------------------
            #endregion

            return SUCCESS;
        }
        //-----------------------------------------------------------
        #endregion

    }
}