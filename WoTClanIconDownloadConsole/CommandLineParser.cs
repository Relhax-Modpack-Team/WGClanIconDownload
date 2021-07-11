using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoTClanIconDownloadConsole
{
    public class CommandLineParser
    {
        public int ApiLoadLimit { get; set; } = Constants.WgApiDefaultLoadLimit;

        public string IconFolderStructure { get; set; } = Constants.DefaultIconFolderStructure;

        public int ConcurrentConnectionsPerRegion { get; set; } = Constants.DefaultConcurrentConnectionsPerRegion;

        public int ConcurrentApiRequestsPerRegion { get; set; } = Constants.DefaultApiRequestsPerRegion;

        public List<Region> RegionsToDownload { get; set; }

        public bool DebugMode { get; set; } = false;

        public bool Quiet { get; set; } = false;

        private string[] CommandLineArgs = null;

        private string[] RegionsString;

        private string ApiLoadLimitString;

        private string _IconFolderStructure;

        private string ConcurrentConnectionsPerRegionString;

        private string ConcurrentApiRequestsPerRegionString;

        /// <summary>
        /// Creates an instance of the CommandLineParser class
        /// </summary>
        /// <param name="args">The list of command line arguments provided from the Environment class</param>
        /// <remarks>The first arg to the exe is skipped</remarks>
        public CommandLineParser(string[] args)
        {
            this.CommandLineArgs = args;
        }

        /// <summary>
        /// Parse the command line arguments
        /// </summary>
        public ApplicationExitCode ParseCommandLineSwitches()
        {
            if (CommandLineArgs == null)
                throw new ArgumentNullException("CommandLineArgs is null");

            Console.WriteLine("Command line: " + string.Join(" ", CommandLineArgs));
            for (int i = 0; i < CommandLineArgs.Length; i++)
            {
                string commandArg = CommandLineArgs[i];

                //remote the slash (/) or dash (-) to list each arg
                char compare = commandArg[0];
                if (compare.Equals('/') || compare.Equals('-'))
                    commandArg = commandArg.Remove(0, 1);

                switch (commandArg)
                {
                    case "quiet":
                        Quiet = true;
                        Console.WriteLine("/{0}, quiet mode set to {1}", commandArg, DebugMode);
                        break;
                    case "debug":
                        DebugMode = true;
                        Quiet = false;
                        Console.WriteLine("/{0}, debug mode set to {1}", commandArg, DebugMode);
                        Console.WriteLine("NOTE: this sets quiet to false");
                        break;

                    case "apiLoadLimit":
                        ApiLoadLimitString = CommandLineArgs[++i];
                        if (int.TryParse(ApiLoadLimitString, out int result))
                        {
                            if (100 < result || result < 1)
                            {
                                Console.WriteLine("ERROR: apiLoadLimit must be between 1 and 100");
                                return ApplicationExitCode.InvalidCmdArg;
                            }
                            ApiLoadLimit = result;
                        }
                        Console.WriteLine("/{0}, WG api load limit set to {1}", commandArg, ApiLoadLimit);
                        break;

                    case "concurrentConnectionsPerRegion":
                        ConcurrentConnectionsPerRegionString = CommandLineArgs[++i];
                        if (int.TryParse(ConcurrentConnectionsPerRegionString, out int result_))
                        {
                            if (50 < result_ || result_ < 1)
                            {
                                Console.WriteLine("ERROR: concurrentConnectionsPerRegion must be between 1 and 50");
                                Environment.Exit((int)ApplicationExitCode.InvalidCmdArg);
                                return ApplicationExitCode.InvalidCmdArg;
                            }
                            ConcurrentConnectionsPerRegion = result_;
                        }
                        Console.WriteLine("/{0}, concurrent connections per region set to {1}", commandArg, ConcurrentConnectionsPerRegion);
                        break;

                    case "concurrentApiRequestsPerRegion":
                        ConcurrentApiRequestsPerRegionString = CommandLineArgs[++i];
                        if (int.TryParse(ConcurrentApiRequestsPerRegionString, out int _result_))
                        {
                            if (5 < _result_ || _result_ < 1)
                            {
                                Console.WriteLine("ERROR: concurrentApiRequestsPerRegion must be between 1 and 5");
                                return ApplicationExitCode.InvalidCmdArg;
                            }
                            ConcurrentApiRequestsPerRegion = _result_;
                        }
                        Console.WriteLine("/{0}, concurrent api requests per region set to {1}", commandArg, ConcurrentApiRequestsPerRegion);
                        break;

                    case "iconFolderStructure":
                        _IconFolderStructure = CommandLineArgs[++i];
                        //check the command to make sure it has the {region} macro inside it somewhere
                        if (!_IconFolderStructure.Contains(@"{region}"))
                        {
                            Console.WriteLine("ERROR: iconFolderStructure must contain the macro {region}");
                            return ApplicationExitCode.InvalidCmdArg;
                        }
                        Console.WriteLine("/{0}, icon folder directory structure set to {1}", _IconFolderStructure);
                        IconFolderStructure = _IconFolderStructure;
                        break;

                    case "regionsToDownload":
                        RegionsString = CommandLineArgs[++i].Split(',');
                        if (RegionsString.Count() == 0)
                        {
                            Console.WriteLine("ERROR: no regions specified in the arg regionsToDownload (count 0)");
                            return ApplicationExitCode.NoRegionsSpecified;
                        }

                        RegionsString = RegionsString.Distinct().ToArray();
                        RegionsToDownload = new List<Region>();

                        for (int j = 0; j < RegionsString.Count(); j++)
                        {
                            switch (RegionsString[j].ToLower())
                            {
                                case "na":
                                    RegionsToDownload.Add(Region.NA);
                                    break;
                                case "eu":
                                    RegionsToDownload.Add(Region.EU);
                                    break;
                                case "ru":
                                    RegionsToDownload.Add(Region.RU);
                                    break;
                                case "asia":
                                    RegionsToDownload.Add(Region.ASIA);
                                    break;
                                default:
                                    Console.WriteLine("ERROR: region {0} is not a valid region. Valid regions: na, eu, ru, asia");
                                    return ApplicationExitCode.InvalidCmdArg;
                            }
                        }

                        StringBuilder sb = new StringBuilder();
                        sb.Append(RegionsToDownload[0].ToString());
                        for (int k = 1; k < 0; k++)
                        {
                            sb.Append(", " + RegionsToDownload[0].ToString());
                        }
                        Console.WriteLine("/{0}, parsed regions {1}", commandArg, sb.ToString());
                        break;
                }
            }
            return ApplicationExitCode.NoError;
        }
    }
}
