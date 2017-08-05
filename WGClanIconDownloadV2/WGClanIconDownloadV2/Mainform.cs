using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace WGClanIconDownload
{

    public partial class Mainform : Form
    {
        // private List<ProgressBar> progressBar = new List<ProgressBar>() { };
        public List<ClassDataArray> dataArray = new List<ClassDataArray>() { };
        public BackgroundWorker UiUpdateWorker;
        // public BackgroundWorker regionHandleWorker;
        // public BackgroundWorker downloadThreadHandler;
        public Object _locker = new Object();

        public Mainform()
        {
            InitializeComponent();

            threads_trackBar.Value = Settings.viaUiThreadsAllowed;

            // add data to dataArray
            dataArray = Settings.fillDataArray();

            // durchlaufe alle Regionen
            foreach (var item in dataArray)
            {
                // Schreibe die möglichen Regionen in die ChecklistBox
                checkedListBoxRegion.Items.Add(item.region);
                string fold = Path.Combine(Settings.baseStorageFolder, item.storagePath);
                // prüfe ob ein entsprechendes Downloadverzeichnis bereits angelegt ist
                if (!Directory.Exists(fold))
                {
                    Directory.CreateDirectory(fold);
                    Utils.appendLog("Directory created => " + fold);
                }
            }
        }

        private void start_button_Click(object sender, EventArgs e)
        {
            try
            {
                // Utils.appendLog("buttonStart_Click");
                if (checkedListBoxRegion.Items.Count > 0)
                {
                    // int p = 0;
                    // Kickoff the worker thread to begin it's DoWork function.
                    for (int i = 0; i < checkedListBoxRegion.Items.Count; i++)
                    {
                        if (checkedListBoxRegion.GetItemCheckState(i) == CheckState.Checked)
                        {
                            // ProgressBar pB = new System.Windows.Forms.ProgressBar();
                            // pB.Location = new System.Drawing.Point(24, 145+p*30);
                            // pB.Size = new System.Drawing.Size(219, 19);
                            // pB.Visible = true;
                            // this.progressBar.Add(pB);
                            // progressBar[p].CreateGraphics().DrawString(progressBar[p].Value.ToString() + "%", new Font("Arial", (float)8.25), Brushes.Black, 100, 5);
                            // p++;

                            //Change the status of the buttons on the UI accordingly
                            //The start button is disabled as soon as the background operation is started
                            //The Cancel button is enabled so that the user can stop the operation 
                            //at any point of time during the execution
                            start_button.Enabled = false;
                            /// https://stackoverflow.com/questions/10694271/c-sharp-multiple-backgroundworkers 
                            /// Create a background worker thread that ReportsProgress &
                            /// SupportsCancellation
                            /// Hook up the appropriate events.
                            downloadThreadArgsParameter pushParameters = new downloadThreadArgsParameter();
                            for (int x = 0; x < 2; x++)
                            {
                                // Do selected stuff
                                // The parameters you want to pass to the do work event of the background worker.
                                EventArgsParameter parameters = new EventArgsParameter();//  = e.Argument as EventArgsParameter;       // the 'argument' parameter resurfaces here
                                parameters.region = (string)checkedListBoxRegion.Items[i];
                                parameters.indexOfDataArray = dataArray.Find(r => r.region == parameters.region).indexOfDataArray;
                                parameters.apiRequestWorkerThread = x;
                                dataArray[parameters.indexOfDataArray].currentPage = 1;
                                dataArray[parameters.indexOfDataArray].regionToDownload = true;
                                apiRequestWorker_start(sender, parameters);
                                pushParameters.region = parameters.region;
                                pushParameters.indexOfDataArray = parameters.indexOfDataArray;
                                Utils.appendLog("apiRequest RunWorkerAsync thread region: " + parameters.region + " thread: " + x + " started");
                            }
                            // var p = (downloadThreadArgsParameter)e.Argument;
                            // downloadThreadArgsParameter parameters = new downloadThreadArgsParameter();
                            // parameters.region = p.region;
                            // parameters.indexOfDataArray = p.indexOfDataArray;
                            regionHandleWorker_initializeStart(sender, pushParameters);
                        }

                    }
                    UiUpdateWorker = new BackgroundWorker();
                    UiUpdateWorker.DoWork += new DoWorkEventHandler(UiUpdateWorker_DoWork);
                    UiUpdateWorker.ProgressChanged += new ProgressChangedEventHandler(UiUpdateWorker_ProgressChanged);
                    UiUpdateWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(UiUpdateWorker_RunWorkerCompleted);
                    UiUpdateWorker.WorkerReportsProgress = true;
                    UiUpdateWorker.WorkerSupportsCancellation = true;
                    UiUpdateWorker.RunWorkerAsync();
                }
                else
                {
                    Utils.appendLog("no selection, no work ;-)");
                }
            }
            catch (Exception ex)
            {
                Utils.exceptionLog("start_button_Click", ex);
            }
        }

        void regionHandleWorker_initializeStart(object sender, downloadThreadArgsParameter parameters)
        {
            try
            {
                System.Threading.Timer timer = null;                                // delay 3000 ms https://stackoverflow.com/questions/545533/delayed-function-calls
                timer = new System.Threading.Timer((obj) =>
                {
                    regionHandleWorker_Start(sender, parameters);
                    timer.Dispose();
                },
                            null, 3000, System.Threading.Timeout.Infinite);
                
            }
            catch (Exception ex)
            {
                Utils.exceptionLog("regionHandleWorker_initializeStart", ex);
            }
        }

        void regionHandleWorker_Start(object sender, downloadThreadArgsParameter parameters)
        {
            try
            {
                BackgroundWorker regionHandleWorker = new BackgroundWorker();
                regionHandleWorker.DoWork += new DoWorkEventHandler(regionHandleWorker_DoWork);
                regionHandleWorker.ProgressChanged += new ProgressChangedEventHandler(regionHandleWorker_ProgressChanged);
                regionHandleWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(regionHandleWorker_RunWorkerCompleted);
                regionHandleWorker.WorkerReportsProgress = true;
                regionHandleWorker.WorkerSupportsCancellation = true;
                regionHandleWorker.RunWorkerAsync(parameters);
            }
            catch (Exception ex)
            {
                Utils.exceptionLog(string.Format("regionHandleWorker_Start:\nregion:{0}",parameters.region), ex);
            }
        }

        void regionHandleWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                downloadThreadArgsParameter parameters = (downloadThreadArgsParameter)e.Argument;
                e.Result = parameters;
                while (dataArray[parameters.indexOfDataArray].clans.Count >0)
                {
                    bool setNewDownloadEvent = false;
                    int Range = 20;
                    downloadThreadArgsParameter pushParameters = new downloadThreadArgsParameter();
                    pushParameters.region = parameters.region;
                    pushParameters.indexOfDataArray = parameters.indexOfDataArray;
                    
                    lock (_locker)
                    {
                        if ((dataArray[pushParameters.indexOfDataArray].dlIconsThreads <= Settings.viaUiThreadsAllowed) && (dataArray[pushParameters.indexOfDataArray].clans.Count > 0))
                        {
                            pushParameters.dlIconThreadID = Settings.viaUiThreadsAllowed;
                            dataArray[pushParameters.indexOfDataArray].dlIconsThreads++;
                            setNewDownloadEvent = true;
                            if (dataArray[pushParameters.indexOfDataArray].clans.Count < Range) { Range = dataArray[pushParameters.indexOfDataArray].clans.Count; }
                            // pushParameters.downloadList = new List<clanData>();
                            // parameters.timeStamp = TimeSpan.ToString(@"hh\:mm\:ss.fffffff");
                            pushParameters.downloadList.AddRange(dataArray[pushParameters.indexOfDataArray].clans.GetRange(0, Range));
                            dataArray[pushParameters.indexOfDataArray].clans.RemoveRange(0, Range);
                        }
                    }
                    if (setNewDownloadEvent)
                    {
                        downloadThreadHandler_DoWork(sender, pushParameters);
                        setNewDownloadEvent = false;
                    };
                    Thread.Sleep(50);
                }
                
            }
            catch (Exception ex)
            {
                Utils.exceptionLog("regionHandleWorker_DoWork", ex);
            }
        }

        void regionHandleWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
        }

        void regionHandleWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                downloadThreadArgsParameter parameters = (downloadThreadArgsParameter)e.Result;
                Utils.appendLog("regionHandleWorker " + parameters.region + " stopped");
            }
            catch (Exception ex)
            {
                Utils.exceptionLog("regionHandleWorker_RunWorkerCompleted", ex);
            }
        }

        void UiUpdateWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            bool finished = false;
            while (!finished)
            {
                finished = true;
                foreach (var r in dataArray)
                {
                    if (r.dlIconsReady != true && r.regionToDownload == true)     // if any region is NOT finished, do not close the UiUpdateWorker
                    {
                        finished = false;
                    }
                    if (r.dlIconsReady == true && r.regionToDownload == true && r.currentPage > (int)(Math.Ceiling((decimal)r.total / (decimal)Constants.limitApiPageRequest)) && !r.regionFinishedMsgDone)
                    {
                        Message_richTextBox.AppendText("Finished with the download of the WG API data for region " + r.region + ".\n");
                        r.regionFinishedMsgDone = true;
                    }
                }
                if (finished)
                {
                    Message_richTextBox.AppendText("Fehlerhaft !! => Finished with all downloads of the selected regions.\n");
                }
            }
        }

        void UiUpdateWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
        }

        void UiUpdateWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
        }

        void downloadThreadHandler_DoWork(object sender, downloadThreadArgsParameter parameters)
        {
            string emblems = "";
            string tag = "";
            try
            {
                lock(parameters)
                { 
                    // downloadThreadArgsParameter parameters = (downloadThreadArgsParameter)e.Argument;
                    if (parameters.downloadList.Count == 0 || parameters.downloadList == null)
                    {
                        lock (_locker)
                        {
                            dataArray[parameters.indexOfDataArray].dlIconsThreads--;
                        }
                        Utils.appendLog("downloadListe = 0");
                        return;
                    }
                    else
                    {
                        if (parameters.downloadList[0] == null)
                        {
                            if (parameters.downloadList.Count > 1)
                            {
                                parameters.downloadList.RemoveAt(0); downloadThreadHandler_DoWork(sender, parameters);
                                return;
                            }
                            else
                            {
                                Utils.appendLog("Error: parameters.downloadList[0] = null / count: "+ parameters.downloadList.Count);
                                dataArray[parameters.indexOfDataArray].dlIconsThreads--;
                                return;
                            }
                        }
                        else
                        {
                            emblems = parameters.downloadList[0].emblems;
                            tag = parameters.downloadList[0].tag;
                            if (tag == null)
                            {
                                Utils.appendLog("Error: parameters.downloadList[0].tag = null");
                                dataArray[parameters.indexOfDataArray].dlIconsThreads--;
                                return;
                            }
                            else
                            {
                                if (emblems == null)
                                {
                                    Utils.appendLog("Error: parameters.downloadList[0].emblems = null");
                                    dataArray[parameters.indexOfDataArray].dlIconsThreads--;
                                    return;
                                }
                            }
                        }
                        string filename = @"" + string.Format(dataArray[parameters.indexOfDataArray].storagePath + @"{0}.png", tag);
                        string completeFilename = Path.Combine(Settings.baseStorageFolder, filename);
                        AwesomeWebClient webClient = new AwesomeWebClient();
                        webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(downloadThreadHandler_DownloadFileCompleted);
                        webClient.DownloadFileAsync(new Uri(emblems), completeFilename, parameters);
                    }
                }
            }
            catch (Exception ex)
            {
                Utils.exceptionLog("downloadThreadHandler_DoWork", ex);
            }
        }

        void downloadThreadHandler_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            try
            {
                downloadThreadArgsParameter parameters = (downloadThreadArgsParameter)e.UserState;
                if (e.Cancelled)
                {
                    Utils.appendLog("threat region: " + parameters.region + " is stopped by cancel");
                }
                else if (e.Error != null)
                {
                    downloadThreadHandler_DoWork(sender, parameters);
                    Utils.appendLog("Error at downloadThreadHandler_DownloadFileCompleted (" + parameters.region + "):\n" + e.Error.ToString());
                    return;
                }
                else
                {
                    if (parameters.downloadList.Count > 0)
                    {
                        parameters.downloadList.RemoveAt(0);
                    }
                    lock (_locker)
                    {
                        dataArray[parameters.indexOfDataArray].countIconDownload++;
                    }
                    if (parameters.downloadList.Count > 0)
                    {
                        downloadThreadHandler_DoWork(sender, parameters);
                        return;
                    };
                    // reducing ammount at threads at IconDownload .... no new downlaodThread creation
                    lock (_locker)
                    {
                        dataArray[parameters.indexOfDataArray].dlIconsThreads--;
                        Utils.appendLog("dlIconsThreads --");
                    }
                }
            }
            catch (Exception ex)
            {
                Utils.exceptionLog("downloadThreadHandler_DownloadFileCompleted", ex);
            }
        }

        void apiRequestWorker_start(object sender, EventArgsParameter parameters)
        {
            try
            {
                BackgroundWorker apiRequestWorker = new BackgroundWorker();
                apiRequestWorker.DoWork += new DoWorkEventHandler(apiRequestWorker_DoWork);
                apiRequestWorker.ProgressChanged += new ProgressChangedEventHandler(apiRequestWorker_ProgressChanged);
                apiRequestWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(apiRequestWorker_RunWorkerCompleted);
                apiRequestWorker.WorkerReportsProgress = false;
                apiRequestWorker.WorkerSupportsCancellation = true;
                apiRequestWorker.RunWorkerAsync(parameters);
            }
            catch (Exception ex)
            {
                Utils.exceptionLog("apiRequestWorker_start", ex);
            }
        }

        /// <summary>
        /// Time consuming operations go here </br>
        /// i.e. Database operations,Reporting
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiRequestWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                EventArgsParameter parameters = (EventArgsParameter)e.Argument;
                var region = parameters.region;
                var indexOfDataArray = parameters.indexOfDataArray;
                var apiRequestWorkerThread = parameters.apiRequestWorkerThread;
            
                int currentPage = 0;
                lock (_locker)
                {
                    currentPage = dataArray[indexOfDataArray].currentPage;
                    dataArray[indexOfDataArray].currentPage++;
                }
                string url = string.Format(Settings.wgApiURL, dataArray[indexOfDataArray].url, Settings.wgAppID, Constants.limitApiPageRequest, currentPage);
                // Utils.appendLog("Info: region: " + region + " thread: "+ apiRequestWorkerThread + " page: " + currentPage);

                //Handle the event for download complete
                parameters.WebClient = new AwesomeWebClient();
                parameters.WebClient.DownloadDataCompleted += apiRequestWorker_DownloadDataCompleted;
                // push any new information to the next working step
                e.Result = parameters;
                //Start downloading file
                parameters.WebClient.DownloadDataAsync(new Uri(url), parameters);
            }
            catch (Exception ex)
            {
                Utils.exceptionLog("apiRequestWorker_DoWork", ex);
            }
        }

        void apiRequestWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // *****************************
            // currently not used
            // *****************************
        }

        void apiRequestWorker_DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            try
            {
                EventArgsParameter parameters = (EventArgsParameter)e.UserState;       // the 'argument' parameter resurfaces here
                parameters.WebClient.DownloadDataCompleted -= apiRequestWorker_DownloadDataCompleted;

                if (e.Error != null)
                {
                    Utils.appendLog("Error: download failed\n" + e.Error.ToString());
                    apiRequestWorker_start(sender, parameters);
                }
                else
                {
                    string result = System.Text.Encoding.UTF8.GetString(e.Result);
                    //Get the data of the file
                    dynamic resultPageApiJson = JsonConvert.DeserializeObject(result);
                    if (resultPageApiJson != null)
                    {
                        if (((string)resultPageApiJson.status).Equals("ok"))
                        {
                            lock (_locker)
                            {
                                dataArray[parameters.indexOfDataArray].total = ((int)resultPageApiJson.meta.total);
                            }
                            if ((int)resultPageApiJson.meta.count > 0)
                            {
                                clanData c;
                                for (var f = 0; f < (int)resultPageApiJson.meta.count; f++)
                                {
                                    c = new clanData();
                                    c.tag = (string)resultPageApiJson.data[f].tag;
                                    c.emblems = (string)resultPageApiJson.data[f].emblems.x32.portal;
                                    if (c.tag == null || c.emblems == null || c.tag.Equals("") || c.emblems.Equals(""))
                                    {
                                        string msg = "";
                                        if (c.tag == null) { msg += "tag: (empty)"; } else { msg += "tag: " + c.tag; }
                                        if (c.emblems == null) { msg += " emblems: (empty)"; } else { msg += " emblems: "+ c.emblems; }
                                        Utils.appendLog("Error: server: "+parameters.region+" / " + msg);
                                    }
                                    else if (Settings.prohibitedFilenames.Contains(c.tag))
                                    {
                                        Utils.appendLog("Error: found prohibited filename => " + c.tag);
                                    }
                                    else
                                    {
                                        lock (_locker)
                                        {
                                            dataArray[parameters.indexOfDataArray].clans.Add(c);
                                        }
                                    }
                                }
                                apiRequestWorker_start(sender, parameters);
                            }
                            else   // es gibt keine Datensätze mehr und das holen der "Pages" ist abgeschlossen.
                            {
                                dataArray[parameters.indexOfDataArray].dlIconsReady = true;
                                Utils.appendLog("apiRequestWorker thread "+parameters.apiRequestWorkerThread+" finished with region: "+parameters.region);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Utils.exceptionLog("apiRequestWorker_DownloadDataCompleted",ex);
            }
        }

        void apiRequestWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                // The background process is complete. We need to inspect
                // our response to see if an error occurred, a cancel was
                // requested or if we completed successfully.  
                if (e.Cancelled)
                {
                    // progressLabel.Text = "Tasks cancelled.";
                    Utils.appendLog("Task cancelled");
                }
                // Check to see if an error occurred in the background process.
                else if (e.Error != null)
                {
                    // progressLabel.Text = "Error while performing background operation.";
                    var result = e.Error.ToString();
                    Utils.appendLog("Error: " + result);
                }
                else
                {
                    // Utils.appendLog("all fine");
                }
            }
            catch (Exception ex)
            {
                Utils.exceptionLog("apiRequestWorker_RunWorkerCompleted",ex);
            }
        }

        private void threads_trackBar_Scroll(object sender, EventArgs e)
        {
            Settings.viaUiThreadsAllowed = threads_trackBar.Value;
            Utils.appendLog("Settings.viaUiThreadsAllowed set to: " + Settings.viaUiThreadsAllowed);
        }
    }
}

