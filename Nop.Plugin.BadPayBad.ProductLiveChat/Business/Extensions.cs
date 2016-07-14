using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Services.Seo;

namespace Nop.Plugin.BadPayBad.ProductLiveChat.Business
{
  public static  class Extensions
    {
      public static ProductCommentModel Copy(this ProductComment data, Product product=null, List<ProductComment> replies=null )
      {
          var productName = product != null ? product.Name : string.Empty;
          var productSeoName = product != null ? product.GetSeName():string.Empty;
          return new ProductCommentModel()
          {
              Id=data.Id,
              ParentId=data.ParentId,
              CreatedDate=data.CreatedDate,
              ProductId=data.ProductId,
              Comment=data.Comment,
              Username=data.Username,
              ProductSeoName= productSeoName,
              Replies=replies,
              ProductName=productName
          };
      }
    }
}
