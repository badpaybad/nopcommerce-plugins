using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Routing;
using Nop.Core.Plugins;
using Nop.Services.Cms;
using Nop.Web.Framework.Menu;

namespace Nop.Plugin.BadPayBad.SampleDataAccess
{
    public class SampleDataAccessPluglin : IPlugin,  IAdminMenuPlugin, IWidgetPlugin
    {
        private SampleTableInDbContext _dbcontext;

        public SampleDataAccessPluglin(SampleTableInDbContext dbcontext)
        {
            _dbcontext = dbcontext;
        }
        public PluginDescriptor PluginDescriptor { get; set; }
        public void Install()
        {
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
            //throw new NotImplementedException();
        }

        public IList<string> GetWidgetZones()
        {
            return new List<string>() { "content_before" };
        }

        public void GetDisplayWidgetRoute(string widgetZone, out string actionName, out string controllerName,
            out RouteValueDictionary routeValues)
        {

            actionName = "FrontEnd";
            controllerName = "SampleDataAccess";
            routeValues = new RouteValueDictionary
            {
                {"Namespaces", "Nop.Plugin.BadPayBad.SampleDataAccess"},
                {"area", null},
                {"widgetZone", widgetZone},
            };
        }

        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "BackEnd";
            controllerName = "SampleDataAccess";
            routeValues = new RouteValueDictionary
            {
                {"Namespaces", "Nop.Plugin.BadPayBad.SampleDataAccess"},
                {"area", null}
            };
        }
    }
}
