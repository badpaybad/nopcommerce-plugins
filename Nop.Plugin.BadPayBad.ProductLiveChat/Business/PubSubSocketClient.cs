using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Task = System.Threading.Tasks.Task;

namespace Nop.Plugin.BadPayBad.ProductLiveChat.Business
{
    public class PubSubSocketClient : IDisposable
    {
        readonly Socket _client;
        readonly EndPoint _serverEndPoint;
        private bool _isStop;

        readonly string _serverIp;
        readonly int _serverPort;

        private bool _isBooted;

        ConcurrentDictionary<string, Action<string>> _actions = new ConcurrentDictionary<string, Action<string>>();

        readonly ConcurrentQueue<Action> _wating = new ConcurrentQueue<Action>();

        public PubSubSocketClient(string serverIp = "127.0.0.1", int serverPort = 12345)
        {
            _serverIp = serverIp;
            _serverPort = serverPort;

            _client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            var ipServer = IPAddress.Parse(_serverIp);
            _serverEndPoint = new IPEndPoint(ipServer, _serverPort);

            Thread threadClient = new Thread(() =>
            {
                EndPoint ep = null;
                while (ep == null)
                {
                    ep = _client.LocalEndPoint;
                    if (ep != null) break;
                    Ping();
                    Thread.Sleep(1000);
                }
                _isBooted = true;
                Console.WriteLine("Client socket {0} booted ...", ep);
                while (!_isStop)
                {
                    try
                    {
                        byte[] data = new byte[1024];
                        var recv = _client.ReceiveFrom(data, ref ep);

                        var msg = Encoding.Unicode.GetString(data, 0, recv);

                        if (string.IsNullOrEmpty(msg)) continue;
                        var idx = msg.IndexOf(",", StringComparison.Ordinal);
                        var channelKey = msg.Substring(0, idx);
                        var msgData = msg.Substring(idx + 1);

                        Task.Run(() =>
                        {
                            Action<string> a;
                            if (_actions.TryGetValue(channelKey, out a))
                            {
                                if (a != null) a(msgData);
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                    finally
                    {
                        Thread.Sleep(0);
                    }
                }
            });
            threadClient.IsBackground = true;
            threadClient.Start();

            new Thread(() =>
            {
                while (!_isBooted)
                {
                    Thread.Sleep(100);
                }

                while (!_isStop)
                {
                    try
                    {
                        Action a;
                        while (_wating.TryDequeue(out a))
                        {
                            var a1 = a;

                            if (a1 != null)
                            {
                                a1();
                                Thread.Sleep(500);
                            }
                        }
                    }
                    finally
                    {
                        Thread.Sleep(0);
                    }
                }
            }).Start();
        }

        void Ping()
        {
            try
            {
                Thread.Sleep(100);
                var buffer = Encoding.Unicode.GetBytes(
                    string.Format("Ping,{0},{1}", "", ""));
                _client.SendTo(buffer, _serverEndPoint);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public void Publish(string chanelKey, string message)
        {
            _wating.Enqueue(() =>
            {
                var buffer = Encoding.Unicode.GetBytes(
                    string.Format("Publish,{0},{1}", chanelKey, message));
                _client.SendTo(buffer, _serverEndPoint);
            });
        }

        public void Subcribe(string channelKey, Action<string> callBack)
        {
            _wating.Enqueue(() =>
            {
                _actions.AddOrUpdate(channelKey, (nk) => callBack, (k, v) => callBack);

                var buffer = Encoding.Unicode.GetBytes(
                    string.Format("Subcribe,{0},{1}", channelKey, ""));
                _client.SendTo(buffer, _serverEndPoint);
            });
        }

        public void Unsubcribe(string chanelKey)
        {
            _wating.Enqueue(() =>
            {
                var buffer = Encoding.Unicode.GetBytes(
                    string.Format("Unsubcribe,{0},{1}", chanelKey, ""));
                _client.SendTo(buffer, _serverEndPoint);
            });
        }

        public void Dispose()
        {
            _isStop = true;
        }
    }
}