using System.Web.Mvc;
using Nop.Core.Infrastructure;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Web.Framework.Controllers;

namespace Nop.Plugin.BadPayBad.EditContentInFrontEnd
{
    public class EditContentInFrontEndController : BasePluginController
    {
        private IUrlRecordService _urlRecordService;
        IPermissionService _permissionService =
           EngineContext.Current.Resolve<Nop.Services.Security.IPermissionService>();

        public EditContentInFrontEndController(IUrlRecordService urlRecordService)
        {
            _urlRecordService = urlRecordService;
        }

        public string GetBaseUrl()
        {
            var r = HttpContext.Request;

            string baseUrl = r.Url.Scheme + "://" + r.Url.Authority + r.ApplicationPath.TrimEnd('/') + "/";

            return baseUrl;
        }

        public ActionResult FrontEnd()
        {
            var model = new EditContentInFrontEndModel();

            var burl = GetBaseUrl();

            var requesturl = HttpContext.Request.Url.OriginalString;

            var slug = HttpContext.Server.UrlDecode(requesturl.Replace(burl, "").Trim('/').Split('?')[0].Trim('/'));

            var slugSeo = (RouteData.Values["generic_se_name"] as string) ?? string.Empty;
            
            var entity = _urlRecordService.GetBySlug(slugSeo);
            if (entity == null)
            {
                entity = _urlRecordService.GetBySlug(slug);
            }
            if (entity != null)
            {
                var entityName = entity.EntityName.Replace("Post","").Replace("Item","");

                model.UrlToEdit = string.Format("{0}Admin/{1}/Edit/{2}", burl, entityName, entity.EntityId);
            }

            model.Slug = slug;
            model.SlugSeo = slugSeo;

            var hasAccessAdmin =
               _permissionService.Authorize(Nop.Services.Security.StandardPermissionProvider.AccessAdminPanel);
            model.HasAdminAccess = hasAccessAdmin;

            var viewPath = "~/Plugins/BadPayBad.EditContentInFrontEnd/FrontEnd.cshtml";
            return PartialView(viewPath, model);
        }

        public ActionResult BackEnd()
        {
            var model = new EditContentInFrontEndModel();


            var viewPath = "~/Plugins/BadPayBad.EditContentInFrontEnd/BackEnd.cshtml";
            return PartialView(viewPath, model);
        }
    }
}