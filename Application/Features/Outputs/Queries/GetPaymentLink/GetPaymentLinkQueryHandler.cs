using Application.ApiContracts.Payment.Requests;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Output;
using Application.Interfaces.Repositories.Role;
using Application.Interfaces.Repositories.User;
using Application.Interfaces.Services;
using Domain.Constants.Order;
using Domain.Constants.Permission;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Features.Outputs.Queries.GetPaymentLink
{
    public sealed class GetPaymentLinkQueryHandler(
        IOutputReadRepository readRepository,
        IOutputUpdateRepository updateRepository,
        IUnitOfWork unitOfWork,
        IVNPayService vnpayService,
        IPayOSService payosService,
        IUserReadRepository userReadRepository,
        IRoleReadRepository roleReadRepository,
        IHttpContextAccessor httpContextAccessor) : IRequestHandler<GetPaymentLinkQuery, Result<string>>
    {
        public async Task<Result<string>> Handle(GetPaymentLinkQuery request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.CurrentUserId) ||
                !Guid.TryParse(request.CurrentUserId, out var currentUserId))
            {
                return Error.Unauthorized("Bạn cần đăng nhập để thực hiện thao tác này.");
            }
            var order = await readRepository.GetByIdWithDetailsAsync(request.OrderId, cancellationToken)
                .ConfigureAwait(false);
            if (order is null)
            {
                return Error.NotFound("Không tìm thấy đơn hàng", "Order");
            }
            bool isOwner = order.BuyerId == currentUserId;
            bool hasEditPermission = false;
            if (!isOwner)
            {
                var user = await userReadRepository.FindUserByIdAsync(currentUserId, cancellationToken)
                    .ConfigureAwait(false);
                if (user != null)
                {
                    var userRoles = await userReadRepository.GetRolesOfUserAsync(user, cancellationToken)
                        .ConfigureAwait(false);
                    var roleEntities = await roleReadRepository.GetRolesByNameAsync(userRoles, cancellationToken)
                        .ConfigureAwait(false);
                    var roleIds = roleEntities.Select(r => r.Id).ToList();
                    var userPermissions = await roleReadRepository.GetPermissionsNameByRoleIdAsync(
                        roleIds,
                        cancellationToken)
                        .ConfigureAwait(false);
                    if (userPermissions != null && userPermissions.Contains(PermissionsList.Outputs.Edit))
                    {
                        hasEditPermission = true;
                    }
                }
            }
            if (!isOwner && !hasEditPermission)
            {
                return Error.Forbidden("Bạn không có quyền lấy link thanh toán cho đơn hàng này.");
            }
            if (string.Compare(order.StatusId, OrderStatus.WaitingDeposit) != 0 &&
                string.Compare(order.StatusId, OrderStatus.Pending) != 0)
            {
                return Error.BadRequest("Đơn hàng này không ở trạng thái cần thanh toán", "Order");
            }
            var context = httpContextAccessor.HttpContext;
            if (context is null)
            {
                return Error.Failure("Lỗi hệ thống: Không tìm thấy HttpContext", "System");
            }
            var amountToPay = string.Compare(order.StatusId, OrderStatus.WaitingDeposit) == 0
                ? (order.OutputInfos.Sum(i => (i.Price ?? 0) * (i.Count ?? 0)) * (order.DepositRatio ?? 0) / 100)
                : order.OutputInfos.Sum(i => (i.Price ?? 0) * (i.Count ?? 0));
            var paymentMethod = order.PaymentMethod;
            if (string.IsNullOrWhiteSpace(paymentMethod) || string.Compare(paymentMethod, PaymentMethod.COD) == 0)
            {
                paymentMethod = PaymentMethod.PayOS;
                order.PaymentMethod = paymentMethod;
            }
            if (!string.IsNullOrEmpty(order.PaymentUrl) &&
                order.PaymentExpiredAt.HasValue &&
                order.PaymentExpiredAt.Value > DateTimeOffset.UtcNow &&
                string.Compare(order.PaymentMethod, paymentMethod) == 0)
            {
                return order.PaymentUrl;
            }
            if (string.Compare(paymentMethod, PaymentMethod.VNPay) == 0)
            {
                var vnpayRequest = new VNPayPaymentRequest
                {
                    OrderId = order.Id,
                    OrderCode = order.Id.ToString(),
                    Amount = amountToPay,
                    Description = $"Thanh toan don hang {order.Id}",
                    CreatedDate = DateTime.UtcNow
                };
                var paymentUrl = vnpayService.CreatePaymentUrl(context, vnpayRequest);
                order.PaymentUrl = paymentUrl;
                order.PaymentCode = order.Id.ToString();
                order.PaymentExpiredAt = DateTimeOffset.UtcNow.AddMinutes(15);
                updateRepository.Update(order);
                await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                return paymentUrl;
            }
            if (string.Compare(paymentMethod, PaymentMethod.PayOS) == 0)
            {
                long orderCode = (long)order.Id * 100000 + (int)(DateTimeOffset.UtcNow.ToUnixTimeSeconds() % 100000);
                var payosRequest = new PayOSPaymentRequest
                {
                    OrderId = order.Id,
                    OrderCode = orderCode,
                    Amount = amountToPay,
                    Description = $"ANHEMMOTOR {order.Id}"
                };
                var response = await payosService.CreatePaymentAsync(payosRequest, cancellationToken)
                    .ConfigureAwait(false);
                if (response.ErrorCode == 0)
                {
                    order.PaymentUrl = response.CheckoutUrl;
                    order.PaymentCode = orderCode.ToString();
                    order.PaymentExpiredAt = DateTimeOffset.UtcNow.AddDays(1);
                    updateRepository.Update(order);
                    await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                    return response.CheckoutUrl;
                }
                return Error.Failure($"Lỗi PayOS: {response.Message}", "PayOS");
            }
            return Error.BadRequest("Phương thức thanh toán không hỗ trợ thanh toán online", "PaymentMethod");
        }
    }
}
