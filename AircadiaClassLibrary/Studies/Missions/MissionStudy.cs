using System;
using Aircadia.ObjectModel.DataObjects;
using Aircadia.ObjectModel.Models;
using Aircadia.ObjectModel.Treatments;
using Aircadia.Services.Serializers;

namespace Aircadia.ObjectModel.Studies.Missions
{
	[Serializable]
	public class MissionStudy : Study
	{
		[Serialize]
		public Mission Mission { get; }

		[DeserializeConstructor]
		public MissionStudy(string name, string description, WorkflowComponent studiedComponent, Mission mission, string parentName = "")
			: base(name, null, null, description, parentName)
		{
			StudiedComponent = studiedComponent;
			Mission = mission;

			SetIDColumn();

			ModelDataInputs = studiedComponent.ModelDataInputs;
			foreach (Data input in ModelDataInputs)
				SetColumn(input.Name, input);

			ModelDataOutputs = studiedComponent.ModelDataOutputs;
			foreach (Data output in ModelDataOutputs)
				SetColumn(output.Name, output);

			TableNames.Add(Name);


			Treatment = new MissionTreatment(name, description, studiedComponent, mission);
			Treatment.CreateFolder();
		}

		public override bool Execute()
		{
			bool status = Treatment.ApplyOn();

			return status;
		}

		public override Study Copy(string id, string name = null, string parentName = null)
		{
			return new MissionStudy(id, Description, StudiedComponent as WorkflowComponent, Mission, parentName ?? Name);
		}
	}



}
