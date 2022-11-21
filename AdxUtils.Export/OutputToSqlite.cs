using AdxUtils.Options;
using Kusto.Cloud.Platform.Aad;
using Kusto.Data;
using Kusto.Data.Net.Client;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using AdxUtils.Options;
using System.Data.Entity;
using System.Text.RegularExpressions;
using System.Security.Permissions;

namespace AdxUtils.Export
{
    public class OutputToSqlite
    {
        public static string Query { get;set; }

        public async static void OutputResultToSqlite(IAuthenticationOptions authenticationOptions)
        {
            //Declare vars
            HashSet<string> columnNames = new HashSet<string>();
            DataTable columns = new DataTable();
            DataTable adxOutput = new DataTable();

            var cluster = authenticationOptions.Endpoint;
            var tableName = authenticationOptions.DatabaseName;  
            
            //Initialise Kusto connection
            var kcsb = new KustoConnectionStringBuilder(cluster, tableName).WithAadAzCliAuthentication(); //withAadCliauthent not sure if needed here cos authenticate method
            var runInput = KustoClientFactory.CreateCslQueryProvider(kcsb);

            if (authenticationOptions.Query.StartsWith("."))
            {
                Console.WriteLine($"Executing command against {tableName}");

                Query = authenticationOptions.Query;
                runInput = KustoClientFactory.CreateCslQueryProvider(kcsb);
            }

            if (Regex.IsMatch(authenticationOptions.Query, @"^[a-zA-Z]"))
            {
                Console.WriteLine($"Executing query against {tableName}");

                Query= authenticationOptions.Query;
                runInput = KustoClientFactory.CreateCslQueryProvider(kcsb);

            }
            else if (!(authenticationOptions.Query.StartsWith(".") || Regex.IsMatch(authenticationOptions.Query, @"^[a-zA-Z]")))
            {
                Console.WriteLine("Invalid query/command input");
            }

            //Initialise SQL connection
            string projectDirectory = Directory.GetParent("AdxUtils.Export").Parent.Parent.Parent.Parent.FullName;

            string cs = $"URI=file:{projectDirectory}\\AdxUtils.Export\\Output\\ADXQueryResults.db";

            using var con = new SQLiteConnection(cs);
            con.Open();

            using var cmd = new SQLiteCommand(con);
            cmd.CommandText = $"DROP TABLE IF EXISTS {tableName}";
            cmd.ExecuteNonQuery();

            cmd.CommandText = $@"CREATE TABLE {tableName} (temp STRING)";
            cmd.ExecuteNonQuery();

            adxOutput.Load(runInput.ExecuteQuery(Query));

            //Create columns in SQL table
            var getColumns = $"SELECT * FROM {tableName}";
            columns.Load(runInput.ExecuteQuery(getColumns));

            foreach (DataRow item in columns.Rows)
            {
                foreach (DataColumn itemName in columns.Columns)
                {
                    columnNames.Add(itemName.ToString());
                    Debug.WriteLine(itemName);
                }
            }
            foreach (var name in columnNames)
            {
                Debug.WriteLine($"{name}");
                cmd.CommandText = $"ALTER TABLE {tableName} ADD {name} varchar(255)";
                cmd.ExecuteNonQuery();
            }

            //Insert data into columns
            Debug.WriteLine(adxOutput.Rows.Count);
            foreach (DataRow row in adxOutput.Rows)
            {
                foreach (DataColumn column in adxOutput.Columns)
                {
                    Debug.WriteLine($"{column}");
                    Debug.WriteLine(row[column].ToString());


                    string columnName = column.ToString();
                    string columnItem = row[column].ToString();

                    cmd.CommandText = $"INSERT INTO {tableName}({columnName}) VALUES ({columnItem})";
                    cmd.ExecuteNonQuery();
                }
            }
            cmd.CommandText = $"ALTER TABLE {tableName} DROP COLUMN temp";
            cmd.ExecuteNonQuery();
            SQLiteConnection.ClearAllPools();
        }
    }
}
