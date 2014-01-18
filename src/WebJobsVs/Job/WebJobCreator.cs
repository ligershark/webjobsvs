using System.IO;
using EnvDTE;

namespace LigerShark.WebJobsVs
{    
    class WebJobCreator
    {
        public void AddReference(Project currentProject, Project selectedProject)
        {
            VSLangProj.VSProject vslangProject = currentProject.Object as VSLangProj.VSProject;
            vslangProject.References.AddProject(selectedProject);
        }

        public void CreateFolders(Project currentProject, string schedule, string projectName)
        {
            string dir = Path.GetDirectoryName(currentProject.FullName);
            DirectoryInfo info = new DirectoryInfo(dir)
                .CreateSubdirectory("App_Data\\jobs")
                .CreateSubdirectory(schedule)
                .CreateSubdirectory(projectName);

            string readmeFile = Path.Combine(info.FullName, "readme.txt");
            AddReadMe(readmeFile);
            currentProject.ProjectItems.AddFromFile(readmeFile);
        }

        private static void AddReadMe(string destinationFileName)
        {
            string dir = Path.GetDirectoryName(typeof(WebJobCreator).Assembly.Location);
            string readme = Path.Combine(dir, "job", Path.GetFileName(destinationFileName));

            File.Copy(readme, destinationFileName, true);
        }
    }
}
