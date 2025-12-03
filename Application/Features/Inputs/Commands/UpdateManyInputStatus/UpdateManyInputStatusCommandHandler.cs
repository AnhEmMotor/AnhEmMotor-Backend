using Application.ApiContracts.Input;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Input;
using Domain.Constants;
using Domain.Helpers;
using Mapster;
using MediatR;

namespace Application.Features.Inputs.Commands.UpdateManyInputStatus;

public sealed class UpdateManyInputStatusCommandHandler(
    IInputReadRepository readRepository,
    IInputUpdateRepository updateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateManyInputStatusCommand, (List<InputResponse>? data, ErrorResponse? error)>
{
    public async Task<(List<InputResponse>? data, ErrorResponse? error)> Handle(
        UpdateManyInputStatusCommand request,
        CancellationToken cancellationToken)
    {
        if (!InputStatus.IsValid(request.StatusId))
        {
            return (null, new ErrorResponse
            {
                Errors = [ new ErrorDetail { Field = "StatusId", Message = $"Trạng thái '{request.StatusId}' không hợp lệ." } ]
            });
        }

        var inputs = await readRepository.GetByIdAsync(
            request.Ids,
            cancellationToken)
            .ConfigureAwait(false);

        var inputsList = inputs.ToList();

        if (inputsList.Count != request.Ids.Count)
        {
            var foundIds = inputsList.Select(i => i.Id).ToList();
            var missingIds = request.Ids.Except(foundIds).ToList();
            return (null, new ErrorResponse
            {
                Errors = [new ErrorDetail { Field = "Ids", Message = $"Không tìm thấy {missingIds.Count} phiếu nhập: {string.Join(", ", missingIds)}" }]
            });
        }

        var errors = new List<ErrorDetail>();

        foreach (var input in inputsList)
        {
            // Logic 1: Nếu phiếu đã Kết thúc hoặc Hủy thì KHÔNG ĐƯỢC phép đổi trạng thái nữa
            // (Bạn có thể tùy chỉnh logic này tùy theo business flow của bạn)
            if (Domain.Constants.InputStatus.IsCannotEdit(input.StatusId))
            {
                errors.Add(new ErrorDetail
                {
                    Field = "Ids",
                    Message = $"Phiếu nhập {input.Id} đang ở trạng thái '{input.StatusId}' nên không thể chuyển sang '{request.StatusId}'."
                });
                continue; // Tìm tiếp các lỗi khác chứ không dừng ngay
            }

            // Logic 2 (Optional): Kiểm tra logic chuyển trạng thái hợp lệ
            // Ví dụ: Không thể chuyển từ Draft thẳng sang Finish mà không qua Approved? 
            // Nếu có thì thêm if vào đây.
        }

        // Nguyên tắc: "Chỉ khi nào tất cả đều có thể chuyển thì mới cho chuyển"
        if (errors.Count > 0)
        {
            return (null, new ErrorResponse { Errors = errors });
        }
        // [KẾT THÚC ĐOẠN CẦN THÊM]

        foreach (var input in inputsList)
        {
            input.StatusId = request.StatusId;
            updateRepository.Update(input);
        }
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return (inputs.Adapt<List<InputResponse>>(), null);
    }
}
