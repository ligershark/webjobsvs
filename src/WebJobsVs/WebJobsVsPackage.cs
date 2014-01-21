using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using EnvDTE;
using EnvDTE80;
using LigerShark.WebJobsVs.Dialog;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using NuGet.VisualStudio;

namespace LigerShark.WebJobsVs
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(GuidList.guidWebJobsVsPkgString)]
    [ProvideAutoLoad(UIContextGuids80.SolutionExists)]
    public sealed class WebJobsVsPackage : Package
    {
        private DTE2 _dte;
        private string _webjobsPkgName = "WebJobsBuilder";

        protected override void Initialize()
        {
            base.Initialize();
            _dte = GetService(typeof(DTE)) as DTE2;

            OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (null != mcs)
            {
                CommandID cmdId = new CommandID(GuidList.guidWebJobsVsCmdSet, (int)PkgCmdIDList.WebJobsAddProject);
                OleMenuCommand button = new OleMenuCommand(ButtonClicked, cmdId);
                button.BeforeQueryStatus += button_BeforeQueryStatus;
                mcs.AddCommand(button);
            }
        }

        void button_BeforeQueryStatus(object sender, EventArgs e)
        {
            var button = (OleMenuCommand)sender;
            var project = GetSelectedProjects().ElementAt(0);

            button.Visible = IsWebProject(project);
        }

        public static bool IsWebProject(Project project)
        {
            if (project.Object is VsWebSite.VSWebSite)
                return true;

            try
            {
                var extenderNames = (object[])project.ExtenderNames;
                return extenderNames.Any(extenderNameObject => extenderNameObject.ToString() == "WebApplication");
            }
            catch
            {
                // Ignore 
            }

            return false;
        }

        public static bool HasExtender(Project proj, string extenderName)
        {
            try
            {
                var extenderNames = (object[])proj.ExtenderNames;
                return extenderNames.Length > 0 && extenderNames.Any(extenderNameObject => extenderNameObject.ToString() == extenderName);
            }
            catch
            {
                // Ignore
            }

            return false;
        }

        private void ButtonClicked(object sender, EventArgs e)
        {
            Project currentProject = GetSelectedProjects().ElementAt(0);
            var projects = GetProjectsInSolution();
            var names = from p in projects
                        where p != currentProject
                        select p.Name;

            ProjectSelector2 selector = new ProjectSelector2(names);
            bool? isSelected = selector.ShowDialog();

            if (isSelected.HasValue && isSelected.Value)
            {
                WebJobCreator creator = new WebJobCreator();
                var selectedProject = projects.First(p => p.Name == selector.SelectedProjectName);
                creator.AddReference(currentProject, selectedProject);
                creator.CreateFolders(currentProject, selector.Schedule, selector.SelectedProjectName);

                // ensure that the NuGet package is installed in the project as well
                InstallWebJobsNuGetPackage(currentProject);
            }
        }

        public IEnumerable<Project> GetSelectedProjects()
        {
            var items = (Array)_dte.ToolWindows.SolutionExplorer.SelectedItems;
            foreach (UIHierarchyItem selItem in items)
            {
                var item = selItem.Object as Project;
                if (item != null)
                {
                    yield return item;
                }
            }
        }

        private IEnumerable<Project> GetProjectsInSolution()
        {
            return _dte.Solution.Projects
                .Cast<Project>()
                .SelectMany(GetChildProjects);
        }

        private static IEnumerable<Project> GetChildProjects(Project parent)
        {
            if (parent.Collection == null)  // Unloaded
                return Enumerable.Empty<Project>();

            if (!String.IsNullOrEmpty(parent.FullName))
                return new[] { parent };

            return parent.ProjectItems
                    .Cast<ProjectItem>()
                    .Where(p => p.SubProject != null)
                    .SelectMany(p => GetChildProjects(p.SubProject));
        }

        private bool InstallWebJobsNuGetPackage(EnvDTE.Project project)
        {
            bool installedPkg = false;
            try
            {
                var componentModel = (IComponentModel)GetService(typeof(SComponentModel));
                IVsPackageInstallerServices installerServices = componentModel.GetService<IVsPackageInstallerServices>();
                if (!installerServices.IsPackageInstalled(project, _webjobsPkgName))
                {
                    _dte.StatusBar.Text = @"Installing WebJobs NuGet package, this may take a minute...";

                    IVsPackageInstaller installer = (IVsPackageInstaller)componentModel.GetService<IVsPackageInstaller>();
                    installer.InstallPackage("All", project, _webjobsPkgName, (System.Version)null, false);

                    _dte.StatusBar.Text = @"Finished installing WebJobs NuGet package";
                }
            }
            catch (Exception ex)
            {
                installedPkg = false;
                LogMessageWriteLineFormat(
                    "Unable to install [{0}] NuGet package. Error: {1}",
                    _webjobsPkgName,
                    ex.ToString());
            }

            return installedPkg;
        }

        private void LogMessageWriteLineFormat(string message, params object[] args)
        {
            if (string.IsNullOrWhiteSpace(message)) { return; }

            string fullMessage = string.Format(message, args);
            Trace.WriteLine(fullMessage);
            Debug.WriteLine(fullMessage);

            IVsActivityLog log = GetService(typeof(SVsActivityLog)) as IVsActivityLog;
            if (log == null) return;

            int hr = log.LogEntry(
                (UInt32)__ACTIVITYLOG_ENTRYTYPE.ALE_INFORMATION,
                this.ToString(),
                fullMessage);
        }
    }
}