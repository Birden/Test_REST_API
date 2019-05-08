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
using Dapper;
//using Querys;

namespace NancyPerson
{
    public class Person
    {
        public string id;
        public string name { set; get; }
        public int age { set; get; }
        public DateTime bDay { set; get; }

        public int? Create(string name, DateTime bDay)
        {
            int? res;
            int age = Convert.ToInt32((DateTime.Now - bDay).TotalDays) / 365;
            if (name == "" || age < 1 || age > 120)
            {
                return null;
            } else
            {
                this.id = Guid.NewGuid().ToString();
                this.name = name;
                this.bDay = bDay;
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


        private int? ConnectDB()
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

                sqlCmd.CommandText = "CREATE TABLE IF NOT EXISTS Persons (id TEXT, name TEXT, bDay TEXT, age INTEGER);";
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

        public int? Insert(Person pr)
        {
            int? res = 0;
            if (!db_connected)
            {
                ConnectDB();
            }
            try
            {
                string sqlQuery = "INSERT INTO Persons (id,name,bDay,age) values ('" + pr.id + "','" + pr.name + "','" + pr.bDay+"',"+ pr.age.ToString() + ");";
                Console.WriteLine(sqlQuery);
                sqlCmd.CommandText = sqlQuery;
                if (sqlCmd.ExecuteNonQuery() != 0)
                {
                    Console.WriteLine("Person " + pr.name + " inserted");
                }
                else
                    res = null;
                
            }
            catch (SQLiteException ex)
            {
                Console.WriteLine("Insert error: " + ex.Message);
                res = null;
            }

            return res;
        }

        public int? InsertDapper(Person pr)
        {
            int? res = 0;
            string sqlQuery = "INSERT INTO Persons (id,name,bDay,age) values ('" + pr.id + "','" + pr.name + "','" + pr.bDay + "'," + pr.age.ToString() + ");";
            var qr = sqlConn.Query<Person>(sqlQuery);
            if (qr != null)
            {
                Console.WriteLine("Person " + pr.name + " inserted");
            }
            else
                res = null;
            return res;
        }

        public string ListTable()
        {
            string pList = "";
            DataTable dTable = new DataTable();
            if (!db_connected)
            {
                ConnectDB();
            }
            try
            {
                string sqlQuery = "SELECT * FROM Persons;";
                SQLiteDataAdapter adapter = new SQLiteDataAdapter(sqlQuery, sqlConn);
                adapter.Fill(dTable);
                for (int i = 0; i < dTable.Rows.Count; i++)
                {
                    string str = "";
                    for (int j = 0; j < 4; j++)
                        str += dTable.Rows[i].ItemArray[j].ToString() + " ";
                    pList += str + "\n";
                }
            } catch (SQLiteException ex)
            {
                Console.WriteLine("Insert error: " + ex.Message);
                pList = ex.Message;
            }
            return pList;
        }
        public Person CreatePerson(string name, DateTime bDay)
        {
            Person pr = new Person();
            if ((pr.Create(name, bDay) != null) && (InsertDapper(pr) != null))
            {
                return pr;
            }
            else
                return null;
            
        }
        public Person Find(Guid id)
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
                string sqlQuery = "SELECT * FROM Persons WHERE id = '" + id.ToString() + "';";
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
        PersonRepository qr = new PersonRepository();

        public TestNancy()
        {
            Get("/", args =>
            {
                return qr.ListTable();
            });
            Get("/{id}", args => 
            {
                pr = qr.Find(args.id);
                return "User: " + pr.name + " " + pr.age;
            });
            Post("/person", args =>
            {
                JSON_pers jpr = this.Bind<JSON_pers>();
                string name = jpr.name;
                DateTime bDay = Convert.ToDateTime(jpr.BirthDay);
                pr = qr.CreatePerson(name, bDay.Date);
                if (pr != null)
                {
                    Console.WriteLine("Create ID:{" + pr.id + "}");
                    return "Created";
                } else
                {
                    Console.WriteLine("Bad request");
                    return "Bad request";
                }
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
