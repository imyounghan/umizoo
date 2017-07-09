using System.Collections.Generic;

namespace Umizoo.Infrastructure.Socketing.BufferManagement
{
    /// <summary>
    /// The pool item creator interface
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IPoolItemCreator<T>
    {
        /// <summary>
        /// Creates the items of the specified count.
        /// </summary>
        /// <param name="count">The count.</param>
        /// <returns></returns>
        IEnumerable<T> Create(int count);
    }
}
