using System;

namespace ScheduledTaskManager
{
    public class ScheduledTaskTracker
    {
        #region Properties

        public ScheduledTaskConfig Config { get; private set; }

        public DateTime? LastStart { get; set; }

        public DateTime? LastEnd { get; set; }

        #endregion

        #region Constructors

        public ScheduledTaskTracker(ScheduledTaskConfig config)
        {
            Config = config;
        }

        #endregion
    }
}
