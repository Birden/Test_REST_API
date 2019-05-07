using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy;
using Nancy.Hosting.Self;
using Nancy.ModelBinding;
using Querys;

namespace NancyPerson
{

    


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
