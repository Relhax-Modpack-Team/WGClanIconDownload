using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace WGClanIconDownload
{

    public partial class Mainform : Form
    {
        private List<ProgressBar> progressBar = new List<ProgressBar>() { };
        public List<ClassDataArray> dataArray = new List<ClassDataArray>() { };

        public Mainform()
        {
            InitializeComponent();

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
            // Utils.appendLog("buttonStart_Click");
            if (checkedListBoxRegion.Items.Count > 0)
            {
                int p = 0;
                // Kickoff the worker thread to begin it's DoWork function.
                for (int i = 0; i < checkedListBoxRegion.Items.Count; i++)
                {
                    if (checkedListBoxRegion.GetItemCheckState(i) == CheckState.Checked)
                    {
                        ProgressBar pB = new System.Windows.Forms.ProgressBar();
                        pB.Location = new System.Drawing.Point(24, 145+p*30);
                        pB.Name = "progressBar"+p;
                        pB.Size = new System.Drawing.Size(219, 19);
                        // this.progressBar[p].TabIndex = 3;
                        this.progressBar.Add(pB);
                        this.SuspendLayout();
                        p++;

                        //Change the status of the buttons on the UI accordingly
                        //The start button is disabled as soon as the background operation is started
                        //The Cancel button is enabled so that the user can stop the operation 
                        //at any point of time during the execution
                        start_button.Enabled = false;
                        /// https://stackoverflow.com/questions/10694271/c-sharp-multiple-backgroundworkers 
                        /// Create a background worker thread that ReportsProgress &
                        /// SupportsCancellation
                        /// Hook up the appropriate events.
                        for (int x = 0; x < 2; x++)
                        {
                            // Do selected stuff
                            // The parameters you want to pass to the do work event of the background worker.
                            EventArgsParameter parameters = new EventArgsParameter();//  = e.Argument as EventArgsParameter;       // the 'argument' parameter resurfaces here
                            parameters.region = (string)checkedListBoxRegion.Items[i];
                            parameters.indexOfDataArray = dataArray.Find(r => r.region == parameters.region).indexOfDataArray;
                            parameters.apiRequestWorkerThread = x;
                            dataArray[parameters.indexOfDataArray].currentPage = 1;
                            apiRequestWorker_start(sender, parameters);
                            Utils.appendLog("apiRequest RunWorkerAsync thread region: " + parameters.region + " thread: " + x + " started");
                        }
                    }
                }
                /*
                try
                {
                    downloadThreadHandler = new BackgroundWorker();
                    downloadThreadHandler.DoWork += downloadThreadHandler_DoWork;
                    // downloadThreadHandler.ProgressChanged += downloadThreadHandler_ProgressChanged;
                    downloadThreadHandler.WorkerReportsProgress = false;
                    downloadThreadHandler.RunWorkerCompleted += downloadThreadHandler_RunWorkerCompleted;
                    downloadThreadHandler.RunWorkerAsync();
                }
                catch (Exception ee)
                {
                    Utils.exceptionLog(ee);
                }
                */
            }
            else
            {
                Utils.appendLog("no selection, no work ;-)");
            }
        }

        void apiRequestWorker_start(object sender, EventArgsParameter parameters)
        {
            BackgroundWorker apiRequestWorker = new BackgroundWorker();
            apiRequestWorker.DoWork += new DoWorkEventHandler(apiRequestWorker_DoWork);
            apiRequestWorker.ProgressChanged += new ProgressChangedEventHandler(apiRequestWorker_ProgressChanged);
            apiRequestWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(apiRequestWorker_RunWorkerCompleted);
            apiRequestWorker.WorkerReportsProgress = true;
            apiRequestWorker.WorkerSupportsCancellation = true;
            apiRequestWorker.RunWorkerAsync(parameters);
        }

        /// <summary>
        /// Time consuming operations go here </br>
        /// i.e. Database operations,Reporting
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiRequestWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            EventArgsParameter parameters = (EventArgsParameter)e.Argument;
            var region = parameters.region;
            var indexOfDataArray = parameters.indexOfDataArray;
            var apiRequestWorkerThread = parameters.apiRequestWorkerThread;
            e.Result = (EventArgsParameter)e.Argument;

            int currentPage = 0;
            lock (dataArray)
            {
                currentPage = dataArray[indexOfDataArray].currentPage;
                dataArray[indexOfDataArray].currentPage++;
            }
            string url = string.Format(Settings.wgApiURL, dataArray[indexOfDataArray].url, Settings.wgAppID, Constants.limitApiPageRequest, currentPage);
            Utils.appendLog("Info: region: " + region + " thread: "+ apiRequestWorkerThread + " page: " + currentPage);


            //Handle the event for download complete
            AwesomeWebClient WebClient = new AwesomeWebClient();
            WebClient.DownloadDataCompleted += apiRequestWorker_DownloadDataCompleted;
            //Start downloading file
            WebClient.DownloadDataAsync(new Uri(url), parameters);
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
                // string region = parameters.region;
                int indexOfDataArray = parameters.indexOfDataArray;

                if (e.Error != null)
                {
                    Utils.appendLog("Error: download failed\n" + e.Error.ToString());
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
                            lock (dataArray)
                            {
                                dataArray[indexOfDataArray].total = ((int)resultPageApiJson.meta.total);
                            }
                            if ((int)resultPageApiJson.meta.count > 0)
                            {
                                clanData c;
                                for (var f = 0; f < (int)resultPageApiJson.meta.count; f++)
                                {
                                    c = new clanData();
                                    c.tag = (string)resultPageApiJson.data[f].tag;
                                    c.emblems = (string)resultPageApiJson.data[f].emblems.x32.portal;
                                    lock (dataArray)
                                    {
                                        dataArray[indexOfDataArray].clans.Add(c);
                                    }
                                }
                                apiRequestWorker_start(sender, parameters);
                            }
                            else   // es gibt keine Datensätze mehr und das holen der "Pages" ist abgeschlossen.
                            {
                                Utils.appendLog("apiRequestWorker_DownloadDataCompleted killed (meta count = 0)");
                            }
                        }
                    }
                }
            }
            catch (Exception ee)
            {
                Utils.exceptionLog(ee);
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
                    Utils.appendLog("all fine");
                }
            }
            catch (Exception el)
            {
                Utils.exceptionLog(el);
            }
        }
    }
}

