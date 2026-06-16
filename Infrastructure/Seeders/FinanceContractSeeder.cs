using Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Seeders;

public static class FinanceContractSeeder
{
    public static IReadOnlyList<FinanceContract> CreateSeedContracts(DateTimeOffset currentDate)
    {
        var today = currentDate.UtcDateTime.Date;

        return
        [
            new FinanceContract
            {
                Id = Guid.Parse("b31dc30d-f0f4-4e5a-86e3-9f8d54a96a01"),
                ContractNumber = "TG-HDSAISON-2026-001",
                CustomerId = Guid.Parse("c18b55b9-a678-4a6f-bda3-1558a8625001"),
                BankName = "HD Saison",
                LoanAmount = 25000000m,
                TermMonths = 12,
                InterestRate = 1.60m,
                DisbursementStatus = "Pending",
                CavetLocation = "Bank",
                SignedDate = today.AddDays(2),
                CreatedAt = currentDate.AddDays(-2),
                UpdatedAt = currentDate.AddDays(-2)
            },
            new FinanceContract
            {
                Id = Guid.Parse("b31dc30d-f0f4-4e5a-86e3-9f8d54a96a02"),
                ContractNumber = "TG-FECREDIT-2026-002",
                CustomerId = Guid.Parse("c18b55b9-a678-4a6f-bda3-1558a8625002"),
                BankName = "FE Credit",
                LoanAmount = 38000000m,
                TermMonths = 18,
                InterestRate = 1.90m,
                DisbursementStatus = "Pending",
                CavetLocation = "Store",
                SignedDate = today.AddDays(5),
                CreatedAt = currentDate.AddDays(-1),
                UpdatedAt = currentDate.AddDays(-1)
            },
            new FinanceContract
            {
                Id = Guid.Parse("b31dc30d-f0f4-4e5a-86e3-9f8d54a96a03"),
                ContractNumber = "TG-HOMECREDIT-2026-003",
                CustomerId = Guid.Parse("c18b55b9-a678-4a6f-bda3-1558a8625003"),
                BankName = "Home Credit",
                LoanAmount = 32000000m,
                TermMonths = 15,
                InterestRate = 1.75m,
                DisbursementStatus = "Pending",
                CavetLocation = "Bank",
                SignedDate = today.AddDays(-4),
                CreatedAt = currentDate.AddDays(-7),
                UpdatedAt = currentDate.AddDays(-4)
            },
            new FinanceContract
            {
                Id = Guid.Parse("b31dc30d-f0f4-4e5a-86e3-9f8d54a96a04"),
                ContractNumber = "TG-MBANK-2026-004",
                CustomerId = Guid.Parse("c18b55b9-a678-4a6f-bda3-1558a8625004"),
                BankName = "MB Bank",
                LoanAmount = 45000000m,
                TermMonths = 24,
                InterestRate = 1.45m,
                DisbursementStatus = "Disbursed",
                CavetLocation = "Customer",
                SignedDate = today.AddDays(-10),
                CreatedAt = currentDate.AddDays(-12),
                UpdatedAt = currentDate.AddDays(-10)
            },
            new FinanceContract
            {
                Id = Guid.Parse("b31dc30d-f0f4-4e5a-86e3-9f8d54a96a05"),
                ContractNumber = "TG-TPBANK-2026-005",
                CustomerId = Guid.Parse("c18b55b9-a678-4a6f-bda3-1558a8625005"),
                BankName = "TPBank",
                LoanAmount = 29000000m,
                TermMonths = 12,
                InterestRate = 1.55m,
                DisbursementStatus = "Pending",
                CavetLocation = "Store",
                SignedDate = today.AddDays(7),
                CreatedAt = currentDate,
                UpdatedAt = currentDate
            }
        ];
    }

    public static async Task SeedAsync(ApplicationDBContext context, CancellationToken cancellationToken)
    {
        var seedContracts = CreateSeedContracts(DateTimeOffset.UtcNow);
        var seedContractNumbers = seedContracts.Select(x => x.ContractNumber).ToList();

        var existingContracts = await context.FinanceContracts
            .Where(x => seedContractNumbers.Contains(x.ContractNumber))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        foreach (var seedContract in seedContracts)
        {
            var existingContract = existingContracts.FirstOrDefault(
                x => string.Equals(x.ContractNumber, seedContract.ContractNumber, StringComparison.OrdinalIgnoreCase));

            if (existingContract is null)
            {
                context.FinanceContracts.Add(seedContract);
                continue;
            }

            ResetSeedContract(existingContract, seedContract);
        }

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private static void ResetSeedContract(FinanceContract target, FinanceContract seed)
    {
        target.CustomerId = seed.CustomerId;
        target.BankName = seed.BankName;
        target.LoanAmount = seed.LoanAmount;
        target.TermMonths = seed.TermMonths;
        target.InterestRate = seed.InterestRate;
        target.DisbursementStatus = seed.DisbursementStatus;
        target.CavetLocation = seed.CavetLocation;
        target.SignedDate = seed.SignedDate;
        target.UpdatedAt = seed.UpdatedAt;
        target.DeletedAt = null;

        if (target.CreatedAt == default)
        {
            target.CreatedAt = seed.CreatedAt;
        }
    }
}
