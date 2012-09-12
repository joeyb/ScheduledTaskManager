using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ScheduledTaskManager.Services
{
    public class DefaultScheduledTaskPluginManagerService : IScheduledTaskPluginManagerService
    {
        #region Fields

        private readonly IScheduledTaskConfigService _configService;
        private readonly List<ScheduledTaskConfig> _configs;

        #endregion

        #region Constructors

        public DefaultScheduledTaskPluginManagerService(IScheduledTaskConfigService configService)
        {
            _configService = configService;
            _configService.ConfigsChanged += OnConfigsChanged;

            _configs = new List<ScheduledTaskConfig>();
            _configs.AddRange(_configService.GetConfigs());
        }

        #endregion

        #region IScheduledTaskPluginManagerService Implementation

        public ICollection<ScheduledTaskConfig> GetScheduledTaskConfigs()
        {
            return _configs;
        }

        public IScheduledTask GetScheduledTaskFromConfig(ScheduledTaskConfig config)
        {
            return LoadAssemblyIfNotLoadedAndGetScheduledTask(config.AssemblyFullPath, config.FullTypeName);
        }

        public event EventHandler ScheduledTasksChanged;

        #endregion

        #region Methods

        private void OnConfigsChanged(object sender, EventArgs eventArgs)
        {
            _configs.Clear();
            _configs.AddRange(_configService.GetConfigs());

            if (ScheduledTasksChanged == null) return;

            ScheduledTasksChanged(this, new EventArgs());
        }

        private IScheduledTask LoadAssemblyIfNotLoadedAndGetScheduledTask(string assemblyFullPath, string fullTypeName)
        {
            var foundAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(x => Path.GetFullPath(x.Location) == assemblyFullPath);

            if (foundAssembly != null)
            {
                return GetInstanceFromTypeName(fullTypeName, foundAssembly);
            }

            var newAssembly = Assembly.LoadFrom(assemblyFullPath);

            return GetInstanceFromTypeName(fullTypeName, newAssembly);
        }

        private static IScheduledTask GetInstanceFromTypeName(string fullTypeName, Assembly assembly)
        {
            if (fullTypeName == null) throw new ArgumentNullException("fullTypeName");
            if (assembly == null) throw new ArgumentNullException("assembly");

            var type = assembly.GetType(fullTypeName);

            if (type == null) return null;

            return (IScheduledTask) Activator.CreateInstance(type);
        }

        #endregion
    }
}
