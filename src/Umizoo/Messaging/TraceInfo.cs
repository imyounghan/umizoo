

namespace Umizoo.Messaging
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// 表示一个
    /// </summary>
    public class TraceInfo
    {
        //public static readonly TraceInfo Empty = new TraceInfo();


        private string traceId;

        private string traceAddress;

        private TraceInfo()
        { }

        public TraceInfo(string traceId, string traceAddress)
        {
            this.traceId = traceId;
            this.traceAddress = traceAddress;
        }
        
        public string Address
        {
            get
            {
                return this.traceAddress;
            }
        }

        /// <summary>
        /// 跟踪ID
        /// </summary>
        public string Id
        {
            get
            {
                return this.traceId;
            }
        }

        public override bool Equals(object obj)
        {
            var other = obj as TraceInfo;
            if(other == null) {
                return false;
            }

            return this.Id == other.Id && this.Address == other.Address;
        }

        public override int GetHashCode()
        {
            return this.traceId.GetHashCode();
        }
    }
}
