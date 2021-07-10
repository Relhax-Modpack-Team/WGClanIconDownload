using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WoTClanIconDownloadConsole
{
    public class ConcurrentWebClient : WebClient
    {
        public int ConcurrentConnections { get; set; } = Constants.DefaultConcurrentConnectionsPerRegion;

        protected override WebRequest GetWebRequest(Uri address)
        {
            HttpWebRequest req = (HttpWebRequest)base.GetWebRequest(address);

            if (req.ServicePoint.ConnectionLimit != ConcurrentConnections)
                req.ServicePoint.ConnectionLimit = ConcurrentConnections;

            return (WebRequest)req;
        }
    }
}
