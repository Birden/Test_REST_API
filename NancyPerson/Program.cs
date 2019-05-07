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
//using Querys;

namespace NancyPerson
{
    public class Person
    {
        static int id_count = 0;
        string db_name = "person_db.sqlite";
        SQLiteConnection sqlConn;
        SQLiteCommand sqlCmd;
        static bool db_connected = false;

        public int? CreateDB()
        {
            if (!File.Exists(db_name))
            {
                SQLiteConnection.CreateFile(db_name);
            }
            try
            {
                sqlConn = new SQLiteConnection("Data source=" + db_name + ";Version=3;");
                sqlConn.Open();
                sqlCmd.Connection = sqlConn;
                sqlCmd.CommandText = "CREATE TABLE IF NOT EXISTS Persons (id INTEGER name TEXT age INTEGER)";
                sqlCmd.ExecuteNonQuery();
                db_connected = true;
            }
            catch (SQLiteException ex)
            {
                Console.WriteLine("Error:" + ex.Message);
                return null;
            }
            return 0;
        }

        public int InsertPerson(int id, string name, int age)
        {
            string sqlQuery;
            if (!db_connected)
            {
                CreateDB();
            }
            else
            {
                try
                {
                    sqlQuery = "INSERT INTO Persons ('id','name','age') values ('" + id + "','" + name + "','" + age.ToString() + "')";
                    sqlCmd.CommandText = sqlQuery;
                    sqlCmd.ExecuteNonQuery();
                }
                catch (SQLiteException ex)
                {
                    Console.WriteLine("Insert error: " + ex.Message);
                }
            }

            return 0;
        }

        public int? CreatePerson(string name, DateTime bDay)
        {

            int age = Convert.ToInt32((DateTime.Now - bDay).TotalDays) / 365;
            if (name == "" || age < 1 || age > 120)
            {
                return null;
            }
            else
            {
                return InsertPerson(++id_count, name, age);
            }
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
            Get("/", args => 
            {
                
                return "Count: ";
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

                Console.WriteLine("Nancy host start listening on localhost:8080");
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
