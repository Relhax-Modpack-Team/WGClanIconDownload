using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using System.Windows.Forms;
using System.ComponentModel;

namespace WGClanIconDownload
{
    public partial class Settings
    {
        // public static string baseStorageFolder = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
        // public static string baseStorageFolder = Path.GetDirectoryName(Application.ExecutablePath);
        public static string baseStorageFolder = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        // public static string baseStorageFolder = Path.GetDirectoryName(Process.GetCurrentProcess).MainModule.FileName);
        // public static string ErrorLogFile = Path.Combine(baseStorageFolder, "logs", "error.log");
        public static string errorLogFile = @"C:\Users\Ich\Desktop\ClanDownload.log";
        public static string processLogFile = Path.Combine(baseStorageFolder, "logs", "process.dat");
        public static string folderStructure = @"download\{reg}\res_mods\mods\shared_resources\xvm\res\clanicons\{reg}\clan\";
        public static string wgAppID = "d0bfec3ab1967d9582a73fef7d86ff02";
        /// <summary>
        /// the URL to wg API with removed unneeded fields
        /// </summary>
        /// <param {0}="baseURLRegion"></param>
        /// <param {1}="wgAppID"></param>
        /// <param {2}="limit"></param>
        /// <param {3}="page"></param>
        public static string wgApiURL = @"https://api.{0}/wgn/clans/list/?application_id={1}&fields=-emblems.x195,-emblems.x24,-emblems.x256,-emblems.x64,-created_at,-color,-clan_id,-members_count,-name&game=wot&limit={2}&page_no={3}";
        public static string[] prohibitedFilenames = new string[] {
            "CON","PRN","AUX","CLOCK$","NUL","COM0","COM1","COM2","COM3","COM4","COM5","COM6","COM7","COM8","COM9","LPT0","LPT1","LPT2","LPT3","LPT4","LPT5","LPT6","LPT7","LPT8","LPT9"
        };
        public static string[] imagesRes = new string[] {
            "x32","x24","x64","x195","x256"
        };
        public static string[] imageIndex = new string[] {
            "wot","portal","wowp"
        };
        public static int viaUiThreadsAllowed = 2;

        public static List<ClassDataArray> fillDataArray()
        {
            List<ClassDataArray> dataArray = new List<ClassDataArray>();
            ClassDataArray d;
    
            d = new ClassDataArray();
            d.region = "ASIA";
            d.indexOfDataArray = dataArray.Count;
            d.url = "worldoftanks.asia";
            d.storagePath = Settings.folderStructure.Replace("{reg}", d.region);
            dataArray.Add(d);

            d = new ClassDataArray();
            d.region = "NA";
            d.indexOfDataArray = dataArray.Count;
            d.url = "worldoftanks.com";
            d.storagePath = Settings.folderStructure.Replace("{reg}", d.region);
            dataArray.Add(d);

            d = new ClassDataArray();
            d.region = "EU";
            d.indexOfDataArray = dataArray.Count;
            d.url = "worldoftanks.eu";
            d.storagePath = Settings.folderStructure.Replace("{reg}", d.region);
            dataArray.Add(d);

            d = new ClassDataArray();
            d.region = "RU";
            d.indexOfDataArray = dataArray.Count;
            d.url = "worldoftanks.ru";
            d.storagePath = Settings.folderStructure.Replace("{reg}", d.region);
            dataArray.Add(d);
            return dataArray;
        }
    }

    public class regionData
    {
        public string url { get; set; } = null;
        public int total { get; set; } = Constants.INVALID_HANDLE_VALUE;
        public int countIconDownload { get; set; } = 0;
        public int currentPage { get; set; } = Constants.INVALID_HANDLE_VALUE;
        public string storagePath { get; set; } = null;
    }

    public class clanData
    {
        public string tag { get; set; } = null;
        public string emblems { get; set; } = null;
        public clanData() { }
    }

    public class ClassDataArray
    {
        public string region { get; set; }
        public int indexOfDataArray { get; set; }
        public int total { get; set; } = Constants.INVALID_HANDLE_VALUE;
        public int currentPage { get; set; } = Constants.INVALID_HANDLE_VALUE;
        public int countIconDownload { get; set; } = 0;
        public int dlErrorCounter { get; set; } = 0;
        /// <summary>
        /// storing the amount of created Icon download threads
        /// the "regionHandleWorker_DoWork" will increase the value, "downloadThreadHandler_RunWorkerCompleted" will reduce it 
        /// </summary>
        public int dlIconsThreads { get; set; } = 0;
        public string url { get; set; } = null;
        public string storagePath { get; set; } = null;
        public List<clanData> clans = new List<clanData>();
        public ClassDataArray() { }
    }

    static class Constants
    {
        public const int limitApiPageRequest = 100;
        public const int INVALID_HANDLE_VALUE = -1;
    }

    public class EventArgsParameter
    {
        public string region { get; set; } = null;
        public int indexOfDataArray { get; set; } = Constants.INVALID_HANDLE_VALUE;
        public int apiRequestWorkerThread { get; set; } = Constants.INVALID_HANDLE_VALUE;
        public AwesomeWebClient WebClient { get; set; } = new AwesomeWebClient();
    }

    public class downloadThreadArgsParameter
    {
        public string region { get; set; } = null;
        public int indexOfDataArray { get; set; } = Constants.INVALID_HANDLE_VALUE;
        public int dlIconThreadID { get; set; } = Constants.INVALID_HANDLE_VALUE;
        public List<clanData> downloadList = new List<clanData>();
    }
}