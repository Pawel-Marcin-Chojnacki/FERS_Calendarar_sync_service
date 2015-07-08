using System.ServiceProcess;

namespace CalendarSyncService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
            { 
                new CalendarSyncService() 
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
