using System;
using System.Collections.Generic;

namespace ScheduledTaskManager.Services
{
    public interface IScheduledTaskConfigService
    {
        ICollection<ScheduledTaskConfig> GetConfigs();

        event EventHandler ConfigsChanged;
    }
}
