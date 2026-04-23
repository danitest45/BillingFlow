using BillingFlow.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace BillingFlow.Application.DTOs.Billing
{
    public class CreateCheckoutSessionRequestDto
    {
        public PlanType PlanType { get; set; }
    }
}
