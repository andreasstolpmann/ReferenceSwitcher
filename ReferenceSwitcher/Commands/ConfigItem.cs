using System;

namespace ReferenceSwitcher.Commands
{
    public class ConfigItem : IEquatable<ConfigItem>
    {
        public string PackageReferenceName { get; }
        public string ProjectReferencePath { get; }

        public ConfigItem(string packageReferenceName, string projectReferencePath)
        {
            PackageReferenceName = packageReferenceName;
            ProjectReferencePath = projectReferencePath;
        }

        public bool Equals(ConfigItem other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(PackageReferenceName, other.PackageReferenceName) && string.Equals(ProjectReferencePath, other.ProjectReferencePath);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((ConfigItem) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((PackageReferenceName != null ? PackageReferenceName.GetHashCode() : 0) * 397) ^ (ProjectReferencePath != null ? ProjectReferencePath.GetHashCode() : 0);
            }
        }
    }
}