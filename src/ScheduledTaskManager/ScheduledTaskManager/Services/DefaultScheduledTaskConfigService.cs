using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace ScheduledTaskManager.Services
{
    public class DefaultScheduledTaskConfigService : IScheduledTaskConfigService
    {
        #region Fields

        private readonly string _configFile;

        private readonly FileSystemWatcher _configFileSystemWatcher;

        #endregion

        #region Constructors

        public DefaultScheduledTaskConfigService(string configFile)
        {
            if (configFile == null) throw new ArgumentNullException("configFile");

            _configFile = Path.GetFullPath(configFile);

            var configFileDirectory = Path.GetDirectoryName(_configFile);

            _configFileSystemWatcher =
                new FileSystemWatcher(string.IsNullOrWhiteSpace(configFileDirectory) ? "." : configFileDirectory);

            _configFileSystemWatcher.Changed += OnConfigFileChanged;
            _configFileSystemWatcher.Created += OnConfigFileChanged;
            _configFileSystemWatcher.Deleted += OnConfigFileChanged;
            _configFileSystemWatcher.Renamed += OnConfigFileChanged;
        }

        #endregion

        #region IScheduledTaskConfigService Implementation

        public ICollection<ScheduledTaskConfig> GetConfigs()
        {
            var configXml = new XmlDocument();

            try
            {
                configXml.Load(_configFile);
            }
            catch (DirectoryNotFoundException)
            {
                return new List<ScheduledTaskConfig>();
            }
            catch (FileNotFoundException)
            {
                return new List<ScheduledTaskConfig>();
            }

            var nodes = configXml.SelectNodes("/ScheduledTaskConfigs/ScheduledTaskConfig");

            return nodes == null
                       ? new List<ScheduledTaskConfig>()
                       : nodes.Cast<XmlNode>().Select(x => new ScheduledTaskConfig(x)).ToList();
        }

        public event EventHandler ConfigsChanged;

        #endregion

        #region Methods

        private void OnConfigFileChanged(object sender, FileSystemEventArgs e)
        {
            if (e.Name != _configFile) return;
            if (ConfigsChanged == null) return;

            ConfigsChanged(this, new EventArgs());
        }

        #endregion
    }
}
