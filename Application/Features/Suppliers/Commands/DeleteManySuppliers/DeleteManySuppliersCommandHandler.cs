using Application.Common.Models;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Suppliers.Commands.DeleteManySuppliers
{
    public sealed class DeleteManySuppliersCommandHandler : IRequestHandler<DeleteManySuppliersCommand, Result>
    {
        public Task<Result> Handle(DeleteManySuppliersCommand request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
