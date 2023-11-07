using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Portal.API.Entities
{
    public class DashboardAccount
    {
        public int IdGroup { get; set; }
        public int IdConvenio { get; set; }
        public int TotalAccounts { get; set; }
        public int ActiveAccounts { get; set; }
        public int BlockedAccounts { get; set; }
        public int CancelledAccounts { get; set; }
        public int TotalCards { get; set; }
        public int ActiveCards { get; set; }
        public int BlockedCards { get; set; }
        public int CurrentNewAccounts { get; set; }
        public int CurrentCancelledAccounts { get; set; }
        public int CurrentBlockedAccounts { get; set; }
        public int CurrentTransactionAccounts { get; set; }
        public decimal ActiviationPercentage { get; set; }
    }
}
