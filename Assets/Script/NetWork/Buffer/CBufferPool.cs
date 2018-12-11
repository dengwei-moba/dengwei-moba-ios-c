using System.Threading;

namespace ClientGame.Net
{
    /// <summary>
    /// 内存池
    /// libing 缓存池有点时候战斗逻辑中会出现NullReferenceException，原因不明，需要检查逻辑
    /// 目前采用全部新创建内存的块的方式执行
    /// </summary>
    public sealed class CBufferPool
    {
        //public const int MAX_POOL = 100;
        //public const int BUFFER_SIZE = 1024;
        //private static readonly CBuffer[] m_Pools = new CBuffer[MAX_POOL];

        //public static void ClearEmpty()
        //{
        //    lock (m_Pools)
        //    {
        //        for (int i = 0; i < m_Pools.Length; i++)
        //            m_Pools[i] = null;
        //    }
        //}

        //private BufferPool() { }
        public static CBuffer GetBuffer(int bufSize)
        {
            CBuffer tempBuffer = null;

            //lock (m_Pools)
            //{
            //    for (int i = 0; i < m_Pools.Length; i++)
            //    {
            //        tempBuffer = m_Pools[i];
            //        if (tempBuffer != null && tempBuffer.MemorySize >= bufSize)
            //        {
            //            m_Pools[i] = null;
            //            tempBuffer.Reset(bufSize);
            //            break;
            //        }
            //    }
            //}
            //if (tempBuffer == null)
            //{
            //int memorySize = System.Math.Max(bufSize, BUFFER_SIZE);
            tempBuffer = new CBuffer(bufSize, bufSize);
            //}

            return tempBuffer;
        }


        public static void ReleaseBuffer(CBuffer buffer)
        {
            //if (buffer == null)
            //    return;

            //lock (m_Pools)
            //{
            //    for (int i = 0; i < m_Pools.Length; i++)
            //    {
            //        if (m_Pools[i] == null)
            //        {
            //            m_Pools[i] = buffer;
            //            break;
            //        }
            //    }
            //}

            //buffer = null;
        }
    }
}