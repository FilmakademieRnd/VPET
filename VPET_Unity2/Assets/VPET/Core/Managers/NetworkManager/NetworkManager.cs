using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using NetMQ;
using NetMQ.Sockets;

namespace vpet
{
    public class NetworkManager : ManagerInterface
    {
        public void Start()
        {
            ArrayList queue1;
            NetworkListener listener = new NetworkListener("1", "2", out queue1);


            Thread recorderThread = new Thread(new ThreadStart(listener.run));
            recorderThread.Start();
        }

        private class NetworkListener
        {
            private string m_ip;
            private string m_port;
            private ArrayList m_receiveMessageQueue;

            public NetworkListener(string ip, string port, out ArrayList receiveMessageQueue)
            {
                m_ip = ip;
                m_port = port;
                receiveMessageQueue = m_receiveMessageQueue;
            }

            //!
            //! client function, listening for messages in receiveMessageQueue from server (executed in separate thread)
            //!
            public void run()
            {
                AsyncIO.ForceDotNet.Force();
                using (var receiver = new SubscriberSocket())
                {
                    receiver.SubscribeToAnyTopic();

                    receiver.Connect("tcp://" + VPETSettings.Instance.serverIP + ":5556");

                    lastReceiveTime = currentTimeTime;

                    Debug.Log("Listener connected: " + "tcp://" + VPETSettings.Instance.serverIP + ":5556");
                    byte[] input;
                    while (isRunning)
                    {
                        if (receiver.TryReceiveFrameBytes(out input))
                        {
                            //Thread.Sleep(100);
                            //this.receiveMessageQueue.Clear();
                            this.receiveMessageQueue.Add(input);

                            lastReceiveTime = currentTimeTime;
                        }
                        else
                        {
                            listenerRestartCount = Mathf.Min(int.MaxValue, listenerRestartCount + 1);
                            // VPETSettings.Instance.msg = string.Format("Exception in Listener: {0}", listenerRestartCount);
                            if (currentTimeTime - lastReceiveTime > receiveTimeout)
                            {
                                // break;
                            }
                        }
                    }
                    receiver.Disconnect("tcp://" + VPETSettings.Instance.serverIP + ":5556");
                    receiver.Close();
                    receiver.Dispose();
                }
            }
        }
    }
}
