using System;

namespace Umizoo.Infrastructure
{
    public struct OffsetPosition : IEquatable<OffsetPosition>
    {
        private int partitionId;
        private long offset;
        public OffsetPosition(int partitionId, long offset)
        {
            this.offset = offset;
            this.partitionId = partitionId;
        }
        
        public long Offset { get { return this.offset; } }

        public int PartitionId { get { return this.partitionId; } }

        bool IEquatable<OffsetPosition>.Equals(OffsetPosition other)
        {
            return this.PartitionId == other.PartitionId &&
                this.Offset == other.Offset;
        }
    }
}
