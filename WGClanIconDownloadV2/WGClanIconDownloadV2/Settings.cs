using System;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Security.Permissions;
using CustomProgressBar;
using System.Diagnostics;

namespace WGClanIconDownload
{
    public partial class Settings
    {
        public static string baseStorageFolder = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
        public static string errorLogFile = Path.Combine(baseStorageFolder, "logs", "error.log");
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
        public static int viaUiThreadsAllowed = 10;

        public static bool downloadPause = false;
        public static bool downloadCancel = false;
        
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
            d.region = "EU";
            d.indexOfDataArray = dataArray.Count;
            d.url = "worldoftanks.eu";
            d.storagePath = Settings.folderStructure.Replace("{reg}", d.region);
            dataArray.Add(d);

            d = new ClassDataArray();
            d.region = "NA";
            d.indexOfDataArray = dataArray.Count;
            d.url = "worldoftanks.com";
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

    public class clanData : IDisposable
    {
        public string tag { get; set; } = null;
        public string emblems { get; set; } = null;
        public clanData() { }
        [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
        public void Dispose()  // Follow the Dispose pattern - public nonvirtual.
        {
            this.Dispose();
            GC.SuppressFinalize(this);
        }
    }

    public class ClassDataArray
    {
        // *** Lock ***
        private object _locker = new object();

        public string region { get; set; }
        public int indexOfDataArray { get; set; }
        public int total { get; set; } = Constants.INVALID_HANDLE_VALUE;
        // *** Property ***
        private int m_currentPage = 1;
        // *** Thread-safe access to Property using locking ***
        internal int currentPage { get { lock(_locker) { return m_currentPage; } } set { lock(_locker) { m_currentPage = value; } } }
        // public int currentPage { get; set; } = 
        public int countIconDownload { get; set; } = 0;
        /// <summary>
        /// to store the last readed countIconDownload
        /// </summary>
        public int dlTickBuffer { get; set; } = 0;
        public int dlErrorCounter { get; set; } = 0;
        /// <summary>
        /// storing the amount of created Icon download threads
        /// the "regionHandleWorker_DoWork" will increase the value, "downloadThreadHandler_RunWorkerCompleted" will reduce it 
        /// </summary>
        public int dlIconsThreads { get; set; } = 0;
        /// <summary>
        /// this flag is set true, if user selected it at the Listbox and pressed start
        /// </summary>
        public bool regionToDownload { get; set; } = false;
        /// <summary>
        /// this flag is set, if the apiRequester finished his job and reached at least 1 page abouve the calculated page request
        /// </summary>
        public bool dlApiDataReady { get; set; } = false;
        /// <summary>
        /// this flag is set, if the "UiUpdateWorker_DoWork" checked that all Icons of the region are downloaded
        /// </summary>
        public bool dlIconsReady { get; set; } = false;
        public bool regionFinishedMsgDone { get; set; } = false;
        /// <summary>
        /// this flag is set true, if the first "regionHandleWorker_DoWork" is adding a "downloadThreadHandler"
        /// </summary>
        public bool dlThreadsStarted { get; set; } = false;
        public string url { get; set; } = null;
        public string storagePath { get; set; } = null;
        /// <summary>
        /// https://msdn.microsoft.com/de-de/library/system.diagnostics.stopwatch.elapsed(v=vs.110).aspx
        /// </summary>
        public Stopwatch stopWatch = new Stopwatch();
        public List<clanData> clans = new List<clanData>();
        public ProgressBarWithCaptionVista customProgressBar = new ProgressBarWithCaptionVista();
        public Label regionThreadsLabel = new Label();
        public Label dlTicksLabel = new Label();
        public PictureBox iconPreview = new PictureBox();
        public ClassDataArray() { }
    }

    static class Constants
    {
        public const int limitApiPageRequest = 100;
        public const int INVALID_HANDLE_VALUE = -1;
        public const string start_button_text_start = "Start";
        public const string start_button_text_pause = "Pause";
        public const string start_button_text_resume = "Resume";
        public const string cancel_button_text_cancel = "Cancel";
        public const string cancel_button_text_quit = "Quit";

        public const UInt32 ERROR_SHARING_VIOLATION = 0x80070020;               /// https://stackoverflow.com/questions/1139957/c-sharp-convert-integer-to-hex-and-back-again
        public const int WS_EX_TRANSPARENT = 0x20;
    }

    public class EventArgsParameter : IDisposable
    {
        public string region { get; set; } = null;
        public int indexOfDataArray { get; set; } = Constants.INVALID_HANDLE_VALUE;
        public int apiRequestWorkerThread { get; set; } = Constants.INVALID_HANDLE_VALUE;
        public AwesomeWebClient WebClient { get; set; } = new AwesomeWebClient();
        [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
        public void Dispose()  // Follow the Dispose pattern - public nonvirtual.
        {
            this.Dispose();
            GC.SuppressFinalize(this);
        }
    }

    public class downloadThreadArgsParameter : IDisposable
    {
        public string region { get; set; } = null;
        public int indexOfDataArray { get; set; } = Constants.INVALID_HANDLE_VALUE;
        public int dlIconThreadID { get; set; } = Constants.INVALID_HANDLE_VALUE;
        public int fileDlErrorCounter { get; set; } = 0;
        public string lastUsedUrl { get; set; } = "";
        public List<clanData> downloadList;
        [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
        public void Dispose()  // Follow the Dispose pattern - public nonvirtual.
        {
            this.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
 