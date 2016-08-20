using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using Dapper;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Npgsql;

namespace ConsoleApplication
{
    public class Program
    {
        private NpgsqlConnection _conn;
        private string _connString;

        public static void Main(string[] args)
        {
            var prog = new Program();
        }

        public void Init()
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("parameters.json");
            _connString = builder.Build().GetConnectionString("postgres");

            InitFixtures();
            RunQuery();
        }

        public void InitFixtures() 
        {
            var conn = new NpgsqlConnection(_connString);
            var samplesql = File.ReadAllText(System.AppContext.BaseDirectory + "/sample.sql");

            using(conn)
            {
                conn.Open();

                var tableCount = (Int64)conn.ExecuteScalar("select count(*) from information_schema.tables where table_schema = 'public';");
                if(tableCount > 0) { throw new Exception("The test database in not empty"); }

                var schemaCommand = new NpgsqlCommand(samplesql, conn);
                schemaCommand.ExecuteNonQuery();
            }
        }

        public void RunQuery() 
        {
            try
            {
                _conn.Open();
                using(var trans = _conn.BeginTransaction())
                {

                }            
            }
            finally
            {
                _conn.Close();
            }
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
    }
}
