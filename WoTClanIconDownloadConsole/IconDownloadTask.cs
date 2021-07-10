using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WoTClanIconDownloadConsole
{
    public struct IconStruct
    {
        public IconDownloadTask IconDownloadTask;
        public string DownloadPath;
        public string DownloadUrl;
    }

    public class IconDownloadTask
    {
        public List<IconDownloadTask> IconDownloadTasks;

        public CommandLineParser CommandLineParser;

        public Region Region { get; set; }

        public string Domain { get; set; }

        public string RegionFolderName { get; set; }

        public int TotalIcons { get; set; }

        public int TotalPages { get; set; }

        public int CurrentIconCount { get; set; }

        public int CurrentPage { get; set; }

        private ConcurrentWebClient client = new ConcurrentWebClient();

        public string DownloadFolder 
        {
            get
            {
                string parsedFolderStructure = CommandLineParser.IconFolderStructure.Replace(@"{region}", RegionFolderName);
                return Path.Combine(Constants.ApplicationStartupPath, parsedFolderStructure);
            }
        }

        public List<IconStruct> IconStructs { get; } = new List<IconStruct>();

        public IconDownloadTask(CommandLineParser commandLineParser, Region region)
        {
            this.Region = region;
            this.CommandLineParser = commandLineParser;
            this.client.ConcurrentConnections = CommandLineParser.ConcurrentConnectionsPerRegion;
        }

        public void GetTotalIconsPages()
        {
            //sample: https://api.worldoftanks.com/wot/clans/list/?application_id=d0bfec3ab1967d9582a73fef7d86ff02&fields=-tag,-emblems,-created_at,-color,-clan_id,-members_count,-name&language=en&limit=1&page_no=1
            string apiUrlTotalIconsList = string.Format(Constants.ApiClansTotalUrlEscaped, Domain, Constants.WgApplicationID);

            JObject root = GetJsonObjectFromWgApi(string.Format(apiUrlTotalIconsList));
            if (root == null)
                return;

            JValue result = root.SelectToken("status") as JValue;
            if (result == null)
            {
                Utils.HandleError("Failed to parse the json api response (no status property)", CommandLineParser.DebugMode, ApplicationExitCode.FailedToParseApi);
                return;
            }

            string apiResult = result.Value.ToString();
            Console.WriteLine("API status: {0}", apiResult);

            if (!apiResult.Equals(Constants.WgApiResultOk))
            {
                Utils.HandleError("Failed to parse the json api response (result was not ok)", CommandLineParser.DebugMode, ApplicationExitCode.FailedToParseApi);
                return;
            }

            JValue totalIconsJValue = (root.SelectToken("meta.total") as JValue);
            if (totalIconsJValue == null)
            {
                Utils.HandleError("Failed to parse the json api response (no meta/total property)", CommandLineParser.DebugMode, ApplicationExitCode.FailedToParseApi);
                return;
            }

            TotalIcons = Convert.ToInt32((long)totalIconsJValue.Value);
            //CommandLineParser.ApiLoadLimit is like "icons per page". try out the sample url above to see what i mean. look at the "limit" part of the url
            TotalPages = (TotalIcons / CommandLineParser.ApiLoadLimit) + 1;

            Console.WriteLine("Total Icons count: {0}", TotalIcons);
            Console.WriteLine("Total Pages count: {0}", TotalPages);
        }

        public void LoadAllPages()
        {
            IconStructs.Clear();
            for (CurrentPage = 1; CurrentPage <= TotalPages; CurrentPage++)
            {
                Console.WriteLine("Downloading page {0} of {1}", CurrentPage, TotalPages);
                //sample: https://api.worldoftanks.com/wot/clans/list/?application_id=d0bfec3ab1967d9582a73fef7d86ff02&fields=-emblems.x195,-emblems.x24,-emblems.x256,-emblems.x64,-created_at,-color,-clan_id,-members_count,-name&limit=100&page_no=280
                string apiUrlTotalIconsList = string.Format(Constants.ApiClansIconsUrlEscaped, Domain, Constants.WgApplicationID, CommandLineParser.ApiLoadLimit, CurrentPage);

                JObject root = GetJsonObjectFromWgApi(string.Format(apiUrlTotalIconsList));
                if (root == null)
                    return;

                JArray dataJArray = root.SelectToken("data") as JArray;
                if (dataJArray == null)
                {
                    Utils.HandleError(string.Format("Failed to parse the json api response (failed on page {0})", CurrentPage), CommandLineParser.DebugMode, ApplicationExitCode.FailedToParseApi);
                    return;
                }

                foreach (JObject clanObject in dataJArray)
                {
                    IconStruct iconStruct = new IconStruct() { IconDownloadTask = this };

                    JValue iconFileNameJValue = (clanObject.SelectToken("tag") as JValue);
                    iconStruct.DownloadPath = Path.Combine(DownloadFolder, iconFileNameJValue.Value.ToString());
                    iconStruct.DownloadPath = Path.ChangeExtension(iconStruct.DownloadPath, ".png");

                    JValue iconDownloadUrlJValue = (clanObject.SelectToken("emblems.x32.portal") as JValue);
                    iconStruct.DownloadUrl = iconDownloadUrlJValue.Value.ToString();

                    IconStructs.Add(iconStruct);
                }
            }
        }

        public void DownloadIcons()
        {
            if (Directory.Exists(DownloadFolder))
            {
                Console.WriteLine("Deleting old directory");
                Directory.Delete(DownloadFolder, true);
            }
            Directory.CreateDirectory(DownloadFolder);

            try
            {
                foreach (IconStruct iconStruct in IconStructs)
                {
                    Console.WriteLine("Downloading icon {0} of {1}: {2}", CurrentIconCount++, TotalIcons, Path.GetFileName(iconStruct.DownloadPath));
                    client.DownloadFile(iconStruct.DownloadUrl, iconStruct.DownloadPath);
                }
            }
            catch (WebException ex)
            {
                DisposeClient();
                Utils.HandleException(ex, CommandLineParser.DebugMode, ApplicationExitCode.FailedToDownloadImages);
                return;
            }
        }

        private JObject GetJsonObjectFromWgApi(string url)
        {
            if (CommandLineParser.DebugMode)
                Console.WriteLine("Downloading api data at url {0}", url);

            string jsonString;
            try
            {
                jsonString = client.DownloadString(url);
            }
            catch (WebException ex)
            {
                DisposeClient();
                Utils.HandleException(ex, CommandLineParser.DebugMode, ApplicationExitCode.FailedToDownloadApi);
                return null;
            }

            JObject root;
            try
            {
                root = JObject.Parse(jsonString);
            }
            catch (Exception ex)
            {
                DisposeClient();
                Utils.HandleException(ex, CommandLineParser.DebugMode, ApplicationExitCode.FailedToParseApi);
                return null;
            }

            return root;
        }

        private void DisposeClient()
        {
            if (client != null)
            {
                client.Dispose();
                client = null;
            }
        }
    }
}
