using Newtonsoft.Json;
using RulesEngine.Models;

namespace ActionFlow.Rules
{
    public class ApiCallRule : RuleWrapper
    {
        public override string RuleType => "ApiCall";
        public string? Url { get; set; }
        public string Method { get; set; } = "GET";
        public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
        public string? ReturnVariableName { get; set; }
        public string? Content { get; set; }

        public override Rule AsRuleEngineRule()
        {
            var rule = base.AsRuleEngineRule();
            var serializedHeaders = JsonConvert.SerializeObject(JsonConvert.SerializeObject(Headers));
            //JsonConvert.SerializeObject(Headers);

            switch (Method)
            {
                case "GET":
                    rule.LocalParams = new List<ScopedParam>
                    {
                        { new ScopedParam { Name = ReturnVariableName!, Expression = $"ApiClient.CallGet(\"{Url}\", {serializedHeaders}).Result" } }
                    };
                    break;

                case "POST":
                    var content = JsonConvert.SerializeObject(Content);

                    rule.LocalParams = new List<ScopedParam>
                    {
                        { new ScopedParam { Name = ReturnVariableName!, Expression = $"ApiClient.CallPost(\"{Url}\", {content}, {serializedHeaders}).Result" } }
                    };
                    break;
            }


            return rule;
        }
    }
}
