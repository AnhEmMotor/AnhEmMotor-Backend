using Application.Common.Models;
using MediatR;

namespace Application.Features.PlateDossiers.Commands.DeletePlateDossier
{
    public class DeletePlateDossierCommand : IRequest<Result<bool>>
    {
        public int Id { get; set; }
    }
}
