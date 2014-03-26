using System.IO;
using EnvDTE;
using System.Xml;
using System.Xml.Linq;

namespace LigerShark.WebJobsVs
{
    class WebJobCreator
    {
        public void AddReference(Project currentProject, Project selectedProject)
        {
            if (currentProject.Object is VSLangProj.VSProject)
            {
                var project = currentProject.Object as VSLangProj.VSProject;
                project.References.AddProject(selectedProject);
            }
            else if (currentProject.Object is VsWebSite.VSWebSite)
            {
                var project = currentProject.Object as VsWebSite.VSWebSite;
                try
                {
                    project.References.AddFromProject(selectedProject);
                }
                catch
                {
                    // Reference was already added. TODO: don't use try/catch for this.
                }
            }
        }

        public void CreateFolders(Project currentProject, string schedule, string projectName, string relativePath)
        {
            //Create a File under Properties Folder which will contain information about all WebJobs
            //https://github.com/ligershark/webjobsvs/issues/6

            // Check if the WebApp is C# or VB
            string dir = GetProjectDirectory(currentProject);
            var propertiesFolderName = "Properties";
            if (currentProject.CodeModel.Language == CodeModelLanguageConstants.vsCMLanguageVB)
            {
                propertiesFolderName = "My Project";
            }

            DirectoryInfo info = new DirectoryInfo(Path.Combine(dir, propertiesFolderName));

            string readmeFile = Path.Combine(info.FullName, "WebJobs.xml");

            // Copy File if it does not exit
            if (!File.Exists(readmeFile))
                AddReadMe(readmeFile);

            //Add a WebJob info to it
            XDocument doc = XDocument.Load(readmeFile);
            XElement root = new XElement("WebJob");
            root.Add(new XAttribute("Project", projectName));
            root.Add(new XAttribute("RelativePath", relativePath));
            root.Add(new XAttribute("Schedule", schedule));
            doc.Element("WebJobs").Add(root);
            doc.Save(readmeFile);
            currentProject.ProjectItems.AddFromFile(readmeFile);
        }

        private static string GetProjectDirectory(Project project)
        {
            if (project.Object is VSLangProj.VSProject)
                return Path.GetDirectoryName(project.FullName);

            return project.Properties.Item("fullPath").Value.ToString();
        }

        private static void AddReadMe(string destinationFileName)
        {
            string dir = Path.GetDirectoryName(typeof(WebJobCreator).Assembly.Location);
            string readme = Path.Combine(dir, "job", Path.GetFileName(destinationFileName));

            File.Copy(readme, destinationFileName, true);
        }
    }
}
