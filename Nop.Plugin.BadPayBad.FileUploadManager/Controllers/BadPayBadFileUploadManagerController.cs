using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Nop.Web.Framework.Controllers;

namespace Nop.Plugin.BadPayBad.FileUploadManager.Controllers
{
   public class BadPayBadFileUploadManagerController: BasePluginController
    {

       public ActionResult AdminConfig()
       {
           return View("~/Plugins/BadPayBad.FileUploadManager/Views/AdminConfig.cshtml");
       }

       public ActionResult Frontend()
       {
            return View("~/Plugins/BadPayBad.FileUploadManager/Views/FrontEnd.cshtml");
        }

    }
}
