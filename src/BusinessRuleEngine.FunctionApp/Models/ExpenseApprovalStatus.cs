using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessRuleEngine.FunctionApp.Models
{
    public class ExpenseApprovalStatus
    {
        public required string ExpenseId { get; set; }
        public Constants.ExpenseConstants.ExpenseStatus Status { get; set; }
        public string? StatusReason { get; set; }

    }
}
