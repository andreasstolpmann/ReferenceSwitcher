using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ProjectSystem.Properties;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Newtonsoft.Json;
using Task = System.Threading.Tasks.Task;

namespace ReferenceSwitcher
{
    internal sealed class Command
    {
        public const int CommandId = 0x0100;

        public static Command Instance { get; private set; }

        public static async Task InitializeAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            var dte = await package.GetServiceAsync(typeof(DTE)) as DTE2;
            var commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;

            Instance = new Command(dte, package);

            var menuCommandId = new CommandID(PackageGuids.guidReferenceSwitcherPackageCmdSet, CommandId);
            var menuItem = new MenuCommand(Instance.Execute, menuCommandId);
            commandService?.AddCommand(menuItem);
        }

        private DTE2 Dte { get; }
        private AsyncPackage AsyncPackage { get; }

        private Command(DTE2 dte, AsyncPackage package)
        {
            Dte = dte ?? throw new ArgumentNullException(nameof(dte));
            AsyncPackage = package ?? throw new ArgumentNullException(nameof(package));
        }

        private async void Execute(object sender, EventArgs e)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            if (!CheckSolutionState())
                return;

            if (!(await AsyncPackage.GetServiceAsync(typeof(SVsSolution)) is IVsSolution solution))
                return;

            if (!ReadConfig(out var config))
                return;

            var projects = Dte.Solution.Projects.OfType<Project>().ToList();
            var toBeAddedToSolution = new HashSet<(string projectName, string projectReference)>();
            foreach (var project in projects)
            {
                await HandleProjectAsync(project, config, toBeAddedToSolution);
            }

            foreach (var (projectName, projectReference) in toBeAddedToSolution.Where(x => projects.All(y => y.FullName != x.projectReference)))
            {
                AddProjectToSolution(solution, projectName, projectReference);
            }
        }

        private bool ReadConfig(out Dictionary<string, string> config)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            config = null;

            var dialog = new OpenFileDialog
            {
                InitialDirectory = Path.GetDirectoryName(Dte.Solution.FileName),
                Filter = "Json Files (*.json)|*.json"
            };

            if (dialog.ShowDialog() != DialogResult.OK)
                return false;

            try
            {
                var text = File.ReadAllText(dialog.FileName);
                config = JsonConvert.DeserializeObject<Dictionary<string, string>>(text);
                return true;
            }
            catch
            {
                MessageBox.Show($"Could not read file {dialog.FileName}!",
                    "Fail", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private async Task HandleProjectAsync(Project project,
                                              Dictionary<string, string> config,
                                              HashSet<(string projectName, string projectReference)> toBeAddedToSolution)
        {
            // ReSharper disable once SuspiciousTypeConversion.Global
            if (!(project is IVsBrowseObjectContext browseObjectContext))
                return;

            var configuredProject = await browseObjectContext.UnconfiguredProject.GetSuggestedConfiguredProjectAsync();

            //var resolvedReferences = await configuredProject.Services.PackageReferences.GetResolvedReferencesAsync();
            var unResolvedReferences = await configuredProject.Services.PackageReferences.GetUnresolvedReferencesAsync();

            var references = unResolvedReferences.Select(x => x.EvaluatedInclude);

            foreach (var packageReference in references)
            {
                if (!config.ContainsKey(packageReference))
                    continue;
                var projectReference = config[packageReference];

                await configuredProject.Services.PackageReferences.RemoveAsync(packageReference);
                await configuredProject.Services.ProjectReferences.AddAsync(projectReference);

                toBeAddedToSolution.Add((packageReference, projectReference));
            }
        }

        private void AddProjectToSolution(IVsSolution solution, string projectName, string projectReference)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var projectType = Guid.Empty;
            var projectId = Guid.Empty;
            var result = solution.CreateProject(ref projectType, projectReference, projectReference, projectName,
                (uint) (__VSCREATEPROJFLAGS.CPF_OPENFILE | __VSCREATEPROJFLAGS.CPF_SILENT), ref projectId, out _);

            if (result != VSConstants.S_OK)
            {
                MessageBox.Show($"Failed to add project {projectReference} to solution. (CreateProject)",
                    "Fail", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool CheckSolutionState()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (Dte.Solution == null || !Dte.Solution.IsOpen)
            {
                MessageBox.Show("Please open a solution first. ", "No solution");
                return false;
            }

            if (Dte.Solution.IsDirty) // solution must be saved otherwise adding/removing projects will raise errors
            {
                MessageBox.Show("Please save your solution first. \n" +
                                "Select the solution in the Solution Explorer and press Ctrl-S. ",
                    "Solution not saved");
                return false;
            }

            if (Dte.Solution.Projects.OfType<Project>().Any(p => p.IsDirty))
            {
                MessageBox.Show("Please save your projects first. \n" +
                                "Select the project in the Solution Explorer and press Ctrl-S. ",
                    "Project not saved");
                return false;
            }

            return true;
        }
    }
}
