using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WoTClanIconDownloadConsole
{
    class Program
    {
        static CommandLineParser parser;

        static List<IconDownloadTask> downloadTasks = new List<IconDownloadTask>();

        static void Main(string[] args)
        {
            Utils.AttachAssemblyResolver();
            ProcessCommandLineArgs(args);
            RunApplication();
        }

        static void ProcessCommandLineArgs(string[] args)
        {
            parser = new CommandLineParser(args);
            parser.ParseCommandLineSwitches();
            if (parser.RegionsToDownload == null || parser.RegionsToDownload.Count == 0)
            {
                Console.WriteLine("No regions to download");
                Environment.Exit((int)ApplicationExitCode.NoRegionsSpecified);
                return;
            }
        }

        static void RunApplication()
        {
            Console.WriteLine("Loading regions for download from command line");
            foreach (Region region in parser.RegionsToDownload)
            {
                downloadTasks.Add(new IconDownloadTask(parser, region) { Domain = Constants.DomainMapper[region], RegionFolderName = Constants.XvmLocationMapper[region] });
            }

            foreach (IconDownloadTask iconDownloadTask in downloadTasks)
            {
                Console.WriteLine("Downloading icons for region {0}", iconDownloadTask.Region.ToString());
                iconDownloadTask.GetTotalIconsPages();

                iconDownloadTask.LoadAllPages();

                iconDownloadTask.DownloadIcons();
            }
        }
    }
}
