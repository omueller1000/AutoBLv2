using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devices
{
    public abstract class Shutter
    {


        //===========================================================
        protected Shutter()
        {
        
        
        }
        //===========================================================


        public abstract Int32 Open(ref string _error);
        public abstract Int32 Close(ref string _error);



    }
}
