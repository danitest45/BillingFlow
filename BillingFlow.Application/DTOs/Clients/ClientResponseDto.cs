using System;
using System.Collections.Generic;
using System.Text;

namespace BillingFlow.Application.DTOs.Clients
{
    public class ClientResponseDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public decimal MonthlyAmount { get; set; }

        public int DueDay { get; set; }
    }
}
