using System;
using System.Diagnostics;
using Nancy;
using Nancy.Hosting.Self;
using Nancy.ModelBinding;
using Serilog;
using Serilog.Sinks.File;
using Querys;
using Topshelf;

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
        PersonRepository qr = new PersonRepository();
       
        public void LogInfo(string str)
        {
            Log.Information(str);
        }
        public void LogError(string str)
        {
            Log.Error(str);
            
        }

        public TestNancy()
        {
            Log.Logger = new LoggerConfiguration().MinimumLevel.Verbose()
                .WriteTo.Console()
                .WriteTo.File("log.txt",rollingInterval:RollingInterval.Day)
                .CreateLogger();
            

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
                        answer = "OK {\"Name\":" + "\""+pr.name + "\"" + ",\"BirthDay\":" + "\""+ pr.bDay+"\"}";
                        LogInfo(answer);
                    }
                    else
                    {
                        answer = "Not found";
                        LogError(answer);
                    }
                }
                else
                {
                    answer = "Invalid ID <" + id.ToString() + ">";
                    LogError(answer);
                }
                return answer;
            });
            Post("/person", args =>
            {
                string answer,log_str;
                JSON_pers jpr = this.Bind<JSON_pers>();
                if (jpr != null)
                {
                    pr = qr.CreatePerson(jpr.name, Convert.ToDateTime(jpr.BirthDay));
                    if (pr != null)
                    {
                        answer = "Create ID:{" + pr.id + "}";
                        log_str = "Create person ID:{" + pr.id + "} name <" + pr.name + "> birthday <" + pr.bDay + "> age <" + pr.age.ToString() + ">";
                        LogInfo(log_str);
                    }
                    else
                    {
                        answer = "Bad request";
                        LogError(answer);
                    }
                }
                else
                    answer = "Bad request";
                return answer;
            });
                        
        }
    }
    class PersonService
    {
        private NancyHost host;

        public void Start()
        {
            host = new NancyHost(new Uri("http://localhost:8000"));
            host.Start();
            Console.WriteLine("Nancy host start listening on localhost:8000");
            Process.Start("http://localhost:8000");
        }
        public void Stop()
        {
            host.Stop();
            Console.WriteLine("Process stopped");
        }
    }


    class Program
    {
        static public void AppStart()
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
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                Console.ReadKey();
            }
            Console.WriteLine("Process stopped");
        }
        static public void ServiceStart()
        {
            HostFactory.Run(x =>
            {
                x.Service<PersonService>(s =>
                {
                    s.ConstructUsing(name => new PersonService());
                    s.WhenStarted(tc => tc.Start());
                    s.WhenStopped(tc => tc.Stop());
                });
                x.RunAsLocalService();
                x.SetDescription("Person service host");
                x.SetServiceName("a_ service name");
            });
        }
        static void Main(string[] args)
        {
            //AppStart();
            ServiceStart();
        }
    }
}
