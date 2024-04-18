using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessRulesEngine.FunctionApp.Constants
{
    public class ExpenseConstants
    {
        public enum ExpenseStatus
        {
            None = 0,
            Approved = 1, 
            RequiresManualApproval = 2, 
            Rejected = 3
        }

        public enum ExpenseType
        { 
            Accommodation = 0,
            Transporation = 1,
            Meals = 2,
            Entertainment = 3,
            Other = 4
        }
    }
}
