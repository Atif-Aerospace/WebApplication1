using System.Collections.Generic;
using Aircadia.ObjectModel.DataObjects;
using Aircadia.ObjectModel.Models;
using Aircadia.ObjectModel.Distributions;
using Aircadia.Services.Serializers;

namespace Aircadia.ObjectModel.Treatments.DOE
{
	public class SensitivityAnalysisTreatment : Treatment
	{
		public ISensitivityAnalyser Analyser { get; }
		[Serialize(Type = SerializationType.Reference)]
		public WorkflowComponent Component { get; private set; }
		[SerializeEnumerable("Inputs")]
		public List<IProbabilityDistribution> InputDistributions { get; }
		[SerializeEnumerable("Outputs")]
		public List<IProbabilityDistribution> OutputDistributions { get; }
		[Serialize]
		public Type AnalyserType { get; }

		[DeserializeConstructor]
		public SensitivityAnalysisTreatment(string name, string description, WorkflowComponent component, List<IProbabilityDistribution> inputDistributions, List<IProbabilityDistribution> outputDistributions, Type analyserType) :
			base(name, description)
		{
			Component = component;
			InputDistributions = inputDistributions;
			OutputDistributions = outputDistributions;
			AnalyserType = analyserType;
			databaseFileName = System.IO.Path.Combine("Studies", name, name + ".sdf");

			switch (AnalyserType)
			{
				case Type.FAST:
					Analyser = new FASTSensitivityAnalyser(true, CsvPath);
					break;
			}
		}


		public override bool ApplyOn(ExecutableComponent ec)
		{
			Component = ec as WorkflowComponent;
			return ApplyOn();
		}

		public override bool ApplyOn()
		{
			Analyser.Analyse(InputDistributions, OutputDistributions, Component);
			return true;
		}

		public override void CreateFolder()
		{
			base.CreateFolder();

			// Add custom metadata for Latin Hypercube
			metadata.AddAttribute("Type", "DesignsStudy");
			metadata.AddAttribute("WorkflowName", Component.Name);

			metadata.AddTag("Name", "Latin Hypercube Sampling");

			metadata.AddParameter(new IntegerData("ID"));
			metadata.AddParameter(new StringData("Output"), "UncertainOutput");

			foreach (IProbabilityDistribution distribution in InputDistributions)
				metadata.AddParameter(distribution.Data, "UncertainInput", 
					("DistributionType", distribution.Name));

			metadata.Save();
		}

		public enum Type
		{
			FAST
		}

	}
}
