using System;
using System.Collections.Generic;
using System.Linq;
using Aircadia.ObjectModel.DataObjects;
using Aircadia.ObjectModel.Models;
using Aircadia.ObjectModel.Treatments.DOE;
using Aircadia.Services.Serializers;

namespace Aircadia.ObjectModel.Studies
{
	[Serializable()]
	public class RandomDOEStudy : DesignOfExperiment
	{
		[SerializeEnumerable("Factors")]
		public List<Factor> Factors { get; }
		[SerializeEnumerable("Responses", "Response")]
		public List<Data> Responses { get; }
		[Serialize]
		public long Samples { get; }
		[Serialize]
		public SamplingStrategy Strategy { get; }

		[DeserializeConstructor]
		public RandomDOEStudy(string name, string description, WorkflowComponent studiedComponent, List<Factor> factors, List<Data> responses, long samples, SamplingStrategy strategy, string parentName = "")
			: this(name, description, studiedComponent, factors, responses, samples, strategy, false, parentName)
		{
		}

		public RandomDOEStudy(string name, string description, WorkflowComponent studiedComponent, List<Factor> factors, List<Data> responses, long samples, SamplingStrategy strategy, bool createFolder, string parentName = "") 
            : base(name, description, studiedComponent, factors.Select(f => f.Data).ToList(), responses, parentName)
        {
            base.StudiedComponent = studiedComponent;

            // Copy factors to avoid aliasing problems when editing studies
            Factors = factors.Select(f => f.Copy()).ToList();
			Responses = responses;
			Samples = samples;
			Strategy = strategy;

			SetIDColumn();

			foreach (Factor factor in Factors)
				SetColumn(factor.Name, factor.Data);

			foreach (Data response in Responses)
				SetColumn(response.Name, response);

			TableNames.Add(Name);

			switch (strategy)
			{
				case SamplingStrategy.Random:
					Treatment = new RandomDoE(name, description, studiedComponent, Factors, responses, samples);
					break;
				case SamplingStrategy.LatinHypercube:
					Treatment = new LatinHypercube(name, description, studiedComponent, Factors, responses, samples);
					break;
				default:
					break;
			}
			if (createFolder)
				Treatment.CreateFolder();
		}

        public override bool Execute()
        {
			Treatment.Result = ActiveResult;

			// Apply Treatment
			return Treatment.ApplyOn();
        }

		public override string StudyType => "RandomDOEStudy";

		public override Study Copy(string id, string name = null, string parentName = null)
		{
			return new RandomDOEStudy(id, Description, StudiedComponent as WorkflowComponent, Factors, Responses, Samples, Strategy, true, parentName ?? Name);
		}
	}

	public enum SamplingStrategy
	{
		Random,
		LatinHypercube
	}
}
