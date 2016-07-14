using System.IO;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.UI;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof (Nop.Plugin.BadPayBad.ProductLiveChat.Business.StartupProductAnnoucementHub))]

namespace Nop.Plugin.BadPayBad.ProductLiveChat.Business
{
    public class StartupProductAnnoucementHub
    {
        public void Configuration(IAppBuilder app)
        {
            // Any connection or hub wire up and configuration should go here
            //app.MapSignalR("~/signalr", new HubConfiguration());
            //app.MapSignalR();
            var hubConfiguration = new HubConfiguration();
            hubConfiguration.EnableDetailedErrors = true;
            hubConfiguration.EnableJavaScriptProxies = true;
            app.MapSignalR("/signalr", hubConfiguration);
        }
    }

    [HubName("liveProductAnnoucementHub")]
    public class LiveProductAnnoucementHub : Hub
    {
        public const string LiveResponseChannelKey = "LiveResponseChannelKey";
        public void RegisterChat(string subcriber, string channelKey)
        {
            if (string.IsNullOrEmpty(subcriber) || string.IsNullOrEmpty(channelKey)) return;
            subcriber = subcriber.Trim();
            channelKey = channelKey.Trim();

            PubSubServices.Instance.Subcribe(subcriber, channelKey, (msg) =>
            {
                Clients.Caller.Announcement(channelKey, msg);
                
                return true;
            });
        }

        public void RegisterLiveResponse(string subcriber)
        {
          var  channelKey = LiveResponseChannelKey;

            PubSubServices.Instance.Subcribe(subcriber, channelKey, (msg) =>
            {
                Clients.Caller.LiveResponseAnnouncement(channelKey, msg);

                return true;
            });
        }

        void WriteLog(string msg)
        {
            using (var sw = new StreamWriter("c:/lognop.txt", true))
            {
                sw.WriteLine(msg);
            }
        }
    }
}