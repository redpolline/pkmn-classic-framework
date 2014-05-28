﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

namespace GlobalTerminalService
{
    public abstract class GTServerBase
    {
        public GTServerBase(int port) : this(port, false, 4)
        {
        }

        public GTServerBase(int port, bool useSsl)
            : this(port, useSsl, 4)
        {
        }

        public GTServerBase(int port, bool useSsl, int threads)
        {
            Threads = threads;
            UseSsl = useSsl;
            m_workers = new List<Thread>(threads);
            // todo: enumerate nic bindings
            m_listener = new TcpListener(port);
            if (UseSsl)
            {
                m_cert = new X509Certificate2("cert.pfx", "letmein");
            }
        }

        public int Threads
        {
            get;
            private set;
        }

        protected bool UseSsl
        {
            get;
            set;
        }

        private List<Thread> m_workers;
        private object m_lock = new object();
        private bool m_closing;
        private TcpListener m_listener;
        private X509Certificate m_cert;

        public void BeginPolling()
        {
            lock (m_lock)
            {
                if (m_workers.Count > 0) return;

                m_closing = false;
                m_listener.Start();
                for (int x = 0; x < Threads; x++)
                {
                    Thread t = new Thread(MainLoop);
                    t.Start();
                }
            }
        }

        public void EndPolling()
        {
            lock (m_lock)
            {
                if (m_workers.Count == 0) return;

                m_listener.Stop();
                m_closing = true;
                // wait for worker threads to exit
                while (m_workers.Count > 0) { }
            }
        }

        private void MainLoop(object o)
        {
            while (!m_closing)
            {
                if (!m_listener.Pending())
                {
                    Thread.Sleep(5);
                    continue;
                }
                Stream s = AcceptRequest();
                if (s == null)
                {
                    Thread.Sleep(5);
                    continue;
                }

                byte[] data = new byte[4];
                s.Read(data, 0, 4);
                int length = BitConverter.ToInt32(data, 0);
                data = new byte[length];
                BitConverter.GetBytes(length).CopyTo(data, 0);
                s.Read(data, 4, length - 4); // todo: stop DoS by timing out blocking requests

                ProcessRequest(data, s);
            }
            m_workers.Remove(Thread.CurrentThread);
        }

        private Stream AcceptRequest()
        {
            // todo: handle case that another thread took the request and return null
            TcpClient c = m_listener.AcceptTcpClient();
            if (UseSsl)
            {
                SslStream sslClient = new SslStream(c.GetStream());
                sslClient.AuthenticateAsServer(m_cert);
                return sslClient;
            }
            else return c.GetStream();
        }

        protected abstract void ProcessRequest(byte[] data, Stream response);
    }
}
