using Application.ApiContracts.Payment.Requests;
using Application.ApiContracts.Payment.Responses;
using Application.Common.Models;
using Application.Features.Payments.Commands.ProcessPayOSCallback;
using Application.Features.Payments.Commands.ProcessPayOSWebhook;
using Application.Features.Payments.Commands.ProcessVNPayIPN;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Output;
using Application.Interfaces.Services;
using Domain.Constants.Order;
using Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace UnitTests;

public class PaymentTests
{
    private readonly Mock<IOutputReadRepository> _readRepositoryMock = new();
    private readonly Mock<IOutputUpdateRepository> _updateRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IPayOSService> _payosServiceMock = new();
    private readonly Mock<IVNPayService> _vnpayServiceMock = new();

    #region PayOS Webhook Tests

    [Fact(DisplayName = "PAY_001 - PayOS Webhook - Thành công (Thanh toán đủ)")]
    public async Task PAY_001_PayOSWebhook_Success_FullAmount()
    {
        // Arrange
        var orderId = 123;
        var fullOrderCode = (long)orderId * 100000;
        var totalAmount = 100000m;
        var order = new Output
        {
            Id = orderId,
            PaymentMethod = PaymentMethod.PayOS,
            PaymentStatus = OrderPaymentStatus.Pending,
            StatusId = OrderStatus.Pending,
            OutputInfos = new List<OutputInfo>
            {
                new OutputInfo { Price = totalAmount, Count = 1 }
            }
        };

        var webhookData = new PayOSWebhookData
        {
            OrderCode = fullOrderCode.ToString(),
            Amount = totalAmount,
            TransactionId = "TX_PAYOS_1"
        };

        _payosServiceMock.Setup(x => x.VerifyWebhook(webhookData)).Returns(true);
        _readRepositoryMock.Setup(x => x.GetByIdWithDetailsAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var handler = new ProcessPayOSWebhookCommandHandler(
            _readRepositoryMock.Object,
            _updateRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _payosServiceMock.Object);

        // Act
        var result = await handler.Handle(new ProcessPayOSWebhookCommand(webhookData), CancellationToken.None).ConfigureAwait(true);

        // Assert
        result.IsSuccess.Should().BeTrue();
        order.PaymentStatus.Should().Be(OrderPaymentStatus.Paid);
        order.StatusId.Should().Be(OrderStatus.PaidProcessing);
        order.TransactionId.Should().Be("TX_PAYOS_1");
        _updateRepositoryMock.Verify(x => x.Update(order), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "PAY_002 - PayOS Webhook - Thành công (Thanh toán cọc)")]
    public async Task PAY_002_PayOSWebhook_Success_DepositAmount()
    {
        // Arrange
        var orderId = 124;
        var fullOrderCode = (long)orderId * 100000;
        var totalAmount = 100000m;
        var depositAmount = 50000m;
        var order = new Output
        {
            Id = orderId,
            DepositRatio = 50,
            PaymentMethod = PaymentMethod.PayOS,
            PaymentStatus = OrderPaymentStatus.Pending,
            StatusId = OrderStatus.Pending,
            OutputInfos = new List<OutputInfo>
            {
                new OutputInfo { Price = totalAmount, Count = 1 }
            }
        };

        var webhookData = new PayOSWebhookData
        {
            OrderCode = fullOrderCode.ToString(),
            Amount = depositAmount,
            TransactionId = "TX_PAYOS_2"
        };

        _payosServiceMock.Setup(x => x.VerifyWebhook(webhookData)).Returns(true);
        _readRepositoryMock.Setup(x => x.GetByIdWithDetailsAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var handler = new ProcessPayOSWebhookCommandHandler(
            _readRepositoryMock.Object,
            _updateRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _payosServiceMock.Object);

        // Act
        var result = await handler.Handle(new ProcessPayOSWebhookCommand(webhookData), CancellationToken.None).ConfigureAwait(true);

        // Assert
        result.IsSuccess.Should().BeTrue();
        order.PaymentStatus.Should().Be(OrderPaymentStatus.Partial);
        order.StatusId.Should().Be(OrderStatus.DepositPaid);
        _updateRepositoryMock.Verify(x => x.Update(order), Times.Once);
    }

    [Fact(DisplayName = "PAY_003 - PayOS Webhook - Thất bại (Số tiền không đủ)")]
    public async Task PAY_003_PayOSWebhook_InsufficientAmount()
    {
        // Arrange
        var orderId = 125;
        var fullOrderCode = (long)orderId * 100000;
        var totalAmount = 100000m;
        var order = new Output
        {
            Id = orderId,
            DepositRatio = 50,
            PaymentMethod = PaymentMethod.PayOS,
            PaymentStatus = OrderPaymentStatus.Pending,
            OutputInfos = new List<OutputInfo>
            {
                new OutputInfo { Price = totalAmount, Count = 1 }
            }
        };

        var webhookData = new PayOSWebhookData
        {
            OrderCode = fullOrderCode.ToString(),
            Amount = 10000m, // Too low
            TransactionId = "TX_PAYOS_3"
        };

        _payosServiceMock.Setup(x => x.VerifyWebhook(webhookData)).Returns(true);
        _readRepositoryMock.Setup(x => x.GetByIdWithDetailsAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var handler = new ProcessPayOSWebhookCommandHandler(
            _readRepositoryMock.Object,
            _updateRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _payosServiceMock.Object);

        // Act
        var result = await handler.Handle(new ProcessPayOSWebhookCommand(webhookData), CancellationToken.None).ConfigureAwait(true);

        // Assert
        result.IsSuccess.Should().BeTrue();
        order.PaymentStatus.Should().Be(OrderPaymentStatus.Pending); // Not updated
        _updateRepositoryMock.Verify(x => x.Update(It.IsAny<Output>()), Times.Never);
    }

    [Fact(DisplayName = "PAY_004 - PayOS Webhook - Sai chữ ký")]
    public async Task PAY_004_PayOSWebhook_InvalidSignature()
    {
        // Arrange
        var webhookData = new PayOSWebhookData { Signature = "INVALID" };
        _payosServiceMock.Setup(x => x.VerifyWebhook(webhookData)).Returns(false);

        var handler = new ProcessPayOSWebhookCommandHandler(
            _readRepositoryMock.Object,
            _updateRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _payosServiceMock.Object);

        // Act
        var result = await handler.Handle(new ProcessPayOSWebhookCommand(webhookData), CancellationToken.None).ConfigureAwait(true);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Field.Should().Be("Signature");
    }

    #endregion

    #region PayOS Callback Tests

    [Fact(DisplayName = "PAY_005 - PayOS Callback - Trả về Result thành công")]
    public async Task PAY_005_PayOSCallback_ReturnsSuccess()
    {
        // Arrange
        var orderId = 126;
        var fullOrderCode = (long)orderId * 100000;
        var order = new Output { Id = orderId };
        var payosData = new PayOSData { Status = PayOSStatus.Paid, Amount = 100000m };

        _payosServiceMock.Setup(x => x.GetPaymentDetailsAsync(fullOrderCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payosData);
        _readRepositoryMock.Setup(x => x.GetByIdWithDetailsAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var handler = new ProcessPayOSCallbackCommandHandler(
            _readRepositoryMock.Object,
            _updateRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _payosServiceMock.Object);

        // Act
        var result = await handler.Handle(new ProcessPayOSCallbackCommand(fullOrderCode), CancellationToken.None).ConfigureAwait(true);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    #endregion

    #region VNPay IPN Tests

    [Fact(DisplayName = "PAY_006 - VNPay IPN - Thành công")]
    public async Task PAY_006_VNPayIPN_Success()
    {
        // Arrange
        var orderId = 200;
        var totalAmount = 200000m;
        var order = new Output
        {
            Id = orderId,
            PaymentStatus = OrderPaymentStatus.Pending,
            StatusId = OrderStatus.Pending,
            OutputInfos = new List<OutputInfo>
            {
                new OutputInfo { Price = totalAmount, Count = 1 }
            }
        };

        var query = new Mock<IQueryCollection>();
        var vnpayResponse = new VNPayPaymentResponse
        {
            Success = true,
            VnPayResponseCode = "00",
            OrderId = orderId.ToString(),
            Amount = totalAmount,
            TransactionId = "TX_VNPAY_1"
        };

        _vnpayServiceMock.Setup(x => x.PaymentExecute(It.IsAny<IQueryCollection>())).Returns(vnpayResponse);
        _readRepositoryMock.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var handler = new ProcessVNPayIPNCommandHandler(
            _readRepositoryMock.Object,
            _updateRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _vnpayServiceMock.Object);

        // Act
        var result = await handler.Handle(new ProcessVNPayIPNCommand(query.Object), CancellationToken.None).ConfigureAwait(true);

        // Assert
        result.IsSuccess.Should().BeTrue();
        order.PaymentStatus.Should().Be(OrderPaymentStatus.Success);
        order.StatusId.Should().Be(OrderStatus.PaidProcessing);
        _updateRepositoryMock.Verify(x => x.Update(order), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "PAY_007 - VNPay IPN - Thất bại (Mã lỗi 09)")]
    public async Task PAY_007_VNPayIPN_InsufficientBalance()
    {
        // Arrange
        var orderId = 201;
        var order = new Output
        {
            Id = orderId,
            PaymentStatus = OrderPaymentStatus.Pending
        };

        var query = new Mock<IQueryCollection>();
        var vnpayResponse = new VNPayPaymentResponse
        {
            Success = false,
            VnPayResponseCode = "09", // Số dư không đủ
            OrderId = orderId.ToString()
        };

        _vnpayServiceMock.Setup(x => x.PaymentExecute(It.IsAny<IQueryCollection>())).Returns(vnpayResponse);
        _readRepositoryMock.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var handler = new ProcessVNPayIPNCommandHandler(
            _readRepositoryMock.Object,
            _updateRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _vnpayServiceMock.Object);

        // Act
        var result = await handler.Handle(new ProcessVNPayIPNCommand(query.Object), CancellationToken.None).ConfigureAwait(true);

        // Assert
        result.Value.Should().NotBeNull();
        result.Value?.VnPayResponseCode.Should().Be("09");
        order.PaymentStatus.Should().Be(OrderPaymentStatus.Failed);
        _updateRepositoryMock.Verify(x => x.Update(order), Times.Once);
    }

    [Fact(DisplayName = "PAY_008 - VNPay IPN - Sai chữ ký")]
    public async Task PAY_008_VNPayIPN_InvalidSignature()
    {
        // Arrange
        var query = new Mock<IQueryCollection>();
        var vnpayResponse = new VNPayPaymentResponse { Success = false }; // Service returns success=false for signature error

        _vnpayServiceMock.Setup(x => x.PaymentExecute(It.IsAny<IQueryCollection>())).Returns(vnpayResponse);

        var handler = new ProcessVNPayIPNCommandHandler(
            _readRepositoryMock.Object,
            _updateRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _vnpayServiceMock.Object);

        // Act
        var result = await handler.Handle(new ProcessVNPayIPNCommand(query.Object), CancellationToken.None).ConfigureAwait(true);

        // Assert
        result.IsSuccess.Should().BeFalse();
        _updateRepositoryMock.Verify(x => x.Update(It.IsAny<Output>()), Times.Never);
    }

    [Fact(DisplayName = "PAY_009 - VNPay IPN - Sai số tiền")]
    public async Task PAY_009_VNPayIPN_AmountMismatch()
    {
        // Arrange
        var orderId = 202;
        var totalAmount = 100000m;
        var order = new Output
        {
            Id = orderId,
            PaymentStatus = OrderPaymentStatus.Pending,
            OutputInfos = new List<OutputInfo>
            {
                new OutputInfo { Price = totalAmount, Count = 1 }
            }
        };

        var query = new Mock<IQueryCollection>();
        var vnpayResponse = new VNPayPaymentResponse
        {
            Success = true,
            VnPayResponseCode = "00",
            OrderId = orderId.ToString(),
            Amount = 50000m // Mismatch
        };

        _vnpayServiceMock.Setup(x => x.PaymentExecute(It.IsAny<IQueryCollection>())).Returns(vnpayResponse);
        _readRepositoryMock.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var handler = new ProcessVNPayIPNCommandHandler(
            _readRepositoryMock.Object,
            _updateRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _vnpayServiceMock.Object);

        // Act
        var result = await handler.Handle(new ProcessVNPayIPNCommand(query.Object), CancellationToken.None).ConfigureAwait(true);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Field.Should().Be("Amount");
    }

    [Fact(DisplayName = "PAY_010 - VNPay IPN - Double-Click (Trạng thái đã cập nhật)")]
    public async Task PAY_010_VNPayIPN_AlreadyPaid()
    {
        // Arrange
        var orderId = 203;
        var order = new Output
        {
            Id = orderId,
            PaymentStatus = OrderPaymentStatus.Success
        };

        var query = new Mock<IQueryCollection>();
        var vnpayResponse = new VNPayPaymentResponse
        {
            Success = true,
            VnPayResponseCode = "00",
            OrderId = orderId.ToString()
        };

        _vnpayServiceMock.Setup(x => x.PaymentExecute(It.IsAny<IQueryCollection>())).Returns(vnpayResponse);
        _readRepositoryMock.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var handler = new ProcessVNPayIPNCommandHandler(
            _readRepositoryMock.Object,
            _updateRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _vnpayServiceMock.Object);

        // Act
        var result = await handler.Handle(new ProcessVNPayIPNCommand(query.Object), CancellationToken.None).ConfigureAwait(true);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _updateRepositoryMock.Verify(x => x.Update(It.IsAny<Output>()), Times.Never);
    }

    #endregion
}
