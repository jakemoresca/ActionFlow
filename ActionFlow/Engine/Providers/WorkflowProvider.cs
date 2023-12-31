﻿using ActionFlow.Domain.Engine;

namespace ActionFlow.Engine.Providers
{
    public class WorkflowProvider : IWorkflowProvider
    {
        public List<Workflow> GetAllWorkflows()
        {
            List<Workflow> workflows = new List<Workflow>();

            //Test
            //Todo: Load from DB, Json, or other source
            var steps = new List<Step>
            {
                new Step("initialize", "Variable", new Dictionary<string, object>
                {
                    { "age", "1" },
                    { "canWalk", "true" },
                }),
                new Step("test variable value", "Variable", new Dictionary<string, object>(), "age == 1 && canWalk == true")
            };

            Workflow workflow = new Workflow("Test Workflow Rule 1", steps);
            workflows.Add(workflow);
            //Test End

            return workflows;
        }
    }
}
