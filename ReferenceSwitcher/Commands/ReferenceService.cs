using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EnvDTE;
using Microsoft.VisualStudio.ProjectSystem.Properties;

namespace ReferenceSwitcher.Commands
{
    public class ReferenceService : IReferenceService
    {
        public async Task<ISet<ConfigItem>> GetPackageReferenceNamesAsync(Project project, IList<ConfigItem> config)
        {
            var toBeAddedToSolution = new HashSet<ConfigItem>();

            // ReSharper disable once SuspiciousTypeConversion.Global
            var configuredProject = await ((IVsBrowseObjectContext)project).UnconfiguredProject.GetSuggestedConfiguredProjectAsync();

            //var resolvedReferences = await configuredProject.Services.PackageReferences.GetResolvedReferencesAsync();
            var unResolvedReferences = await configuredProject.Services.PackageReferences.GetUnresolvedReferencesAsync();

            var references = unResolvedReferences.Select(x => x.EvaluatedInclude);

            foreach (var packageReference in references)
            {
                var configItem = config.SingleOrDefault(x => x.PackageReferenceName == packageReference);
                if (configItem == null)
                    continue;

                await configuredProject.Services.PackageReferences.RemoveAsync(configItem.PackageReferenceName);
                await configuredProject.Services.ProjectReferences.AddAsync(configItem.ProjectReferencePath);

                toBeAddedToSolution.Add(configItem);
            }

            return toBeAddedToSolution;
        }
    }
}