using Domain.Enums;
using Sieve.Models;

namespace Application.Sieve
{
    public static class SieveHelper
    {
        public static void ApplyDefaultSorting(
            SieveModel sieveModel,
            DataFetchMode mode = DataFetchMode.ActiveOnly,
            bool isApplyDefaultPageAndPageSize = true)
        {
            if(isApplyDefaultPageAndPageSize)
            {
                sieveModel.Page ??= 1;
                sieveModel.PageSize ??= 10;
            } else
            {
                sieveModel.Page ??= 1;
                sieveModel.PageSize ??= int.MaxValue;
            }

            if(!string.IsNullOrWhiteSpace(sieveModel.Sorts))
            {
                return;
            }

            sieveModel.Sorts = mode switch
            {
                DataFetchMode.DeletedOnly => "-deletedAt",
                DataFetchMode.ActiveOnly => "-id",
                DataFetchMode.All => "-id",
                _ => "-id",
            };
        }
    }
}
