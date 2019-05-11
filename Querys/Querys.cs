using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using Dapper;

namespace Querys
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
            }
            else
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

        
        private int? ConnectDB()
        {
            int? res = 0;
            if (!File.Exists(db_name))
            {
                SQLiteConnection.CreateFile(db_name);
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
                }
                else
                {
                    res = null;
                }
            }
            catch (SQLiteException ex)
            {
                return null;
            }
            return res;
        }

        //      Вставляет запись в таблицу
        //      возвращает 0 в случае успеха
        //      либо null
        //
        public int? Insert(Person pr)
        {
            int? res = 0;
            if (sqlConn == null)
                ConnectDB();
            string sqlQuery = "INSERT INTO Persons (id,name,bDay,age) values ('" + pr.id + "','" + pr.name + "','" + pr.bDay + "'," + pr.age.ToString() + ");";
            var qr = sqlConn.Query<Person>(sqlQuery);
            if (qr == null)
            {
                res = null;
            }
            return res;
        }
        //      Ищем запись в базе по ID
        //      возвращает Person, либо null
        //
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
            }
            return pr;
        }
        //      Для тестирования, возвращает все записи в таблице, 
        //      либо "Empty", если таблица пустая
        //
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
            }
            catch (SQLiteException ex)
            {
                pList = ex.Message;
            }
            return pList;
        }
        //      Создание сущности <Person> в таблице, по имени и дате рождения
        //      name д.б. не пустой и возраст не д.б. > 120лет
        //      возвращает <Person>, либо null
        //
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
}
