using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Nop.Plugin.BadPayBad.ProductLiveChat.Business
{
    public interface IPubSubServices
    {
        
        void PublishToAllChannel(string message);
        void SubcribeAllChannel(string subcriber, Func<string, bool> callBack);
        void Publish(string chanelKey, string message);
        void Subcribe(string subcriber, string channelKey, Func<string, bool> callBack);
        void Unsubcribe(string subcriber, string channelKey);
        void UnsubcribeAllChannel(string subcriber);
        void RemoveChannel(string channelKey);
    }

    public class PubSubServices : IPubSubServices, IDisposable
    {
        static readonly IPubSubServices _instance = new PubSubServices();

        public static IPubSubServices Instance
        {
            get { return _instance; }
        }

        readonly ConcurrentDictionary<string, List<string>> _chanels = new ConcurrentDictionary<string, List<string>>();

        readonly ConcurrentDictionary<string, List<string>> _subcribers =
            new ConcurrentDictionary<string, List<string>>();

        readonly ConcurrentDictionary<KeyValuePair<string, string>, Func<string, bool>> _sub_cha_action =
            new ConcurrentDictionary<KeyValuePair<string, string>, Func<string, bool>>();

        ConcurrentQueue<KeyValuePair<string, string>> _waiting = new ConcurrentQueue<KeyValuePair<string, string>>();

        ConcurrentQueue<KeyValuePair<KeyValuePair<string, string>, string>> _error =
            new ConcurrentQueue<KeyValuePair<KeyValuePair<string, string>, string>>();

        private bool _isStop;

        private PubSubServices()
        {
            new Thread(() =>
            {
                KeyValuePair<string, string> ck;
                while (!_isStop)
                {
                    try
                    {
                        while (_waiting.TryDequeue(out ck))
                        {
                            var ck1 = ck;
                            Task.Run(() => { SendToSubcribers(ck1.Key, ck1.Value); });
                        }
                    }
                    finally
                    {
                        Thread.Sleep(0);
                    }
                }

                while (_waiting.TryDequeue(out ck))
                {
                    var ck2 = ck;
                    Task.Run(() => { SendToSubcribers(ck2.Key, ck2.Value); });
                }
            }).Start();

            new Thread(() =>
            {
                KeyValuePair<KeyValuePair<string, string>, string> ck;
                while (!_isStop)
                {
                    try
                    {
                        while (_error.TryDequeue(out ck))
                        {
                            InvokeSendToSubcriber(ck.Key.Key, ck.Key.Value, ck.Value);
                        }
                    }
                    finally
                    {
                        Thread.Sleep(500);
                    }
                }

                while (_error.TryDequeue(out ck))
                {
                    InvokeSendToSubcriber(ck.Key.Key, ck.Key.Value, ck.Value);
                }
            }).Start();
        }

        private void SendToSubcribers(string channelKey, string msg)
        {
            List<string> subcribers;

            if (!_chanels.TryGetValue(channelKey, out subcribers)) return;

            if (subcribers.Count <= 0) return;

            foreach (var subcriber in subcribers)
            {
                InvokeSendToSubcriber(subcriber, channelKey, msg);
            }
        }

        void InvokeSendToSubcriber(string subcriber, string channelKey, string msg)
        {
            var ckAction = new KeyValuePair<string, string>(subcriber, channelKey);

            Func<string, bool> action;

            if (_sub_cha_action.TryGetValue(ckAction, out action))
            {
                Task.Run(() =>
                {
                    try
                    {
                        var success = action(msg);
                        //if (!success)
                        //{
                        //    _error.Enqueue(new KeyValuePair<string, string>(subcriber, msg));
                        //}
                    }
                    catch
                    {
                        _error.Enqueue(new KeyValuePair<KeyValuePair<string, string>, string>(
                            new KeyValuePair<string, string>(subcriber, channelKey)
                            , msg
                            ));
                    }
                });
            }
        }

        public void PublishToAllChannel(string message)
        {
            List<string> c = _subcribers.Select(i => i.Key).ToList();

            foreach (var chanel in c)
            {
                Publish(chanel, message);
            }
        }

        public void SubcribeAllChannel(string subcriber, Func<string, bool> callBack)
        {
            List<string> c = _chanels.Select(i => i.Key).ToList();

            foreach (var chanel in c)
            {
                Subcribe(subcriber, chanel, callBack);
            }
        }

        public void Publish(string chanelKey, string message)
        {
            _chanels.AddOrUpdate(chanelKey, (k) => new List<string>(), (k, v) => v ?? new List<string>());
            _waiting.Enqueue(new KeyValuePair<string, string>(chanelKey, message));
        }

        public void Subcribe(string subcriber, string channelKey, Func<string, bool> callBack)
        {
            _subcribers.AddOrUpdate(subcriber, (nk) => new List<string>() {channelKey}, (k, ov) =>
            {
                if (ov == null) return new List<string>() {channelKey};
                if (ov.Contains(channelKey)) return ov;

                ov.Add(channelKey);
                return ov;
            });

            _chanels.AddOrUpdate(channelKey
                , (nk) => new List<string>() {subcriber},
                (k, ov) =>
                {
                    if (ov == null) return new List<string>() {subcriber};

                    if (ov.Contains(subcriber)) return ov;

                    ov.Add(subcriber);
                    return ov;
                });

            var keyAcion = new KeyValuePair<string, string>(subcriber, channelKey);

            _sub_cha_action.AddOrUpdate(keyAcion, (nk) => callBack, (k, ov) => callBack);
        }

        public void UnsubcribeAllChannel(string subcriber)
        {
            var cks = _chanels.Select(i => i.Key);
            foreach (var ck in cks)
            {
                Unsubcribe(subcriber, ck);
            }
        }

        public void RemoveChannel(string channelKey)
        {
            List<string> subcribers;
            _chanels.TryRemove(channelKey, out subcribers);
            if (subcribers == null) return;

            foreach (var s in subcribers)
            {
                List<string> channels;
                if (_subcribers.TryGetValue(s, out channels))
                {
                    lock (channels)
                    {
                        channels.RemoveAll(i => i.Equals(channelKey));
                    }

                    _subcribers.TryUpdate(s, channels, null);
                }
            }
        }

        public void Unsubcribe(string subcriber, string channelKey)
        {
            List<string> oldChannelkeys;
            _subcribers.TryRemove(subcriber, out oldChannelkeys);

            List<string> subcribers;
            if (!_chanels.TryGetValue(channelKey, out subcribers)) return;
            if (subcribers == null) return;

            lock (subcribers)
            {
                subcribers.RemoveAll(i => i.Equals(subcriber));
            }
            _chanels.TryUpdate(channelKey, subcribers, null);
        }

        public void Dispose()
        {
            _isStop = true;
        }
    }
}