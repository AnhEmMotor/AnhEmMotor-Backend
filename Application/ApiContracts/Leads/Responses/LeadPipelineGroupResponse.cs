using System;
using System.Collections.Generic;
using System.Text;

namespace Application.ApiContracts.Leads.Responses
{
    public class LeadPipelineGroupResponse
    {
        public string Status { get; set; } = string.Empty;

        public string StatusDisplayName { get; set; } = string.Empty;

        public List<LeadResponse> Leads { get; set; } = new();
    }
}
