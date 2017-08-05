using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;

namespace WGClanIconDownload
{
    public class Utils
    {
        public static void clearLog()
        {
            File.Create(Settings.errorLogFile).Dispose();
            appendLog("Log opened. (Time zone: " + DateTime.Now.ToString("\"GMT\" zzz") + ")");
        }

        public static void appendLog(string info)
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
