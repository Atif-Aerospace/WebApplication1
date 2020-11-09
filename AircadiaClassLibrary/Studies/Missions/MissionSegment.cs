using Aircadia.ObjectModel.DataObjects;
using Aircadia.ObjectModel.Models;
using Aircadia.Services.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Aircadia.ObjectModel.Studies.Missions
{
	public class MissionSegment : IAircadiaComponent
	{
		private Dictionary<string, MissionParameter> parameters  = new Dictionary<string, MissionParameter>();

		[SerializeEnumerable("Parameters")]
		public List<MissionParameter> Parameters => parameters.Values.ToList();
		[Serialize]
		public string Name { get; }
		[Serialize]
		public string Description { get; }
		[Serialize]
		public int Samples { get; }
		[Serialize]
		public double Start { get; }
		[Serialize]
		public double Duration { get; }

		public double[] Times { get; }

		[DeserializeConstructor]
		public MissionSegment(string name, string description, int samples, double start, double duration, List<MissionParameter> parameters = null)
		{
			Name = name;
			Description = description;
			Samples = samples;
			Start = start;
			Duration = duration;
			Times = MathNet.Numerics.Generate.LinearSpaced(samples, start, start + duration);

			if (parameters == null)
			{
				parameters = new List<MissionParameter>();
			}

			foreach (MissionParameter parameter in parameters)
			{
				Add(parameter);
			}
		}

		public void AddRange(IEnumerable<MissionParameter> parameters, bool replaceExisting = false)
		{
			foreach (MissionParameter parameter in parameters)
			{
				Add(parameter, replaceExisting);
			}
		}

		public void Add(MissionParameter parameter, bool replaceExisting = false)
		{
			CheckConsistency(parameter);

			switch (parameter)
			{
				case ConstantOutputMissionParameter comp:
					break;
				case ConstantMissionParameter cmp:
					break;
				case VariableMissionParameter vmp:
					break;
				default:
					break;
			}

			if (!parameters.ContainsKey(parameter.Name))
			{
				parameters.Add(parameter.Name, parameter);
			}
			else if (replaceExisting)
			{
				parameters[parameter.Name] = parameter;
			}
		}

		private void CheckConsistency(MissionParameter parameter)
		{
			Check("Number of Samples", parameter.Samples, Samples);
			Check("Duration", parameter.Duration, Duration);
			Check("Start", parameter.Start, Start);

			void Check(string name, double v1, double v2)
			{
				if (v1 != v2)
				{
					throw new ArgumentException($"The parameter '{name}' is not consistenst with the segment '{name}'. {v1} vs {v2}");
				}
			}
		}

		public void Remove(MissionParameter parameter)
		{
			switch (parameter)
			{
				case ConstantOutputMissionParameter comp:
					break;
				case ConstantMissionParameter cmp:
					break;
				case VariableMissionParameter vmp:
					break;
				default:
					break;
			}

			if (parameters.ContainsKey(parameter.Name))
			{
				parameters.Remove(parameter.Name);
			}
		}

		public void RemoveRange(IEnumerable<MissionParameter> parameters)
		{
			foreach (MissionParameter parameter in parameters)
			{
				Remove(parameter);
			}
		}

		public override string ToString() => Name;
		
		public (int[] variableIn, int[] constantIn, int[] variableOut, int[] constantOut, List<MissionParameter> missionParameters) GetComponentCalssificationAndIndices(WorkflowComponent component)
		{
			var variableInputs = new List<string>();
			var constantInputs = new List<string>();
			var variableOutputs = new List<string>();
			var constantOutputs = new List<string>();

			var modelDataDict = new Dictionary<string, int>();
			var missionParameters = new List<MissionParameter>(component.ModelDataInputs.Count + component.ModelDataOutputs.Count);
			for (int i = 0; i < component.ModelDataInputs.Count; i++)
			{
				Data data = component.ModelDataInputs[i];
				string name = data.Name;

				try
				{
					MissionParameter parameter = parameters[name];
					missionParameters.Add(parameter);

					if (parameter is VariableMissionParameter vmp)
					{
						variableInputs.Add(name);
					}
					else if (parameter is ConstantMissionParameter cmp)
					{
						constantInputs.Add(name);
					}
				}
				catch (KeyNotFoundException knfe)
				{
					throw new KeyNotFoundException($"No parameter for variable '{data}' was found in segment {Name}", knfe);
				}

				modelDataDict[name] = i;
			}

			for (int i = 0; i < component.ModelDataOutputs.Count; i++)
			{
				Data data = component.ModelDataInputs[i];
				string name = data.Name;

				try
				{
					MissionParameter parameter = parameters[name];
					missionParameters.Add(parameter);

					if (parameter is VariableMissionParameter vmp)
					{
						variableOutputs.Add(name);
					}
					else if (parameter is ConstantMissionParameter cmp)
					{
						constantOutputs.Add(name);
					}
				}
				catch (KeyNotFoundException knfe)
				{
					throw new KeyNotFoundException($"No parameter for variable '{data}' was found in segment {Name}", knfe);
				}

				modelDataDict[name] = i;
			}

			int[] variableIn = variableInputs.Select(v => modelDataDict[v]).ToArray();
			int[] constantIn = constantInputs.Select(v => modelDataDict[v]).ToArray();
			int[] variableOut = variableOutputs.Select(v => modelDataDict[v]).ToArray();
			int[] constantOut = constantOutputs.Select(v => modelDataDict[v]).ToArray();

			return (variableIn, constantIn, variableOut, constantOut, missionParameters);
		}
	}
}
