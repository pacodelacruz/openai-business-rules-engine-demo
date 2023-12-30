using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessRuleEngine.FunctionApp.Models
{

    /// <summary>
    /// Class to implement the Options Pattern described here
    /// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options?view=aspnetcore-8.0
    /// </summary>
    public class BusinessRulesEngineOptions
    {
        public string OpenAiEndpoint { get; set; } = "";

        public string OpenAiKey { get; set; } = "";

    }
}
