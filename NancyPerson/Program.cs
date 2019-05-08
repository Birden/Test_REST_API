﻿using System;
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
using Dapper;
using Serilog;
using Serilog.Sinks.SystemConsole;
//using Querys;

namespace NancyPerson
{
    public class Person
    {
        public string id;
        public string name { set; get; }
        public int age { set; get; }
        public string bDay { set; get; }

        public int? Create(string name, DateTime bDay)
        {
            int age = Convert.ToInt32((DateTime.Now - bDay).TotalDays) / 365;
            if (name == "" || age < 1 || age > 120)
            {
                return null;
            } else
            {
                this.id = Guid.NewGuid().ToString();
                this.name = name;
                this.bDay = bDay.ToString("dd:MM:yyy");
                this.age = age;
                return 0;
            }
        }
    }

    public interface IPersonRepository
    {
        int? Insert(Person pr);
        Person Find(Guid id);
    }

    public class PersonRepository : IPersonRepository
    {
        static string db_name = "person_db.sqlite";
        static SQLiteConnection sqlConn = new SQLiteConnection();
        static SQLiteCommand sqlCmd = new SQLiteCommand();
        static bool db_connected = false;
        

        public void LoggerConfig()
        {
            Log.Logger = new LoggerConfiguration().MinimumLevel.Verbose().WriteTo.Console().CreateLogger();
        }
        public void LogInfo(string str)
        {
            Log.Information(str);
        }
        public void LogError(string str)
        {
            Log.Error(str);
        }

        private int? ConnectDB()
        {
            
            if (!File.Exists(db_name))
            {
                SQLiteConnection.CreateFile(db_name);
                //Console.WriteLine("File for DB created");
                LogInfo("File for DB created");
            }
            try
            {
                sqlConn = new SQLiteConnection("Data source=" + db_name + ";Version=3;");
                sqlConn.Open();
                sqlCmd.Connection = sqlConn;

                sqlCmd.CommandText = "CREATE TABLE IF NOT EXISTS Persons (id TEXT, name TEXT, bDay TEXT, age INTEGER);";
                if (sqlCmd.ExecuteNonQuery() != 0)
                {
                    db_connected = true;
                    //Console.WriteLine("Table created");
                    LogInfo("Table created");
                }
                else
                {
                    //Console.WriteLine("Table not created");
                    LogError("Table not created");
                }
            }
            catch (SQLiteException ex)
            {
                //Console.WriteLine("Error:" + ex.Message);
                LogError("Error:" + ex.Message);
                return null;
            }
            return 0;
        }

        public int? Insert(Person pr)
        {
            int? res = 0;
            if (sqlConn == null)
                ConnectDB();
            string sqlQuery = "INSERT INTO Persons (id,name,bDay,age) values ('" + pr.id + "','" + pr.name + "','" + pr.bDay + "'," + pr.age.ToString() + ");";
            var qr = sqlConn.Query<Person>(sqlQuery);
            if (qr != null)
            {
                //Console.WriteLine("Person " + pr.name + " inserted");
                //LogInfo("Person " + pr.name + " inserted");
            }
            else
                res = null;
            return res;
        }
        public Person Find(Guid id)
        {
            Person pr = new Person();
            if (sqlConn == null)
                ConnectDB();
            string sqlQuery = "SELECT * FROM Persons WHERE id = '" + id.ToString() + "';";
            var res = sqlConn.Query<Person>(sqlQuery);
            if (res.Count() != 0)
            {
                pr = res.First();
//                Console.WriteLine("Find person " + pr.name);
            } else
            {
//                Console.WriteLine("Person with ID<" + id.ToString() + "> not found");
            }
            return pr;
        }

        public string ListTable()
        {
            string pList = "";
            int count = 0;
            if (!db_connected)
            {
                ConnectDB();
            }
            try
            {
                string sqlQuery = "SELECT * FROM Persons;";
                var res = sqlConn.Query<Person>(sqlQuery);
                if (res.Count() > 0)
                {
                    foreach (Person pr in res)
                    {
                        string str = (++count).ToString() + " : " + pr.id + " : " + pr.name + " : " + pr.bDay;
                        pList += str + "\n";
                    }
                }
                else
                    pList = "Empty\n";
            } catch (SQLiteException ex)
            {
                //Console.WriteLine("List error: " + ex.Message);
                LogError("List error: " + ex.Message);
                pList = ex.Message;
            }
            return pList;
        }
        public Person CreatePerson(string name, DateTime bDay)
        {
            Person pr = new Person();
            if ((pr.Create(name, bDay) != null) && (Insert(pr) != null))
            {
                return pr;
            }
            else
                return null;
            
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
        PersonRepository qr = new PersonRepository();
       

        public TestNancy()
        {
            //var logger = new LoggerConfiguration().MinimumLevel.Verbose().WriteTo.Console().CreateLogger();
            qr.LoggerConfig();

            Get("/", args =>
            {
                string answer = "Person list:\n";
                answer += qr.ListTable();
                return answer;
            });
            Get("/person/{id}", args => 
            {
                string answer = "";

                if (Guid.TryParse(args.id, out Guid id))
                {
                    pr = qr.Find(args.id);
                    if (pr.name != null)
                    {
                        answer = "Found: " + pr.name + " " + pr.bDay;
                        qr.LogInfo(answer);
                    }
                    else
                    {
                        answer = "Not found";
                        qr.LogError(answer);
                    }
                }
                else
                {
                    answer = "Invalid ID <" + id.ToString() + ">";
                    qr.LogError(answer);
                }
                return answer;
            });
            Post("/person", args =>
            {
                string answer,log_str;
                JSON_pers jpr = this.Bind<JSON_pers>();
                pr = qr.CreatePerson(jpr.name, Convert.ToDateTime(jpr.BirthDay));
                if (pr != null)
                {
                    answer = "Create ID:{" + pr.id + "}";
                    log_str = "Create person ID:{" + pr.id + "} name <" + pr.name + "> birthday <" + pr.bDay + "> age <" + pr.age.ToString() + ">";
                    qr.LogInfo(log_str);
                } else
                {
                    answer = "Bad request";
                    qr.LogError(answer);
                }
                return answer;
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
