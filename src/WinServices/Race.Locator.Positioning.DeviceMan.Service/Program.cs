using System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceProcess;
using System.IO;
using System.Reflection;
using NLog;

namespace Race.Locator.Positioning.DeviceMan.Service
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            string servicePath = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            NLog.LogManager.Configuration = new NLog.Config.XmlLoggingConfiguration(Path.Combine(servicePath, "Config/nlog.config"), true);

            if (args.Length > 0 && args[0].ToLower() == "debug")
            {
                DeviceManService service = new DeviceManService();
                service.Start(args);
                Console.ReadLine();
                service.Stop();
            }
            else
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[] {  new DeviceManService() };
                ServiceBase.Run(ServicesToRun);
            } 
        }
    }
}
