using Domain.Constants;
using Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Seeders;

public static class SupplierContractSeeder
{
    public static async Task SeedAsync(ApplicationDBContext context, CancellationToken cancellationToken)
    {
        if (!await context.Suppliers.AnyAsync(cancellationToken).ConfigureAwait(false))
            return;
        if (!await context.ProductVariants.AnyAsync(cancellationToken).ConfigureAwait(false))
            return;
        var supplierHonda = await context.Suppliers
            .FirstOrDefaultAsync(s => s.Name != null && s.Name.Contains("Honda"), cancellationToken)
            .ConfigureAwait(false);
        var supplierYamaha = await context.Suppliers
            .FirstOrDefaultAsync(s => s.Name != null && s.Name.Contains("Yamaha"), cancellationToken)
            .ConfigureAwait(false);
        var supplierSuzuki = await context.Suppliers
            .FirstOrDefaultAsync(s => s.Name != null && s.Name.Contains("Suzuki"), cancellationToken)
            .ConfigureAwait(false);
        var variants = await context.ProductVariants.ToListAsync(cancellationToken).ConfigureAwait(false);
        var hondaVariants = variants.Where(v => v.SKU != null && v.SKU.StartsWith("HO")).Take(6).ToList();
        var yamahaVariants = variants.Where(v => v.SKU != null && v.SKU.StartsWith("YM")).Take(5).ToList();
        var suzukiVariants = variants.Where(v => v.SKU != null && v.SKU.StartsWith("SZ")).Take(4).ToList();
        var existingContracts = await context.SupplierContracts
            .Include(c => c.ContractItems)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        if (supplierHonda != null && hondaVariants.Count > 0)
        {
            var contractNumber = "HD-HONDA-2024-001";
            if (!existingContracts.Any(c => string.Compare(c.ContractNumber, contractNumber) == 0))
            {
                var contract = new SupplierContract
                {
                    Id = Guid.NewGuid(),
                    SupplierId = supplierHonda.Id,
                    ContractNumber = contractNumber,
                    EffectiveDate = new DateTime(2024, 1, 1),
                    ExpirationDate = new DateTime(2025, 12, 31, 23, 59, 59),
                    ContractValue = 15000000000,
                    Status = SupplierContractStatus.Active,
                    Terms =
                        "Cong ty Honda Viet Nam cung cap cac dong xe may chinh hang voi chat lương dam bao. Hop dong co hieu luc trong 2 nam voi cac dieu khoan ve gia ca, chat lương va chinh sach bao hanh theo quy dinh cua nha san xuat.",
                    Note = "Hop dong mua ban chinh thuc nam 2024. Da ky ket va co hieu luc.",
                    CreditLimit = 5000000000,
                    PaymentWindowDays = 30,
                    BankAccountNumber = "1234567890",
                    BankName = "Ngan hang TMCP Ngoai thuong Viet Nam (Vietcombank)",
                    MinimumVolumePerMonth = 100,
                    DiscountRate = 3.5m,
                    ContractFilePath = null,
                    CreatedAt = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
                    UpdatedAt = new DateTimeOffset(2024, 6, 15, 0, 0, 0, TimeSpan.Zero),
                    ContractItems = []
                };
                foreach (var v in hondaVariants)
                {
                    contract.ContractItems
                        .Add(
                            new SupplierContractItem
                            {
                                Id = Guid.NewGuid(),
                                SupplierContractId = contract.Id,
                                ProductVariantId = v.Id,
                                WholesalePrice = (v.Price ?? 0) * 0.85m
                            });
                }
                context.SupplierContracts.Add(contract);
                context.SupplierContractAuditLogs
                    .Add(
                        new SupplierContractAuditLog
                        {
                            Id = Guid.NewGuid(),
                            SupplierContractId = contract.Id,
                            Action = "Create",
                            Details = "Created contract HD-HONDA-2024-001",
                            ChangedBy = "system",
                            IpAddress = null,
                            OldValue = null,
                            NewValue = contractNumber
                        });
            }
        }
        if (supplierYamaha != null && yamahaVariants.Count > 0)
        {
            var contractNumber = "HD-YAMAHA-2024-002";
            if (!existingContracts.Any(c => string.Compare(c.ContractNumber, contractNumber.ToString()) == 0))
            {
                var contract = new SupplierContract
                {
                    Id = Guid.NewGuid(),
                    SupplierId = supplierYamaha.Id,
                    ContractNumber = contractNumber,
                    EffectiveDate = new DateTime(2024, 3, 1),
                    ExpirationDate = new DateTime(2025, 3, 1, 0, 0, 0),
                    ContractValue = 12000000000,
                    Status = SupplierContractStatus.Active,
                    Terms =
                        "Yamaha Motor Viet Nam cung cap xe may Yamaha chinh hang voi day du chung tu. Hop dong bao gom chinh sach gia si, thoi gian giao hang va che do bao hanh.",
                    Note = "Hop dong cung cap xe Yamaha - da phe duyet.",
                    CreditLimit = 3000000000,
                    PaymentWindowDays = 45,
                    BankAccountNumber = "9876543210",
                    BankName = "Ngan hang TMCP Ky thuong Viet Nam (Techcombank)",
                    MinimumVolumePerMonth = 80,
                    DiscountRate = 2.0m,
                    ContractFilePath = null,
                    CreatedAt = new DateTimeOffset(2024, 3, 1, 0, 0, 0, TimeSpan.Zero),
                    UpdatedAt = new DateTimeOffset(2024, 3, 1, 0, 0, 0, TimeSpan.Zero),
                    ContractItems = []
                };
                foreach (var v in yamahaVariants)
                {
                    contract.ContractItems
                        .Add(
                            new SupplierContractItem
                            {
                                Id = Guid.NewGuid(),
                                SupplierContractId = contract.Id,
                                ProductVariantId = v.Id,
                                WholesalePrice = (v.Price ?? 0) * 0.88m
                            });
                }
                context.SupplierContracts.Add(contract);
                context.SupplierContractAuditLogs
                    .Add(
                        new SupplierContractAuditLog
                        {
                            Id = Guid.NewGuid(),
                            SupplierContractId = contract.Id,
                            Action = "Create",
                            Details = "Created contract HD-YAMAHA-2024-002",
                            ChangedBy = "system",
                            IpAddress = null,
                            OldValue = null,
                            NewValue = contractNumber
                        });
            }
        }
        if (supplierSuzuki != null && suzukiVariants.Count > 0)
        {
            var contractNumber = "HD-SUZUKI-2024-003";
            if (!existingContracts.Any(c => string.Compare(c.ContractNumber, contractNumber.ToString()) == 0))
            {
                var contract = new SupplierContract
                {
                    Id = Guid.NewGuid(),
                    SupplierId = supplierSuzuki.Id,
                    ContractNumber = contractNumber,
                    EffectiveDate = new DateTime(2024, 6, 1),
                    ExpirationDate = null,
                    ContractValue = 8000000000,
                    Status = SupplierContractStatus.PendingApproval,
                    Terms =
                        "Suzuki Viet Nam cung cap cac dong xe Suzuki voi gia ca canh tranh. Dang cho phe duyet cuoi cung tu phong ke toan.",
                    Note = "Dang cho phe duyet dieu khoan chi tiet.",
                    CreditLimit = 2000000000,
                    PaymentWindowDays = 60,
                    BankAccountNumber = "5555666677",
                    BankName = "Ngan hang TMCP A Chau (ACB)",
                    MinimumVolumePerMonth = 50,
                    DiscountRate = 1.5m,
                    ContractFilePath = null,
                    CreatedAt = new DateTimeOffset(2024, 6, 1, 0, 0, 0, TimeSpan.Zero),
                    UpdatedAt = new DateTimeOffset(2024, 6, 1, 0, 0, 0, TimeSpan.Zero),
                    ContractItems = []
                };
                foreach (var v in suzukiVariants)
                {
                    contract.ContractItems
                        .Add(
                            new SupplierContractItem
                            {
                                Id = Guid.NewGuid(),
                                SupplierContractId = contract.Id,
                                ProductVariantId = v.Id,
                                WholesalePrice = (v.Price ?? 0) * 0.82m
                            });
                }
                context.SupplierContracts.Add(contract);
                context.SupplierContractAuditLogs
                    .Add(
                        new SupplierContractAuditLog
                        {
                            Id = Guid.NewGuid(),
                            SupplierContractId = contract.Id,
                            Action = "Create",
                            Details = "Created contract HD-SUZUKI-2024-003",
                            ChangedBy = "system",
                            IpAddress = null,
                            OldValue = null,
                            NewValue = contractNumber
                        });
            }
        }
        var allSuppliers = await context.Suppliers.ToListAsync(cancellationToken).ConfigureAwait(false);
        var fallbackSupplier = allSuppliers.FirstOrDefault();
        var extraContracts = new (string ContractNumber, int SupplierId, DateTime EffectiveDate, DateTime? ExpirationDate, decimal Value, string Status, string Terms, string Note, decimal? CreditLimit, int? PaymentDays, string? BankName, string? BankAccount, int? MinVolume, decimal? Discount, DateTimeOffset CreatedAt)[]
        {
            ("HD-DRAFT-2024-004", supplierHonda?.Id ?? 0, new DateTime(2024, 7, 1), null, 5000000000, SupplierContractStatus.Draft, "Nhap hop dong bo sung phu kien Honda. Chua hoan thien.", "Dang soan thao", 1000000000, 30, "Vietcombank", "1111222233", 20, 1.0m, new DateTimeOffset(
            2024,
            7,
            1,
            0,
            0,
            0,
            TimeSpan.Zero)),
            ("HD-YAMAHA-EXPIRED-2023-001", supplierYamaha?.Id ?? 0, new DateTime(2023, 1, 1), new DateTime(
            2024,
            1,
            1,
            0,
            0,
            0), 10000000000, SupplierContractStatus.Expired, "Hop dong cu da het han.", "Hop dong cu da het han", 2000000000, 30, "Techcombank", "7777888899", 60, 2.0m, new DateTimeOffset(
            2023,
            1,
            1,
            0,
            0,
            0,
            TimeSpan.Zero)),
            ("HD-HONDA-COMPLETED-2022-001", supplierHonda?.Id ?? 0, new DateTime(2022, 1, 1), new DateTime(
            2023,
            12,
            31,
            0,
            0,
            0), 20000000000, SupplierContractStatus.Completed, "Hop dong da hoan thanh.", "Da hoan thanh toan bo giao dich", 5000000000, 30, "Vietcombank", "1234567890", 150, 4.0m, new DateTimeOffset(
            2022,
            1,
            1,
            0,
            0,
            0,
            TimeSpan.Zero)),
            ("HD-TERMINATED-2023-002", fallbackSupplier?.Id ?? 0, new DateTime(2023, 4, 1), new DateTime(
            2024,
            4,
            1,
            0,
            0,
            0), 6000000000, SupplierContractStatus.Terminated, "Hop dong da thanh ly do vi pham dieu khoan.", "Thanh ly som", 1500000000, 45, "ACB", "5555666677", 40, 1.0m, new DateTimeOffset(
            2023,
            4,
            1,
            0,
            0,
            0,
            TimeSpan.Zero)),
        };
        foreach (var (contractNumber, supplierId, effective, expiration, value, status, terms, note, creditLimit, paymentDays, bankName, bankAccount, minVolume, discount, createdAt) in extraContracts)
        {
            if (supplierId == 0)
                continue;
            if (existingContracts.Any(c => string.Compare(c.ContractNumber, contractNumber.ToString()) == 0))
                continue;
            var contract = new SupplierContract
            {
                Id = Guid.NewGuid(),
                SupplierId = supplierId,
                ContractNumber = contractNumber,
                EffectiveDate = effective,
                ExpirationDate = expiration,
                ContractValue = value,
                Status = status,
                Terms = terms,
                Note = note,
                CreditLimit = creditLimit,
                PaymentWindowDays = paymentDays,
                BankAccountNumber = bankAccount,
                BankName = bankName,
                MinimumVolumePerMonth = minVolume,
                DiscountRate = discount,
                ContractFilePath = null,
                CreatedAt = createdAt,
                UpdatedAt = createdAt,
                ContractItems = []
            };
            context.SupplierContracts.Add(contract);
            context.SupplierContractAuditLogs
                .Add(
                    new SupplierContractAuditLog
                    {
                        Id = Guid.NewGuid(),
                        SupplierContractId = contract.Id,
                        Action = "Create",
                        Details = $"Created contract {contractNumber}",
                        ChangedBy = "system",
                        IpAddress = null,
                        OldValue = null,
                        NewValue = contractNumber
                    });
        }
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
