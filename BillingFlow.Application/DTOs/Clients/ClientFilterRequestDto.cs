using System;
using System.Collections.Generic;
using System.Text;

namespace BillingFlow.Application.DTOs.Clients
{
    public class ClientFilterRequestDto
    {
        public string? Search { get; set; }
        public int? DueDay { get; set; }
        public decimal? MinMonthlyAmount { get; set; }
        public decimal? MaxMonthlyAmount { get; set; }

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
