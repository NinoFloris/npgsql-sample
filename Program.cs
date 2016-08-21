using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Npgsql;

namespace ConsoleApplication
{
    public class Program
    {
        private string _connString;
        private NpgsqlConnection _conn;

        public static void Main(string[] args)
        {
            var prog = new Program();
            prog.Init().GetAwaiter().GetResult();
        }

        public Task Init()
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("parameters.json");
            _connString = builder.Build().GetConnectionString("postgres");

            _conn = new NpgsqlConnection(_connString);
            Npgsql.Logging.NpgsqlLogManager.Provider = new Npgsql.Logging.ConsoleLoggingProvider(Npgsql.Logging.NpgsqlLogLevel.Trace, true, true);

            SqlMapper.AddTypeHandler(typeof(Json), new JsonTypeHandler());

            InitFixtures();
            return RunQuery();
        }

        public void InitFixtures() 
        {
            var samplesql = File.ReadAllText("sample.sql");

            try
            {
                _conn.Open();
                var createFixture = new NpgsqlCommand(samplesql, _conn);
                createFixture.ExecuteNonQuery();
            }
            finally
            {
                _conn.Close();
            }
        }

        public async Task RunQuery() 
        {
            var conn = new NpgsqlConnection(_connString);

            try
            {
                conn.Open();
                using(var trans = conn.BeginTransaction())
                {
                    // It returns the correct exception if you remove asynchronicity up to the root (main method).
                    // Another key to this bug is that there must still be something left in the reader to read, 
                    // if the exception is in the last (or only) statement, it also works correctly.
                    var reader = await conn.QueryMultipleAsync(
                        @"SELECT * FROM sample;
                          SELECT * FROM sample;
                        "
                        , null, trans);

                    var samples = reader.Read<Sample>();
                    var nextSamples = reader.Read<Sample>();
                }            
            }
            finally
            {
                conn.Close();
            }

            return;
        }

        public class JsonTypeHandler: SqlMapper.TypeHandler<Json> {
            private readonly JsonSerializerSettings settings;

            public JsonTypeHandler(JsonSerializerSettings settings = null) {
                this.settings = settings ?? JsonConvert.DefaultSettings?.Invoke();
            }

            public override Json Parse(object value) {
                if (value == null || value == DBNull.Value) {
                    return new Json();
                }

                return JsonConvert.DeserializeObject<Json>((string)value, this.settings);
            }

            public override void SetValue(IDbDataParameter parameter, Json value)
            {
                parameter.Value = "test";
            }
        }

        public class Json: Dictionary<Object, Object>
        {

        }

        public class Sample 
        {
            public int Id { get; set; }
            public Json Metadata { get; set; }
        }
    }
}
