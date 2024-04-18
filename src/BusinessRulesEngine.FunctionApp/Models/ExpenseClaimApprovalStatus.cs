using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace BusinessRulesEngine.FunctionApp.Models
{
    public class ExpenseClaimApprovalStatus
    {
        public string? ExpenseId { get; set; }
        public string? Status { get; set; }
        public string? StatusReason { get; set; }

    }
}
