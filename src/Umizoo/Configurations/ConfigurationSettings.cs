
namespace Umizoo.Configurations
{
    using System.Configuration;

    using Umizoo.Infrastructure.Utilities;

    public static class ConfigurationSettings
    {

        static ConfigurationSettings()
        {
            HandleRetrytimes = ConfigurationManager.AppSettings["umizoo.retry_count"].ChangeIfError(5);
            HandleRetryInterval = ConfigurationManager.AppSettings["umizoo.retry_interval"].ChangeIfError(1000);
            MaxRequests = ConfigurationManager.AppSettings["umizoo.request_limit"].ChangeIfError(2000);
            OperationTimeout = ConfigurationManager.AppSettings["umizoo.request_timeout"].ChangeIfError(120);
            CommandFilterEnabled = ConfigurationManager.AppSettings["umizoo.filter_command"].ChangeIfError(false);
            QueryFilterEnabled = ConfigurationManager.AppSettings["umizoo.filter_query"].ChangeIfError(false);
            ParallelQueryThead = ConfigurationManager.AppSettings["umizoo.thread_query"].ChangeIfError(1);
            OuterAddress = ConfigurationManager.AppSettings["umizoo.service_outeraddress"].IfEmpty(() => string.Format("{0}:9999", SocketUtil.GetLocalIPV4()));
            InnerAddress = ConfigurationManager.AppSettings["umizoo.service_inneraddress"].IfEmpty(() => string.Format("{0}:9999", SocketUtil.GetLocalIPV4("192.", "10.", "127.")));
            ServiceName = ConfigurationManager.AppSettings["umizoo.service_name"].IfEmpty("Umizoo");
        }

        /// <summary>
        /// 消息处理器运行过程中遇到错误的重试次数
        /// 默认5次
        /// </summary>
        public static int HandleRetrytimes { get; set; }

        /// <summary>
        /// 消息处理器运行过程中遇到错误等待下次执行的间隔时间（毫秒）
        /// 默认1000ms
        /// </summary>
        public static int HandleRetryInterval { get; set; }

        /// <summary>
        /// 并行查询线程
        /// </summary>
        public static int ParallelQueryThead { get; set; }

        /// <summary>
        /// 最大处理请求数
        /// 默认为2000
        /// </summary>
        public static int MaxRequests { get; set; }

        /// <summary>
        /// 操作超时设置(单位:秒)
        /// 默认为120秒
        /// </summary>
        public static int OperationTimeout { get; set; }

        /// <summary>
        /// 是否启用命令过滤器
        /// 默认为false，不启用
        /// </summary>
        public static bool CommandFilterEnabled { get; set; }

        /// <summary>
        /// 是否启用命令过滤器
        /// 默认为false，不启用
        /// </summary>
        public static bool QueryFilterEnabled { get; set; }

        /// <summary>
        /// 用于局域网外的地址
        /// </summary>
        public static string OuterAddress { get; set; }

        /// <summary>
        /// 用于局域网内的地址
        /// </summary>
        public static string InnerAddress { get; set; }

        /// <summary>
        /// 服务名称
        /// </summary>
        public static string ServiceName { get; set; }
    }
}
