using Microsoft.WindowsAzure.Jobs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelloWorld
{
    class Program
    {
        // Please set the following connectionstring values in app.config
        // AzureJobsRuntime and AzureJobsData
        static void Main()
        {
            JobHost host = new JobHost();
            host.RunAndBlock();
        }

        public static void ProcessQueueMessage([QueueInput] string inputText, [BlobOutput("containername/blobname")]TextWriter writer)
        {
            writer.WriteLine(inputText);
        }
    }
}
