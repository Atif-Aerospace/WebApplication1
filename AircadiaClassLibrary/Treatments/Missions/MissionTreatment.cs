using System;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra.Double;
using System.IO;
using Aircadia.ObjectModel.Models;
using Aircadia.ObjectModel.Studies.Missions;
using System.Linq;
using Aircadia.ObjectModel.DataObjects;

namespace Aircadia.ObjectModel.Treatments
{
	[Serializable()]
	public class MissionTreatment : Treatment
	{
		public List<WorkflowComponent> Components => Mission.Components;
		public Mission Mission { get; }
		//public List<MissionParameter> MissionParameters { get; }
		public List<VariableMissionParameter> ComponentVariableInputs { get; protected set; }
		public List<ConstantMissionParameter> ComponentConstantInputs { get; protected set; }
		public List<MissionParameter> ComponentOutputs { get; protected set; }

		double[] times;
		public DenseMatrix H;
		readonly Random random = new Random();

		public MissionTreatment(string name, string description, WorkflowComponent component, Mission mission) : base(name, description)
		{
			Mission = mission;
		}

		protected void ExecuteSegment(WorkflowComponent component, MissionSegment segment)
		{
			times = segment.Times;
			int NSamples = segment.Samples;

			//segment.GetComponentCalssificationAndIndices(Component, 
			//	out int[] variablesInputsInModel, out int[] variablesInputsInTable, out int[] constantInputsInModel, out int[] constantInputsInSegment,
			//	out int[] variablesOutputsInModel, out int[] variablesOutputsInTable, out int[] constantOutputsInModel, out int[] constantOutputsInSegment);

			var (variablesInputsInModel, constantInputsInModel, variablesOutputsInModel, constantOutputsInModel, missionParameters) = segment.GetComponentCalssificationAndIndices(component);

			int NInVariables = variablesInputsInModel.Length;
			int NInConstants = constantInputsInModel.Length;
			int NOutVariables = variablesOutputsInModel.Length;
			int NOutConstants = constantOutputsInModel.Length;

			// Constants
			for (int i = 0; i < NInConstants; i++)
			{
				var parameter = missionParameters[i] as ConstantMissionParameter;
				component.ModelDataInputs[constantInputsInModel[i]].Value = parameter.Value;
			}

			string csvPath = $"{Path.GetFileNameWithoutExtension(CsvPath)}.{component.Name}.{segment}.csv";
			using (var csvFile = new CSVFiler(CsvPath))
			{
				for (int i = 0; i < NSamples; i++)
				{
					// If user request to cancel the iterations the method will throw
					EndIteratoinIfCancelled();

					// Variables
					for (int j = 0; j < NInVariables; j++)
					{
						var parameter = missionParameters[NInConstants + j] as VariableMissionParameter;
						component.ModelDataInputs[variablesInputsInModel[j]].Value = parameter.Values[i];
					}

					bool statusToCheck = component.Execute();

					// Write csv file
					if (statusToCheck)
					{
						// Update outputs in segment
						for (int j = 0; j < NOutVariables; j++)
						{
							var parameter = missionParameters[NInConstants + NInVariables + j] as VariableMissionParameter;
							parameter.Update(component.ModelDataOutputs[variablesOutputsInModel[j]].Value, i);
						}

						// Output Constants
						for (int j = 0; j < NInConstants; j++)
						{
							var parameter = missionParameters[NInConstants + NInVariables + NOutVariables + j] as ConstantMissionParameter;
							parameter.Update(component.ModelDataOutputs[constantOutputsInModel[j]].Value);
						}

						csvFile.NewRow();

						csvFile.AddToRow(i);
						csvFile.AddToRow(times[i], 2);
						
						for (int d = 0; d < NInVariables; d++)
							csvFile.AddToRow(component.ModelDataInputs[variablesInputsInModel[d]]);

						for (int d = 0; d < NInConstants; d++)
							csvFile.AddToRow(component.ModelDataInputs[constantInputsInModel[d]]);

						for (int d = 0; d < NOutVariables; d++)
							csvFile.AddToRow(component.ModelDataOutputs[variablesOutputsInModel[d]]);

						for (int d = 0; d < NOutConstants; d++)
							csvFile.AddToRow(component.ModelDataOutputs[constantOutputsInModel[d]]);

						csvFile.WriteRow();
					}
				}
			}
		}

		public override bool ApplyOn(ExecutableComponent ec) => ApplyOn();

		public override bool ApplyOn()
		{
			//if (MissionSegments.Count == 0)
			//	return false;

			IterationSize = Components.Aggregate(0, (t, c) => t += Mission.MissionSegments(c).Count);
			int i = 0;
			foreach (WorkflowComponent component in Components)
			{
				foreach (MissionSegment segment in Mission.MissionSegments(component))
				{
					ExecuteSegment(component, segment);
					ReportProgress(++i);
				}
			}

			WriteResults();

			return true;
		}

		private void WriteResults()
		{
			//using (var csvFile = new CSVFiler(CsvPath))
			//{
			//	var parametersLookup = Mission.Segments.SelectMany(s => s.Parameters.Select(p => (p, s.Times))).ToLookup(t => t.Item1.Data.Name);

			//	csvFile.NewRow();
			//	csvFile.AddToRow(1);

			//	foreach (var data in Mission.Data)
			//	{
			//		var tuples = parametersLookup[data.Name];
			//		double[] times = tuples.Select(t => t.Item2).Aggregate(new List<double>(), (t, l) =>
			//		{
			//			t.AddRange(l);
			//			return t;
			//		}, t => t.ToArray());

			//		var values = tuples.Select(t =>
			//		{
			//			(MissionParameter parameter, double[] timeArray) = t;
			//			var vals = new double[0];
			//			if (parameter is ConstantMissionParameter constantParameter)
			//			{
			//				if (constantParameter.Data.Value is double d)
			//				{
			//					vals = MathNet.Numerics.Generate.Repeat(timeArray.Length, d);

			//				}
			//				else if (constantParameter.Data.Value is int i)
			//				{
			//					vals = MathNet.Numerics.Generate.Repeat(timeArray.Length, (double)i);
			//				}
			//			}
			//			else if (parameter is VariableMissionParameter variableParameter)
			//			{
			//				vals = variableParameter.Values.Cast<double>().ToArray();
			//			}
			//			return vals;
			//		}).Aggregate(new List<double>(), (t, l) =>
			//		{
			//			t.AddRange(l);
			//			return t;
			//		}, t => t.ToArray());

			//		csvFile.AddToRow(DoubleVectorData.ValueToString(times));
			//		csvFile.AddToRow(DoubleVectorData.ValueToString(values));
			//	}

			//	csvFile.WriteRow();
			//}

			using (var csvFile = new CSVFiler(CsvPath))
			{
				csvFile.NewRow();
				csvFile.AddToRow(1);

				foreach (var segment in Mission.Segments)
				{
					double[] times = segment.Times;
					csvFile.AddToRow(DoubleVectorData.ValueToString(times));

					foreach (var parameter in segment.Parameters)
					{
						var values = new double[0];
						if (parameter is ConstantMissionParameter constantParameter)
						{
							if (constantParameter.Data.Value is double d)
							{
								values = MathNet.Numerics.Generate.Repeat(times.Length, d);

							}
							else if (constantParameter.Data.Value is int i)
							{
								values = MathNet.Numerics.Generate.Repeat(times.Length, (double)i);
							}
						}
						else if (parameter is VariableMissionParameter variableParameter)
						{
							values = variableParameter.Values.Cast<double>().ToArray();
						}
						csvFile.AddToRow(DoubleVectorData.ValueToString(values));
					}
				}

				csvFile.WriteRow();
			}
		}

		public override void CreateFolder()
		{
			base.CreateFolder();


			metadata.AddAttribute("Type", "DesignsStudy");
			metadata.AddAttribute("WorkflowName", Mission.Workflow.Name);

			//var csvElement = new XElement("CSV", new XAttribute("Path", $"{studyName}.csv"));
			//resultElement.Add(csvElement);

			metadata.AddTag("Name", "Mission Study");

			metadata.AddParameter(new IntegerData("ID"));

			foreach (MissionSegment segment in Mission.Segments)
			{
				string name = segment.Name;
				metadata.AddParameter(new DoubleVectorData($"{name}_Time"), "MissionParameter");

				foreach (MissionParameter parameter in segment.Parameters)
				{
					metadata.AddParameter(new DoubleVectorData($"{name}_{parameter.Name}"), "MissionParameter");
				}
			}

			//foreach (Data data in Mission.Data)
			//{
			//	metadata.AddParameter(new DoubleVectorData($"{data.Name}_Time"), "MissionParameter");
			//	metadata.AddParameter(new DoubleVectorData($"{data.Name}"), "MissionParameter");
			//}

			metadata.Save();
		}

		
	}
}
