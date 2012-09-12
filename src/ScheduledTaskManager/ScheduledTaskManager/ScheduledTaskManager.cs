using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NCrontab;
using ScheduledTaskManager.Extensions;
using ScheduledTaskManager.Services;

namespace ScheduledTaskManager
{
    public class ScheduledTaskManager
    {
        #region Constants

        private const int StopTimeoutInMinutes = 10;

        #endregion

        #region Fields

        private readonly IScheduledTaskPluginManagerService _pluginManagerService;

        private bool _isStopping;
        private Timer _timer;

        private readonly ICollection<string> _queuedOrRunningTasks = new HashSet<string>();
        private readonly object _queuedOrRunningTasksLock = new object();

        private readonly IList<ScheduledTaskTracker> _trackers = new List<ScheduledTaskTracker>();
        private readonly object _trackersLock = new object();

        #endregion

        #region Constructors

        public ScheduledTaskManager(IScheduledTaskPluginManagerService pluginManagerService)
        {
            _pluginManagerService = pluginManagerService;
        }

        #endregion

        #region Methods

        public void Start()
        {
            LoadScheduledTaskTrackers();

            _pluginManagerService.ScheduledTasksChanged += OnScheduledTasksChanged;

            InitTimer();
        }

        public void Stop()
        {
            _isStopping = true;

            _timer.Change(Timeout.Infinite, Timeout.Infinite);

            var stopTimeoutTime = DateTime.Now.AddMinutes(StopTimeoutInMinutes);

            // Wait for the remaining queued/running tasks to finish
            while (_queuedOrRunningTasks.Count > 0)
            {
                if (stopTimeoutTime >= DateTime.Now)
                {
                    // We haven't reached the timeout time, keep waiting
                    Thread.Sleep(1000);
                }
                else
                {
                    // We've passed the timeout time, give ups
                    // TODO: Log this event
                    break;
                }
            }
        }

        private void LoadScheduledTaskTrackers()
        {
            lock (_trackersLock)
            {
                var configs = _pluginManagerService.GetScheduledTaskConfigs();

                // Remove trackers that are no longer valid
                var trackersToRemove = _trackers.Where(tracker => configs.All(x => !x.Equals(tracker.Config)));

                foreach (var tracker in trackersToRemove)
                {
                    _trackers.Remove(tracker);
                }

                // Add new trackers
                var configsToAdd = configs.Where(config => _trackers.All(x => !x.Config.Equals(config)));

                foreach (var config in configsToAdd)
                {
                    _trackers.Add(new ScheduledTaskTracker(config));
                }
            }
        }

        private void OnScheduledTasksChanged(object sender, EventArgs eventArgs)
        {
            LoadScheduledTaskTrackers();
        }

        private void InitTimer()
        {
            const int ticksInMinute = 60000;

            // Start the timer at the start of the next minute
            var startInSeconds = 60 - DateTime.Now.Second;
            var startInTicks = startInSeconds * 1000;

            if (_timer == null)
            {
                // Create a new timer that triggers every minute
                _timer = new Timer(TimerCallback, null, startInTicks, ticksInMinute);
            }
            else
            {
                // Reset the start time and period on an existing timer
                _timer.Change(startInTicks, ticksInMinute);
            }
        }

        private void TimerCallback(object obj)
        {
            if (_isStopping) return;

            lock (_trackersLock)
            {
                foreach (var tracker in _trackers)
                {
                    QueueTask(tracker);
                }
            }
        }

        private void QueueTask(ScheduledTaskTracker tracker)
        {
            var schedule = CrontabSchedule.Parse(tracker.Config.CronExpression);

            var startTime = DateTime.Now.WithoutSeconds();
            var lastRunTime = tracker.LastStart.GetValueOrDefault(startTime.AddMinutes(-1)).WithoutSeconds();

            var nextOccurenceTime = schedule.GetNextOccurrence(lastRunTime);

            if (startTime < nextOccurenceTime) return;

            lock (_queuedOrRunningTasksLock)
            {
                // Make sure the task isn't already queued
                if (_queuedOrRunningTasks.Contains(tracker.Config.FullTypeName)) return;

                _queuedOrRunningTasks.Add(tracker.Config.FullTypeName);

                Task.Factory
                    .StartNew(() =>
                    {
                        tracker.LastStart = DateTime.Now;

                        RunTask(tracker);

                        tracker.LastEnd = DateTime.Now;

                        lock (_queuedOrRunningTasksLock)
                        {
                            _queuedOrRunningTasks.Remove(tracker.Config.FullTypeName);
                        }
                    });
            }
        }

        private void RunTask(ScheduledTaskTracker tracker)
        {
            if (_isStopping) return;

            var scheduledTask = _pluginManagerService.GetScheduledTaskFromConfig(tracker.Config);

            if (scheduledTask == null)
            {
                throw new InvalidOperationException(
                    string.Format("Scheduled Task not found with type name = " + tracker.Config.FullTypeName));
            }

            scheduledTask.Execute();
        }

        #endregion
    }
}
