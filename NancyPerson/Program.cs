using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy;
using Nancy.Hosting.Self;
using Nancy.ModelBinding;

namespace NancyPerson
{
    

    public class Person
    {
        public int id { get; set; }
        public string name { get; set; }
        public int age { get; set; }
    }
    public class JSON_pers
    {
        public string name;
        public string BirthDay;
    }

    public class TestNancy : NancyModule
    {
        static List<Person> pList = new List<Person>();
        static int lcnt = 0;

        public TestNancy()
        {
            Get("/", args => 
            {
                
                return "Count: " + pList.Count().ToString();
            });
            Post("/person", args =>
            {
                JSON_pers jpr = this.Bind<JSON_pers>();
                int age = Convert.ToInt32((DateTime.Now - Convert.ToDateTime(jpr.BirthDay)).TotalDays / 365);

                if (jpr.name == "")
                {
                    return "Badrequest (invalid name)";
                } else if (age < 1 || age > 120 )
                {
                    return "Badrequest (invalid birthday)";
                } else 
                {
                    Person pr = new Person()
                    {
                        id = ++lcnt,
                        name = jpr.name,
                        age = age
                    };
                    pList.Add(pr);
                    Console.WriteLine("Binding: id " + pr.id + " " + pr.name + " " + pr.age.ToString());
                    return "Created:  " + pr.name + " age " + pr.age.ToString();
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
