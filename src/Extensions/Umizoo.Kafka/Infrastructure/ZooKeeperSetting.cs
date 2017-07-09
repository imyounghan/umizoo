
namespace Umizoo.Infrastructure
{
    using System.Configuration;

    public static class ZooKeeperSetting
    {
        static ZooKeeperSetting()
        {
            SessionTimeoutMs = 3000;
            ConnectionTimeoutMs = 4000;
            SyncTimeMs = 8000;

            Address = ConfigurationManager.AppSettings["umizoo.zookeeper_address"];
        }

        public static int SessionTimeoutMs { get; set; }

        public static int ConnectionTimeoutMs { get; set; }

        public static int SyncTimeMs { get; set; }

        public static string Address { get; set; }
    }
}
