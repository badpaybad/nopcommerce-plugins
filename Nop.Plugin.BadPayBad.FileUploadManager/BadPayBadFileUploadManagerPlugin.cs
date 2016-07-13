using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Routing;
using Nop.Core.Domain.Security;
using Nop.Core.Infrastructure;
using Nop.Core.Plugins;
using Nop.Services.Cms;
using Nop.Services.Security;
using Nop.Web.Framework.Menu;

namespace Nop.Plugin.BadPayBad.FileUploadManager
{
    public class BadPayBadFileUploadManagerPlugin : IPlugin, IAdminMenuPlugin, IWidgetPlugin
    {
        IPermissionService _permissionService =
            EngineContext.Current.Resolve<Nop.Services.Security.IPermissionService>();

        public SiteMapNode BadPayBadRootNode { get; set; }

        public string WidgetSystemName
        {
            get { return this.GetType().ToString(); }
        }

        public BadPayBadFileUploadManagerPlugin()
        {
            InternalInit();
        }


        private void InternalInit()
        {
            var systemName = WidgetSystemName;
            var isExisted =
                _permissionService.GetAllPermissionRecords()
                    .FirstOrDefault(i => i.SystemName == systemName);
            if (isExisted == null)
            {
                _permissionService.InsertPermissionRecord(new PermissionRecord()
                {
                    SystemName = systemName,
                    Category = "badpaybad.info",
                    Name = systemName
                });
            }

            InitMenu();
        }

        public void ManageSiteMap(SiteMapNode rootNode)
        {
            BuildBadPaybadRootNode();

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

        bool IsHasPermission()
        {
            var permitRec = _permissionService.GetPermissionRecordBySystemName(WidgetSystemName);
            var hasManagePluginsPermission = _permissionService.Authorize(permitRec);
            var hasAccessAdmin =
                _permissionService.Authorize(Nop.Services.Security.StandardPermissionProvider.AccessAdminPanel);

            return hasAccessAdmin && hasManagePluginsPermission;
        }

        protected void InitMenu()
        {
            var hasAccess = IsHasPermission();

            BuildBadPaybadRootNode();

            string menuIconUrl = "";
            string text = "Roxy file manager";

            var urlt = "~/Admin/Widget/ConfigureWidget?systemName=" + WidgetSystemName;

            var systemName = WidgetSystemName;

            if (string.IsNullOrEmpty(text))
            {
                text = systemName;
            }

            if (string.IsNullOrEmpty(menuIconUrl))
            {
                menuIconUrl = "~/Plugins/BadPayBad.Core/Contents/Imgs/favicon.png";
            }

            SiteMapNode pluginNode = new SiteMapNode()
            {
                Url = urlt,
                ImageUrl = menuIconUrl,
                SystemName = systemName,
                Title = text,
                Visible = hasAccess
            };

            BadPayBadRootNode.ChildNodes.Add(pluginNode);
        }

        private void BuildBadPaybadRootNode()
        {
            if (BadPayBadRootNode == null)
            {
                var hasAccess =
                    _permissionService.Authorize(Nop.Services.Security.StandardPermissionProvider.AccessAdminPanel);

                //var urlt = "~/Admin/Widget/ConfigureWidget?systemName=Nop.Plugin.BadPayBad.Core.BadPayBadCoreNopePlugin";

                BadPayBadRootNode = new SiteMapNode()
                {
                    SystemName = "badpaybad.info",
                    Title = "Extensions",
                    Visible = hasAccess,
                    ImageUrl = "~/Plugins/BadPayBad.Core/Contents/Imgs/favicon.png",
                    RouteValues = new RouteValueDictionary() {{"area", null}},
                };
            }
        }
        
        public IList<string> GetWidgetZones()
        {
            var wn = WidgetSystemName;

            return new List<string>() {wn};
        }
        
        public void GetConfigurationRoute(out string actionName, out string controllerName,
            out RouteValueDictionary routeValues)
        {
            actionName = "AdminConfig";
            controllerName = "BadPayBadFileUploadManager";
            routeValues = new RouteValueDictionary()
            {
                {"Namespaces", "Nop.Plugin.BadPayBad.FileUploadManager.Controllers"},
                {"area", null}
            };
        }


        public void GetDisplayWidgetRoute(string widgetZone, out string actionName, out string controllerName,
            out RouteValueDictionary routeValues)
        {
            actionName = "Frontend";
            controllerName = "BadPayBadFileUploadManager";

            routeValues = new RouteValueDictionary
            {
                {"Namespaces", "Nop.Plugin.BadPayBad.FileUploadManager.Controllers"},
                {"area", null},
                {"widgetZone", widgetZone},
            };
        }

        public PluginDescriptor PluginDescriptor { get; set; }

        public void Install()
        {
            PluginManager.MarkPluginAsInstalled(this.PluginDescriptor.SystemName);
        }

        public void Uninstall()
        {
            PluginManager.MarkPluginAsUninstalled(this.PluginDescriptor.SystemName);
            var pm = _permissionService.GetPermissionRecordBySystemName(WidgetSystemName);
            if (pm != null)
            {
                _permissionService.DeletePermissionRecord(pm);
            }
        }
    }
}