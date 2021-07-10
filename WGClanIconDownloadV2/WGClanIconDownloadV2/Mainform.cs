﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using CustomProgressBar;

namespace WGClanIconDownload
{

    public partial class Mainform : Form
    {
        public List<ClassDataArray> dataArray;
        public BackgroundWorker UiUpdateWorker;
        public BackgroundWorker TickCounterWorker;
        public Object _locker = new Object();

        public Mainform()
        {
            InitializeComponent();
            this.Visible = false;

            threads_trackBar.Value = Settings.viaUiThreadsAllowed;

            setMainformSmallHeight();
            addCustomElementsToMainForm();
            this.Visible = true;
        }

        private void setMainformSmallHeight()
        {

            int borderWidth = (this.Width - ClientSize.Width) / 2;
            int titlebarHeight = this.Height - this.ClientSize.Height - 2 * borderWidth;
            this.Height = titlebarHeight + 2 * borderWidth + Message_richTextBox.Top + Message_richTextBox.Height + checkedListBoxRegion.Top;
        }

        private void addCustomElementsToMainForm()
        {
            if (dataArray != null)
                ((IDisposable)dataArray).Dispose();
            // add data to dataArray
            dataArray = new List<ClassDataArray>() { };
            dataArray = Settings.fillDataArray();

            checkedListBoxRegion.Items.Clear();
            // durchlaufe alle Regionen
            foreach (var r in dataArray)
            {
                // Schreibe die möglichen Regionen in die ChecklistBox
                checkedListBoxRegion.Items.Add(r.region);
            }
        }

        private void start_button_Click(object sender, EventArgs e)
        {
            if (start_button.Text.Equals(Constants.start_button_text_start))
                create_UiElementsAndWorker(sender, e);
            else if (start_button.Text.Equals(Constants.start_button_text_pause))
                downloadPause(sender, e);
            else if (start_button.Text.Equals(Constants.start_button_text_resume))
                downloadResume(sender, e);
            else
                MessageBox.Show("Function not recogniced", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void cancel_button_Click(object sender, EventArgs e)
        {
            if (cancel_button.Text.Equals(Constants.cancel_button_text_cancel))
                downloadCancel(sender, e);
            else if (cancel_button.Text.Equals(Constants.cancel_button_text_quit))
                programQuit(sender, e);
            else
                MessageBox.Show("Function not recogniced", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void downloadCancel(object sender, EventArgs e)
        {
            Message_richTextBox.AppendText("User command: cancel ALL downloads\n");
            Utils.appendLog("User command: all downloads will be stopped");
            Settings.downloadCancel = true;
        }

        private void downloadPause(object sender, EventArgs e)
        {
            Message_richTextBox.AppendText("User command: all downloads will be paused\n");
            Utils.appendLog("User command: all downloads will be paused");
            start_button.Text = Constants.start_button_text_resume;
            Settings.downloadPause = true;
        }

        private void downloadResume(object sender, EventArgs e)
        {
            if (UiUpdateWorker != sender)
            {
                Message_richTextBox.AppendText("User command: all downloads will be started again\n");
                Utils.appendLog("User command: all downloads will be started again");
            }
            start_button.Text = Constants.start_button_text_pause;
            Settings.downloadPause = false;
        }

        private void create_UiElementsAndWorker(object sender, EventArgs e)
        {
            try
            {
                if (checkedListBoxRegion.Items.Count > 0)
                {
                    Message_richTextBox.AppendText("started request at WG API for Clan data ...\n");
                    int t = 0;
                    // Kickoff the worker thread to begin it's DoWork function.
                    for (int i = 0; i < checkedListBoxRegion.Items.Count; i++)
                    {
                        if (checkedListBoxRegion.GetItemCheckState(i) == CheckState.Checked)
                        {
                            //Change the status of the buttons on the UI accordingly
                            //The start button is disabled as soon as the background operation is started
                            //The Cancel button is enabled so that the user can stop the operation 
                            //at any point of time during the execution
                            /// https://stackoverflow.com/questions/10694271/c-sharp-multiple-backgroundworkers 
                            /// Create a background worker thread that ReportsProgress &
                            /// SupportsCancellation
                            /// Hook up the appropriate events.
                            downloadThreadArgsParameter pushParameters = new downloadThreadArgsParameter();
                            for (int x = 0; x < 3; x++)
                            {
                                // Do selected stuff
                                // The parameters you want to pass to the do work event of the background worker.
                                EventArgsParameter parameters = new EventArgsParameter();//  = e.Argument as EventArgsParameter;       // the 'argument' parameter resurfaces here
                                parameters.region = (string)checkedListBoxRegion.Items[i];
                                parameters.indexOfDataArray = dataArray.Find(r => r.region == parameters.region).indexOfDataArray;
                                parameters.apiRequestWorkerThread = x;
                                dataArray[parameters.indexOfDataArray].regionToDownload = true;
                                apiRequestWorker_start(sender, parameters);
                                pushParameters.region = parameters.region;
                                pushParameters.indexOfDataArray = parameters.indexOfDataArray;
                                Utils.appendLog("apiRequest RunWorkerAsync thread region: " + parameters.region + " thread: " + x + " started");
                            }
                            // erstelle den kompletten Pfad der Downloadordner
                            string fold = Path.Combine(Settings.baseStorageFolder, dataArray[pushParameters.indexOfDataArray].storagePath);
                            // prüfe ob ein entsprechendes Downloadverzeichnis bereits angelegt ist
                            if (!Directory.Exists(fold))
                            {
                                Directory.CreateDirectory(fold);
                                Utils.appendLog("Directory created => " + fold);
                            }
                            dataArray[pushParameters.indexOfDataArray].stopWatch.Start();           /// start stopwatch
                            regionHandleWorker_initializeStart(sender, pushParameters);
                            create_dynamicElements(pushParameters, t);
                            t++;
                        }
                    }
                    int borderWidth = (this.Width - ClientSize.Width) / 2;
                    int titlebarHeight = this.Height - this.ClientSize.Height - 2 * borderWidth;
                    this.Height = titlebarHeight + 2 * borderWidth + Message_richTextBox.Top + Message_richTextBox.Height + (t + (overallTickLabel.Visible ? 1 : 0)) * 32 + checkedListBoxRegion.Top;  /// set the new Height of the Mainform

                    cancel_button.Text = Constants.cancel_button_text_cancel;
                    checkedListBoxRegion.Enabled = false;
                    start_button.Text = Constants.start_button_text_pause;

                    UiUpdateWorker = new BackgroundWorker();
                    UiUpdateWorker.DoWork += new DoWorkEventHandler(UiUpdateWorker_DoWork);
                    UiUpdateWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(UiUpdateWorker_RunWorkerCompleted);
                    UiUpdateWorker.WorkerReportsProgress = false;
                    UiUpdateWorker.WorkerSupportsCancellation = true;
                    UiUpdateWorker.RunWorkerAsync();

                    TickCounterWorker = new BackgroundWorker();
                    TickCounterWorker.DoWork += new DoWorkEventHandler(TickCounterWorker_DoWork);
                    TickCounterWorker.WorkerReportsProgress = false;
                    TickCounterWorker.WorkerSupportsCancellation = true;
                    TickCounterWorker.RunWorkerAsync();
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

        public void create_dynamicElements(downloadThreadArgsParameter parameters, int t)
        {
            dataArray[parameters.indexOfDataArray].regionNameLabel = new System.Windows.Forms.Label();
            dataArray[parameters.indexOfDataArray].regionNameLabel.AutoSize = true;
            dataArray[parameters.indexOfDataArray].regionNameLabel.Location = new System.Drawing.Point(24, 217 + t * 32);
            dataArray[parameters.indexOfDataArray].regionNameLabel.Size = new System.Drawing.Size(35, 13);
            dataArray[parameters.indexOfDataArray].regionNameLabel.Text = parameters.region;
            dataArray[parameters.indexOfDataArray].regionNameLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Controls.Add(dataArray[parameters.indexOfDataArray].regionNameLabel);

            dataArray[parameters.indexOfDataArray].customProgressBar = new ProgressBarWithCaptionVista();
            dataArray[parameters.indexOfDataArray].customProgressBar.Size = new System.Drawing.Size(217, 19);
            dataArray[parameters.indexOfDataArray].customProgressBar.Maximum = 1;
            dataArray[parameters.indexOfDataArray].customProgressBar.Minimum = 0;
            dataArray[parameters.indexOfDataArray].customProgressBar.Location = new System.Drawing.Point(65, dataArray[parameters.indexOfDataArray].regionNameLabel.Top-1);         /// 24
            dataArray[parameters.indexOfDataArray].customProgressBar.Visible = true;
            dataArray[parameters.indexOfDataArray].customProgressBar.DisplayStyle = ProgressBarDisplayText.CustomText;
            dataArray[parameters.indexOfDataArray].customProgressBar.CustomText = "";
            this.Controls.Add(dataArray[parameters.indexOfDataArray].customProgressBar);

            dataArray[parameters.indexOfDataArray].regionThreadsLabel = new System.Windows.Forms.Label();
            dataArray[parameters.indexOfDataArray].regionThreadsLabel.AutoSize = true;
            dataArray[parameters.indexOfDataArray].regionThreadsLabel.Location = new System.Drawing.Point(291, dataArray[parameters.indexOfDataArray].regionNameLabel.Top);       /// 260
            dataArray[parameters.indexOfDataArray].regionThreadsLabel.Size = new System.Drawing.Size(35, 13);
            dataArray[parameters.indexOfDataArray].regionThreadsLabel.Text = "0";
            dataArray[parameters.indexOfDataArray].regionThreadsLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Controls.Add(dataArray[parameters.indexOfDataArray].regionThreadsLabel);

            dataArray[parameters.indexOfDataArray].dlTicksLabel = new System.Windows.Forms.Label();
            dataArray[parameters.indexOfDataArray].dlTicksLabel.AutoSize = true;
            dataArray[parameters.indexOfDataArray].dlTicksLabel.Location = new System.Drawing.Point(331, dataArray[parameters.indexOfDataArray].regionThreadsLabel.Top); /// 295
            dataArray[parameters.indexOfDataArray].dlTicksLabel.Size = new System.Drawing.Size(35, 13);
            dataArray[parameters.indexOfDataArray].dlTicksLabel.Text = "";
            dataArray[parameters.indexOfDataArray].dlTicksLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Controls.Add(dataArray[parameters.indexOfDataArray].dlTicksLabel);

            dataArray[parameters.indexOfDataArray].iconPreview = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(dataArray[parameters.indexOfDataArray].iconPreview)).BeginInit();
            dataArray[parameters.indexOfDataArray].iconPreview.AccessibleRole = System.Windows.Forms.AccessibleRole.Cursor;
            dataArray[parameters.indexOfDataArray].iconPreview.Location = new System.Drawing.Point(439, 211 + t * 32);              /// 439
            dataArray[parameters.indexOfDataArray].iconPreview.Margin = new System.Windows.Forms.Padding(0);
            dataArray[parameters.indexOfDataArray].iconPreview.Name = "iconPreview";
            dataArray[parameters.indexOfDataArray].iconPreview.Size = new System.Drawing.Size(32, 32);
            dataArray[parameters.indexOfDataArray].iconPreview.TabStop = false;
            this.Controls.Add(dataArray[parameters.indexOfDataArray].iconPreview);

            overallTickLabel.Location = new System.Drawing.Point(dataArray[parameters.indexOfDataArray].dlTicksLabel.Left - 14, dataArray[parameters.indexOfDataArray].regionThreadsLabel.Top + 32);
            overallTickLabel.Visible = (t > 0);

            separatorBevelLineLabel.Location = new System.Drawing.Point(overallTickLabel.Left, overallTickLabel.Top - 3);
            separatorBevelLineLabel.Visible = overallTickLabel.Visible;
        }

        private void regionHandleWorker_initializeStart(object sender, downloadThreadArgsParameter parameters)
        {
            try
            {
                System.Threading.Timer timer = null;                                // delay 3000 ms https://stackoverflow.com/questions/545533/delayed-function-calls
                timer = new System.Threading.Timer((obj) =>
                    {
                        timer.Dispose();
                    },
                        null, 500, System.Threading.Timeout.Infinite);
                regionHandleWorker_Start(sender, parameters);
            }
            catch (Exception ex)
            {
                Utils.exceptionLog("regionHandleWorker_initializeStart", ex);
            }
        }

        private void regionHandleWorker_Start(object sender, downloadThreadArgsParameter parameters)
        {
            while (Settings.downloadPause)
            {
                Thread.Sleep(100);
            }

            lock (_locker)
            {
                try
                {
                    if (Settings.downloadCancel)
                    {
                        Utils.appendLog(string.Format("try to stop regionHandleWorker {0}", parameters.region));
                        dataArray[parameters.indexOfDataArray].clans.Clear();
                        return;
                    }

                    Message_richTextBox.AppendText("started downloading of Clanicons in Region " + parameters.region + " ...\n");
                    BackgroundWorker regionHandleWorker = new BackgroundWorker();
                    regionHandleWorker.DoWork += new DoWorkEventHandler(regionHandleWorker_DoWork);
                    regionHandleWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(regionHandleWorker_RunWorkerCompleted);
                    regionHandleWorker.WorkerReportsProgress = false;
                    regionHandleWorker.WorkerSupportsCancellation = true;
                    regionHandleWorker.RunWorkerAsync(parameters);
                }
                catch (Exception ex)
                {
                    Utils.exceptionLog(string.Format("regionHandleWorker_Start Region:{0}", parameters.region), ex);
                }
            }
        }

        private void regionHandleWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                downloadThreadArgsParameter parameters = (downloadThreadArgsParameter)e.Argument;
                e.Result = parameters;
                dataArray[parameters.indexOfDataArray].dlIconsThreads = Constants.INVALID_HANDLE_VALUE;
                while (!dataArray[parameters.indexOfDataArray].dlIconsReady)
                {
                    while (Settings.downloadPause)
                    {
                        Thread.Sleep(100);
                    }

                    lock (_locker)
                    {
                        if (dataArray[parameters.indexOfDataArray].dlIconsThreads == Constants.INVALID_HANDLE_VALUE) { dataArray[parameters.indexOfDataArray].dlIconsThreads = 0; };
                        bool setNewDownloadEvent = false;
                        int Range = 20;
                        downloadThreadArgsParameter pushParameters = new downloadThreadArgsParameter();
                        pushParameters.region = parameters.region;
                        pushParameters.indexOfDataArray = parameters.indexOfDataArray;
                        if ((dataArray[pushParameters.indexOfDataArray].dlIconsThreads < Settings.viaUiThreadsAllowed) && (dataArray[pushParameters.indexOfDataArray].clans.Count > 0) && !Settings.downloadCancel)
                        {
                            pushParameters.dlIconThreadID = Settings.viaUiThreadsAllowed;
                            dataArray[pushParameters.indexOfDataArray].dlIconsThreads++;
                            dataArray[pushParameters.indexOfDataArray].dlThreadsStarted = true;
                            setNewDownloadEvent = true;
                            if (dataArray[pushParameters.indexOfDataArray].clans.Count < Range) { Range = dataArray[pushParameters.indexOfDataArray].clans.Count; }
                            var oldList = pushParameters.downloadList;
                            pushParameters.downloadList = new List<clanData>();
                            pushParameters.downloadList.AddRange(dataArray[pushParameters.indexOfDataArray].clans.GetRange(0, Range));
                            dataArray[pushParameters.indexOfDataArray].clans.RemoveRange(0, Range);
                            if (oldList != null)
                                ((IDisposable)oldList).Dispose();
                        }
                        if (setNewDownloadEvent)
                        {
                            downloadThreadHandler_DoWork(sender, pushParameters);
                            setNewDownloadEvent = false;
                        };
                    }
                    Thread.Sleep(20);
                }
                if (Settings.downloadCancel)
                {
                    Utils.appendLog(string.Format("successfully stopped regionHandleWorker {0}", parameters.region));
                }
            }
            catch (Exception ex)
            {
                Utils.exceptionLog("regionHandleWorker_DoWork", ex);
            }
        }

        void regionHandleWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                downloadThreadArgsParameter parameters = e.Result as downloadThreadArgsParameter;

                BackgroundWorker worker = sender as BackgroundWorker;
                if (worker != null)
                {
                    worker.WorkerReportsProgress = false;
                    worker.DoWork -= regionHandleWorker_DoWork;
                    worker.RunWorkerCompleted -= regionHandleWorker_RunWorkerCompleted;
                    worker.Dispose();
                }
                Utils.appendLog("regionHandleWorker " + parameters.region + " finished");
            }
            catch (Exception ex)
            {
                Utils.exceptionLog("regionHandleWorker_RunWorkerCompleted", ex);
            }
        }

        void UiUpdateWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                bool finished = false;
                while (!finished)
                {
                    finished = true;

                    foreach (var r in dataArray.Where(x => x.regionToDownload))
                    {
                        int lastPage = (int)(Math.Ceiling((decimal)r.total / (decimal)Constants.limitApiPageRequest));
                        if (!Settings.downloadCancel && (r.total == Constants.INVALID_HANDLE_VALUE || r.dlIconsThreads != 0 || r.total > 0 && !r.dlThreadsStarted))     // if any region is NOT finished, do not close the UiUpdateWorker
                        {
                            finished = false;
                        }
                        if (Settings.downloadCancel && (r.dlIconsThreads != 0 || !r.dlApiDataReady || !r.dlIconsReady))
                        {
                            finished = false;
                        }
                        if (r.dlApiDataReady == true && r.currentPage > lastPage && !r.regionFinishedMsgDone && !Settings.downloadCancel)
                        {
                            r.regionFinishedMsgDone = true;
                            Message_richTextBox.AppendText(String.Format("finished with the request at WG API Clan data for Region {0}\n", r.region));
                        }
                        if (r.dlApiDataReady == true && r.currentPage > lastPage && r.dlIconsThreads == 0 && !r.dlIconsReady == true && !Settings.downloadCancel)
                        {
                            r.dlIconsReady = true;
                            r.stopWatch.Stop();
                            Message_richTextBox.AppendText(String.Format("finished with download of {0} Clanicon for Region {1} (elapsed Time: {2})\n", r.countIconDownload, r.region, Utils.getStopWatchTime(r.stopWatch.Elapsed)));
                        }
                        if (r.total > 0)
                        {
                            /*
                            int countIconDownload = r.countIconDownload;
                            r.customProgressBar.CustomText = string.Format("{0}/{1} ({2})", countIconDownload, r.total, r.clans.Count().ToString());
                            r.customProgressBar.Maximum = r.total;
                            if (r.customProgressBar.Maximum < countIconDownload) { r.customProgressBar.Maximum = countIconDownload; };
                            r.customProgressBar.Value = countIconDownload;
                            r.regionThreadsLabel.Text = r.dlIconsThreads.ToString();
                            */
                        }
                        if (Settings.downloadCancel && r.dlApiDataReady == true && !r.regionFinishedMsgDone)
                        {
                            r.regionFinishedMsgDone = true;
                            Message_richTextBox.AppendText((string.Format("successfully stopped requests at WG API data for Region {0}\n", r.region)));
                        }
                        if (Settings.downloadCancel && r.dlApiDataReady == true && r.dlIconsThreads == 0 && !r.dlIconsReady == true)   // && r.currentPage > lastPage
                        {
                            r.dlIconsReady = true;
                            r.stopWatch.Stop();
                            Message_richTextBox.AppendText((string.Format("successfully stopped Clanicon downloads for Region {0}\n", r.region)));
                        }
                    }
                    if (finished)
                    {
                        if (Settings.downloadCancel)
                            Message_richTextBox.AppendText("... canceled ALL downloads at all Regions.\n");
                        else
                            Message_richTextBox.AppendText("... finished with ALL downloads of the selected Regions.\n");
                        TickCounterWorker.CancelAsync();
                        Utils.appendLog("UiUpdateWorker_DoWork finished");
                    }
                    Thread.Sleep(250);
                }
            }
            catch (Exception ex)
            {
                Utils.exceptionLog("UiUpdateWorker_DoWork", ex);
            }
        }

        void UiUpdateWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                downloadResume(sender, null);               // if this function is called (even with at a user pause event) the complete download is already finished, so it must be cleared
                Settings.downloadCancel = false;            // if this function is called (even with at a user pause event) the complete download is already finished, so it must be cleared
                setMainformSmallHeight();
                checkedListBoxRegion.Enabled = true;
                start_button.Text = Constants.start_button_text_start;
                cancel_button.Text = Constants.cancel_button_text_quit;

                UiUpdateWorker.Dispose();
                TickCounterWorker.Dispose();

                foreach (var r in dataArray)
                {
                    if (r.regionToDownload)
                    {
                        r.countIconDownload = 0;
                        r.clans.Clear();
                        r.currentPage = 1;
                        r.dlApiDataReady = false;
                        r.dlErrorCounter = 0;
                        r.dlIconsReady = false;
                        r.dlIconsThreads = 0;
                        r.dlThreadsStarted = false;
                        r.dlTickBuffer = 0;
                        r.regionFinishedMsgDone = false;
                        r.regionToDownload = false;
                        r.stopWatch.Reset();
                        r.total = Constants.INVALID_HANDLE_VALUE;
                        this.Controls.Remove(r.customProgressBar);
                        this.Controls.Remove(r.regionThreadsLabel);
                        this.Controls.Remove(r.iconPreview);
                        this.Controls.Remove(r.dlTicksLabel);
                        if (r.regionNameLabel != null)
                            r.regionNameLabel.Dispose();
                        r.regionNameLabel = new Label();
                        if (r.customProgressBar != null)
                            r.customProgressBar.Dispose();
                        r.customProgressBar = new ProgressBarWithCaptionVista();
                        if (r.regionThreadsLabel != null)
                            r.regionThreadsLabel.Dispose();
                        r.regionThreadsLabel = new Label();
                        if (r.iconPreview != null)
                            r.iconPreview.Dispose();
                        r.iconPreview = new PictureBox();
                        if (r.dlTicksLabel != null)
                            r.dlTicksLabel.Dispose();
                        r.dlTicksLabel = new Label();
                    }
                }
                Utils.appendLog("UiUpdateWorker_RunWorkerCompleted finished");
            }
            catch (Exception ex)
            {
                Utils.exceptionLog("UiUpdateWorker_RunWorkerCompleted", ex);
            }
        }

        void TickCounterWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            char d = (char)0x2300;
            char s = (char)0x2211;
            List<int> avgBuffer = new List<int>();
            try
            {
                while (!TickCounterWorker.CancellationPending)
                {
                    int t = 0;
                    foreach (var r in dataArray)
                    {
                        int i = 0;
                        int lastTick = r.dlTickBuffer;
                        r.dlTickBuffer = r.countIconDownload;
                        i = r.dlTickBuffer - lastTick;
                        //r.dlTicksLabel.Text = "dl/sec: " + (i).ToString();
                        t += i;
                    }
                    //this.overallTickLabel.Text = s + " dl/sec: " + t;
                    if (!(t == 0 && avgBuffer.Count == 0))
                    {
                        avgBuffer.Add(t);
                        while (avgBuffer.Count > 10) { avgBuffer.RemoveAt(0); };        // max 60 sec buffer
                        //avgOverTimeTicksLabel.Text = d + " dl/sec: " + (avgBuffer.Sum() / avgBuffer.Count) + " (" + avgBuffer.Count + " sec)";
                    }
                    Thread.Sleep(1000);
                }
            }
            catch (Exception ex)
            {
                Utils.exceptionLog("TickCounterWorker_DoWork", ex);
            }
        }

        void downloadThreadHandler_DoWork(object sender, downloadThreadArgsParameter parameters)
        {
            while (Settings.downloadPause)
            {
                Thread.Sleep(100);
            }

            if (Settings.downloadCancel)
            {
                parameters.downloadList.Clear();
            }

            lock (_locker)
            {
                string emblems = "";
                string tag = "";
                try
                {
                    if (parameters.downloadList.Count == 0 || parameters.downloadList == null)
                    {
                        dataArray[parameters.indexOfDataArray].dlIconsThreads--;
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
                        parameters.lastUsedUrl = emblems;
                        string filename = @"" + string.Format(dataArray[parameters.indexOfDataArray].storagePath + @"{0}.png", tag);
                        string completeFilename = Path.Combine(Settings.baseStorageFolder, filename);
                        AwesomeWebClient webClient = new AwesomeWebClient();
                        webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(downloadThreadHandler_DownloadFileCompleted);
                        webClient.DownloadFileAsync(new Uri(emblems), completeFilename, parameters);
                    }
                }
                catch (Exception ex)
                {
                    Utils.exceptionLog("downloadThreadHandler_DoWork", ex);
                }
            }
        }

        void downloadThreadHandler_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            lock (_locker)
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
                        parameters.fileDlErrorCounter++;
                        if (parameters.fileDlErrorCounter > 3)
                        {
                            if (e.Error.GetBaseException() is WebException)
                            {
                                WebException we = (WebException)e.Error.GetBaseException();
                                if (we.Status == WebExceptionStatus.ProtocolError)
                                {
                                    Utils.appendLog(string.Format("Error: WebException at downloadThreadHandler_DownloadFileCompleted ({0})\nStatus: {1}  Description: {2}  URL: {3}", parameters.region, Utils.IntToHex((int)we.Status), ((HttpWebResponse)we.Response).StatusDescription, parameters.lastUsedUrl));
                                }
                                else
                                {
                                    Utils.appendLog("Error: WebException at downloadThreadHandler_DownloadFileCompleted (" + parameters.region + ") Status: " + e.Error.ToString());
                                }
                            }
                            else if (e.Error.GetBaseException() is IOException)
                            {
                                Utils.appendLog("IOException at downloadThreadHandler_DownloadFileCompleted (" + parameters.region + ") Status: " + e.Error.ToString());
                            }
                            else
                            {
                                Utils.appendLog("Error at downloadThreadHandler_DownloadFileCompleted (" + parameters.region + ") Status: " + e.Error.ToString());
                            }
                            parameters.fileDlErrorCounter = 0;
                            dataArray[parameters.indexOfDataArray].countIconDownload++;
                            if (parameters.downloadList.Count > 0)
                            {
                                parameters.downloadList.RemoveAt(0);
                                downloadThreadHandler_DoWork(sender, parameters);
                                return;
                            }
                        }
                        else if (parameters.fileDlErrorCounter > 0)
                        {
                            downloadThreadHandler_DoWork(sender, parameters);
                            return;
                        }
                    }
                    else if (Settings.downloadCancel)
                    {
                        // no new download thread starting
                    }
                    else
                    {
                        parameters.fileDlErrorCounter = 0;          // download hat funktioniert, daher Zähler auf 0.
                    }
                    if (parameters.downloadList.Count > 0)
                    {
                        try
                        {
                            string filename = @"" + string.Format(dataArray[parameters.indexOfDataArray].storagePath + @"{0}.png", parameters.downloadList[0].tag);
                            var oldImage = dataArray[parameters.indexOfDataArray].iconPreview.Image;
                            dataArray[parameters.indexOfDataArray].iconPreview.Image = Image.FromFile(Path.Combine(Settings.baseStorageFolder, filename));
                            if (oldImage != null)
                                ((IDisposable)oldImage).Dispose();
                        }
                        catch { }
                        parameters.downloadList.RemoveAt(0);
                    }
                    dataArray[parameters.indexOfDataArray].countIconDownload++;
                    if (parameters.downloadList.Count > 0)
                    {
                        downloadThreadHandler_DoWork(sender, parameters);
                        return;
                    };
                    // reducing ammount at threads at IconDownload .... no new downlaodThread creation
                    dataArray[parameters.indexOfDataArray].dlIconsThreads--;
                }
                catch (Exception ex)
                {
                    Utils.exceptionLog("downloadThreadHandler_DownloadFileCompleted", ex);
                }
            }
        }

        void apiRequestWorker_start(object sender, EventArgsParameter parameters)
        {
            while (Settings.downloadPause)
            {
                Thread.Sleep(100);
            }

            lock (_locker)
            {
                try
                {
                    if (Settings.downloadCancel)
                    {
                        Utils.appendLog(string.Format("successfully stopped apiRequestWorker {0} ({1})", parameters.region, parameters.apiRequestWorkerThread));
                        dataArray[parameters.indexOfDataArray].currentPage = (dataArray[parameters.indexOfDataArray].total / Constants.limitApiPageRequest) + 2;
                        dataArray[parameters.indexOfDataArray].dlApiDataReady = true;
                        return;
                    }

                    BackgroundWorker apiRequestWorker = new BackgroundWorker();
                    apiRequestWorker.DoWork += new DoWorkEventHandler(apiRequestWorker_DoWork);
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
        }

        void apiRequestWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            apiRequestWorker_DoWork(sender, e, false);
        }

        void apiRequestWorker_DoWork(object sender, DoWorkEventArgs e, bool reload)
        {
            lock (_locker)
            {
                try
                {
                    EventArgsParameter parameters = (EventArgsParameter)e.Argument;
                    var region = parameters.region;
                    var indexOfDataArray = parameters.indexOfDataArray;
                    var apiRequestWorkerThread = parameters.apiRequestWorkerThread;
                    int currentPage = 0;
                    if (!reload)
                    {
                        currentPage = dataArray[indexOfDataArray].currentPage;
                        parameters.currentPage = currentPage;
                        dataArray[indexOfDataArray].currentPage++;
                    }
                    else
                    {
                        currentPage = parameters.currentPage;
                    }
                    string url = string.Format(Settings.wgApiURL, dataArray[indexOfDataArray].url, Settings.wgAppID, Constants.limitApiPageRequest, currentPage);
                    //Handle the event for download complete
                    parameters.WebClient = new AwesomeWebClient();
                    parameters.WebClient.DownloadDataCompleted += apiRequestWorker_DownloadDataCompleted;
                    // push any new information to the next working step
                    e.Result = parameters;
                    // Start downloading file
                    parameters.WebClient.DownloadDataAsync(new Uri(url), parameters);
                }
                catch (Exception ex)
                {
                    Utils.exceptionLog("apiRequestWorker_DoWork", ex);
                }
            }
        }

        void apiRequestWorker_DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            lock (_locker)
            {
                try
                {
                    EventArgsParameter parameters = (EventArgsParameter)e.UserState;
                    parameters.WebClient.DownloadDataCompleted -= apiRequestWorker_DownloadDataCompleted;

                    if (e.Error != null)
                    {
                        Utils.appendLog("Error: download failed\n" + e.Error.ToString());
                        apiRequestWorker_start(sender, parameters);
                    }
                    else if(Settings.downloadCancel)
                    {
                        Utils.appendLog((string.Format("successfully stopped apiRequestWorker {0} ({1})", parameters.region, parameters.apiRequestWorkerThread)));
                        dataArray[parameters.indexOfDataArray].dlApiDataReady = true;
                        // pass the funtion if "downloadCancel" is true
                    }
                    else
                    {
                        string result = System.Text.Encoding.UTF8.GetString(e.Result);
                        //Get the data of the object
                        dynamic resultPageApiJson = JsonConvert.DeserializeObject(result);
                        if (resultPageApiJson != null)
                        {
                            if (((string)resultPageApiJson.status).Equals("ok"))
                            {
                                if (dataArray[parameters.indexOfDataArray].total < ((int)resultPageApiJson.meta.total))
                                {
                                    dataArray[parameters.indexOfDataArray].total = ((int)resultPageApiJson.meta.total);
                                };
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
                                            if (c.emblems == null) { msg += " emblems: (empty)"; } else { msg += " emblems: " + c.emblems; }
                                            Utils.appendLog("Error: server: " + parameters.region + " / " + msg);
                                        }
                                        else if (Settings.prohibitedFilenames.Contains(c.tag))
                                        {
                                            Message_richTextBox.AppendText("found prohibited filename " + c.tag + " at Region " + parameters.region + " (no Icon possible)\n");
                                            Utils.appendLog("Error: found prohibited filename => " + c.tag + " (" + parameters.region + ")");
                                            dataArray[parameters.indexOfDataArray].countIconDownload++;
                                        }
                                        else
                                        {
                                            dataArray[parameters.indexOfDataArray].clans.Add(c);
                                        }
                                    }
                                    apiRequestWorker_start(sender, parameters);
                                    return;
                                }
                                else   // es gibt keine Datensätze mehr und das holen der "Pages" ist abgeschlossen.
                                {
                                    dataArray[parameters.indexOfDataArray].dlApiDataReady = true;
                                    Utils.appendLog("apiRequestWorker thread " + parameters.apiRequestWorkerThread + " finished with Region: " + parameters.region);
                                }
                                return;
                            }
                        }
                        apiRequestWorker_start(sender, parameters);
                    }
                }
                catch (Exception ex)
                {
                    Utils.exceptionLog("apiRequestWorker_DownloadDataCompleted", ex);
                }
                finally
                {
                    if (sender != null)
                        ((IDisposable)sender).Dispose();
                }
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
                Utils.exceptionLog("apiRequestWorker_RunWorkerCompleted", ex);
            }
            finally
            {
                try
                {
                    BackgroundWorker worker = sender as BackgroundWorker;
                    if (worker != null)
                    {
                        worker.DoWork -= apiRequestWorker_DoWork;
                        worker.RunWorkerCompleted -= apiRequestWorker_RunWorkerCompleted;
                        worker.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    Utils.exceptionLog("apiRequestWorker_RunWorkerCompleted finally", ex);
                }
            }
        }

        private void threads_trackBar_Scroll(object sender, EventArgs e)
        {
            Settings.viaUiThreadsAllowed = threads_trackBar.Value;
            UiThreadsAllowed_label.Text = "x" + threads_trackBar.Value;
        }

        private void checkedListBoxRegion_MouseClick(object sender, MouseEventArgs e)
        {
            int index = this.checkedListBoxRegion.IndexFromPoint(e.Location);
            if (index != Constants.INVALID_HANDLE_VALUE)
            {
                checkedListBoxRegion.SetItemChecked(index, checkedListBoxRegion.GetItemCheckState(index) != CheckState.Checked);
            }
        }

        private void Message_richTextBox_TextChanged(object sender, EventArgs e)
        {
            // set the current caret position to the end
            Message_richTextBox.SelectionStart = Message_richTextBox.Text.Length;
            // scroll it automatically
            Message_richTextBox.ScrollToCaret();
        }

        private void checkedListBoxRegion_SelectedValueChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < checkedListBoxRegion.Items.Count; i++)
            {
                if (checkedListBoxRegion.GetItemCheckState(i) == CheckState.Checked)
                {
                    start_button.Enabled = true;
                    return;
                }
            }
            start_button.Enabled = false;
        }

        public void programQuit(object sender, EventArgs e)
        {
            Utils.appendLog("User command: exit application");
            Application.Exit();
        }
    }
}

