using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using EnvDTE;
using Microsoft.VisualStudio.Shell;

namespace ReferenceSwitcher.Commands
{
    public class LegacyReferenceService : IReferenceService
    {
        public async Task<ISet<ConfigItem>> GetPackageReferenceNamesAsync(Project project, IList<ConfigItem> config)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var toBeAddedToSolution = new HashSet<ConfigItem>();

            var doc = new XmlDocument();
            doc.Load(project.FullName);

            var packageReferenceElements = doc.GetElementsByTagName("PackageReference");
            foreach (var packageReferenceElement in packageReferenceElements.Cast<XmlElement>().ToList())
            {
                var packageReferenceName = packageReferenceElement.Attributes?.Cast<XmlAttribute>().FirstOrDefault(x =>
                    string.Equals(x.Name, "Include", StringComparison.OrdinalIgnoreCase))?.Value;

                var configItem = config.SingleOrDefault(x => x.PackageReferenceName == packageReferenceName);
                if (configItem == null)
                    continue;

                var projectReferenceElement = doc.CreateElement("ProjectReference", packageReferenceElement.NamespaceURI);
                projectReferenceElement.SetAttribute("Include", configItem.ProjectReferencePath);

                packageReferenceElement.ParentNode?.ReplaceChild(projectReferenceElement, packageReferenceElement);

                toBeAddedToSolution.Add(configItem);
            }

            doc.Save(project.FullName);
            return toBeAddedToSolution;
        }
    }


}