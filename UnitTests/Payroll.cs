using Application.Features.HR.Commands.ApprovePayroll;
using Application.Features.HR.Queries.GetPayrollSummary;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.HR.Commission;
using Application.Interfaces.Repositories.HR.Employee;
using Domain.Entities;
using FluentAssertions;
using Moq;

namespace UnitTests;

public class Payroll
{
    private readonly Mock<IEmployeeReadRepository> _employeeRepositoryMock = new();
    private readonly Mock<ICommissionReadRepository> _commissionRepositoryMock = new();

    [Fact]
    public async Task GetPayrollSummary_TransfersConfirmedCommissionAndVolumeBonusIntoMonthlySalary()
    {
        var period = new DateTime(2026, 7, 1);
        var employee = CreateEmployee(1, 10_000_000m);
        var records = CreateRecords(employee.Id, CommissionStatus.Confirmed, count: 10, amount: 500_000m, period);
        records.Add(CreateRecord(employee.Id, CommissionStatus.Pending, 200, 700_000m, period.AddDays(10)));
        records.Add(CreateRecord(employee.Id, CommissionStatus.Confirmed, 201, 900_000m, period.AddMonths(-1)));
        var handler = CreateSummaryHandler([employee], records);
        var result = await handler
            .Handle(new GetPayrollSummaryQuery(7, 2026), CancellationToken.None)
            .ConfigureAwait(true);
        result.IsSuccess.Should().BeTrue();
        var payroll = result.Value.Should().ContainSingle().Subject;
        payroll.PendingCommission.Should().Be(700_000m);
        payroll.ConfirmedCommission.Should().Be(5_000_000m);
        payroll.PaidCommission.Should().Be(0m);
        payroll.VolumeBonus.Should().Be(1_500_000m);
        payroll.TotalNetPayable.Should().Be(16_500_000m);
        payroll.TotalActualReceived.Should().Be(16_500_000m);
    }

    [Fact]
    public async Task GetPayrollSummary_DoesNotApplyVolumeBonusBelowThreshold()
    {
        var period = new DateTime(2026, 7, 1);
        var employee = CreateEmployee(1, 10_000_000m);
        var records = CreateRecords(employee.Id, CommissionStatus.Confirmed, count: 9, amount: 500_000m, period);
        var handler = CreateSummaryHandler([employee], records);
        var result = await handler
            .Handle(new GetPayrollSummaryQuery(7, 2026), CancellationToken.None)
            .ConfigureAwait(true);
        result.IsSuccess.Should().BeTrue();
        var payroll = result.Value.Should().ContainSingle().Subject;
        payroll.ConfirmedCommission.Should().Be(4_500_000m);
        payroll.VolumeBonus.Should().Be(0m);
        payroll.TotalNetPayable.Should().Be(14_500_000m);
    }

    [Fact]
    public async Task GetPayrollSummary_KeepsPaidCommissionInHistoricalPayrollTotals()
    {
        var period = new DateTime(2026, 7, 1);
        var employee = CreateEmployee(1, 10_000_000m);
        var records = CreateRecords(employee.Id, CommissionStatus.Paid, count: 10, amount: 500_000m, period);
        var handler = CreateSummaryHandler([employee], records);
        var result = await handler
            .Handle(new GetPayrollSummaryQuery(7, 2026), CancellationToken.None)
            .ConfigureAwait(true);
        result.IsSuccess.Should().BeTrue();
        var payroll = result.Value.Should().ContainSingle().Subject;
        payroll.ConfirmedCommission.Should().Be(0m);
        payroll.PaidCommission.Should().Be(5_000_000m);
        payroll.VolumeBonus.Should().Be(1_500_000m);
        payroll.TotalNetPayable.Should().Be(16_500_000m);
    }

    [Fact]
    public async Task ApprovePayroll_OnlyPaysConfirmedRecordsForRequestedMonth()
    {
        var period = new DateTime(2026, 7, 1);
        var targetRecord = CreateRecord(1, CommissionStatus.Confirmed, 1, 500_000m, period.AddDays(3));
        var previousMonthRecord = CreateRecord(1, CommissionStatus.Confirmed, 2, 500_000m, period.AddMonths(-1));
        var allConfirmedRecords = new List<CommissionRecord> { targetRecord, previousMonthRecord };
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        _commissionRepositoryMock
            .Setup(r => r.GetRecordsByStatusAsync(CommissionStatus.Confirmed, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(allConfirmedRecords);
        unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var handler = new ApprovePayrollCommandHandler(_commissionRepositoryMock.Object, unitOfWorkMock.Object);
        var result = await handler
            .Handle(new ApprovePayrollCommand(1, 7, 2026), CancellationToken.None)
            .ConfigureAwait(true);
        result.IsSuccess.Should().BeTrue();
        targetRecord.Status.Should().Be(CommissionStatus.Paid);
        targetRecord.PaidAt.Should().NotBeNull();
        previousMonthRecord.Status.Should().Be(CommissionStatus.Confirmed);
        previousMonthRecord.PaidAt.Should().BeNull();
        unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    private GetPayrollSummaryQueryHandler CreateSummaryHandler(
        List<EmployeeProfile> employees,
        List<CommissionRecord> records)
    {
        _employeeRepositoryMock
            .Setup(r => r.GetAllWithUsersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(employees);
        _commissionRepositoryMock
            .Setup(r => r.GetRecordsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(records);
        return new GetPayrollSummaryQueryHandler(_employeeRepositoryMock.Object, _commissionRepositoryMock.Object);
    }

    private static EmployeeProfile CreateEmployee(int id, decimal baseSalary)
    {
        var userId = Guid.NewGuid();
        return new EmployeeProfile
        {
            Id = id,
            UserId = userId,
            User = new ApplicationUser { Id = userId, FullName = $"Employee {id}", },
            JobTitle = "Sales",
            BaseSalary = baseSalary,
        };
    }

    private static List<CommissionRecord> CreateRecords(
        int employeeId,
        CommissionStatus status,
        int count,
        decimal amount,
        DateTime periodStart)
    {
        return Enumerable
            .Range(1, count)
            .Select(index => CreateRecord(employeeId, status, index, amount, periodStart.AddDays(index - 1)))
            .ToList();
    }

    private static CommissionRecord CreateRecord(
        int employeeId,
        CommissionStatus status,
        int outputId,
        decimal amount,
        DateTime earnedAt)
    {
        return new CommissionRecord
        {
            EmployeeProfileId = employeeId,
            OutputId = outputId,
            Amount = amount,
            Status = status,
            DateEarned = earnedAt,
        };
    }
}
