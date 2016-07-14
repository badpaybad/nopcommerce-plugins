using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Nop.Core.Plugins;
using Nop.Services.Cms;
using Nop.Web.Framework.Menu;

namespace Nop.Plugin.BadPayBad.ProductLiveChat
{
    public class PluginRegister : IPlugin,  IAdminMenuPlugin, IWidgetPlugin
    {
        public SiteMapNode BadPayBadRootNode { get; set; }

        static PluginRegister()
        {
            Database.SetInitializer<ProductCommentLiveChatDbContext>(null);
            RouteTable.Routes.MapRoute(
             "DefaultAspNetMvcProductLiveChat", // Route name
             "{controller}/{action}/{id}", // URL with parameters
             new { controller = "Home", action = "Index", id = UrlParameter.Optional }
             );
        }

        private ProductCommentLiveChatDbContext _dbcontext;

        public PluginRegister(ProductCommentLiveChatDbContext dbcontext)
        {
            _dbcontext = dbcontext;

            if (BadPayBadRootNode == null)
            {
                BadPayBadRootNode = new SiteMapNode()
                {
                    SystemName = "badpaybad.info",
                    Title = "Extensions",
                    Visible = true,
                    // Url=urlt,
                    ImageUrl = "~/Plugins/BadPayBad.Core/Contents/Imgs/favicon.png",
                    RouteValues = new RouteValueDictionary() { { "area", null } },
                };
            }

            var urlt = "~/Admin/Widget/ConfigureWidget?systemName=" + "Nop.Plugin.BadPayBad.ProductLiveChat.PluginRegister";

            var systemName = this.GetType().ToString();

            SiteMapNode pluginNode = new SiteMapNode()
            {
                Url = urlt,
                ImageUrl = "",
                SystemName = systemName,
                Title = "Product live chat",
                Visible = true
            };

            BadPayBadRootNode.ChildNodes.Add(pluginNode);
        }
        public PluginDescriptor PluginDescriptor { get; set; }
      
        public void Install()
        {
            Database.SetInitializer<ProductCommentLiveChatDbContext>(null);
            _dbcontext.Install();
            PluginManager.MarkPluginAsInstalled(this.PluginDescriptor.SystemName);
        }

        public void Uninstall()
        {
            _dbcontext.Uninstall();
            PluginManager.MarkPluginAsUninstalled(this.PluginDescriptor.SystemName);
        }

        public void ManageSiteMap(SiteMapNode rootNode)
        {
            var exited =
                rootNode.ChildNodes.FirstOrDefault(
                    i => i.SystemName.Equals("badpaybad.info", StringComparison.OrdinalIgnoreCase));
            var siteMapNodes = BadPayBadRootNode.ChildNodes.ToList();
            if (exited == null)
            {
                exited = BadPayBadRootNode;
                rootNode.ChildNodes.Insert(0, BadPayBadRootNode);
            }

            foreach (var smn in siteMapNodes)
            {
                var temp =
                    exited.ChildNodes.FirstOrDefault(
                        i => i.SystemName.Equals(smn.SystemName, StringComparison.OrdinalIgnoreCase));
                if (temp == null)
                {
                    exited.ChildNodes.Add(smn);
                }
            }
        }

        public IList<string> GetWidgetZones()
        {
            return new List<string>() { "productdetails_bottom" };
        }

        public void GetDisplayWidgetRoute(string widgetZone, out string actionName, out string controllerName,
            out RouteValueDictionary routeValues)
        {

            actionName = "FrontEnd";
            controllerName = "ProductCommentLiveChat";
            routeValues = new RouteValueDictionary
            {
                {"Namespaces", "Nop.Plugin.BadPayBad.ProductLiveChat"},
                {"area", null},
                {"widgetZone", widgetZone},
            };
        }

        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "BackEnd";
            controllerName = "ProductCommentLiveChat";
            routeValues = new RouteValueDictionary
            {
                {"Namespaces", "Nop.Plugin.BadPayBad.ProductLiveChat"},
                {"area", null}
            };
        }
    }
}
