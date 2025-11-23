using Domain.Enums;
using Sieve.Models;

namespace Application.Sieve
{
    public static class SieveHelper
    {
        public static void ApplyDefaultSorting(SieveModel sieveModel, DataFetchMode mode = DataFetchMode.ActiveOnly)
        {
            sieveModel.Page ??= 1;
            sieveModel.PageSize ??= 10;

            if (!string.IsNullOrWhiteSpace(sieveModel.Sorts))
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
