using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRPAMSystem.BusinessTier.Constants
{
    public class PagingModel
    {
        public int Page { get; set; } = 1;

        public int Size { get; set; } = 10;
    }
    public static class PagingModelHelper
    {
        public static void NormalizePaging(PagingModel pagingModel)
        {
            if (pagingModel == null) throw new ArgumentNullException(nameof(pagingModel));

            if (pagingModel.Page < 1)
            {
                pagingModel.Page = 1;
            }

            if (pagingModel.Size < 1)
            {
                pagingModel.Size = 10;
            }

            if (pagingModel.Size > 100)
            {
                pagingModel.Size = 100;
            }
        }
    }
}
