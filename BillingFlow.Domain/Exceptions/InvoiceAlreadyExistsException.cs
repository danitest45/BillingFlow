using System;
using System.Collections.Generic;
using System.Text;

namespace BillingFlow.Domain.Exceptions
{
    public class InvoiceAlreadyExistsException : Exception
    {
        public InvoiceAlreadyExistsException()
            : base("Já existe uma cobrança para este cliente no mês atual.")
        {
        }
    }
}
