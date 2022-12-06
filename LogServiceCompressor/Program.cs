using System;
using System.Configuration;
using Topshelf;

namespace LogFilesServiceCompressor
{
    class Program
    {
        public static void Main()
        {   
            LogHelper.Info("Start...");
            LogHelper.Info("Getting Net Framework version");
            Console.WriteLine("Getting Net Framework version");
            Utilities.ReadNetFrameworkVersion();

            string _windowsServiceInstanceName = "";
            string _windowsServiceName = "";
            string _windowsServiceDisplayName = "";
            string _windowsServiceDescription = "";


            // Read the service parameters
            // Not in use since it cause issues trying to change the process name
            if (ConfigurationManager.AppSettings.Get("WindowsServiceInstanceName") != null && !ConfigurationManager.AppSettings.Get("WindowsServiceInstanceName").ToString().Equals(""))
                _windowsServiceInstanceName = ConfigurationManager.AppSettings.Get("WindowsServiceInstanceName").ToString();

            if (ConfigurationManager.AppSettings.Get("WindowsServiceName") != null && !ConfigurationManager.AppSettings.Get("WindowsServiceName").ToString().Equals(""))
                _windowsServiceName = ConfigurationManager.AppSettings.Get("WindowsServiceName").ToString();

            if (ConfigurationManager.AppSettings.Get("WindowsServiceDisplayName") != null && !ConfigurationManager.AppSettings.Get("WindowsServiceDisplayName").ToString().Equals(""))
                _windowsServiceDisplayName = ConfigurationManager.AppSettings.Get("WindowsServiceDisplayName").ToString();

            if (ConfigurationManager.AppSettings.Get("WindowsServiceDescription") != null && !ConfigurationManager.AppSettings.Get("WindowsServiceDescription").ToString().Equals(""))
                _windowsServiceDescription = ConfigurationManager.AppSettings.Get("WindowsServiceDescription").ToString();

            var exitCode = HostFactory.Run(x =>
            {
                x.Service<ScheduleService>(s =>
                {
                    s.ConstructUsing(name => new ScheduleService());
                    s.WhenStarted(tc => tc.Start());
                    s.WhenStopped(tc => tc.Stop());
                    LogHelper.Info("Started Service Setup");
                    Console.WriteLine("Started Service Setup");
                });
               
                
                x.RunAsLocalSystem();
                

                if (
                //!string.IsNullOrEmpty(_windowsServiceInstanceName) &&
                !string.IsNullOrEmpty(_windowsServiceName) &&
                !string.IsNullOrEmpty(_windowsServiceDisplayName) &&
                !string.IsNullOrEmpty(_windowsServiceDescription)
                )
                {
                    LogHelper.Info("Setting Up service name custom");
                    Console.WriteLine("Setting Up service name custom");
                    x.SetDescription(_windowsServiceDescription);
                    x.SetDisplayName(_windowsServiceDisplayName);
                    x.SetServiceName(_windowsServiceName);
                    // Process name always is the same: LogFilesServiceCompressor or the namespace
                    //x.SetInstanceName(_windowsServiceInstanceName);

                }
                else {
                    LogHelper.Info("Setting Up service Name default");
                    Console.WriteLine("Setting Up service name custom");
                    x.SetDescription("Log Archiving");
                    x.SetDisplayName("Log Files Compressor");
                    x.SetServiceName("LogFilesServiceCompressor");
                }
                LogHelper.Info("Finished Service Setup");
                Console.WriteLine("Finished Service Setup");

            });

            var exitCodeValue = (int)Convert.ChangeType(exitCode, exitCode.GetTypeCode());
            Environment.ExitCode = exitCodeValue;
        }
        //private class ConsoleLogProvider : ILogProvider
        //{
        //    public Logger GetLogger(string name)
        //    {
        //        return (level, func, exception, parameters) =>
        //        {
        //            if (level >= LogLevel.Info && func != null)
        //            {
        //                Console.WriteLine("LogProvider [" + DateTime.Now.ToLongTimeString() + "] [" + level + "] " + func(), parameters);
        //            }
        //            return true;
        //        };
        //    }

        //    public IDisposable OpenMappedContext(string key, object value, bool destructure = false)
        //    {
        //        throw new NotImplementedException();
        //    }

        //    public IDisposable OpenNestedContext(string message)
        //    {
        //        throw new NotImplementedException();
        //    }
        //}
    }
}
