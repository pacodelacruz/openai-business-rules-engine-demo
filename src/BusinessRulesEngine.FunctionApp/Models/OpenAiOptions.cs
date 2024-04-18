using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessRulesEngine.FunctionApp.Models
{

    /// <summary>
    /// Class to implement the Options Pattern described at
    /// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options?view=aspnetcore-8.0
    /// </summary>
    public class OpenAiOptions
    {
        private float _temperature = 0;

        public string OpenAiEndpoint { get; set; } = "";
        public string OpenAiKey { get; set; } = "";
        public string OpenAiModelDeployment { get; set; } = "";

        //public string Temperature { get; set; } = "0";
        public float Temperature
        {
            get { return _temperature; }
            set { 
                if (!float.TryParse(value.ToString(), out _temperature))
                    _temperature = value; 
            }

            

        }
    }
}
