using System.Collections.Generic;
using System.Threading.Tasks;
using EnvDTE;

namespace ReferenceSwitcher.Commands
{
    public interface IReferenceService
    {
        Task<ISet<ConfigItem>> GetPackageReferenceNamesAsync(Project project, IList<ConfigItem> config);
    }
}