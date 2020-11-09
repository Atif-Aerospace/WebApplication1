using System;
using System.Collections.Generic;

using Aircadia.ObjectModel.DataObjects;
using Aircadia.ObjectModel.Models;
using Aircadia.ObjectModel.Workflows;
using Aircadia.ObjectModel.Distributions;
using Aircadia.Services.Serializers;
using System.Linq;

namespace Aircadia
{
	[Serializable()]
	public class WorkflowUncertaintyParametrised : Workflow
	{
        [Serialize("InnerWorkflow", SerializationType.Reference)]
        public WorkflowComponent InnerWorkflow => innerModel.InnerWorkflow;

        [SerializeEnumerable("InputDistributions")]
		public List<IProbabilityDistribution> InputDistributions => innerModel.InputDistributions;
        [SerializeEnumerable("OutputDistributions")]
		public List<IProbabilityDistribution> OutputDistributions => innerModel.OutputDistributions;

        [Serialize]
		public IUncertaintyPropagator UncertaintyPropagator => innerModel.UncertaintyPropagator;

        private readonly UncertatintyWorkflowModel innerModel;

        [DeserializeConstructor]
        public WorkflowUncertaintyParametrised(string name, string description, WorkflowComponent innerWorkflow, List<Data> modelDataInputs, List<Data> modelDataOutputs, 
            List<IProbabilityDistribution> inputDistributions, List<IProbabilityDistribution> outputDistributions, IUncertaintyPropagator uncertaintyPropagator, string parentName = "")
            : base(name, description, modelDataInputs, modelDataOutputs, new List<WorkflowComponent>() { innerWorkflow }, new List<WorkflowComponent>() { innerWorkflow }, parentName: parentName)
        {
            innerModel = new UncertatintyWorkflowModel(innerWorkflow, modelDataInputs, modelDataOutputs, inputDistributions, outputDistributions, uncertaintyPropagator);

            Components.Clear();
            Components.Add(innerModel);

            ScheduledComponents.Clear();
            ScheduledComponents.Add(innerModel);
        }

        public override bool Execute()
		{
			// Update the values of the probability distributions
			int v = 0;
			var parameters = new List<double>();
			foreach (IProbabilityDistribution dist in InputDistributions)
			{
				int param = dist.GetNParameters();
				while (param > 0)
				{
					parameters.Add((double)ModelDataInputs[v].Value);
					v++;
					param--;
				}

				dist.Update(parameters.ToArray());

				parameters.Clear();
			}

			UncertaintyPropagator.Propagate(InputDistributions, OutputDistributions, InnerWorkflow);

			v = 0;
			foreach (IProbabilityDistribution dist in OutputDistributions)
			{
				ModelDataOutputs[v + 0].Value = dist.Mean;
				ModelDataOutputs[v + 1].Value = dist.Variance;
				ModelDataOutputs[v + 2].Value = dist.Skewness;
				ModelDataOutputs[v + 3].Value = dist.Kurtosis;
				v += 4;
			}

			return true;
		}

		public override Workflow Copy(string id, string name = null, string parentName = null)
		{
			return new WorkflowUncertaintyParametrised(id, Description, InnerWorkflow, ModelDataInputs.ToList(), ModelDataOutputs.ToList(),
				InputDistributions.ToList(), OutputDistributions.ToList(), UncertaintyPropagator, parentName ?? parentName);
		}
	}
}
