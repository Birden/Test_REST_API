using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy;

namespace Test_REST_API
{
    class Person
    {
        string name;
        int age;
        //DateTime bornDate;

        public int? CreatePerson(string name, DateTime bornDate)
        {
            int age = Convert.ToInt32((DateTime.Now - bornDate).TotalDays / 365);
            if ((name == "") || (age < 0) || (age > 120))
                return null;
            else
            {
                this.name = name;
                this.age = age;
                Console.WriteLine("Create person <"+name+"> with age " + age.ToString());
            }
            return 0;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Person ps1 = new Person();
            List<Person> pList = new List<Person>();


            ps1.CreatePerson("Alex", Convert.ToDateTime("01.08.1979"));
            Console.ReadKey();
        }
    }
}
