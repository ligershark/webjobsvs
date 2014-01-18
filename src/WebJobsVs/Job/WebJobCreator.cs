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

        public void CreateFolders(Project currentProject, string schedule)
        {
            string dir = Path.GetDirectoryName(currentProject.FullName);

            string appData = Path.Combine(dir, "App_Data");
            if (!Directory.Exists(appData))
                Directory.CreateDirectory(appData);

            string jobs = Path.Combine(appData, "jobs");
            if (!Directory.Exists(jobs))
                Directory.CreateDirectory(jobs);

            string method = Path.Combine(jobs, schedule);
            if (!Directory.Exists(method))
                Directory.CreateDirectory(method);

            string readmeFile = Path.Combine(method, "readme.txt");
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
