using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Aircadia.ObjectModel.DataObjects;
using Aircadia.ObjectModel.Models;
using Aircadia.Services.Serializers;
using Aircadia.WorkflowManagement;

namespace Aircadia.ObjectModel.Workflows
{
	[Serializable()]
	public class Workflow : WorkflowComponent
	{
		// Specifies the Workflow type for serialization
		public virtual string WorkflowType => "Workflow";
		[Serialize(Default = "GroupNonReversibleOnly")]
		public virtual string ScheduleMode { get; protected set; }

		[SerializeEnumerable("Components", "Component")]
		public List<WorkflowComponent> Components { get; }
		[SerializeEnumerable("ScheduledComponents", "Component", ConstructorOnly = true)]
		public List<WorkflowComponent> ScheduledComponents { get; protected set; }

		[Serialize]
		public IDependencyAnalysis DependencyAnalysis { get; set; }

		/// <summary>
		/// Creation Constructor
		/// </summary>
		/// <param name="name"></param>
		/// <param name="description"></param>
		/// <param name="inputs"></param>
		/// <param name="modelDataOutputs"></param>
		/// <param name="components"></param>
		/// <param name="scheduledComponents"></param>
		/// <param name="isAuxiliary"></param>
		/// <param name="scheduleMode"></param>
		[DeserializeConstructor]
		public Workflow(string name, string description, List<Data> modelDataInputs, List<Data> modelDataOutputs, List<WorkflowComponent> components, 
			List<WorkflowComponent> scheduledComponents, bool isAuxiliary = false, string scheduleMode = "", string parentName = "") 
			: base(name, description, isAuxiliary, parentName)
		{
			ModelDataInputs = modelDataInputs;
			ModelDataOutputs = modelDataOutputs;
			Components = components;
			ScheduledComponents = scheduledComponents;
			if (!String.IsNullOrWhiteSpace(scheduleMode))
				ScheduleMode = scheduleMode;

			//if (!isScheduled)
			//{
			//	// Used as temp variable to hold all components
			//	ScheduledComponents = GetAllModels();
			//	int[] indObjects = null;
			//	List<Data> allData = WorkflowComponent.GetDataObjects(ScheduledComponents, inputs, out indObjects);
			//	// Get Iputs indices
			//	ScheduledComponents = cComputationalProcessManager.ComputeModelsSchedule(ScheduledComponents, allData, indObjects, name);
			//}
		}

		public override bool Execute() => Execute_();

		protected bool Execute_()
		{
			// Storing initial values
			//object[] initin = this.ModelDataInputs.Select(d => d.Value).ToArray();
			//object[] initout = this.ModelDataOutputs.Select(d => d.Value).ToArray();

			bool status = true;

			// Solving Step-by-Step Locally
			foreach (WorkflowComponent component in ScheduledComponents)
			{
				Debug.WriteLine("");
				Debug.WriteLine($"Executing {component.FullName}");

				Debug.Write(String.Join("", component.ModelDataInputs.Select(d => $"\ti {d.FullName}: {d.ValueAsString}\n")));

				status = component.Execute();

				Debug.Write(String.Join("", component.ModelDataOutputs.Select(d => $"\to {d.FullName}: {d.ValueAsString}\n")));

				if (status == false)
				{
					//MessageBox.Show("Execution of model \"" + component.Name + "\" in workflow \"" + Name + "\" has been failed.", "Workflow execution terminated");
					break;
				}

			}

			//if (status == false)
			//{
			//	//Console.WriteLine("Values of data reset to initial in Subprocess " + name + "\r\n");
			//	// Restoring back to initial values
			//	for (int i = 0; i < this.ModelDataInputs.Count; i++)
			//		this.ModelDataInputs[i].Value = initin[i];
			//	for (int i = 0; i < this.ModelDataOutputs.Count; i++)
			//		this.ModelDataOutputs[i].Value = initout[i];
			//}
			return status;
		}

		/// <summary>
		/// This functions extracts all the base models/subprocess inside this subprocess. 
		/// Recursive algorithm
		/// </summary>
		/// <returns></returns>
		public List<WorkflowComponent> GetAllComponents(bool recurseInSCCs = false) => Components.GetAllComponents(recurseInSCCs);

		public virtual Workflow Copy(string id, string name = null, string parentName = null)
		{
			return new Workflow(id, Description, ModelDataInputs.ToList(), ModelDataOutputs.ToList(),
				Components.ToList(), ScheduledComponents.ToList(), IsAuxiliary, ScheduleMode, parentName ?? parentName);
		}


		#region Additional functions
		/// <summary>
		/// Checks whether the subprocess contains a modified model or an SCC
		/// </summary>
		/// <returns></returns>
		public bool ContainsNumericallyTreatedComponents()
		{
			foreach (WorkflowComponent component in ScheduledComponents)
			{
				if (component is INumericallyTreatedWorkflow)
					return true;
			}
			return false;
		}

		public bool IsAcyclic
		{
			get
			{
				foreach (WorkflowComponent component in ScheduledComponents)
					if (component is WorkflowSCC)
						return false;
				return true;
			}
		}

		public bool ContainsComponent(WorkflowComponent component)
		{
			foreach (WorkflowComponent comp in Components)
			{
				if (comp == component)
					return true;
				else if (comp is Workflow workflow)
					return workflow.ContainsComponent(component);
			}
			return false;
		}

		public string GetSCCWorkflowName(IEnumerable<WorkflowComponent> componentsInScc) => GetSCCWorkflowName(Name, Components, componentsInScc);

		public string GetGlobalWorkflowName(IEnumerable<WorkflowComponent> componentsInGlobal) => GetGlobalWorkflowName(Name, Components, componentsInGlobal);

		public static string GetSCCWorkflowName(string name, IEnumerable<WorkflowComponent> allComponents, IEnumerable<WorkflowComponent> componentsInScc)
		{
			int code = GetCodeForSubsetOfModels(allComponents, componentsInScc);
			return $"{name}:SCC:{code}";
		}

		public static string GetGlobalWorkflowName(string name, IEnumerable<WorkflowComponent> allComponents, IEnumerable<WorkflowComponent> componentsInGlobal)
		{
			int code = GetCodeForSubsetOfModels(allComponents, componentsInGlobal);
			return $"{name}:Global:{code}";
		}

		protected static int GetCodeForSubsetOfModels(IEnumerable<WorkflowComponent> allComponents, IEnumerable<WorkflowComponent> componentsInGlobal)
		{
			var hash = new HashSet<string>(componentsInGlobal.GetNames().Select(n => n.Split(':').First()));
			int total = 0;
			int factor = 1;
			foreach (var component in allComponents)
			{
				total += hash.Contains(component.Name) ? factor : 0;
				factor *= 2;
			}

			return total;
		}
		#endregion
	}
}
