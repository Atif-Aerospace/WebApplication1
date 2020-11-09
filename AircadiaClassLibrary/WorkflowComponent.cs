/**************************************************************************************************
AIRCADIA - Multiphysics model-based design tool                                                   *
Engineering Design Group                                                                          *
Department of Aerospace Engineering, School of Engineering, Cranfield University                  *
Cranfield, Bedfordshire, MK43 0AL, UK                                                             *
/**************************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using Aircadia.ObjectModel.DataObjects;
using Aircadia.ObjectModel.Workflows;
using Aircadia.Services.Serializers;

namespace Aircadia.ObjectModel.Models
{
	[Serializable()]
    public abstract class WorkflowComponent : ExecutableComponent
    {
		// Specifies whether the model is auxiliary or not
		[Serialize]
		public bool IsAuxiliary { get; set; }

		protected WorkflowComponent(string name, string description, bool isAuxiliary = false, string parentName = "")
            : base(name, description, parentName)
        {
			IsAuxiliary = isAuxiliary;
        }

		protected WorkflowComponent(string name, string description, List<Data> modelDataInputs, List<Data> modelDataOutputs, bool isAuxiliary = false, string parentName = "")
            : this(name, description, isAuxiliary, parentName)
        {
			// Call ToList() to avoid some obscure bugs introduced by aliasing of the lists
			ModelDataInputs = modelDataInputs?.ToList() ?? new List<Data>();
			ModelDataOutputs = modelDataOutputs?.ToList() ?? new List<Data>();
        }


		///// <summary>
		///// The string returned by this function is the one which will be printed in the properties window of AirCADia for the selected model object.
		///// </summary>
		///// <returns></returns>
		//public override string PropertiesSummaryText()
		//{
		//	string output = "NAME: " + Name + " \r\n\r\n";

		//	// output = output + "CATEGORY: " + "\r\n" + category + "\r\n";

		//	// output = output + "AUXILIARY: " + "\r\n" + aux + "\r\n";

		//	output = output + "INPUTS: " + "\r\n";
		//	string inputData = "";
		//	foreach (Data data in ModelDataInputs)
		//		inputData += (data.Id + ": " + data.ValueAsString + "\r\n ");
		//	output += inputData;

		//	output = output + "OUTPUTS: " + "\r\n";
		//	string outputData = "";
		//	foreach (Data data in ModelDataOutputs)
		//		outputData += (data.Id + ": " + data.ValueAsString + "\r\n ");
		//	output += outputData;


		//	return output;
		//}

		public bool IsReversed(IEnumerable<Data> outputs)
		{
			if (outputs.Count() != ModelDataOutputs.Count)
				return true;

			var hash = new HashSet<Data>(ModelDataOutputs);
			foreach (Data data in outputs)
				if (!hash.Contains(data))
					return true;

			return false;
		}

		public bool IsReversed(IEnumerable<string> outputs)
		{
			if (outputs.Count() != ModelDataOutputs.Count)
				return true;

			var hash = new HashSet<string>(ModelDataOutputs.Select(d => d.Id));
			foreach (string data in outputs)
				if (!hash.Contains(data))
					return true;

			return false;
		}
    }

	public static class WorkflowComponentExtensions
	{
		/// <summary>
		/// Return a mapping between data and their status, as well as List with the inputs and outputs
		/// </summary>
		/// <param name="components"></param>
		/// <param name="allData"></param>
		/// <param name="inputs"></param>
		/// <param name="outputs"></param>
		/// <returns></returns>
		public static Dictionary<string, IOStatus> GetInputsOutputsStatus(this IEnumerable<WorkflowComponent> components, IEnumerable<Data> allData, out List<Data> inputs, out List<Data> outputs)
		{
			Dictionary<string, IOStatus> status = null;
			(inputs, outputs, status) = GetInputsOutputsStatus(components, allData);
			return status;
		}

		public static (List<Data> inputs, List<Data> outputs, Dictionary<string, IOStatus> status) GetInputsOutputsStatus(this IEnumerable<WorkflowComponent> components, IEnumerable<Data> allData)
		{
			Dictionary<string, IOStatus> dataStatus = GetDataStatus(components, allData);

			var inputs = new List<Data>();
			var outputs = new List<Data>();
			foreach (Data d in allData)
			{
				if (dataStatus[d.Id] == IOStatus.Input)
					inputs.Add(d);
				else
					outputs.Add(d);
			}

			return (inputs, outputs, dataStatus);
		}

		/// <summary>
		/// Return a mapping between data and their status
		/// </summary>
		/// <param name="components"></param>
		/// <param name="allData"></param>
		/// <returns></returns>
		public static Dictionary<string, IOStatus> GetDataStatus(this IEnumerable<WorkflowComponent> components, IEnumerable<Data> allData)
		{
			var dataStatus = allData.ToDictionary(d => d.Id, d => IOStatus.NonRelated);
			foreach (WorkflowComponent component in components)
			{
				foreach (Data input in component.ModelDataInputs)
				{
					string name = input.Id;
					if (dataStatus.ContainsKey(name))
					{
						switch (dataStatus[name])
						{
							// If it has been visited and determined output, will be changed to both,
							case IOStatus.Output:
								dataStatus[name] = IOStatus.Both;
								break;
							// Hasn't been visited, set as input
							case IOStatus.NonRelated:
								dataStatus[name] = IOStatus.Input;
								break;
								// Otherwise it has been visited and will keep the current satatus
						}
					}
				}

				foreach (Data output in component.ModelDataOutputs)
				{
					string name = output.Id;
					if (dataStatus.ContainsKey(name))
					{
						switch (dataStatus[name])
						{
							// Hasn't been visited, set as output
							case IOStatus.NonRelated:
								dataStatus[name] = IOStatus.Output;
								break;
							// If it has been visited and determined input, will be changed to both,
							case IOStatus.Input:
								dataStatus[name] = IOStatus.Both;
								break;
							// If it's already output we have a problem, this workflow cannot be created
							case IOStatus.Both:
							case IOStatus.Output:
								dataStatus[name] = IOStatus.Conflict;
								break;
						}
					}
				}
			}

			return dataStatus;
		}

		/// <summary>
		/// Return all the associated variables with a set of components
		/// </summary>
		/// <param name="models"></param>
		/// <returns></returns>
		public static List<Data> GetAllData(this IEnumerable<WorkflowComponent> models)
		{
			var allData = new HashSet<Data>();
			foreach (WorkflowComponent model in models)
			{
				foreach (Data data in model.GetAllData())
				{
					allData.Add(data);
				}
			}
			return allData.ToList();
		}

		public static List<Data> GetAllData(this IEnumerable<WorkflowComponent> models, IEnumerable<Data> inputs, out int[] dataIndices)
		{
			List<Data> allData = models.GetAllData();
			var indices = allData.Select((d, i) => new { d.Id, i } ).ToDictionary(d => d.Id, d => d.i);
			dataIndices = new int[allData.Count];
			foreach (Data input in inputs)
			{
				if (!indices.ContainsKey(input.Id))
				{
					continue;
				}

				int inputIndex = indices[input.Id];
				dataIndices[inputIndex] = 1;
			}
			return allData;
		}

		public static List<WorkflowComponent> GetAllComponents(this IEnumerable<WorkflowComponent> components, bool recurseInSCCs = true)
		{
			var allComponents = new List<WorkflowComponent>();
			foreach (ExecutableComponent component in components)
			{
				// Recurse
				if (component is WorkflowSCC scc && !recurseInSCCs)
				{
					allComponents.Add(scc);
				}
				else if (component is Workflow wf)
				{
					allComponents.AddRange(GetAllComponents(wf.Components, recurseInSCCs));
				}
				else if (component is WorkflowComponent m) // Base Case
				{
					allComponents.Add(m);
				}
			}

			return allComponents;
		}
	}

	/// <summary>
	/// Input: exclusively input in default condition
	/// Ouput: exclusively output in default condition
	/// Baoth: input for some model output for others in default condition
	/// Input: output for more than one in default condition
	/// NonRelated: non related to the set of models
	/// </summary>
	public enum IOStatus { Input, Output, Both, Conflict, NonRelated }
}
