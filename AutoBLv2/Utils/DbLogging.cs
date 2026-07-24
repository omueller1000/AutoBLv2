using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{
    public class DbLogging
    {
        #region Variables
        //-----------------------------------------------------------        
        private IMongoCollection<DbLogData> __collection;
        //-----------------------------------------------------------        
        #endregion



        //===========================================================
        public DbLogging(string _conStr, string _db)
        {
            var client = new MongoClient(_conStr);
            var db = client.GetDatabase(_db);
            __collection = db.GetCollection<DbLogData>("AUTOBL_LOGS");
        }
        //===========================================================




        #region Public Methods
        //-----------------------------------------------------------
        public void DbLog(ref DbLogData _data)
        {            
            __collection.InsertOne(_data);
        }
        public void DbLog(ref string _requestStr)
        {
            DbLogData log = new DbLogData();
            log.time_stamp = DateTime.Now;
            log.request_string = _requestStr;
            log.is_status = false;

            if (_requestStr.Contains("GET_STATUS"))
                log.is_status = true;

            DbLog(ref log);

        }
        public void DbLog(ref string _requestStr, ref string _responseStr)
        {
            DbLogData log = new DbLogData();
            log.time_stamp = DateTime.Now;
            log.request_string = _requestStr;
            log.is_status = false;


            // if "GET_STATUS" is part of the response string, the entire response should be logged
            // IndexOf + Substring should be faster than .Split("\r")[0]
            Int32 del;
            string sub;

            del = _responseStr.IndexOf("\r");
            if (del > 0)
            {
                sub = _responseStr.Substring(0, del);

                if (sub.Contains("GET_STATUS"))
                {
                    log.response_string = _responseStr; // full response string, it's usually short and contains very useful information
                    log.is_status = true;
                }
                else
                    log.response_string = sub;          // don't need the entire response string, if it contains data it could be quite large
            }
            else
            {
                log.response_string = _responseStr;
            }


            DbLog(ref log);
        }
        //-----------------------------------------------------------
        #endregion

    }


    [BsonIgnoreExtraElements] public class DbLogData
    {
        [BsonElement("request_string")]
        public string request_string { get; set; }

        [BsonElement("response_string")]
        public string response_string { get; set; }

        [BsonElement("is_status")]
        public bool is_status { get; set; }

        [BsonElement("time_stamp")]
        public DateTime time_stamp { get; set; }

    }


}
