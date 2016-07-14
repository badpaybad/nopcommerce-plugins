using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Task = System.Threading.Tasks.Task;

namespace Nop.Plugin.BadPayBad.ProductLiveChat.Business
{
    public class PubSubSocketServer : IDisposable
    {
        private string _host;
        private int _port;
        private Thread _serverThread;

        //static PubSubSocketServer _instance = new PubSubSocketServer();

        //public static PubSubSocketServer Instance
        //{
        //    get { return _instance; }
        //}

        private bool _isStarted;
        public event Action<string> Report;

        public PubSubSocketServer(string host = "127.0.0.1", int port = 12345)
        {
            _host = host;
            _port = port;

            _serverThread = new Thread(() =>
            {
                try
                {
                    IPAddress ipV4 = IPAddress.Parse(_host);

                    _serverEp = new IPEndPoint(ipV4, _port);
                    _server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    _server.Bind(_serverEp);

                    Console.WriteLine("Server: {0}", _serverEp);
                    ServerListening(_server);
                }
                catch (Exception ex)
                {
                    DoReport("-2:" + ex);
                }
            });
            _serverThread.IsBackground = true;
        }

        private Socket _server;
        private IPEndPoint _serverEp;

        public void Start()
        {
            if (_isStarted) return;

            _isStarted = true;
            _serverThread.Start();
        }

        public void Stop()
        {
            _isStarted = false;
            //try
            //{
            //    _serverThread.Abort();
            //}
            //catch
            //{
            //}
        }

        void DoReport(string msg)
        {
            if (Report != null)
            {
                Task.Run(() => { Report(msg); });
            }
        }

        private void ServerListening(Socket server)
        {
            DoReport("1:Server started");

            while (_isStarted)
            {
                try
                {
                    EndPoint clientEp = new IPEndPoint(IPAddress.Any, 0);

                    var recv = 0;
                    var data = new byte[1024];
                    recv = server.ReceiveFrom(data, ref clientEp);
                    var ep = clientEp;

                    string msgFromClient = Encoding.Unicode.GetString(data, 0, recv);

                    DoReport("0:" + msgFromClient);

                    if (string.IsNullOrEmpty(msgFromClient)) continue;

                    var idx = msgFromClient.IndexOf(",", StringComparison.Ordinal);

                    var command = msgFromClient.Substring(0, idx);
                    var msgData = msgFromClient.Substring(idx + 1);

                    idx = msgData.IndexOf(",", StringComparison.Ordinal);

                    var channelKey = msgData.Substring(0, idx);
                    var realMsg = msgData.Substring(idx + 1);

                    var subcriber = ep.GetHashCode().ToString();

                    if (string.IsNullOrEmpty(command))
                    {
                        continue;
                    }
                    //todo: optimize if else by dictionary<string,action>
                    if (command == "Publish")
                    {
                        PubSubServices.Instance.Publish(channelKey, realMsg);
                    }
                    else if (command == "Subcribe")
                    {
                        PubSubServices.Instance.Subcribe(subcriber, channelKey, (msgToSend) =>
                        {
                            var buffer = Encoding.Unicode.GetBytes(channelKey+","+ msgToSend);
                            server.SendTo(buffer, buffer.Length
                                , SocketFlags.None, ep);
                            return true;
                        });
                    }
                    else if (command == "Unsubcribe")
                    {
                        PubSubServices.Instance.Unsubcribe(subcriber, channelKey);
                    }
                    else
                    {
                    }
                }
                catch (Exception ex)
                {
                    DoReport("-2:" + ex);
                }
                finally
                {
                    Thread.Sleep(0);
                }
            }

            DoReport("1:Server stoped");
        }

        public void Dispose()
        {
            Stop();
            try
            {
                _serverThread.Abort();
            } catch { }
        }
    }
}