using System;
using System.Collections.Generic;

namespace Umizoo.Infrastructure
{
    /// <summary>
    /// 表示分页数据
    /// </summary>
    public class PageResult<T>
    {
        /// <summary>
        /// 空数据
        /// </summary>
        public static readonly PageResult<T> Empty = new PageResult<T>();

        private PageResult()
        { }

        /// <summary>
        /// 构造实例
        /// </summary>
        /// <param name="totalRecords">总记录个数</param>
        /// <param name="pageSize">每页显示记录</param>
        /// <param name="pageIndex">当前页索引</param>
        /// <param name="data">页面数据</param>
        public PageResult(int totalRecords, int pageSize, int pageIndex, IEnumerable<T> data)
        {
            this.TotalRecords = totalRecords;
            this.PageSize = pageSize;
            this.PageIndex = pageIndex;
            this.TotalPages = (int)Math.Ceiling((double)totalRecords / (double)pageSize);

            this.Data = data;
        }

        /// <summary>
        /// 获取或设置总记录数。
        /// </summary>
        public int TotalRecords { get; private set; }
        /// <summary>
        /// 获取或设置页数。
        /// </summary>
        public int TotalPages { get; private set; }
        /// <summary>
        /// 获取或设置页面大小。
        /// </summary>
        public int PageSize { get; private set; }
        /// <summary>
        /// 获取或设置页码。
        /// </summary>
        public int PageIndex { get; private set; }
        /// <summary>
        /// 获取或设置页面数据
        /// </summary>
        public IEnumerable<T> Data { get; private set; }
    }
}
