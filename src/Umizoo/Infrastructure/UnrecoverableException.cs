
namespace Umizoo.Infrastructure
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// 表示这是一个重试也无法恢复的异常
    /// </summary>
    public class UnrecoverableException : ApplicationException
    {
        public UnrecoverableException()
            : this("This is an exception that cannot be retried.")
        { }

        public UnrecoverableException(string errorMessage)
            : this(errorMessage, -1)
        { }

        public UnrecoverableException(string errorMessage, int errorCode)
            : this(errorMessage, errorCode, null)
        { }

        public UnrecoverableException(string errorMessage, int errorCode, Exception innerException)
            : base(errorMessage, innerException)
        {
            this.HResult = errorCode;
        }

        protected UnrecoverableException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }

        protected UnrecoverableException(SerializationInfo info)
            : this(info.GetString("Message"), info.GetInt32("HResult"))
        { }
    }
}
