using System;
using System.Collections.Generic;

namespace ScheduledTaskManager.Services
{
    public interface IScheduledTaskPluginManagerService
    {
        ICollection<ScheduledTaskConfig> GetScheduledTaskConfigs();

        IScheduledTask GetScheduledTaskFromConfig(ScheduledTaskConfig config);

        event EventHandler ScheduledTasksChanged;
    }
}
