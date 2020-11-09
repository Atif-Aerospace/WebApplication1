using System.Collections.Generic;
using System.IO;
using Aircadia.ObjectModel.DataObjects;
using Aircadia.ObjectModel.Models;
using Aircadia.ObjectModel.Studies;
using Aircadia.ObjectModel.Distributions;

namespace Aircadia.ObjectModel.Treatments
{
	public class UncertaintyPropagationTreatment : Treatment
	{
		public WorkflowComponent Workflow { get; protected set; }

		public List<IProbabilityDistribution> InputDistributions { get; }
		public List<IProbabilityDistribution> OutputDistributions { get; }

		public IUncertaintyPropagator UncertaintyPropagator { get; }

		public WorkflowComponent workflow;

		public RobOptTemplate robOptTemplate;


		public List<Data> uncertainParameters;
		public List<Data> factors; // List of factors
		public List<int> noOfLevels; // List of corresponding number of levels for factors
		public List<decimal> startingValues;
		public List<decimal> stepSizes;
		public List<Data> responses; // List of responses

		public UncertaintyPropagationTreatment(string name, string description, WorkflowComponent innerWorkflow, List<IProbabilityDistribution> inputDistributions, List<IProbabilityDistribution> outputDistributions, IUncertaintyPropagator uncertaintyPropagator) : base(name, description)
		{
			InputDistributions = inputDistributions;
			OutputDistributions = outputDistributions;

			Workflow = innerWorkflow;

			string directory = Path.GetDirectoryName(databaseFileName);
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(databaseFileName);
			string csvPath = Path.Combine(directory, fileNameWithoutExtension + ".csv");
			UncertaintyPropagator = new UnivariateReducedQuadrature(csvPath);
		}

		public override bool ApplyOn()
		{
			// Update the values of the probability distributions
			int v = 0;
			var parameters = new List<double>();
			foreach (IProbabilityDistribution dist in InputDistributions)
			{
				int param = dist.GetNParameters();
				while (param > 0)
				{
					parameters.Add((double)Workflow.ModelDataInputs[v].Value);
					v++;
					param--;
				}

				dist.Update(parameters.ToArray());

				parameters.Clear();
			}

			UncertaintyPropagator.Propagate(InputDistributions, OutputDistributions, Workflow);

			v = 0;
			foreach (IProbabilityDistribution dist in OutputDistributions)
			{
				Workflow.ModelDataOutputs[v + 0].Value = dist.Mean;
				Workflow.ModelDataOutputs[v + 1].Value = dist.Variance;
				Workflow.ModelDataOutputs[v + 2].Value = dist.Skewness;
				Workflow.ModelDataOutputs[v + 3].Value = dist.Kurtosis;
				v += 4;
			}

			return true;
		}

		public override bool ApplyOn(ExecutableComponent ec)
		{
			Workflow = ec as WorkflowComponent;

			if (workflow != null)
				return ApplyOn();

			return false;
		}



		public double[] Const_vect_mltpl(double Constant, double[] Vector)
		{
			double[] Product = new double[Vector.Length];
			for (int i = 0; i < Vector.Length; i++)
			{
				Product[i] = Constant * Vector[i];
			}
			return Product;
		}



		public override void CreateFolder()
		{
			base.CreateFolder();

			// Add custom metadata for Latin Hypercube
			metadata.AddAttribute("Type", "DesignsStudy");
			metadata.AddAttribute("WorkflowName", workflow.Name);

			metadata.AddTag("Propagator", UncertaintyPropagator.GetType().Name);

			metadata.AddParameter(new IntegerData("ID"));

			foreach (IProbabilityDistribution distribution in InputDistributions)
			{
				Dictionary<string, double> parameters = distribution.GetParameterValuePairs();
				foreach (string name in parameters.Keys)
					metadata.AddParameter(new DoubleData($"{distribution.Name}:{name}", parameters[name]), "UncertainInputParameter");
			}

			foreach (IProbabilityDistribution distribution in OutputDistributions)
			{
				Dictionary<string, double> parameters = distribution.GetParameterValuePairs();
				foreach (string name in parameters.Keys)
					metadata.AddParameter(new DoubleData($"{distribution.Name}:{name}", parameters[name]), "UncertainOutputsParameter");
			}

			metadata.Save();
		}
	}
}
