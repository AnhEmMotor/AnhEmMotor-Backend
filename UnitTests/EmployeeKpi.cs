using Application.Features.HR.Commands.CreateEmployeeKpi;
using Application.Features.HR.Commands.DeleteEmployeeKpi;
using Application.Features.HR.Commands.UpdateEmployeeKpi;
using Application.Features.HR.Queries.GetEmployeeKPIs;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.HR.Employee;
using Application.Interfaces.Repositories.HR.Kpi;
using Domain.Entities;
using FluentAssertions;
using Moq;

namespace UnitTests;

public class EmployeeKpi
{
    private readonly Mock<IEmployeeReadRepository> _employeeRepository = new();
    private readonly Mock<IEmployeeKpiRepository> _kpiRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    [Fact]
    public async Task CreateKpi_PersistsValidatedEmployeeMetric()
    {
        var employee = CreateEmployee();
        _employeeRepository
            .Setup(repository => repository.GetByIdAsync(employee.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);
        _kpiRepository
            .Setup(
                repository => repository.HasDuplicateAsync(
                    employee.Id,
                    "Doanh số tháng",
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>(),
                    null,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _kpiRepository
            .Setup(repository => repository.AddAsync(It.IsAny<KPI>(), It.IsAny<CancellationToken>()))
            .Callback<KPI, CancellationToken>((kpi, _) => kpi.Id = 42)
            .Returns(Task.CompletedTask);
        var handler = new CreateEmployeeKpiCommandHandler(
            _employeeRepository.Object,
            _kpiRepository.Object,
            _unitOfWork.Object);
        var result = await handler.Handle(
            new CreateEmployeeKpiCommand
            {
                EmployeeProfileId = employee.Id,
                MetricName = "  Doanh số tháng  ",
                TargetValue = 10,
                ActualValue = 8,
                PeriodStart = new DateTime(2026, 7, 1),
                PeriodEnd = new DateTime(2026, 7, 31),
                Description = "  Số xe bàn giao  "
            },
            CancellationToken.None);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
        _kpiRepository.Verify(
            repository => repository.AddAsync(
                It.Is<KPI>(
                    kpi => kpi.MetricName == "Doanh số tháng" &&
                        kpi.Description == "Số xe bàn giao" &&
                        kpi.EmployeeProfileId == employee.Id),
                CancellationToken.None),
            Times.Once);
        _unitOfWork.Verify(repository => repository.SaveChangesAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task CreateKpi_RejectsDuplicateEmployeeMetricPeriod()
    {
        var employee = CreateEmployee();
        _employeeRepository
            .Setup(repository => repository.GetByIdAsync(employee.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);
        _kpiRepository
            .Setup(
                repository => repository.HasDuplicateAsync(
                    employee.Id,
                    "Doanh số tháng",
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>(),
                    null,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var handler = new CreateEmployeeKpiCommandHandler(
            _employeeRepository.Object,
            _kpiRepository.Object,
            _unitOfWork.Object);
        var result = await handler.Handle(
            new CreateEmployeeKpiCommand
            {
                EmployeeProfileId = employee.Id,
                MetricName = "Doanh số tháng",
                TargetValue = 10,
                PeriodStart = new DateTime(2026, 7, 1),
                PeriodEnd = new DateTime(2026, 7, 31)
            },
            CancellationToken.None);
        result.IsFailure.Should().BeTrue();
        _kpiRepository.Verify(
            repository => repository.AddAsync(It.IsAny<KPI>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _unitOfWork.Verify(repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateKpi_PersistsNewValues()
    {
        var employee = CreateEmployee();
        var kpi = CreateKpi(employee);
        _kpiRepository
            .Setup(repository => repository.GetByIdAsync(kpi.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(kpi);
        _employeeRepository
            .Setup(repository => repository.GetByIdAsync(employee.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);
        _kpiRepository
            .Setup(
                repository => repository.HasDuplicateAsync(
                    employee.Id,
                    "Tỷ lệ chốt đơn",
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>(),
                    kpi.Id,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        var handler = new UpdateEmployeeKpiCommandHandler(
            _employeeRepository.Object,
            _kpiRepository.Object,
            _unitOfWork.Object);
        var result = await handler.Handle(
            new UpdateEmployeeKpiCommand
            {
                Id = kpi.Id,
                EmployeeProfileId = employee.Id,
                MetricName = "Tỷ lệ chốt đơn",
                TargetValue = 90,
                ActualValue = 82,
                PeriodStart = new DateTime(2026, 7, 1),
                PeriodEnd = new DateTime(2026, 7, 31),
                Description = "Theo số đơn xác nhận"
            },
            CancellationToken.None);
        result.IsSuccess.Should().BeTrue();
        kpi.MetricName.Should().Be("Tỷ lệ chốt đơn");
        kpi.TargetValue.Should().Be(90);
        kpi.ActualValue.Should().Be(82);
        _kpiRepository.Verify(repository => repository.Update(kpi), Times.Once);
        _unitOfWork.Verify(repository => repository.SaveChangesAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task DeleteKpi_RemovesPersistedRecord()
    {
        var employee = CreateEmployee();
        var kpi = CreateKpi(employee);
        _kpiRepository
            .Setup(repository => repository.GetByIdAsync(kpi.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(kpi);
        var handler = new DeleteEmployeeKpiCommandHandler(_kpiRepository.Object, _unitOfWork.Object);
        var result = await handler.Handle(new DeleteEmployeeKpiCommand(kpi.Id), CancellationToken.None);
        result.IsSuccess.Should().BeTrue();
        _kpiRepository.Verify(repository => repository.Delete(kpi), Times.Once);
        _unitOfWork.Verify(repository => repository.SaveChangesAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task GetKpis_ReturnsEditableFieldsAndGuardsZeroTarget()
    {
        var employee = CreateEmployee();
        var kpi = CreateKpi(employee);
        kpi.TargetValue = 0;
        _kpiRepository
            .Setup(repository => repository.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([kpi]);
        var handler = new GetEmployeeKPIsQueryHandler(_kpiRepository.Object);
        var result = await handler.Handle(new GetEmployeeKPIsQuery(), CancellationToken.None);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainSingle();
        result.Value[0].Id.Should().Be(kpi.Id);
        result.Value[0].EmployeeName.Should().Be(employee.User.FullName);
        result.Value[0].ActualValue.Should().Be(kpi.ActualValue);
        result.Value[0].Score.Should().Be(0);
    }

    private static EmployeeProfile CreateEmployee()
    {
        return new EmployeeProfile
        {
            Id = 7,
            UserId = Guid.NewGuid(),
            User =
                new ApplicationUser
                {
                    FullName = "Nguyễn Minh An",
                    Email = "an@anhemmotor.com",
                    UserName = "an@anhemmotor.com"
                },
            JobTitle = "Nhân viên kinh doanh"
        };
    }

    private static KPI CreateKpi(EmployeeProfile employee)
    {
        return new KPI
        {
            Id = 9,
            EmployeeProfileId = employee.Id,
            EmployeeProfile = employee,
            MetricName = "Doanh số tháng",
            TargetValue = 10,
            ActualValue = 8,
            PeriodStart = new DateTime(2026, 7, 1),
            PeriodEnd = new DateTime(2026, 7, 31),
            Description = "Số xe bàn giao"
        };
    }
}
