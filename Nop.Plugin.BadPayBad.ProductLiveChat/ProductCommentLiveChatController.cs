using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Nop.Core;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Plugin.BadPayBad.ProductLiveChat.Business;
using Nop.Services.Localization;
using Nop.Services.Seo;
using Nop.Web.Framework.Controllers;

namespace Nop.Plugin.BadPayBad.ProductLiveChat
{
    public class ProductCommentLiveChatController : BasePluginController
    {
        private IRepository<Product> _productRepo;
        private IRepository<ProductComment> _commentRepo;
        private readonly ILanguageService _languageService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly IUrlRecordService _urlRecordService;
        private IWorkContext _workContext;
        CustomerSettings _customerSettings;


        public ProductCommentLiveChatController(
            IRepository<Product> productRepo
            , IRepository<ProductComment> commentRepo
            , ILanguageService languageService
            , ILocalizedEntityService localizedEntityService
            , IUrlRecordService urlRecordService
            , IWorkContext workContext
            , CustomerSettings customerSettings)
        {
            _productRepo = productRepo;
            _commentRepo = commentRepo;
            _languageService = languageService;
            _localizedEntityService = localizedEntityService;
            _urlRecordService = urlRecordService;
            _workContext = workContext;
            _customerSettings = customerSettings;
        }
        
        public ActionResult FrontEnd(int productId)
        {
            var currentUser = _workContext.CurrentCustomer;
            if (currentUser == null || !currentUser.IsRegistered())
            {
                var nologeView = "~/Plugins/BadPayBad.ProductLiveChat/NoLoged.cshtml";
                return PartialView(nologeView);
            }

            var model = new ProductListCommentModel();

            var product = _productRepo.GetById(productId);

            model.AllowComment = product.AllowCustomerReviews;

            model.ProductId = productId;
            model.Username = _customerSettings.UsernamesEnabled ? currentUser.Username : currentUser.Email;

            var comments = _commentRepo.Table
                .Where(i => i.ProductId == productId && i.ParentId == 0)
                .OrderByDescending(i => i.CreatedDate).ToList();

            model.Comments = comments.Select(i => new ProductCommentModel()
            {
                Id = i.Id,
                Comment = i.Comment,
                ProductId = i.ProductId,
                Replies = _commentRepo.Table
                    .Where(c => c.ParentId == i.Id)
                    .OrderBy(c => c.CreatedDate).ToList(),
                CreatedDate = i.CreatedDate,
                Username = i.Username
            }).ToList();


            var frontEndView = "~/Plugins/BadPayBad.ProductLiveChat/FrontEnd.cshtml";
            return PartialView(frontEndView, model);
        }

        public ActionResult BackEnd()
        {
            BackEndModel model = new BackEndModel();
            var currentUser = _workContext.CurrentCustomer;
            if (currentUser == null || !currentUser.IsRegistered())
            {
                var backendViewNoLoged = "~/Plugins/BadPayBad.ProductLiveChat/NoLoged.cshtml";
                return PartialView(backendViewNoLoged);
            }

            model.Username = _customerSettings.UsernamesEnabled ? currentUser.Username : currentUser.Email;
            model.ChannelKeyLiveResponse = LiveProductAnnoucementHub.LiveResponseChannelKey;

            model.Last10Comments = GetLast10CommentsFromProducts();

            var backEndView = "~/Plugins/BadPayBad.ProductLiveChat/BackEnd.cshtml";
            return PartialView(backEndView, model);
        }

        private List<ProductCommentModel> GetLast10CommentsFromProducts()
        {
            List<ProductComment> coments=new List<ProductComment>();
            var x10product =
                _productRepo.Table.OrderByDescending(i => i.CreatedOnUtc).Select(i => i.Id).Take(10).ToList();

            var x10coment =
                _commentRepo.Table.Where(i => i.ParentId == 0 && x10product.Contains(i.ProductId))
                    .OrderByDescending(i => i.CreatedDate).Take(10).ToList();

            coments.AddRange(x10coment);

            var reamin = 10 - x10coment.Count;
            if (reamin > 0)
            {
                coments.AddRange(_commentRepo.Table
                    .Where(i => i.ParentId == 0 && !x10product.Contains(i.ProductId))
                    .OrderByDescending(i => i.CreatedDate)
                    .Take(reamin).ToList());
            }

            List < ProductCommentModel > res=new List<ProductCommentModel>();

            foreach (var c in coments)
            { 
                res.Add(c.Copy(_productRepo.GetById(c.ProductId)));
            }

            return res;
        }

        public ActionResult LiveResponse(int commentId)
        {
            var currentUser = _workContext.CurrentCustomer;
            if (currentUser == null || !currentUser.IsRegistered())
            {
                var nologedView = "~/Plugins/BadPayBad.ProductLiveChat/NoLoged.cshtml";
                return PartialView(nologedView);
            }


            ProductCommentModel model = new ProductCommentModel();

            var pc = _commentRepo.GetById(commentId);
            var product = _productRepo.GetById(pc.ProductId);

            model.ProductName = product.Name;
            model.ProductSeoName = _productRepo.GetById(pc.ProductId).GetSeName();
            model.Replies = _commentRepo.Table
                .Where(i => i.ParentId == pc.Id)
                .OrderBy(i => i.CreatedDate).ToList();
            model.CreatedDate = pc.CreatedDate;
            model.Comment = pc.Comment;
            model.ProductId = pc.ProductId;
            model.Username = pc.Username;
            model.Id = pc.Id;

            var liveResponseView = "~/Plugins/BadPayBad.ProductLiveChat/LiveResponse.cshtml";
            return PartialView(liveResponseView, model);
        }


        [HttpPost]
        public ActionResult AddComment(ProductCommentModel data)
        {
            var currentUser = _workContext.CurrentCustomer;
            if (currentUser == null || !currentUser.IsRegistered())
            {
                var nologedView = "~/Plugins/BadPayBad.ProductLiveChat/NoLoged.cshtml";
                return PartialView(nologedView);
            }
            data.Username = _customerSettings.UsernamesEnabled ? currentUser.Username : currentUser.Email;

            var productComment = new ProductComment()
            {
                Comment = data.Comment,
                CreatedDate = DateTime.Now,
                LastComment = DateTime.Now,
                ParentId = 0,
                ProductId = data.ProductId,
                RatesByCommas = string.Empty,
                RatesCalculated = string.Empty,
                Status = 0,
                Username = data.Username
            };
            _commentRepo.Insert(productComment);

            data.Id = productComment.Id;

            PubSubServices.Instance.Publish(LiveProductAnnoucementHub.LiveResponseChannelKey
                , new JavaScriptSerializer().Serialize(productComment.Copy(_productRepo.GetById(productComment.ProductId) )));

            return Content(productComment.Id.ToString());
        }
        

        [HttpPost]
        public ActionResult AddReply(ProductCommentModel data)
        {
            var currentUser = _workContext.CurrentCustomer;
            if (currentUser == null || !currentUser.IsRegistered())
            {
                var nologedView = "~/Plugins/BadPayBad.ProductLiveChat/NoLoged.cshtml";
                return PartialView(nologedView);
            }

            data.Username = _customerSettings.UsernamesEnabled ? currentUser.Username : currentUser.Email;
            data.Username = data.Username;

            var productComment = new ProductComment()
            {
                Comment = data.Comment,
                CreatedDate = DateTime.Now,
                LastComment = DateTime.Now,
                ParentId = data.ParentId,
                ProductId = data.ProductId,
                RatesByCommas = string.Empty,
                RatesCalculated = string.Empty,
                Status = 0,
                Username = data.Username
            };
            _commentRepo.Insert(productComment);

            data.Id = productComment.Id;

            PubSubServices.Instance.Publish("commentChannelKey_" + data.ParentId,
                new JavaScriptSerializer().Serialize(productComment));

            var parentComment = _commentRepo.GetById(productComment.ParentId);
           
            PubSubServices.Instance.Publish(LiveProductAnnoucementHub.LiveResponseChannelKey
                , new JavaScriptSerializer().Serialize(parentComment.Copy(_productRepo.GetById(productComment.ProductId))));


            return Content(productComment.Id.ToString());
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            var currentUser = _workContext.CurrentCustomer;
            if (currentUser == null || !currentUser.IsRegistered())
            {
                var nologedView = "~/Plugins/BadPayBad.ProductLiveChat/NoLoged.cshtml";
                return PartialView(nologedView);
            }

            _commentRepo.Delete(_commentRepo.GetById(id));

            var replies = _commentRepo.Table.Where(i => i.ParentId == id).ToList();
            foreach (var pc in replies)
            {
                _commentRepo.Delete(pc);
            }

            return Content("success");
        }


        public ActionResult TestPubSub(string s, string c, string m)
        {
            if (!string.IsNullOrEmpty(m))
            {
                PubSubServices.Instance.Publish(c, m);

                return Content("ok publish");
            }
            PubSubServices.Instance.Subcribe(s, c, (msg) =>
            {
                WriteLog("published: " + msg);
                return true;
            });

            return Content("ok subcriber");
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