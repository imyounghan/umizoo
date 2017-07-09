namespace Umizoo.Infrastructure.Socketing.BufferManagement
{
    public interface IBufferPool : IPool<byte[]>
    {
        int BufferSize { get; }
    }
}
