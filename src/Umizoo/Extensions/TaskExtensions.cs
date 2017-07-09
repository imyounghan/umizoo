
namespace Umizoo
{
    using System;
    using System.Threading.Tasks;

    public static class TaskExtensions
    {
        public static TResult WaitResult<TResult>(this Task<TResult> task, int millisecondsTimeout, TResult defautValue = default(TResult))
        {
            if (millisecondsTimeout > 0)
            {
                if (task.Wait(millisecondsTimeout))
                {
                    return task.Result;
                }
                return defautValue;
            }

            return task.Result;
        }

        public static TResult WaitResult<TResult>(this Task<TResult> task, int millisecondsTimeout, Func<TResult> defaultValueFactory)
        {
            if(millisecondsTimeout > 0) {
                if(task.Wait(millisecondsTimeout)) {
                    return task.Result;
                }
                return defaultValueFactory.Invoke();
            }

            return task.Result;
        }
    }
}
