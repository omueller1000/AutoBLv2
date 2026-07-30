using Devices;
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
        public const Int32 ABORT = 0;
        //-----------------------------------------------------------
        #endregion





        public Int32 FindMaxGain(ref SRS570[] _amplifiers, ref string _error)
        {
            Int32 rt;
            Int32 minSensId = 21;
            Int32 maxSensId = 12;



            return SUCCESS;
        }



    }
}
