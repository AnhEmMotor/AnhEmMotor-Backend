using Application.ApiContracts.Output;
using Application;
using Application.ApiContracts;

namespace Application.ApiContracts.Output.Requests
{
    public class CreateOutputInfoRequest
    {
        public int? ProductId { get; set; }

        public short? Count { get; set; }
    }
}
