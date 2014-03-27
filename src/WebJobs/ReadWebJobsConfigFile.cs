using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace WebJobs {
    public class ReadWebJobsConfigFile : Task {

        public string ConfigFile { get; set; }
        
        [Output]
        public ITaskItem[] JobsFound { get; set; }

        public override bool Execute() {
            // read the xml file and populate the JobsFound result

            if (File.Exists(ConfigFile)) {
                
                var doc = XDocument.Load(ConfigFile);
                var jobs = from wj in doc.Root.Elements("WebJob")
                           select new {
                               RelPath = wj.Attribute("RelativePath").Value,
                               Schedule = wj.Attribute("Schedule").Value
                           };

                var resultList = new List<ITaskItem>();

                foreach (var job in jobs) {

                    var item = new TaskItem(job.RelPath);
                    item.SetMetadata("Schedule", job.Schedule);

                    resultList.Add(item);
                }

                JobsFound = resultList.ToArray();
            }
            else {
                Log.LogMessage("web jobs config file not found at [{0}]", ConfigFile);
            }

            return !Log.HasLoggedErrors;
        }
    }
}
