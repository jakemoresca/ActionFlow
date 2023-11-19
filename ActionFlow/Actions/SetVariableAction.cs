using ActionFlow.Domain.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ActionFlow.Actions
{
    public class SetVariableAction : ActionBase
    {
        public static string VariablesKey = "Variables";

        public override Task ExecuteAction()
        {
            var variables = ExecutionContext!.GetActionProperty<Dictionary<string, string>>(VariablesKey);

            foreach (var variable in variables)
            {
                var parameter = new Parameter
                {
                    Name = variable.Key,
                    Expression = variable.Value
                };

                ExecutionContext.AddOrUpdateParameter(parameter);
            }

            return Task.CompletedTask;
        }
    }
}
