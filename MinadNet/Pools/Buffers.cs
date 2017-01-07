using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MinadNet.Pools
{
    public class BufferPool
    {
        private readonly ConcurrentBag<ByteBuffer> buffers = new ConcurrentBag<ByteBuffer>();
        private readonly int BUFSIZE;

        public BufferPool(int bufSize)
        {
            BUFSIZE = bufSize;
        }

        public ByteBuffer Get()
        {
            ByteBuffer buf = null;
            buffers.TryTake(out buf);
            if (buf == null) { buf = new ByteBuffer(BUFSIZE, this); }
            return buf;
        }

        internal void Return(ByteBuffer buf)
        {
            if(buf.isFromPool)
                buffers.Add(buf);
        }
    }
}
