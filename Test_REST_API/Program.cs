using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using Nancy;

namespace Test_REST_API
{
    class Program
    {
        class Person
        {
            public string name;
            public int age;
        }

        private static List<Person> pList = new List<Person>();

        static int? CreatePerson(string name, DateTime bornDate)
        {
            int age = Convert.ToInt32((DateTime.Now - bornDate).TotalDays / 365);
            if ((name == "") || (age < 0) || (age > 120))
            {
                Console.WriteLine("invalid person");
                return null;
            }
            else
            {
                Person pr = new Person();
                pr.name = name;
                pr.age = age;
                pList.Add(pr);

                Console.WriteLine("Create person <" + name + "> with age " + age.ToString());
                return 0;
            }
        }
        //static Person Find()
        //{

        //}

        static void PersonList()
        {
            foreach (Person pr in pList)
                Console.WriteLine(pr.name + " " + pr.age.ToString());
        }

        static void Main(string[] args)
        {
            CreatePerson("Alex", Convert.ToDateTime("01.01.1980"));
            CreatePerson("Tom", Convert.ToDateTime("01.01.1927"));
            CreatePerson("Bob", Convert.ToDateTime("01.01.1870"));
            CreatePerson("Hank", Convert.ToDateTime("01.01.2010"));
            PersonList();
            Console.ReadKey();
        }
    }
}
