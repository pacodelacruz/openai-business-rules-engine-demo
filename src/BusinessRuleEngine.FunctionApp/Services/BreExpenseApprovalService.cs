using BusinessRuleEngine.FunctionApp.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessRuleEngine.FunctionApp.Services
{
    public class BreExpenseApprovalService
    {
        private IOptions<BusinessRulesEngineOptions> _options;

        public BreExpenseApprovalService(IOptions<BusinessRulesEngineOptions> options)
        {
            _options = options;
        }

        public string ReturnEndpoint()
        {
            return _options.Value.OpenAiEndpoint;
        }

    }
}
