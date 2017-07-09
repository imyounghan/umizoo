
namespace Umizoo.Messaging
{
    using System.Runtime.Serialization;
    using Umizoo.Infrastructure;

    /// <summary>
    /// 分页查询参数的抽象类
    /// </summary>
    [DataContract]
    public abstract class PageQuery : MessageBase, IQuery
    {
        /// <summary>
        /// 当前页码
        /// </summary>
        [DataMember(Name = "pageIndex")]
        public int PageIndex { get; set; }

        /// <summary>
        /// 当前页显示数量大小
        /// </summary>
        [DataMember(Name = "pageSize")]
        public int PageSize { get; set; }
    }
}
