using System;
using System.Collections.Generic;
using System.Text;

namespace BillingFlow.Domain.Exceptions
{
    public class InvoiceAlreadyPaidException : Exception
    {
        public InvoiceAlreadyPaidException()
            : base("Esta cobrança já foi paga e não pode ser alterada ou excluída.")
        {
        }
    }
}
