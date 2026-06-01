using Application.Common.Models;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Suppliers.Commands.DeleteSupplier
{
    public sealed class DeleteSupplierCommandHandler : IRequestHandler<DeleteSupplierCommand, Result>
    {
        public Task<Result> Handle(DeleteSupplierCommand request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
