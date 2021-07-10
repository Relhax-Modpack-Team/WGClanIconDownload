using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoTClanIconDownloadConsole
{
    public static class Constants
    {
        public const string WgApplicationID = "d0bfec3ab1967d9582a73fef7d86ff02";

        public const int WgApiDefaultLoadLimit = 100;

        public const string WgApiResultOk = "ok";

        public const string ApiClansTotalUrlEscaped = "https://api.worldoftanks.{0}/wot/clans/list/?application_id={1}&fields=-tag,-emblems,-created_at,-color,-clan_id,-members_count,-name&language=en&limit=1&page_no=1";

        public const string ApiClansIconsUrlEscaped = "https://api.worldoftanks.{0}/wot/clans/list/?application_id={1}&fields=-emblems.x195,-emblems.x24,-emblems.x256,-emblems.x64,-created_at,-color,-clan_id,-members_count,-name&limit={2}&page_no={3}";

        public const string DefaultIconFolderStructure = @"download\{region}\res_mods\mods\shared_resources\xvm\res\clanicons\{region}\clan";

        public const int DefaultConcurrentConnectionsPerRegion = 2;

        /// <summary>
        /// The Startup root path of the application. Does not include the application name
        /// </summary>
        public static readonly string ApplicationStartupPath = AppDomain.CurrentDomain.BaseDirectory;

        public static readonly Dictionary<Region, string> DomainMapper = new Dictionary<Region, string>()
        {
            { Region.NA, "com" },
            { Region.EU, "eu" },
            { Region.RU, "ru" },
            { Region.ASIA, "asia" }
        };

        public static readonly Dictionary<Region, string> XvmLocationMapper = new Dictionary<Region, string>()
        {
            { Region.NA, "NA" },
            { Region.EU, "EU" },
            { Region.RU, "RU" },
            { Region.ASIA, "ASIA" }
        };
    }
}
