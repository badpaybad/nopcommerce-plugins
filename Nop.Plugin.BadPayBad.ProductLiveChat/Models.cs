using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Nop.Core.Domain.Catalog;
using Nop.Web.Framework.Kendoui;
using Nop.Web.Framework.Localization;

namespace Nop.Plugin.BadPayBad.ProductLiveChat
{
    public class ProductListCommentModel
    {
        public int ProductId { get; set; }
        public List<ProductCommentModel> Comments { get; set; }
       
        public bool AllowComment { get; set; }
        public string Username { get; set; }
    }

    public class ProductCommentModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductSeoName { get; set; }

        public int Id { get; set; }
        public string Username { get; set; }
        public string Comment { get; set; }
        public int ParentId { get; set; }
        public DateTime CreatedDate { get; set; }
        public List<ProductComment> Replies { get; set; }

        public string CommentChannelKey { get { return "commentChannelKey_" + Id; } }
     
    }


    public class BackEndModel
    {
        public string Username { get; set; }
        public string ChannelKeyLiveResponse { get; set; }

        public List<ProductCommentModel> Last10Comments { get; set; } 
    }
}