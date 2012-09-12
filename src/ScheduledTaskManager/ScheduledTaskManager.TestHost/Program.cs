using System;

namespace ScheduledTaskManager.TestHost
{
    class Program
    {
        static void Main()
        {
            var configService = new Services.DefaultScheduledTaskConfigService("ScheduledTasks.xml");
            var pluginManagerService = new Services.DefaultScheduledTaskPluginManagerService(configService);
            var scheduledTaskManager = new ScheduledTaskManager(pluginManagerService);

            scheduledTaskManager.Start();

            Console.WriteLine("ScheduledTaskManager is running.");
            Console.WriteLine("Press any key to exit.");

            Console.ReadKey();

            scheduledTaskManager.Stop();
        }
    }
}
