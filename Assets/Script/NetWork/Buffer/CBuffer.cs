using System;

namespace ClientGame.Net
{
    public sealed class CBuffer : CDisposable
    {
        public static CBuffer Create(int bufSize)
        {
            return CBufferPool.GetBuffer(bufSize);
        }

        private byte[] m_Bytes;
        private int m_Length;
        private int m_BufSize;

        public byte[] Bytes { get { return m_Bytes; } }
        public int Length { get { return m_Length; } }
        public int BufSize { get { return m_BufSize; } }
        public int MemorySize { get { return m_Bytes.Length; } }

        public bool IsFull { get { return m_Length == m_BufSize; } }

        public CBuffer(int memorySize, int bufSize)
        {
            Malloc(memorySize);
            m_BufSize = bufSize;
            m_Length = 0;
        }

        public CBuffer Clone()
        {
            CBuffer buf = CBuffer.Create(BufSize);
            buf.Fill(this);
            return buf;
        }

        public void Reset(int bufSize)
        {
            m_BufSize = bufSize;

            //for (int i = 0; i < this.m_Bytes.Length; i++)
            //    Buffer.SetByte(this.m_Bytes, i, 0);

            Clear();
        }

        //public void Clear

        public void Clear()
        {
            m_Length = 0;
            m_Disposed = false;
        }

        public int GetRemainSize()
        {
            return MemorySize - Length;
        }

        public int GetLimitRemain()
        {
            return m_BufSize - m_Length;
        }

        private void Malloc(int size)
        {
            if (this.m_Bytes == null || this.MemorySize < size)
            {
                byte[] newBytes = new byte[size];

                if (this.m_Length > 0)
                {
                    Buffer.BlockCopy(this.m_Bytes, 0, newBytes, 0, this.m_Length);
                }

                this.m_Bytes = newBytes;
            }
        }


        public int Fill(byte[] bytes, int index, int count, bool limitBufSize = false)
        {
            if (limitBufSize)
            {
                count = Math.Min(GetLimitRemain(), count);
            }

            Malloc(this.m_Length + count);
            Buffer.BlockCopy(bytes, index, this.m_Bytes, this.m_Length, count);
            this.m_Length += count;
            return count;
        }

        public int Fill(byte[] bytes, int count, bool limitBufSize = false)
        {
            return Fill(bytes, 0, count, limitBufSize);
        }

        public int Fill(byte[] bytes, bool limitBufSize = false)
        {
            return Fill(bytes, 0, bytes.Length, limitBufSize);
        }

        public int Fill(CBuffer buffer, bool limitBufSize = false)
        {
            return Fill(buffer.Bytes, buffer.Length, limitBufSize);
        }

        public int Fill(System.IO.Stream stream, int count, bool limitBufSize = false)
        {
            if (limitBufSize)
            {
                count = Math.Min(GetLimitRemain(), count);
            }

            Malloc(this.m_Length + count);

            int length = stream.Read(this.m_Bytes, this.m_Length, count);
            this.m_Length += length;
            return length;
        }

        /// <summary>
        /// 释放那些实现IDisposable接口的托管对象
        /// </summary>
        /// <param name="disposing"></param>
        protected override void OnDispose(bool disposing)
        {
            if (disposing)
            {
                // 释放那些实现IDisposable接口的托管对象
            }

            CBufferPool.ReleaseBuffer(this);
            m_Length = 0;
        }
    }
}