using Aircadia.ObjectModel.DataObjects;
using Aircadia.ObjectModel.Distributions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aircadia.ObjectModel.Models
{
    class UncertatintyWorkflowModel : Model
    {
        public WorkflowComponent InnerWorkflow { get; }

        public IUncertaintyPropagator UncertaintyPropagator { get; }

        public List<IProbabilityDistribution> InputDistributions { get; }

        public List<IProbabilityDistribution> OutputDistributions { get; }

        public UncertatintyWorkflowModel(WorkflowComponent innerWorkflow, List<Data> inputs, List<Data> outputs, 
            List<IProbabilityDistribution> inputDistributions, List<IProbabilityDistribution> outputDistributions, IUncertaintyPropagator uncertaintyPropagator, 
			string parentName = "", string displayName = "") 
            : base($"{innerWorkflow.Name}.InnerWF", innerWorkflow.Description, inputs, outputs, true, parentName, displayName)
        {
            InnerWorkflow = innerWorkflow;
            InputDistributions = inputDistributions;
            OutputDistributions = outputDistributions;
            UncertaintyPropagator = uncertaintyPropagator;
        }

        public override string ModelType => "UncertatintyWorkflowModel";

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

        public override void PrepareForExecution() { }

		public override Model Copy(string id, string name = null, string parentName = null)
			=> new UncertatintyWorkflowModel(InnerWorkflow, ModelDataInputs, ModelDataOutputs, InputDistributions, OutputDistributions, UncertaintyPropagator, parentName ?? ParentName, name ?? Name);
	}
}
