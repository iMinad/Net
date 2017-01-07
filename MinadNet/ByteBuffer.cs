using MinadNet.Pools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace MinadNet
{
    public sealed class ByteBuffer
    {
        private static readonly IndexOutOfRangeException IE = new IndexOutOfRangeException();

        private byte[] b;
        public int Offset { get; private set; }
        public int Length { get; private set; }
        public byte[] Buf { get { return b; }  } //Private set!

        internal readonly bool isFromPool;
        internal readonly BufferPool pool;
        private int refs;

        internal ByteBuffer(int size, BufferPool p)
        {
            pool = p;
            isFromPool = pool != null;
            b = new byte[size];
            Offset = 0;
            Length = Buf.Length;
        }

        public ByteBuffer(int size)
        {
            b = new byte[size];
            Offset = 0;
            Length = size;
        }

        public ByteBuffer(byte[] buf)
        {
            b = buf;
            Offset = 0;
            Length = b.Length;
        }

        public ByteBuffer(string str)
        {
            b = Encoding.UTF8.GetBytes(str);
            Offset = 0;
            Length = b.Length;
        }

        public void Set(int offset, int length)
        {
            int bufLength = Buf.Length;
            if (offset + length > bufLength) throw IE;
            Offset = offset;
            Length = length;
        }

        public void Set(byte[] buf, int offset, int length)
        {
            int bufLength = buf.Length;
            if (offset + length > bufLength) throw IE;
            b = buf;
            Offset = offset;
            Length = length;
        }

        //Write string to buffer.
        public void Set(string str, int offset=0)
        {
            byte[] buf = Encoding.UTF8.GetBytes(str);
            Write(buf, offset, buf.Length);
            Set(offset, buf.Length+offset);
        }

        public void Write(byte[] buf, int pos, int count)
        {
            if (pos + count > Buf.Length) Array.Resize<byte>(ref b, Buf.Length+pos+count);
            Buffer.BlockCopy(buf, 0, b, pos, count);
        }

        public void Write(string str, int pos, int count)
        {
            byte[] buf = Encoding.UTF8.GetBytes(str);
            Write(buf, pos, buf.Length);
        }

        public void Write(ByteBuffer buf)
        {
            Write(buf.Buf, buf.Offset, buf.Length);
        }

        public override string ToString()
        {
            return Encoding.UTF8.GetString(b, Offset, Length);
        }

        public string ToString(int offset, int count)
        {
            return Encoding.UTF8.GetString(b, offset, count);
        }

        public void Reset()
        {
            Set(0, Buf.Length);
        }

        public int Ref()
        {
            if (!isFromPool) return 0;
            return Interlocked.Increment(ref refs);
        }

        public int DeRef()
        {
            if (!isFromPool) return 0;
            int result = Interlocked.Decrement(ref refs);
            if (Interlocked.Decrement(ref refs) == 0)
            {
                pool.Return(this);
            }

            return result;
        }
    }
}
