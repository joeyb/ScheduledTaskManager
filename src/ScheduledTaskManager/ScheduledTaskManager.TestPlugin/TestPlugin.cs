using System;

namespace ScheduledTaskManager.TestPlugin
{
    public class TestPlugin : IScheduledTask
    {
        public void Execute()
        {
            Console.WriteLine("Test Plugin");
        }
    }
}
