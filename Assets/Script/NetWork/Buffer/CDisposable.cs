using System;

namespace ClientGame.Net
{
    public abstract class CDisposable : IDisposable
    {
        protected bool m_Disposed; // 是否回收完毕

        ~CDisposable()
        {
            if (!m_Disposed)
            {
                OnDispose(false);
                m_Disposed = true;
            }
        } 
 

        public void Dispose()
        {
            if (!m_Disposed)
            {
                OnDispose(true);
                GC.SuppressFinalize(this);
                m_Disposed = true;
            }
        }

        /// <summary>
        /// 释放那些实现IDisposable接口的托管对象
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void OnDispose(bool disposing)
        {
            if (m_Disposed) return; //如果已经被回收，就中断执行
            if (disposing)
            {
                // 释放那些实现IDisposable接口的托管对象
            }

            // 释放非托管资源，设置对象为null
            m_Disposed = true;
        }
    }
}