using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
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
        public CommandLineParser CommandLineParser;

        public Region Region { get; set; }

        public string Domain { get; set; }

        public string RegionFolderName { get; set; }

        public int TotalIcons { get; set; }

        public int TotalPages { get; set; }

        public int CurrentIconCount { get; set; }

        public int CurrentPage { get; set; }

        public ApplicationExitCode ExitCode { get; private set; }

        private object lockerObject = new object();

        private AutoResetEvent iconDownloadWaitEvent;

        public CancellationTokenSource tokenSource;

        public CancellationToken cancellationToken;

        public string DownloadFolder 
        {
            get
            {
                string parsedFolderStructure = CommandLineParser.IconFolderStructure.Replace(@"{region}", RegionFolderName);
                return Path.Combine(Constants.ApplicationStartupPath, parsedFolderStructure);
            }
        }

        public List<IconStruct> IconStructs { get; } = new List<IconStruct>();

        public IconDownloadTask(CommandLineParser commandLineParser, Region region, CancellationTokenSource tokenSource)
        {
            this.Region = region;
            this.CommandLineParser = commandLineParser;
            this.tokenSource = tokenSource;
            this.cancellationToken = this.tokenSource.Token;
        }

        private Task[] iconDownloaderTasks;

        private ConcurrentWebClient[] iconDownloaderClients;

        private bool pageLoadersDone;

        public void GetTotalIconsPages()
        {
            ExitCode = ApplicationExitCode.NoError;
            HttpClient client = new HttpClient();
            //sample: https://api.worldoftanks.com/wot/clans/list/?application_id=d0bfec3ab1967d9582a73fef7d86ff02&fields=-tag,-emblems,-created_at,-color,-clan_id,-members_count,-name&language=en&limit=1&page_no=1
            string apiUrlTotalIconsList = string.Format(Constants.ApiClansTotalUrlEscaped, Domain, Constants.WgApplicationID);

            Task<JObject> rootTask = GetJsonObjectFromWgApi(string.Format(apiUrlTotalIconsList), client);

            rootTask.Wait();
            JObject root = rootTask.Result;
            if (root == null)
                return;

            JValue result = root.SelectToken("status") as JValue;
            if (result == null)
            {
                HandleError("Failed to parse the json api response (no status property)", ApplicationExitCode.FailedToParseApi, tokenSource);
                return;
            }

            string apiResult = result.Value.ToString();
            Console.WriteLine("API status: {0}", apiResult);

            if (!apiResult.Equals(Constants.WgApiResultOk))
            {
                HandleError("Failed to parse the json api response (result was not ok)", ApplicationExitCode.FailedToParseApi, tokenSource);
                return;
            }

            JValue totalIconsJValue = (root.SelectToken("meta.total") as JValue);
            if (totalIconsJValue == null)
            {
                HandleError("Failed to parse the json api response (no meta/total property)", ApplicationExitCode.FailedToParseApi, tokenSource);
                return;
            }

            TotalIcons = Convert.ToInt32((long)totalIconsJValue.Value);
            //CommandLineParser.ApiLoadLimit is like "icons per page". try out the sample url above to see what i mean. look at the "limit" part of the url
            TotalPages = (TotalIcons / CommandLineParser.ApiLoadLimit) + 1;

            Console.WriteLine("Total Icons count: {0}", TotalIcons);
            Console.WriteLine("Total Pages count: {0}", TotalPages);
        }

        public void RunDownloadTasks()
        {
            ExitCode = ApplicationExitCode.NoError;
            if (Directory.Exists(DownloadFolder))
            {
                Console.WriteLine("Deleting old directory");
                Directory.Delete(DownloadFolder, true);
            }
            Directory.CreateDirectory(DownloadFolder);

            StartIconDownloadThreads();
            RunPageLoaders();

            //wait for the icon download threads to be done
            try
            {
                Task.WaitAll(iconDownloaderTasks);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        #region api requests
        private void RunPageLoaders()
        {
            IconStructs.Clear();
            int subStatusTracker = 0;
            pageLoadersDone = false;
            Task[] tasks = new Task[TotalPages];
            Task[] subStatus = new Task[CommandLineParser.ConcurrentApiRequestsPerRegion];
            HttpClient[] clients = new HttpClient[CommandLineParser.ConcurrentApiRequestsPerRegion];
            for (int i = 0; i < clients.Count(); i++)
                clients[i] = new HttpClient();

            for (CurrentPage = 1; CurrentPage <= TotalPages; CurrentPage++)
            {
                try
                {
                    tasks[CurrentPage - 1] = LoadPage(CurrentPage, clients[subStatusTracker]);
                }
                catch
                {
                    for (int i = 0; i < clients.Count(); i++)
                        clients[i].CancelPendingRequests();
                }

                if (tokenSource.IsCancellationRequested)
                    return;

                subStatus[subStatusTracker++] = tasks[CurrentPage - 1];
                if (subStatusTracker >= CommandLineParser.ConcurrentApiRequestsPerRegion)
                {
                    try
                    {
                        Task.WaitAll(subStatus);
                    }
                    catch { return; }
                    subStatusTracker = 0;
                    for (int j = 0; j < CommandLineParser.ConcurrentApiRequestsPerRegion; j++)
                    {
                        iconDownloadWaitEvent.Set();
                    }
                }
            }

            try
            {
                Task.WaitAll(tasks);
            }
            catch { return; }
            pageLoadersDone = true;
        }

        private async Task LoadPage(int page, HttpClient client)
        {
            Console.WriteLine("Downloading page {0} of {1}", page, TotalPages);
            if (CommandLineParser.DebugMode)
                Console.WriteLine("Executing on thread {0}", Thread.CurrentThread.ManagedThreadId);
            //sample: https://api.worldoftanks.com/wot/clans/list/?application_id=d0bfec3ab1967d9582a73fef7d86ff02&fields=-emblems.x195,-emblems.x24,-emblems.x256,-emblems.x64,-created_at,-color,-clan_id,-members_count,-name&limit=100&page_no=280
            string apiUrlTotalIconsList = string.Format(Constants.ApiClansIconsUrlEscaped, Domain, Constants.WgApplicationID, CommandLineParser.ApiLoadLimit, page);

            JObject root = await GetJsonObjectFromWgApi(string.Format(apiUrlTotalIconsList), client);
            if (root == null)
                return;

            JArray dataJArray = root.SelectToken("data") as JArray;
            if (dataJArray == null)
            {
                HandleError(string.Format("Failed to parse the json api response (failed on page {0})", page), ApplicationExitCode.FailedToParseApi, tokenSource);
                return;
            }

            cancellationToken.ThrowIfCancellationRequested();
            lock (lockerObject)
            {
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

        private async Task<JObject> GetJsonObjectFromWgApi(string url, HttpClient client)
        {
            if (CommandLineParser.DebugMode)
                Console.WriteLine("Downloading api data at url {0}", url);

            string jsonString;
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                jsonString = await client.GetStringAsync(url);
            }
            catch (Exception ex)
            {
                HandleException(ex, ApplicationExitCode.FailedToDownloadApi, tokenSource);
                return null;
            }

            JObject root;
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                root = JObject.Parse(jsonString);
            }
            catch (Exception ex)
            {
                HandleException(ex, ApplicationExitCode.FailedToParseApi, tokenSource);
                return null;
            }

            return root;
        }
        #endregion

        #region icon download
        private void StartIconDownloadThreads()
        {
            iconDownloaderTasks = new Task[CommandLineParser.ConcurrentConnectionsPerRegion];
            iconDownloaderClients = new ConcurrentWebClient[CommandLineParser.ConcurrentConnectionsPerRegion];
            iconDownloadWaitEvent = new AutoResetEvent(false);
            ServicePointManager.DefaultConnectionLimit = CommandLineParser.ConcurrentConnectionsPerRegion;
            for (int i = 0; i < iconDownloaderTasks.Count(); i++)
            {
                iconDownloaderClients[i] = new ConcurrentWebClient() { ConcurrentConnections = CommandLineParser.ConcurrentConnectionsPerRegion };
                bool valueLocked = false;
                iconDownloaderTasks[i] = Task.Run(() =>
                {
                    int index = i;
                    valueLocked = true;
                    DownloadIconsFromPage(iconDownloaderClients[index]);
                });

                while (!valueLocked)
                    Thread.Sleep(1);
            }
        }

        private void DownloadIconsFromPage(ConcurrentWebClient client)
        {
            while (true)
            {
                int retry;
                if (IconStructs.Count == 0 && pageLoadersDone)
                {
                    return;
                }
                else if (IconStructs.Count == 0)
                {
                    iconDownloadWaitEvent.WaitOne();
                }

                IconStruct iconStruct;
                lock (lockerObject)
                {
                    iconStruct = IconStructs[0];
                    IconStructs.RemoveAt(0);
                    CurrentIconCount++;
                }

                for (retry = 1; retry < 4; retry++)
                {
                    try
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        if (!CommandLineParser.Quiet || (CommandLineParser.Quiet && CurrentIconCount % 100 == 0))
                            Console.WriteLine("Downloading icon {0} of {1}: {2}", CurrentIconCount, TotalIcons, Path.GetFileName(iconStruct.DownloadPath));

                        if (!Directory.Exists(DownloadFolder))
                        {
                            Directory.CreateDirectory(DownloadFolder);
                        }

                        if (CommandLineParser.DebugMode)
                        {
                            Console.WriteLine("Download url:  {0}, {1}Download path: {2}", iconStruct.DownloadUrl, Environment.NewLine, iconStruct.DownloadPath);
                        }
                        cancellationToken.ThrowIfCancellationRequested();

                        client.DownloadFile(iconStruct.DownloadUrl, iconStruct.DownloadPath);
                        cancellationToken.ThrowIfCancellationRequested();
                        retry = 4;
                    }
                    catch (OperationCanceledException ex)
                    {
                        pageLoadersDone = true;
                        HandleException(ex, ApplicationExitCode.FailedToDownloadImages, tokenSource);
                        return;
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Download of icon {0} failed, retry {1} of 3...", Path.GetFileName(iconStruct.DownloadPath), retry);
                        Thread.Sleep(500);
                    }
                }

                if (retry != 5)
                {
                    Console.WriteLine("Download of icon {0} failed", Path.GetFileName(iconStruct.DownloadPath));
                }
            }
        }
        #endregion

        private void HandleException(Exception ex, ApplicationExitCode exitCode, CancellationTokenSource source)
        {
            Console.WriteLine(ex.ToString());

            HandleClose(exitCode, source);
        }

        private void HandleError(string message, ApplicationExitCode exitCode, CancellationTokenSource source)
        {
            Console.WriteLine(message);

            HandleClose(exitCode, source);
        }

        private void HandleClose(ApplicationExitCode exitCode, CancellationTokenSource source)
        {
            if (!source.IsCancellationRequested)
            {
                source.Cancel();
                ExitCode = exitCode;
            }
        }
    }
}
