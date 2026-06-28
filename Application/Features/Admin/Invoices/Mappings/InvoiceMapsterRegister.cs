using Application.ApiContracts.Admin.Invoices;
using Domain.Entities;
using Mapster;

namespace Application.Features.Admin.Invoices.Mappings;

public class InvoiceMapsterRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Invoice, AdminInvoiceSummaryResponse>();
        config.NewConfig<Invoice, AdminInvoiceDetailResponse>()
            .Map(dest => dest.PaymentBreakdown, src => new List<InvoicePaymentBreakdownItem>());
    }
}
