using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace WoTClanIconDownloadConsole
{
    class Program
    {
        static CommandLineParser parser;

        static List<IconDownloadTask> downloadTasks = new List<IconDownloadTask>();

        static CancellationTokenSource tokenSource = new CancellationTokenSource();

        static void Main(string[] args)
        {
            Utils.AttachAssemblyResolver();
            ProcessCommandLineArgs(args);
            RunApplication();
        }

        static void ProcessCommandLineArgs(string[] args)
        {
            parser = new CommandLineParser(args);
            ApplicationExitCode code = parser.ParseCommandLineSwitches();

            if (code != ApplicationExitCode.NoError)
            {
                HandleNonZeroExit(parser.DebugMode, ApplicationExitCode.NoRegionsSpecified);
            }

            if (parser.RegionsToDownload == null || parser.RegionsToDownload.Count == 0)
            {
                Console.WriteLine("No regions to download");
                HandleNonZeroExit(parser.DebugMode, ApplicationExitCode.NoRegionsSpecified);
            }
        }

        static void RunApplication()
        {
            Console.WriteLine("Loading regions for download from command line");
            foreach (Region region in parser.RegionsToDownload)
            {
                downloadTasks.Add(new IconDownloadTask(parser, region, tokenSource) { Domain = Constants.DomainMapper[region], RegionFolderName = Constants.XvmLocationMapper[region] });
            }

            foreach (IconDownloadTask iconDownloadTask in downloadTasks)
            {
                Console.WriteLine("Downloading icons for region {0}", iconDownloadTask.Region.ToString());

                iconDownloadTask.GetTotalIconsPages();
                if (iconDownloadTask.ExitCode != ApplicationExitCode.NoError)
                    HandleNonZeroExit(parser.DebugMode, iconDownloadTask.ExitCode);

                iconDownloadTask.RunDownloadTasks();
                if (iconDownloadTask.ExitCode != ApplicationExitCode.NoError)
                    HandleNonZeroExit(parser.DebugMode, iconDownloadTask.ExitCode);
            }
        }

        static void HandleNonZeroExit(bool debugMode, ApplicationExitCode code)
        {
            int waitAnyResult = 0;
            if (debugMode)
            {
                Console.WriteLine("Press enter to pause application, or it will time out in 10 seconds");
                Task[] tasks = new Task[] { Task.Run(() => Console.ReadLine()) };
                waitAnyResult = Task.WaitAny(tasks, 10000);
            }
            else
            {
                Task.Delay(5000);
            }

            if (debugMode && waitAnyResult != -1)
            {
                Console.ReadLine();
            }

            Environment.Exit((int)code);
        }
    }
}
