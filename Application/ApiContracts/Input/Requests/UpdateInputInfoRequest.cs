using Application.ApiContracts.Input;
using Application;
using Application.ApiContracts;

namespace Application.ApiContracts.Input.Requests
{
    public class UpdateInputInfoRequest
    {
        public int? Id { get; set; }

        public int? ProductId { get; set; }

        public short? Count { get; set; }

        public long? InputPrice { get; set; }
    }
}
