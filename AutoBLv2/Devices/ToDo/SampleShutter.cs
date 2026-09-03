using SharpCompress.Compressors.ZStandard.Unsafe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devices
{
    public class SampleShutter : Shutter
    {


        #region Variables
        //-----------------------------------------------------------------
        private string __epicsName;
        private UInt32 __do;
        //-----------------------------------------------------------------
        #endregion




        //=================================================================
        public SampleShutter(string _epicsName, UInt32 _do)
        {
            __epicsName = _epicsName;
            __do = _do;
        }
        //=================================================================




        #region Public Override Methods
        //-----------------------------------------------------------------
        public override Int32 Open(ref string _error)
        {
            return SUCCESS;
        }
        public override Int32 Close(ref string _error)
        {
            return SUCCESS;
        }
        //-----------------------------------------------------------------
        #endregion



    }
}
