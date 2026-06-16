using Domain.Entities;
using Domain.Enums;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Seeders;

public static class ContractTemplateSeeder
{
    public static IReadOnlyList<ContractTemplate> CreateSeedTemplates(DateTimeOffset currentDate) =>
    [
        new ContractTemplate
        {
            Id = Guid.Parse("8a7eb1d6-11c3-42a0-9c58-a2f7a0010001"),
            Name = "Hợp đồng Mua bán Hàng hóa",
            Type = "Sales",
            Code = "SALES_CONTRACT_DEFAULT",
            Version = 1.0m,
            Content = """
                      <h1>HỢP ĐỒNG MUA BÁN HÀNG HÓA</h1>
                      <p><em>Số: {{ContractNumber}}</em></p>

                      <h2>THÔNG TIN BÊN MUA</h2>
                      <p><strong>Họ tên:</strong> {{CustomerName}}</p>
                      <p><strong>CCCD:</strong> {{CustomerIdCard}}</p>
                      <p><strong>Địa chỉ:</strong> {{CustomerAddress}}</p>

                      <h2>THÔNG TIN BÊN BÁN</h2>
                      <p><strong>Đơn vị bán:</strong> {{ShowroomName}}</p>
                      <p><strong>Địa chỉ:</strong> {{ShowroomAddress}}</p>

                      <h2>HÀNG HÓA</h2>
                      <p><strong>Tên hàng:</strong> {{VehicleName}}</p>
                      <p><strong>Màu xe:</strong> {{VehicleColor}}</p>
                      <p><strong>Số khung:</strong> {{ChassisNumber}}</p>
                      <p><strong>Số máy:</strong> {{EngineNumber}}</p>
                      <p><strong>Giá bán:</strong> {{VehiclePrice}}</p>
                      <p><strong>Bằng chữ:</strong> {{VehiclePriceInWords}}</p>

                      <h2>THANH TOÁN</h2>
                      <p><strong>Tiền cọc:</strong> {{DepositAmount}}</p>
                      <p><strong>Còn lại:</strong> {{RemainingAmount}}</p>
                      <p><strong>Hình thức:</strong> {{PaymentMethod}}</p>

                      <h2>BÀN GIAO</h2>
                      <p><strong>Ngày giao:</strong> {{DeliveryDate}}</p>
                      <p><strong>Địa điểm:</strong> {{DeliveryLocation}}</p>
                      <p><strong>Bảo hành:</strong> {{WarrantyMonths}}</p>

                      <h2>XÁC NHẬN</h2>
                      <p>Hai bên xác nhận thông tin trên là đúng và đồng ý thực hiện hợp đồng mua bán hàng hóa này.</p>
                      """,
            DynamicFields = """["ContractNumber","CustomerName","CustomerIdCard","CustomerAddress","ShowroomName","ShowroomAddress","VehicleName","VehicleColor","ChassisNumber","EngineNumber","VehiclePrice","VehiclePriceInWords","DepositAmount","RemainingAmount","PaymentMethod","DeliveryDate","DeliveryLocation","WarrantyMonths"]""",
            IsActive = true,
            Status = ContractTemplateStatus.Active,
            IsUsed = false,
            CreatedAt = currentDate,
            UpdatedAt = currentDate
        },
        new ContractTemplate
        {
            Id = Guid.Parse("8a7eb1d6-11c3-42a0-9c58-a2f7a0010002"),
            Name = "Hợp đồng Tài chính Trả góp",
            Type = "Finance",
            Code = "FINANCE_INSTALLMENT_CONTRACT_DEFAULT",
            Version = 1.0m,
            Content = """
                      <h1>HỢP ĐỒNG TÀI CHÍNH TRẢ GÓP</h1>
                      <p>Số hợp đồng: {{contractNumber}}</p>
                      <p>Khách hàng: {{customerName}}</p>
                      <p>CCCD: {{customerCccd}}</p>
                      <p>Địa chỉ: {{customerAddress}}</p>
                      <p>Đối tác tài chính: {{financialPartner}}</p>
                      <p>Giá trị vay: {{loanAmount}}</p>
                      <p>Kỳ hạn: {{termMonths}}</p>
                      """,
            DynamicFields = """["contractNumber","customerName","customerCccd","customerAddress","financialPartner","loanAmount","termMonths"]""",
            IsActive = true,
            Status = ContractTemplateStatus.Active,
            IsUsed = false,
            CreatedAt = currentDate,
            UpdatedAt = currentDate
        },
        new ContractTemplate
        {
            Id = Guid.Parse("8a7eb1d6-11c3-42a0-9c58-a2f7a0010003"),
            Name = "Hợp đồng Nhà cung cấp",
            Type = "Supplier",
            Code = "SUPPLIER_CONTRACT_DEFAULT",
            Version = 1.0m,
            Content = """
                      <h1>HỢP ĐỒNG NHÀ CUNG CẤP</h1>
                      <p>Số hợp đồng: {{contractNumber}}</p>
                      <p>Nhà cung cấp: {{supplierName}}</p>
                      <p>Mã số thuế: {{taxCode}}</p>
                      <p>Địa chỉ: {{supplierAddress}}</p>
                      <p>Giá trị hợp đồng: {{contractValue}}</p>
                      <p>Điều khoản thanh toán: {{paymentTerms}}</p>
                      """,
            DynamicFields = """["contractNumber","supplierName","taxCode","supplierAddress","contractValue","paymentTerms"]""",
            IsActive = true,
            Status = ContractTemplateStatus.Active,
            IsUsed = false,
            CreatedAt = currentDate,
            UpdatedAt = currentDate
        }
    ];

    public static async Task SeedAsync(ApplicationDBContext context, CancellationToken cancellationToken)
    {
        var seedTemplates = CreateSeedTemplates(DateTimeOffset.UtcNow);
        var seedCodes = seedTemplates.Select(x => x.Code).ToList();
        var existingTemplates = await context.ContractTemplates
            .Where(x => seedCodes.Contains(x.Code))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        foreach (var seedTemplate in seedTemplates)
        {
            var existingTemplate = existingTemplates.FirstOrDefault(
                x => string.Equals(x.Code, seedTemplate.Code, StringComparison.OrdinalIgnoreCase));

            if (existingTemplate is null)
            {
                context.ContractTemplates.Add(seedTemplate);
                continue;
            }

            ResetSeedTemplate(existingTemplate, seedTemplate);
        }

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private static void ResetSeedTemplate(ContractTemplate target, ContractTemplate seed)
    {
        target.Name = seed.Name;
        target.Type = seed.Type;
        target.Version = seed.Version;
        target.Content = seed.Content;
        target.DynamicFields = seed.DynamicFields;
        target.IsActive = seed.IsActive;
        target.Status = seed.Status;
        target.ParentId = seed.ParentId;
        target.IsUsed = seed.IsUsed;
        target.UpdatedAt = seed.UpdatedAt;
        target.DeletedAt = null;

        if (target.CreatedAt == default)
        {
            target.CreatedAt = seed.CreatedAt;
        }
    }
}
