using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy;
using Nancy.Hosting.Self;
using Nancy.ModelBinding;
using System.Data.SQLite;
using System.Data;
//using Querys;

namespace NancyPerson
{
    public class Person
    {
        static string db_name = "person_db.sqlite";
        static SQLiteConnection sqlConn = new SQLiteConnection();
        static SQLiteCommand sqlCmd = new SQLiteCommand();
        static bool db_connected = false;

        public string name;
        public int age;

        public int? ConnectDB()
        {
            
            if (!File.Exists(db_name))
            {
                SQLiteConnection.CreateFile(db_name);
                Console.WriteLine("File for DB created");
            }
            try
            {
                sqlConn = new SQLiteConnection("Data source=" + db_name + ";Version=3;");
                sqlConn.Open();
                sqlCmd.Connection = sqlConn;

                sqlCmd.CommandText = "CREATE TABLE IF NOT EXISTS Persons (id TEXT, name TEXT, age INTEGER);";
                if (sqlCmd.ExecuteNonQuery() != 0)
                {
                    db_connected = true;
                    Console.WriteLine("Table created");
                }
                else
                {
                    Console.WriteLine("Table not created");
                }
            }
            catch (SQLiteException ex)
            {
                Console.WriteLine("Error:" + ex.Message);
                return null;
            }
            return 0;
        }

        public int InsertPerson(string id, string name, int age)
        {
            int res = 0;
            if (!db_connected)
            {
                ConnectDB();
            }
            try
            {
                string sqlQuery = "INSERT INTO Persons (id,name,age) values ('" + id + "','" + name + "'," + age.ToString() + ");";
                sqlCmd.CommandText = sqlQuery;
                sqlCmd.ExecuteNonQuery();
                Console.WriteLine("Person " + name + " inserted");
            }
            catch (SQLiteException ex)
            {
                Console.WriteLine("Insert error: " + ex.Message);
                res = -1;
            }

            return res;
        }

        public int? CreatePerson(string name, DateTime bDay)
        {
            int? res;
            int age = Convert.ToInt32((DateTime.Now - bDay).TotalDays) / 365;
            if (name == "" || age < 1 || age > 120)
            {
                res = null;
                Console.WriteLine("Invalid person params");
            }
            else
            {
                string id = Guid.NewGuid().ToString();
                Console.WriteLine("Person ID:" + id +" " + name + " created");
                res = InsertPerson(id, name, age);
            }
            return res;
        }
        public Person GetPerson(string id)
        {
            Person pr = new Person();
            DataTable dTable = new DataTable();
            pr.name = "";
            if (!db_connected)
            {
                ConnectDB();
            }
            try
            {
                string sqlQuery = "SELECT * FROM Persons WHERE id = '" + id + "';";
                SQLiteDataAdapter adapter = new SQLiteDataAdapter(sqlQuery, sqlConn);
                adapter.Fill(dTable);
                if (dTable.Rows.Count > 0)
                {
                    pr.name = dTable.Rows[0].ItemArray[1].ToString();
                    pr.age = Convert.ToInt32(dTable.Rows[0].ItemArray[2]);
                }
                Console.WriteLine(pr.name + " " + pr.age);
            } 
            catch (SQLiteException ex)
            {
                Console.WriteLine("Read error: " + ex.Message);
            }
            return pr;
        }
    }



    public class TestNancy : NancyModule
    {
        public class JSON_pers
        {
            public string name;
            public string BirthDay;
        }
        Person pr = new Person();

        public TestNancy()
        {
            Get("/{id}", args => 
            {
                pr = pr.GetPerson(args.id);
                return "User: " + pr.name + " " + pr.age;
            });
            Post("/person", args =>
            {
                JSON_pers jpr = this.Bind<JSON_pers>();
                string name = jpr.name;
                DateTime bDay = Convert.ToDateTime(jpr.BirthDay);
                int? res = pr.CreatePerson(name, bDay);

                Console.WriteLine("Created:" + res);
                return "Created";
            });
                        
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            using (var host = new NancyHost(new Uri("http://localhost:8000")))
            {
                
                try
                {
                    host.Start();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                Console.WriteLine("Nancy host start listening on localhost:8000");
                try
                {
                    Process.Start("http://localhost:8000");
                }
                catch (Exception ex )
                {
                    Console.WriteLine(ex.Message);
                }
                Console.ReadKey();
            }
            Console.WriteLine("Process stopped");
        }

    }
}
