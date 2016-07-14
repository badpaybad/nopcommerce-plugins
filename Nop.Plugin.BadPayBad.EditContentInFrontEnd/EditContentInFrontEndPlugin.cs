using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Routing;
using Nop.Core.Plugins;
using Nop.Services.Cms;
using Nop.Web.Framework.Menu;

namespace Nop.Plugin.BadPayBad.EditContentInFrontEnd
{
    public class EditContentInFrontEndPlugin : IPlugin, IWidgetPlugin, IAdminMenuPlugin
    {
        public PluginDescriptor PluginDescriptor { get; set; }
        public SiteMapNode BadPayBadRootNode { get; set; }

        public EditContentInFrontEndPlugin()
        {
            if (BadPayBadRootNode == null)
            {
                BadPayBadRootNode = new SiteMapNode()
                {
                    SystemName = "badpaybad.info",
                    Title = "Extensions",
                    Visible = true,
                    // Url=urlt,
                    ImageUrl = "~/Plugins/BadPayBad.Core/Contents/Imgs/favicon.png",
                    RouteValues = new RouteValueDictionary() {{"area", null}},
                };
            }

            var urlt = "~/Admin/Widget/ConfigureWidget?systemName=" +
                       "Nop.Plugin.BadPayBad.ProductLiveChat.PluginRegister";

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
            return new List<string>() {"body_start_html_tag_after"};
        }

        public void Install()
        {
            PluginManager.MarkPluginAsInstalled(this.PluginDescriptor.SystemName);
        }

        public void Uninstall()
        {
            PluginManager.MarkPluginAsUninstalled(this.PluginDescriptor.SystemName);
        }

        public void GetConfigurationRoute(out string actionName, out string controllerName,
            out RouteValueDictionary routeValues)
        {
            actionName = "BackEnd";
            controllerName = "EditContentInFrontEnd";
            routeValues = new RouteValueDictionary
            {
                {"Namespaces", "Nop.Plugin.BadPayBad.EditContentInFrontEnd"},
                {"area", null}
            };
        }

        public void GetDisplayWidgetRoute(string widgetZone, out string actionName, out string controllerName,
            out RouteValueDictionary routeValues)
        {
            actionName = "FrontEnd";
            controllerName = "EditContentInFrontEnd";
            routeValues = new RouteValueDictionary
            {
                {"Namespaces", "Nop.Plugin.BadPayBad.EditContentInFrontEnd"},
                {"area", null},
                {"widgetZone", widgetZone},
            };
        }
    }
}