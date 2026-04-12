using System;
using System.Collections.Generic;
using System.Text;

namespace BillingFlow.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Client> Clients { get; set; } = new List<Client>();
    }
}
