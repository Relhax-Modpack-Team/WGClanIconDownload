using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Forms;

namespace WGClanIconDownload
{
    public class Utils
    {
        public static Object _lockerLog = new Object();

        public static void clearLog()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(Settings.errorLogFile));
                File.Create(Settings.errorLogFile).Dispose();
                appendLog("Log opened. (Time zone: " + DateTime.Now.ToString("\"GMT\" zzz") + ")");
            }
            catch
            {
                MessageBox.Show("Program cannot create the Log-File and will now terminate.\n\nMaybe not enough access rights to create files and folders?", "Critical Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(9);
            }
        }

        public static void appendLog(string info)
        {
            lock (_lockerLog)
            {
                string currentDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff   ");
                info = info.Replace("\n", "\n" + string.Concat(Enumerable.Repeat(" ", 26))) + "\n";
                bool _ready = false;
                while (!_ready)
                {
                    try
                    {
                        File.AppendAllText(Settings.errorLogFile, currentDate + info);
                        _ready = true;
                    }
                    catch { }
                }
            }
        }

        public static bool isDebugMode()
        {
#if DEBUG
            return true;
#else   
                return false;
#endif
        }

        // print all information about the object to the logfile
        public static void dumpObjectToLog(string objectName, object n)
        {
            Utils.appendLog(string.Format("----- dump of object {0} ------", objectName));
            foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(n))
            {
                string name = descriptor.Name;
                object value = descriptor.GetValue(n);
                switch (value)
                {
                    case null:
                        value = "(null)";
                        break;
                    case "":
                        value = "(string with lenght 0)";
                        break;
                    default:
                        break;
                }
                Utils.appendLog(string.Format("{0}={1}", name, value));
            }
            Utils.appendLog("----- end of dump ------");
        }

        /// <summary>
        /// default logging function of exception informations
        /// </summary>
        /// <param e=Exception>the exception object that would be catched</param>
        public static void exceptionLog(Exception e)
        {
            exceptionLog("", e);
        }

        public static void exceptionLog(string name, Exception e)
        {
            string msg = "";
            if (name.Equals(""))
                msg += "EXCEPTION (call stack traceback):";
            else
                msg += string.Format("EXCEPTION (call stack traceback) => Marker: {0}\n", name);
            try { msg += e.StackTrace; } catch { };
            try { msg += "\nmessage: " + e.Message; } catch { };
            try { msg += "\nsource: " + e.Source; } catch { };
            try { msg += "\ntarget: " + e.TargetSite; } catch { };
            try { msg += "\nInnerException: " + e.InnerException; } catch { };
            try { appendLog(msg); } catch { };
            try { if (e.Data != null) Utils.dumpObjectToLog("Data", e.Data); } catch { };             /// https://msdn.microsoft.com/de-de/library/system.exception.data(v=vs.110).aspx
        }

        public static string IntToHex(int i)
        {
            return IntToHex(i, 4);
        }

        /// <summary>
        /// https://stackoverflow.com/questions/1139957/c-sharp-convert-integer-to-hex-and-back-again
        /// </summary>
        /// <param name="i">number to be converted</param>
        /// <param name="x">How many places should have the hex string at least</param>
        /// <returns></returns>
        public static string IntToHex(int i,int x)
        {
            return "0x"+i.ToString("X"+x);
        }

        public static Int32 HexToInt(string s)                  /// https://stackoverflow.com/questions/1139957/c-sharp-convert-integer-to-hex-and-back-again
        {
            return int.Parse(s, System.Globalization.NumberStyles.HexNumber);
        }

        public IList createList(Type myType)               //  https://stackoverflow.com/questions/2493215/create-list-of-variable-type
        {
            Type genericListType = typeof(List<>).MakeGenericType(myType);
            return (IList)Activator.CreateInstance(genericListType);
        }

        /// <summary>
        /// https://msdn.microsoft.com/de-de/library/system.diagnostics.stopwatch.elapsed(v=vs.110).aspx
        /// </summary>
        /// <param name="ts">stopWatch.Elapsed</param>
        /// <returns></returns>
        public static string getStopWatchTime(TimeSpan ts)
        {
            // Format and display the TimeSpan value. (elapsedTime)
            return String.Format("{0:00}:{1:00}:{2:00}", ts.Hours, ts.Minutes, ts.Seconds);
        }

        /// <summary>
        /// https://stackoverflow.com/questions/16387904/how-to-catch-404-webexception-for-webclient-downloadfileasync
        /// </summary>
        /// <param name="err">e.error</param>
        /// <returns></returns>
        public static HttpStatusCode GetHttpStatusCode(System.Exception err)
        {
            if (err is WebException)
            {
                WebException we = (WebException)err;
                if (we.Response is HttpWebResponse)
                {
                    HttpWebResponse response = (HttpWebResponse)we.Response;
                    return response.StatusCode;
                }
                else
                {
                    Utils.appendLog("we.Data: "+we.Data);
                    dumpObjectToLog("we.Data: ", we.Data);
                    dumpObjectToLog("key: ",we.Data.Keys);
                    Utils.appendLog("we.HelpLink: " + we.HelpLink);
                    Utils.appendLog("we.HResult: " + we.HResult);
                    Utils.appendLog("we.InnerException: " + we.InnerException);
                    Utils.appendLog("we.Message: " + we.Message);
                    Utils.appendLog("we.Response: " + we.Response);
                    Utils.appendLog("we.Source: " + we.Source);
                    Utils.appendLog("we.Status: " + we.Status);
                    Utils.appendLog("we.TargetSite: " + we.TargetSite);
                    dumpObjectToLog("TargetSite: ", we.TargetSite);
                }
                Utils.appendLog("GetHttpStatusCode: err.ToString()" + we.ToString());
            }
            Utils.appendLog("GetHttpStatusCode: we.Respons()" + err.ToString());

            return 0;
        }
    }

    /// <summary>
    /// http://www.mycsharp.de/wbb2/thread.php?threadid=62769
    /// </summary>
    public class UnlinkedBitmap
    {
        // private MemoryStream memstream;
        // private Bitmap bmp;
        public static Bitmap FromFile(string filename, bool http)
        {
            try
            {
                Byte[] buffer = null;
                if (http)
                {
                    AwesomeWebClient client = new AwesomeWebClient();
                    buffer = client.DownloadData(filename);
                }
                else
                {
                    buffer = File.ReadAllBytes(filename);
                }
                MemoryStream memstream = new MemoryStream(buffer);
                return new Bitmap(memstream);
            }
            catch (Exception ex)
            {
                Utils.exceptionLog("UnlinkedBitmap Bitmap FromFile", ex);
                return null;
            }
            
        }
    }

    /// <summary>
    /// https://stackoverflow.com/questions/866350/how-can-i-programmatically-remove-the-2-connection-limit-in-webclient
    /// </summary>
    public class AwesomeWebClient : WebClient
    {
        protected override WebRequest GetWebRequest(Uri address)
        {
            HttpWebRequest req = (HttpWebRequest)base.GetWebRequest(address);
            req.ServicePoint.ConnectionLimit = 10;
            return (WebRequest)req;
        }
    }
}
