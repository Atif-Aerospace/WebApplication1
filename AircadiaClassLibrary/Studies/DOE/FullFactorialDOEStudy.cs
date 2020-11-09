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
    public class FullFactorialDOEStudySergio : DesignOfExperiment
    {
		[SerializeEnumerable("Factors")]
		public List<BoundedDesignVariableNoInitial> Factors { get; }
		[SerializeEnumerable("Responses", "Response")]
		public List<Data> Responses { get; }

		[DeserializeConstructor]
		public FullFactorialDOEStudySergio(string name, string description, WorkflowComponent studiedComponent, List<BoundedDesignVariableNoInitial> factors, List<Data> responses, string parentName = "") 
			: this(name, description, studiedComponent, factors, responses, false, parentName)
		{
		}

		public FullFactorialDOEStudySergio(string name, string description, WorkflowComponent studiedComponent, List<BoundedDesignVariableNoInitial> factors, List<Data> responses, bool createFolder, string parentName = "") 
            : base(name, description, studiedComponent, factors.Select(f => f.Data).ToList(), responses, parentName)
        {
            base.StudiedComponent = studiedComponent;

            // Copy factors to avoid aliasing problems when editing studies
			Factors = factors.Select(f => f.Copy() as BoundedDesignVariableNoInitial).ToList();
			Responses = responses;


			SetIDColumn();

			foreach (BoundedDesignVariableNoInitial factor in Factors)
				SetColumn(factor.Name, factor.Data);


			foreach (Data response in Responses)
				SetColumn(response.Name, response);

			TableNames.Add(Name);

			Treatment = new FullFactorial("FullFactorial", "", studiedComponent, Factors, responses);
			if (createFolder)
				Treatment.CreateFolder();
		}

        public override bool Execute()
        {
			Treatment.Result = ActiveResult;

			// Apply Treatment
			return Treatment.ApplyOn();
        }

		public override string StudyType => "FullFactorialDOEStudy";

		public override Study Copy(string id, string name = null, string parentName = null)
		{
			return new FullFactorialDOEStudySergio(id, Description, StudiedComponent as WorkflowComponent, Factors, Responses, true, parentName ?? Name);
		}
	}
}
