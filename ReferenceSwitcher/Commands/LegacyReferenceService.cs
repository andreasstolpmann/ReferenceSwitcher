using System.Collections.Generic;
using System.Threading.Tasks;
using EnvDTE;

namespace ReferenceSwitcher.Commands
{
    public class LegacyReferenceService : IReferenceService
    {
        public Task<ISet<ConfigItem>> GetPackageReferenceNamesAsync(Project project, IList<ConfigItem> config)
        {
            throw new System.NotImplementedException();
        }
    }


}