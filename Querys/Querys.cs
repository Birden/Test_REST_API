using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;

namespace Querys
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
            } catch (SQLiteException ex)
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
            } else
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
            
            int age = Convert.ToInt32(DateTime.Now - bDay)/365;
            if (name == "" || age < 1 || age > 120)
            {
                return null;
            } else
            {
                return InsertPerson(++id_count, name, age);
            }
        }
    }
}
