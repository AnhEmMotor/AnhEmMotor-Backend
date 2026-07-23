using Application.Features.HR.Commands.DeleteEmployee;
using Application.Features.HR.Commands.UpdateEmployee;
using Application.Features.HR.Queries.GetEmployeeById;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.HR.Employee;
using Application.Interfaces.Repositories.User;
using Domain.Entities;
using FluentAssertions;
using Moq;

namespace UnitTests;

public class EmployeeCrud
{
    private readonly Mock<IEmployeeReadRepository> _readRepository = new();
    private readonly Mock<IEmployeeUpdateRepository> _updateRepository = new();
    private readonly Mock<IEmployeeDeleteRepository> _deleteRepository = new();
    private readonly Mock<IUserUpdateRepository> _userUpdateRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    [Fact]
    public async Task UpdateEmployee_PersistsAccountAndProfileFields()
    {
        var employee = CreateEmployee();
        _readRepository.Setup(repository => repository.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);
        _userUpdateRepository
            .Setup(repository => repository.UpdateUserAsync(employee.User, It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, Array.Empty<string>()));
        var handler = new UpdateEmployeeCommandHandler(
            _readRepository.Object,
            _updateRepository.Object,
            _userUpdateRepository.Object,
            _unitOfWork.Object);
        var result = await handler.Handle(
            new UpdateEmployeeCommand
            {
                Id = 1,
                FullName = "Nguyễn Văn An",
                Email = "an.nguyen@anhemmotor.com",
                IdentityNumber = "079123456789",
                Address = "Quận 7, TP.HCM",
                ContractDate = new DateTime(2026, 7, 1),
                BankName = "Vietcombank",
                BankAccountNumber = "1234567890",
                JobTitle = "Kỹ thuật viên",
                BaseSalary = 15_000_000m
            },
            CancellationToken.None)
            .ConfigureAwait(true);
        result.IsSuccess.Should().BeTrue();
        employee.User.FullName.Should().Be("Nguyễn Văn An");
        employee.User.Email.Should().Be("an.nguyen@anhemmotor.com");
        employee.User.UserName.Should().Be("an.nguyen@anhemmotor.com");
        employee.JobTitle.Should().Be("Kỹ thuật viên");
        employee.BaseSalary.Should().Be(15_000_000m);
        _updateRepository.Verify(repository => repository.Update(employee), Times.Once);
        _unitOfWork.Verify(repository => repository.SaveChangesAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task GetEmployeeById_ReturnsCompleteProfile()
    {
        var employee = CreateEmployee();
        _readRepository.Setup(repository => repository.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);
        var handler = new GetEmployeeByIdQueryHandler(_readRepository.Object);
        var result = await handler.Handle(new GetEmployeeByIdQuery(1), CancellationToken.None).ConfigureAwait(true);
        result.IsSuccess.Should().BeTrue();
        result.Value.FullName.Should().Be(employee.User.FullName);
        result.Value.Email.Should().Be(employee.User.Email);
        result.Value.IdentityNumber.Should().Be(employee.IdentityNumber);
    }

    [Fact]
    public async Task DeleteEmployee_SoftDeletePipelineReceivesProfile()
    {
        var employee = CreateEmployee();
        _readRepository.Setup(repository => repository.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);
        var handler = new DeleteEmployeeCommandHandler(
            _readRepository.Object,
            _deleteRepository.Object,
            _unitOfWork.Object);
        var result = await handler.Handle(new DeleteEmployeeCommand(1), CancellationToken.None).ConfigureAwait(true);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(1);
        _deleteRepository.Verify(repository => repository.Delete(employee), Times.Once);
        _unitOfWork.Verify(repository => repository.SaveChangesAsync(CancellationToken.None), Times.Once);
    }

    private static EmployeeProfile CreateEmployee()
    {
        return new EmployeeProfile
        {
            Id = 1,
            UserId = Guid.NewGuid(),
            User =
                new ApplicationUser
                {
                    FullName = "Nhân viên cũ",
                    Email = "old@anhemmotor.com",
                    UserName = "old@anhemmotor.com"
                },
            IdentityNumber = "001200012345",
            Address = "TP.HCM",
            ContractDate = new DateTime(2025, 1, 1),
            BankName = "ACB",
            BankAccountNumber = "0123456789",
            JobTitle = "Nhân viên",
            BaseSalary = 10_000_000m
        };
    }
}
