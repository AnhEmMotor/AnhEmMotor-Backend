using Domain.Constants;
using Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Seeders;

public static class SalesContractSeeder
{
public static async Task SeedAsync(ApplicationDBContext context, CancellationToken cancellationToken)
{
if (await context.SalesContracts.AnyAsync(cancellationToken).ConfigureAwait(false))
return;

var customers = await context.Users
.Where(u => u.Email == "kimngan@gmail.com" || u.Email == "minhuyen@gmail.com")
.ToListAsync(cancellationToken).ConfigureAwait(false);

var outputs = await context.OutputOrders
.Include(o => o.OutputInfos)
.ThenInclude(oi => oi.ProductVariant)
.ThenInclude(pv => pv.Product)
.ThenInclude(p => p.Brand)
.Where(o => o.OutputInfos.Any())
.OrderBy(o => o.Id)
.Take(300)
.ToListAsync(cancellationToken).ConfigureAwait(false);

if (customers.Count == 0 || outputs.Count == 0)
return;

var random = new Random(77);
var now = DateTimeOffset.UtcNow;

var statuses = new[] { SalesContractStatus.Draft, SalesContractStatus.PendingApproval, SalesContractStatus.Approved, SalesContractStatus.Signed, SalesContractStatus.Fulfilled };

var contracts = new List<SalesContract>();

foreach (var output in outputs.Take(40))
{
var customer = customers[random.Next(customers.Count)];
var info = output.OutputInfos.First();
var variant = info.ProductVariant;
var product = variant?.Product;
var brand = product?.Brand;
var price = info.Price ?? variant?.Price ?? 30000000m;
var deposit = Math.Round(price * 0.1m / 100000) * 100000;
var remaining = price - deposit;

var signedDate = output.CreatedAt.HasValue
? output.CreatedAt.Value.AddDays(random.Next(1, 3))
: now.AddDays(-random.Next(1, 7));

var status = statuses[random.Next(statuses.Length)];

var contract = new SalesContract
{
Id = Guid.NewGuid(),
ContractNumber = $"HDXC-{now.Year}-{contracts.Count + 1:D4}",
OutputId = output.Id,
CustomerId = customer.Id,
ShowroomName = "Anh Em Motor Showroom",
ShowroomTaxCode = $"0{random.Next(10000, 99999)}{random.Next(10000, 99999)}",
ShowroomAddress = "123 Nguyễn Trãi, Thanh Xuân, Hà Nội",
ShowroomRepresentative = "Nguyễn Văn A",
CustomerFullName = customer.FullName ?? customer.Email,
CustomerCCCD = $"0{random.Next(100000, 999999)}{random.Next(100000, 999999)}",
CustomerAddress = customer.Email.Split('@')[0] + " Address",
CustomerPhone = $"+84{random.Next(900000000, 999999999)}",
VehicleModel = product?.Name ?? brand?.Name ?? "Xe máy",
VehicleVersion = variant?.VariantName ?? "Tiêu chuẩn",
VehicleColor = info.ProductVariantColorId.HasValue ? "Đen" : "Trắng",
FrameNumber = $"RLH{random.Next(100000, 999999)}",
EngineNumber = $"E{random.Next(10000, 99999)}",
ActualSalePrice = price,
DepositAmount = deposit,
RemainingAmount = remaining,
FinalPaymentDeadline = signedDate.AddDays(30),
WarrantyPeriod = "2 năm",
WarrantyScope = "Bảo hành chính hãng toàn quốc",
Status = status,
SignedDate = status == SalesContractStatus.Draft ? null : signedDate,
ScannedFileUrl = status == SalesContractStatus.Signed || status == SalesContractStatus.Fulfilled
? "/uploads/contracts/sample.pdf"
: null,
Note = status == SalesContractStatus.Fulfilled ? "Đã thanh toán đủ" : null,
CreatedAt = output.CreatedAt ?? now.AddDays(-random.Next(1, 30)),
UpdatedAt = now.AddDays(-random.Next(0, 7))
};

contracts.Add(contract);
}

context.SalesContracts.AddRange(contracts);
await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
}
}
