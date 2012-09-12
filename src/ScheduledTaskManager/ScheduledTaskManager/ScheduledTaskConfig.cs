using System.IO;
using System.Xml;

namespace ScheduledTaskManager
{
    public class ScheduledTaskConfig
    {
        #region Properties

        public string FullTypeName { get; private set; }

        public string AssemblyName { get; private set; }

        public string FolderName { get; private set; }

        public string CronExpression { get; private set; }

        public string AssemblyFullPath
        {
            get { return Path.GetFullPath(Path.Combine(FolderName, AssemblyName)); }
        }

        #endregion

        #region Constructors

        public ScheduledTaskConfig(XmlNode node)
        {
            var fullTypeNameNode = node["FullTypeName"];
            var assemblyNameNode = node["AssemblyName"];
            var folderNameNode = node["FolderName"];
            var cronExpressionNode = node["CronExpression"];

            FullTypeName = fullTypeNameNode == null ? null : fullTypeNameNode.InnerText;
            AssemblyName = assemblyNameNode == null ? null : assemblyNameNode.InnerText;
            FolderName = folderNameNode == null ? null : folderNameNode.InnerText;
            CronExpression = cronExpressionNode == null ? null : cronExpressionNode.InnerText;
        }

        #endregion

        #region Object Overrides

        public override bool Equals(object obj)
        {
            if (obj == null) return false;

            var config = obj as ScheduledTaskConfig;

            if (config == null) return false;

            return config.FullTypeName == FullTypeName &&
                   config.AssemblyName == AssemblyName &&
                   config.FolderName == FolderName &&
                   config.CronExpression == CronExpression;
        }

        public override int GetHashCode()
        {
            return (FullTypeName ?? "").GetHashCode() ^
                   (AssemblyName ?? "").GetHashCode() ^
                   (FolderName ?? "").GetHashCode() ^
                   (CronExpression ?? "").GetHashCode();
        }

        #endregion
    }
}
