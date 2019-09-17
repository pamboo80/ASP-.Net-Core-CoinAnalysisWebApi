using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

using System.Data;
using MySql.Data.MySqlClient;
using StackExchange.Redis;

namespace CoinAnalysisWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuotesController : ControllerBase
    {
        private String RedisServer = "127.0.0.1:6379";

        // GET api/values
        [HttpGet]
        public ActionResult<string> Get()
        {

            String retValue = "{\"status\": {\"timestamp\": \"\",\"error_code\": 19722,\"error_message\":" + "\"WebApi Call Error\"" + ",\"elapsed\": 0,\"credit_count\": 1}}";
            Database db = new Database();          
          
            //Redis caching         

            // ConfigurationOptions option = new ConfigurationOptions
            // {
            //     AbortOnConnectFail = false,
            //     ResolveDns = true
            // };
            ConnectionMultiplexer redis=null;
            IDatabase cache = null;
            try{
                redis = ConnectionMultiplexer.Connect( RedisServer +",ResolveDns=true");
                cache = redis.GetDatabase();
            }
            catch(Exception)
            {
                cache = null;                
            }
            if(cache!=null)
            {
                var raw_json = cache.StringGet("raw_json_exchange_rate");
                if (!raw_json.IsNull)
                {
                    var created_time = cache.StringGet("created_time_exchange_rate");
                    if (!created_time.IsNull)
                    {
                        String sCreated_time = created_time.ToString();
                        DateTime dtCreated_time = DateTime.Parse(sCreated_time);  
                        //It will get from the cache for any two mins interval calls
                        if((DateTime.Now.AddMinutes(330) - dtCreated_time).TotalMinutes < 30 )   //+5:30  (60*5+30)                
                        {                            
                            //return dtCreated_time+" "+ " " +  DateTime.Now + " "+ DateTime.UtcNow + ((dtCreated_time - DateTime.Now).TotalMinutes).ToString();
                            return raw_json.ToString();
                        }                        
                    }
                }
            }
                
            try
            {
                //Query the mySQL table
                MySqlConnection connection = db.getConnection();
                db.OpenConnection();

                String sql = "select * from coinanalysis.`fixer.io` where id = (select max(id) from coinanalysis.`fixer.io` group by error_code having error_code=0)";
                MySqlCommand cmd = new MySqlCommand(sql, connection);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        retValue = reader["raw_json"].ToString();
                        //Update cache values 
                        cache.StringSet("raw_json_exchange_rate", retValue);
                        cache.StringSet("created_time_exchange_rate", reader["created_time"].ToString());
                    }
                }
            }
            catch (Exception ex)
            { 
                retValue = retValue.Replace("WebApi Call Error","WebApi Call Exception:"+ ex.Source+ ":" + ex.Message);                
            }
            finally
            {
                if (db != null && db.ConnectionState() != ConnectionState.Closed)
                {
                    db.CloseConnection();
                }
            }

            return retValue;
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
