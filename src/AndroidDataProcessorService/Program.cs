using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Security.Principal;

namespace AndroidDataProcessorService
{
    static class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine($"Current Date and Time (UTC - YYYY-MM-DD HH:mm:ss formatted): {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");

            try
            {
                string currentUser = WindowsIdentity.GetCurrent().Name;
                Console.WriteLine($"Current User's Login: {currentUser}");
            }
            catch
            {
                Console.WriteLine("Could not determine current user");
            }

            Console.WriteLine("To run as a service, install and start using Windows Service Manager.");
            bool runAsService = false;

            if (args != null && args.Length > 0 && args[0].ToLower() == "-service")
            {
                runAsService = true;
            }

            if (!runAsService && Environment.UserInteractive)
            {
                Console.WriteLine("Starting Android Data Processor in console mode...");

                try
                {
                    AndroidDataService service = new AndroidDataService();
                    service.StartService();  // Use the public wrapper method

                    Console.WriteLine("Service is running in debug mode.");
                    Console.WriteLine("The service is listening for Android app connections.");
                    Console.WriteLine("Press Enter to stop the service...");

                    Console.ReadLine();

                    service.StopService();  // Use the public wrapper method
                    Console.WriteLine("Service stopped.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR: {ex.Message}");
                    Console.WriteLine(ex.ToString());
                }
            }
            else
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
                    new AndroidDataService()
                };
                ServiceBase.Run(ServicesToRun);
            }
        }
    }
}