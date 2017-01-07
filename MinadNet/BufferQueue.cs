using System.Collections.Concurrent;

namespace MinadNet
{
    public sealed class BufferQueue
    {
        private readonly ConcurrentQueue<ByteBuffer> buffers = new ConcurrentQueue<ByteBuffer>();

        public ByteBuffer TryDequeue()
        {
            ByteBuffer buf = null;
            this.buffers.TryDequeue(out buf);
            return buf;
        }

        public void Enqueue(ByteBuffer buf) { buffers.Enqueue(buf); }
        public int Count() => buffers.Count;
    }
}
