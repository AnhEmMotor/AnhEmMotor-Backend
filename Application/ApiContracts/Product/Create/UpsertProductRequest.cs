using Application.ApiContracts.Product.Common;

namespace Application.ApiContracts.Product.Create;

public class UpsertProductRequest : ProductWriteRequestBase
{
    public int? Id { get; set; }
}
