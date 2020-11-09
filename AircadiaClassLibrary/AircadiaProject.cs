using System;
using System.Collections.Generic;
using System.Linq;
using Aircadia.ObjectModel.DataObjects;
using Aircadia.ObjectModel.Models;
using Aircadia.ObjectModel.Studies;
using Aircadia.ObjectModel.Workflows;
using Aircadia.ObjectModel;
using Aircadia.Utilities;
using System.Text.RegularExpressions;
using static Aircadia.DuplicateMode;
using static Aircadia.ChangeType;
using Aircadia.Services.Compilers;

namespace Aircadia
{
	public class AircadiaProject
	{
		public static AircadiaProject Instance { get; protected set; }

		public string ProjectName { get; private set; }
		public string ProjectPath { get; private set; }

		public IEnumerable<Data> DataStore => dataStore.Values;
		public IEnumerable<Data> AuxiliaryDataStore => dataStoreAux.Values;
		public IEnumerable<Model> ModelStore => modelStore.Values;
		public IEnumerable<Model> AuxiliaryModelStore => modelStoreAux.Values;
		public IEnumerable<Workflow> WorkflowStore => workflowStore.Values;
		public IEnumerable<Workflow> AuxiliaryWorkflowStore => workflowStoreAux.Values;
		public IEnumerable<Study> StudyStore => studyStore.Values;



		private static Dictionary<string, Data> dataStore = new Dictionary<string, Data>();
		private static Dictionary<string, Data> dataStoreAux = new Dictionary<string, Data>();
		private static Dictionary<string, Model> modelStore = new Dictionary<string, Model>();
        private static Dictionary<string, Model> modelStoreAux = new Dictionary<string, Model>();
        private static Dictionary<string, Workflow> workflowStore = new Dictionary<string, Workflow>();
        private static Dictionary<string, Workflow> workflowStoreAux = new Dictionary<string, Workflow>();
		private static Dictionary<string, Study> studyStore = new Dictionary<string, Study>();

		private static Graph dependencies = new Graph();


		private readonly Func<AircadiaComponent, string, bool> AskUserOverwriteConfirmation;
		private readonly Func<AircadiaComponent, string, bool> AskUserConfirmImportantChanges;
		private readonly Func<AircadiaComponent, string, bool> AskUserDeleteDependencies;

		protected AircadiaProject(string projectName, string projectPath, Func<AircadiaComponent, string, bool> askUserOverwriteConfirmation, 
			Func<AircadiaComponent, string, bool> askUserConfirmImportantChanges, Func<AircadiaComponent, string, bool> askUserDeleteDependencies)
		{
			ProjectName = projectName;
			ProjectPath = projectPath;
			AskUserOverwriteConfirmation = askUserOverwriteConfirmation ?? DefaultConfirmation;
			AskUserConfirmImportantChanges = askUserConfirmImportantChanges ?? DefaultConfirmation;
			AskUserDeleteDependencies = askUserDeleteDependencies ?? DefaultConfirmation;
		}

		public static void Initialize(string name, string path, 
			Func<AircadiaComponent, string, bool> askUserOverwriteConfirmation = null,
			Func<AircadiaComponent, string, bool> askUserConfirmImportantChanges = null, 
			Func<AircadiaComponent, string, bool> askUserDeleteDependencies = null)
		{
			Instance = new AircadiaProject(name, path, askUserOverwriteConfirmation, askUserConfirmImportantChanges, askUserDeleteDependencies);

			Compiler.ProjectPath = path;
		}

		public void Terminate() => Instance = null;

		public void Rebase(string name, string path) => (ProjectName, ProjectPath) = (name, path);

		private bool DefaultConfirmation(AircadiaComponent component, string name) => true;

		#region Validation


		/// <summary>
		/// Check if a name is valid according to a set of illegal words and characters
		/// </summary>
		/// <param name="name">Name to validate</param>
		/// <returns>Returns true if name is valid</returns>
		public static bool ValidateName(string name)
		{
			if (String.IsNullOrWhiteSpace(name))
				return false;

			// Chek the nme is not a keyword
			if (KeyWords.Contains(name))
				return false;

			// Check it doesnt contain eny illegal character
			foreach (char c in name)
				if (KeySymbols.Contains(c))
					return false;

			// Check it doesnt contain start by number
			if (Regex.IsMatch(name, @"^\d"))
				return false;

			return true;
		}

		public static HashSet<string> KeyWords = new HashSet<string> { "abstract", "const", "extern", "internal", "new", "override", "private", "protected", "public", "readonly", "sealed", "static", "virtual", "volatile", "void", "as", "explicit", "implicit", "is", "operator", "sizeof", "typeof", "bool", "byte", "char", "class", "decimal", "double", "enum", "float", "int", "interface", "long", "object", "sbyte", "short", "string", "struct", "uint", "ulong", "ushort", "break", "case", "continue", "default", "do", "else", "for", "foreach", "in", "goto", "if", "return", "switch", "while", "catch", "checked", "finally", "throw", "try", "unchecked", "delegate", "event", "namespace", "new", "stackalloc", "out", "params", "ref", "null", "false", "true", "this", "value", "base", "void" };


		public static HashSet<char> KeySymbols = new HashSet<char> { '+', '-', '*', '/', '&', '#' };

		#endregion

		public IEnumerable<AircadiaComponent> DependenciesOf(AircadiaComponent component) => dependencies.BreadthFirstSearch(GraphString(component)).Select(c => GraphComponent(c));

		#region Get Methods

		public Data GetData(string name)
		{
			if (dataStore.TryGetValue(name, out Data data))
				return data;

			if (dataStoreAux.TryGetValue(name, out data))
				return data;

			return null;
		}

		public Model GetModel(string name)
		{
			if (modelStore.TryGetValue(name, out Model model))
				return model;

			if (modelStoreAux.TryGetValue(name, out model))
				return model;

			return null;
		}

		public Workflow GetWorkflow(string name)
		{
			if (workflowStore.TryGetValue(name, out Workflow workflow))
				return workflow;

			if (workflow == null)
				workflowStoreAux.TryGetValue(name, out workflow);

			return workflow;
		}

		public WorkflowComponent GetComponent(string name) => (GetModel(name) as WorkflowComponent) ?? GetWorkflow(name);

		public Study GetStudy(string name)
		{
			studyStore.TryGetValue(name, out Study study);
			return study;
		}

		#endregion


		#region Add Methods

		public void Add(AircadiaComponent component, DuplicateMode mode = UserDecides)
		{
			if (component is Data data)
			{
				if (data.IsAuxiliary)
					AddDataAux(data, mode);
				else
					AddData(data, mode);
			}
			else if (component is Model model)
			{
				if (model.IsAuxiliary)
					AddModelAux(model, mode);
				else
					AddModel(model, mode);
			}
			else if (component is Workflow workflow)
			{
				if (workflow.IsAuxiliary)
					AddWorkflowAux(workflow, mode);
				else
					AddWorkflow(workflow, mode);
			}
			else if (component is Study study)
			{
				AddStudy(study, mode);
			}
		}

		private void AddData(Data data, DuplicateMode mode)
		{

			if (!AircadiaProject.dataStore.ContainsKey(data.Id)) // Model not there
			{
				// Add dependencies
				AddDependencies(data);

				AircadiaProject.dataStore[data.Id] = data;
				OnHandler(DataChanged, data);
			}
			else if (mode == Replace || (mode == UserDecides && ShowConfirmationMessageBox(data)))
			{
				Data oldData = AircadiaProject.dataStore[data.Id];
				HashSet<string> affected = GetAffected(data);
				if (affected.Count > 0)
				{
					// If there are major changes confirmation is required
					if (ThereAreMajorChanges(oldData, data))
					{
						if (ShowConfirmationModMessageBox(data, affected))
						{
							// Remove dependencies
							Remove(oldData, false);
							// Add new component
							Add(data, Replace);

							return;
						}
						else
						{
							// Do not do anything
							return;
						}
					}
					else
					{
						// Relink component
						RelinkData(data, affected);
					}
				}

				AircadiaProject.dataStore[data.Id] = data;
				OnHandler(DataChanged, data, Updated);
			}

		}

		private void AddDataAux(Data data, DuplicateMode mode)
		{
			if (!dataStoreAux.ContainsKey(data.Id))
			{
				// Add dependencies
				AddDependencies(data);

				AircadiaProject.dataStoreAux[data.Id] = data;
				OnHandler(DataAuxChanged, data);
			}
			else if (mode == Replace || (mode == UserDecides && ShowConfirmationMessageBox(data))) // Model not there or user approves substitution
			{
				HashSet<string> affected = GetAffected(data);
				if (affected.Count > 0)
				{
					// If there are major changes confirmation is required
					if (ThereAreMajorChanges(dataStoreAux[data.Id], data))
					{
						if (ShowConfirmationModMessageBox(data, affected))
						{
							// Remove dependencies
							Remove(dataStoreAux[data.Id], false);
							// Add new component
							Add(data, Replace);

							return;
						}
						else
						{
							// Do not do anything
							return;
						}
					}
					else
					{
						// Relink component
						RelinkData(data, affected);
					}
				}

				dataStoreAux[data.Id] = data;
				OnHandler(DataAuxChanged, data, Updated);
			}
		}

		private void AddModel(Model model, DuplicateMode mode)
		{
			if (!modelStore.ContainsKey(model.Id))
			{
				// Add dependencies
				AddDependencies(model, model.GetAllData());

				AircadiaProject.modelStore[model.Id] = model;
				OnHandler(ModelChanged, model);
			}
			else if (mode == Replace || (mode == UserDecides && ShowConfirmationMessageBox(model)))
			{
				Model oldModel = AircadiaProject.modelStore[model.Id];
				HashSet<string> affected = GetAffected(model);
				if (affected.Count > 0)
				{
					// If there are major changes confirmation is required
					if (ThereAreMajorChanges(oldModel, model))
					{
						if (ShowConfirmationModMessageBox(model, affected))
						{
							// Remove dependencies
							Remove(oldModel, false);
							// Add new component
							Add(model, Replace);

							return;
						}
						else
						{
							// Do not do anything
							return;
						}
					}
					else
					{
						// Relink component
						RelinkComponent(model, affected);
					}
				}

				AircadiaProject.modelStore[model.Id] = model;
				OnHandler(ModelChanged, model, Updated);
			}
		}

		private void AddModelAux(Model model, DuplicateMode mode)
		{
			if (!modelStoreAux.ContainsKey(model.Id))
			{
				// Add dependencies
				AddDependencies(model, model.GetAllData());

				AircadiaProject.modelStoreAux[model.Id] = model;
				OnHandler(ModelAuxChanged, model);
			}
			else if (mode == Replace || (mode == UserDecides && ShowConfirmationMessageBox(model))) // Model not there or user approves substitution
			{
				HashSet<string> affected = GetAffected(model);
				if (affected.Count > 0)
				{
					// If there are major changes confirmation is required
					if (ThereAreMajorChanges(modelStoreAux[model.Id], model))
					{
						if (ShowConfirmationModMessageBox(model, affected))
						{
							// Remove dependencies
							Remove(modelStoreAux[model.Id], false);
							// Add new component
							Add(model, Replace);

							return;
						}
						else
						{
							// Do not do anything
							return;
						}
					}
					else
					{
						// Relink component
						RelinkComponent(model, affected);
					}
				}

				modelStoreAux[model.Id] = model;
				OnHandler(ModelAuxChanged, model, Updated);
			}
		}

		private void AddWorkflow(Workflow workflow, DuplicateMode mode)
		{
			if (!workflowStore.ContainsKey(workflow.Id)) // Model not there
			{
				// Add dependencies
				AddDependencies(workflow, workflow.ScheduledComponents);

				foreach (Workflow component in workflow.ScheduledComponents.Where(c => c is Workflow wf && wf.IsAuxiliary))
					AddWorkflowAux(component, Replace);

				AircadiaProject.workflowStore[workflow.Id] = workflow;
				OnHandler(WorkflowChanged, workflow);
			}
			else if (mode == Replace || (mode == UserDecides && ShowConfirmationMessageBox(workflow)))
			{
				Workflow oldWorkflow = AircadiaProject.workflowStore[workflow.Id];
				HashSet<string> affected = GetAffected(workflow);
				if (affected.Count > 0)
				{
					// If there are major changes confirmation is required
					if (ThereAreMajorChanges(oldWorkflow, workflow))
					{
						if (ShowConfirmationModMessageBox(workflow, affected))
						{
							// Remove dependencies
							Remove(oldWorkflow, false);
							// Add new component
							Add(workflow, Replace);

							return;
						}
						else
						{
							// Do not do anything
							return;
						}
					}
					else
					{
						// Relink component
						RelinkComponent(workflow, affected);
					}
				}

				foreach (Workflow component in oldWorkflow.ScheduledComponents.Where(c => c is Workflow wf && wf.IsAuxiliary))
					RemoveWorkflowAux(component);

				foreach (Workflow component in workflow.ScheduledComponents.Where(c => c is Workflow wf && wf.IsAuxiliary))
					AddWorkflowAux(component, Replace);

				AircadiaProject.workflowStore[workflow.Id] = workflow;
				OnHandler(WorkflowAuxChanged, workflow, Updated);
			}
		}

		private void AddWorkflowAux(Workflow workflow, DuplicateMode mode)
		{
			if (!workflowStoreAux.ContainsKey(workflow.Id))
			{
				// Add dependencies
				AddDependencies(workflow, workflow.ScheduledComponents);

				foreach (Workflow component in workflow.ScheduledComponents.Where(c => c is Workflow wf && wf.IsAuxiliary))
					AddWorkflowAux(component, Replace);

				AircadiaProject.workflowStoreAux[workflow.Id] = workflow;
				OnHandler(WorkflowAuxChanged, workflow);
			}
			else if (mode == Replace || (mode == UserDecides && ShowConfirmationMessageBox(workflow))) // Workflow not there or user approves substitution
			{
				HashSet<string> affected = GetAffected(workflow);
				if (affected.Count > 0)
				{
					// Relink component
					RelinkComponent(workflow, affected);
				}

				workflowStoreAux[workflow.Id] = workflow;
				OnHandler(WorkflowAuxChanged, workflow, Updated);
			}
		}

		private void AddStudy(Study study, DuplicateMode mode)
		{
			if (!studyStore.ContainsKey(study.Id))  // Model not there
			{
				// Add dependencies
				AddDependencies(study, study.StudiedComponent);

				AircadiaProject.studyStore[study.Id] = study;
				OnHandler(StudyChanged, study, Added);
			}
			else if (mode == Replace || (mode == UserDecides && ShowConfirmationMessageBox(study)))
			{
				Study oldStudy = AircadiaProject.studyStore[study.Id];
				HashSet<string> affected = GetAffected(study);
				if (affected.Count > 0)
				{
					// If there are major changes confirmation is required
					if (ThereAreMajorChanges(oldStudy, study))
					{
						if (ShowConfirmationModMessageBox(study, affected))
						{
							// Remove dependencies
							Remove(oldStudy, false);
							// Add new component
							Add(study, Replace);

							return;
						}
						else
						{
							// Do not do anything
							return;
						}
					}
					else
					{
						// Relink component
						RelinkStudy(study, affected);
					}
				}

				studyStore[study.Id] = study;
				OnHandler(StudyChanged, study, Updated);
			}
		}

		#endregion


		#region Dependencies Management

		private void AddDependencies(AircadiaComponent component, AircadiaComponent dependent) => AddDependencies(component, new AircadiaComponent[] { dependent });

		private void AddDependencies(AircadiaComponent component, IEnumerable<AircadiaComponent> dependents = null)
		{
			dependencies.AddVertex(GraphString(component));
			if (dependents != null)
			{
				foreach (AircadiaComponent dependent in dependents)
					dependencies.AddEdge(GraphString(dependent), GraphString(component));
			}
		}

		private void RemoveDependencies(AircadiaComponent component, AircadiaComponent dependent) => RemoveDependencies(component, new AircadiaComponent[] { dependent });

		private void RemoveDependencies(AircadiaComponent component, IEnumerable<AircadiaComponent> dependents = null)
		{
			if (dependents != null)
			{
				foreach (AircadiaComponent dependent in dependents)
					dependencies.RemoveEdge(GraphString(dependent), GraphString(component));
			}
			dependencies.RemoveVertex(GraphString(component));
		}

		private HashSet<string> GetAffected(AircadiaComponent component)
		{
			HashSet<string> affected = dependencies.BreadthFirstSearch(GraphString(component));
			affected.Remove(GraphString(component));
			return affected;
		}

		#endregion


		#region Relink Methods

		private void RelinkData(Data data, HashSet<string> affected)
		{
			foreach (string name in affected)
			{
				if (!(GraphComponent(name) is WorkflowComponent model))
					return;

				Relink(model.ModelDataInputs, data);
				Relink(model.ModelDataOutputs, data);

				if (model is WorkflowSCC wscc)
				{
					Relink(wscc.FeedbackVariables, data);
				}
				else if (model is IReversableWorkflow rwf)
				{
					Relink(rwf.ReversedInputs, data);
					Relink(rwf.ReversedOutputs, data);
				}
			}
		}

		private void RelinkComponent(WorkflowComponent component, HashSet<string> affected)
		{
			foreach (string name in affected.Where(v => v.EndsWith(" Workflow")).Select(v => v.Substring(0, v.Length - " Workflow".Length)))
				RelinkComponentToWorkflow(component, name);

			foreach (string name in affected.Where(v => v.EndsWith(" Study")).Select(v => v.Substring(0, v.Length - " Study".Length)))
				RelinkComponentToStudy(component, name);
		}

		private void RelinkStudy(Study study, HashSet<string> affected)
		{
			foreach (string modelName in affected.Where(v => v.EndsWith(" Study")).Select(v => v.Substring(0, v.Length - " Study".Length)))
				RelinkComponentToStudy(study, modelName);
		}

		private void RelinkComponentToWorkflow(WorkflowComponent component, string name)
		{
			Workflow workflow = GetWorkflow(name);
			if (workflow == null)
				return;

			Relink(workflow.Components, component);
			Relink(workflow.ScheduledComponents, component);
		}

		private void RelinkComponentToStudy(ExecutableComponent component, string name)
		{
			Study study = GetStudy(name);
			if (study == null)
				return;

			if (study.StudiedComponent?.Id == component.Id)
				study.StudiedComponent = component;
		}

		private void Relink<T>(List<T> list, T component) where T : AircadiaComponent
		{
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].Id == component.Id)
				{
					list[i] = component;
					return;
				}
			}
		} 

		#endregion


		#region Remove Methods

		public bool Remove(AircadiaComponent component, bool askUser = true)
		{
			if (component is Data data)
			{
				if (data.IsAuxiliary)
					return RemoveDataAux(data, askUser);
				else
					return RemoveData(data, askUser);
			}
			else if (component is Model model)
			{
				if (model.IsAuxiliary)
					return RemoveModelAux(model, askUser);
				else
					return RemoveModel(model, askUser);
			}
			else if (component is Workflow workflow)
			{
				if (workflow.IsAuxiliary)
					return RemoveWorkflowAux(workflow);
				else
					return RemoveWorkflow(workflow, askUser);
			}
			else if (component is Study study)
			{
				return RemoveStudy(study, askUser);
			}
			return false;
		}

		private bool RemoveData(Data data, bool askUser = true)
		{
			// Handle Dependencies
			HashSet<string> affected = GetAffected(data);

			if (affected.Count > 0 && askUser && !ShowConfirmationDelMessageBox(data, affected))
				return false;

			bool removed = AircadiaProject.dataStore.Remove(data.Id);
			if (removed)
			{
				RemoveDependencies(data);
				foreach (string dependency in affected)
					Remove(GraphComponent(dependency), false);
				OnHandler(DataChanged, data, ChangeType.Deleted);
			}
			return removed;
		}

		private bool RemoveDataAux(Data data, bool askUser = true)
		{
			// Handle Dependencies
			HashSet<string> affected = GetAffected(data);

			if (affected.Count > 0 && askUser && !ShowConfirmationDelMessageBox(data, affected))
				return false;

			bool removed = dataStoreAux.Remove(data.Id);
			if (removed)
			{
				RemoveDependencies(data);
				foreach (string dependency in affected)
					Remove(GraphComponent(dependency), false);
				OnHandler(DataAuxChanged, data, ChangeType.Deleted);
			}
			return removed;
		}

		private bool RemoveModel(Model model, bool askUser = true)
		{
			// Handle Dependencies
			HashSet<string> affected = GetAffected(model);

			if (affected.Count > 0 && askUser && !ShowConfirmationDelMessageBox(model, affected))
				return false;

			bool removed = AircadiaProject.modelStore.Remove(model.Id);
			if (removed)
			{
				RemoveDependencies(model, model.GetAllData());
				foreach(string dependency in affected)
						Remove(GraphComponent(dependency), false);
				OnHandler(ModelChanged, model, ChangeType.Deleted);
			}
			return removed;
		}

		private bool RemoveModelAux(Model model, bool askUser = true)
		{
			// Handle Dependencies
			HashSet<string> affected = GetAffected(model);

			if (affected.Count > 0 && askUser && !ShowConfirmationDelMessageBox(model, affected))
				return false;

			bool removed = modelStoreAux.Remove(model.Id);
			if (removed)
			{
				RemoveDependencies(model, model.GetAllData());
				foreach (string dependency in affected)
					Remove(GraphComponent(dependency), false);
				OnHandler(ModelAuxChanged, model, ChangeType.Deleted);
			}
			return removed;
		}

		private bool RemoveWorkflow(Workflow workflow, bool askUser = true)
		{
			// Handle Dependencies
			HashSet<string> affected = GetAffected(workflow);

			if (affected.Count > 0 && askUser && !ShowConfirmationDelMessageBox(workflow, affected))
				return false;

			bool removed = AircadiaProject.workflowStore.Remove(workflow.Id);
			if (removed)
			{
				RemoveDependencies(workflow, workflow.ScheduledComponents);
				foreach (Workflow component in workflow.ScheduledComponents.Where(c => c is Workflow wf && wf.IsAuxiliary))
					RemoveWorkflowAux(component);
				foreach (string dependency in affected)
					Remove(GraphComponent(dependency), false);
				OnHandler(WorkflowChanged, workflow, ChangeType.Deleted);
			}
			return removed;
		}

		private bool RemoveWorkflowAux(Workflow workflow)
		{
			bool removed = workflowStoreAux.Remove(workflow.Id);
			if (removed)
			{
				// Handle Dependencies
				RemoveDependencies(workflow, workflow.ScheduledComponents);
				foreach (Workflow component in workflow.ScheduledComponents.Where(c => c is Workflow wf && wf.IsAuxiliary))
					RemoveWorkflowAux(component);
				OnHandler(WorkflowChanged, workflow, ChangeType.Deleted);
			}
			return removed;
		}

		private bool RemoveStudy(Study study, bool askUser = true)
		{
			// Handle Dependencies
			HashSet<string> affected = GetAffected(study);

			if (affected.Count > 0 && askUser && !ShowConfirmationDelMessageBox(study, affected))
				return false;

			bool removed = studyStore.Remove(study.Id);
			if (removed)
			{
				// Handle dependencies
				RemoveDependencies(study, study.StudiedComponent);
				foreach (string dependency in affected)
					Remove(GraphComponent(dependency), false);
				OnHandler(StudyChanged, study, ChangeType.Deleted);
			}
			return removed;
		}

		#endregion


		#region Rename Methods
		public  bool Rename(AircadiaComponent component, string name)
		{
			if (!String.IsNullOrWhiteSpace(name))
			{
				if (ValidateName(name))
				{
					// Check if the name is already there
					if (component is Data data)
						return RenameData(data, name);
					else if (component is Model model)
						return RenameModel(model, name);
					else if (component is Workflow workflow)
						return RenameWorkflow(workflow, name);
					else if (component is Study study)
						return RenameStudy(study, name);

					return false;
				}
				else
				{
					throw new ArgumentException($"Invalid new name. The name {name} contains an illegal keyword or symbol");
				}
			}
			else
			{
				throw new ArgumentException($"Invalid new name. The name {name} cannot be blank");
			}
		}

		private bool RenameData(Data data, string name)
		{
			bool nameExists = false;

			if (data.IsAuxiliary)
				nameExists = AircadiaProject.dataStoreAux.ContainsKey(name);
			else
				nameExists = AircadiaProject.dataStore.ContainsKey(name);

			if (nameExists)
			{
				throw new ArgumentException($"Invalid new name. The name {name} already exists");
			}

			string oldGraphName = GraphString(data);
			string oldName = data.Id;
			if (data.IsAuxiliary)
				RenameInDictionary(AircadiaProject.dataStoreAux, data, name);
			else
				RenameInDictionary(AircadiaProject.dataStore, data, name);
			string newGraphName = GraphString(data);


			// Add dependencies from new node and remove dependencies from old node
			dependencies.AddVertex(newGraphName);
			var adjacents = dependencies.Vertices[oldGraphName].Adjacent.Select(a => a.Id).ToList();
			foreach (string adjacent in adjacents)
			{
				dependencies.AddEdge(newGraphName, adjacent);
				dependencies.RemoveEdge(oldGraphName, adjacent);

				if (adjacent.EndsWith("Model"))
					(GraphComponent(adjacent) as Model).RenameVariable(oldName, data.Id);
			}
			dependencies.RemoveVertex(oldGraphName);

			OnRenamedHandler(data, oldGraphName.Substring(0, oldGraphName.Length - " Data".Length));

			return true;
		}

		private bool RenameModel(Model model, string name)
		{
			bool nameExists = false;

			if (model.IsAuxiliary)
				nameExists = AircadiaProject.modelStoreAux.ContainsKey(name);
			else
				nameExists = AircadiaProject.modelStore.ContainsKey(name);

			if (nameExists)
			{
				throw new ArgumentException($"Invalid new name. The name {name} already exists");
			}

			string newName = $"{name} Model";
			string oldName = GraphString(model);
			if (model.IsAuxiliary)
				RenameInDictionary(AircadiaProject.modelStoreAux, model, name);
			else
				RenameInDictionary(AircadiaProject.modelStore, model, name);


			// Add dependencies from new node and remove dependencies from old node
			dependencies.AddVertex(newName);
			var adjacents = dependencies.Vertices[oldName].Adjacent.Select(a => a.Id).ToList();
			foreach (string adjacent in adjacents)
			{
				dependencies.AddEdge(newName, adjacent);
				dependencies.RemoveEdge(oldName, adjacent);
			}
			foreach (Data data in model.ModelDataInputs.Concat(model.ModelDataOutputs))
			{
				dependencies.AddEdge(GraphString(data), newName);
				dependencies.RemoveEdge(GraphString(data), oldName);
			}
			dependencies.RemoveVertex(oldName);


			OnRenamedHandler(model, oldName.Substring(0, oldName.Length - " Model".Length));

			return true;
		}

		private bool RenameWorkflow(Workflow workflow, string name)
		{
			bool nameExists = false;

			if (workflow.IsAuxiliary)
				nameExists = AircadiaProject.workflowStoreAux.ContainsKey(name);
			else
				nameExists = AircadiaProject.workflowStore.ContainsKey(name);

			if (nameExists)
			{
				throw new ArgumentException($"Invalid new name. The name {name} already exists");
			}

			string newName = $"{name} Workflow";
			string oldName = GraphString(workflow);
			if (workflow.IsAuxiliary)
				RenameInDictionary(AircadiaProject.workflowStoreAux, workflow, name);
			else
				RenameInDictionary(AircadiaProject.workflowStore, workflow, name);


			// Add dependencies from new node and remove dependencies from old node
			dependencies.AddVertex(newName);
			var adjacents = dependencies.Vertices[oldName].Adjacent.Select(a => a.Id).ToList();
			foreach (string adjacent in adjacents)
			{
				dependencies.AddEdge(newName, adjacent);
				dependencies.RemoveEdge(oldName, adjacent);
			}
			foreach (WorkflowComponent component in workflow.ScheduledComponents)
			{
				dependencies.AddEdge(GraphString(component), newName);
				dependencies.RemoveEdge(GraphString(component), oldName);
			}
			dependencies.RemoveVertex(oldName);

			OnRenamedHandler(workflow, oldName.Substring(0, oldName.Length - " Workflow".Length));

			return true;
		}

		private bool RenameStudy(Study study, string name)
		{
			if (studyStore.ContainsKey(name))
			{
				throw new ArgumentException($"Invalid new name. The name {name} already exists");
			}


			string newName = $"{name} Study";
			string oldName = GraphString(study);
			RenameInDictionary(AircadiaProject.studyStore, study, name);

			// Add dependencies from new node and remove dependencies from old node
			dependencies.AddVertex(newName);
			var adjacents = dependencies.Vertices[oldName].Adjacent.Select(a => a.Id).ToList();
			foreach (string adjacent in adjacents)
			{
				dependencies.AddEdge(newName, adjacent);
				dependencies.RemoveEdge(oldName, adjacent);
			}
			dependencies.AddEdge(GraphString(study.StudiedComponent), newName);
			dependencies.RemoveEdge(GraphString(study.StudiedComponent), oldName);
			dependencies.RemoveVertex(oldName);

			OnRenamedHandler(study, oldName.Substring(0, oldName.Length - " Study".Length));

			return true;
		} 

		private void RenameInDictionary<T>(Dictionary<string, T> dictionary, T component, string newName) where T : AircadiaComponent
		{
			string oldName = component.Id;
			component.Rename(newName);
			dictionary[newName] = component;
			dictionary.Remove(oldName);
		}
		#endregion


		#region Miscellaneous

		private bool ShowConfirmationMessageBox(AircadiaComponent component)
		{
			string type = String.Empty;
			switch (component)
			{
				case Data d:
					type = "Data";
					break;
				case Model m:
					type = "Model";
					break;
				case Workflow w:
					type = "Workflow";
					break;
				case Study s:
					type = "Study";
					break;
				default:
					break;
			}
			return AskUserOverwriteConfirmation(component, type);
		}

		private bool ShowConfirmationModMessageBox(AircadiaComponent component, IEnumerable<string> dependencies)
		{
			string depString = dependencies.Aggregate("", (total, last) => total += last + "\r\n");
			return AskUserConfirmImportantChanges(component, depString);
		}

		private bool ShowConfirmationDelMessageBox(AircadiaComponent component, IEnumerable<string> dependencies)
		{
			string depString = dependencies.Aggregate("", (total, last) => total += last + "\r\n");
			return AskUserDeleteDependencies(component, depString);
		}

		private static bool ThereAreMajorChanges(Data oldData, Data newData)
		{
			if (oldData.IsAuxiliary != newData.IsAuxiliary)
				return true;
			if (oldData.GetDataType() != newData.GetDataType())
				return true;

			return false;
		}

		private static bool ThereAreMajorChanges(WorkflowComponent oldComponent, WorkflowComponent newComponent)
		{
			if (oldComponent.IsAuxiliary != newComponent.IsAuxiliary)
				return true;

			return ThereAreMajorChanges(oldComponent as ExecutableComponent, newComponent);
		}

		private static bool ThereAreMajorChanges(ExecutableComponent oldComponent, ExecutableComponent newComponent)
		{
			if (oldComponent.ModelDataInputs.Count != newComponent.ModelDataInputs.Count)
				return true;

			if (oldComponent.ModelDataOutputs.Count != newComponent.ModelDataOutputs.Count)
				return true;

			var hash = new HashSet<string>(oldComponent.ModelDataInputs.Select(d => d.Id));
			foreach (Data data in newComponent.ModelDataInputs)
				if (!hash.Contains(data.Id))
					return true;

			hash = new HashSet<string>(oldComponent.ModelDataOutputs.Select(d => d.Id));
			foreach (Data data in newComponent.ModelDataOutputs)
				if (!hash.Contains(data.Id))
					return true;

			return false;
		} 
		#endregion


		public void Clear()
		{
			dataStore.Clear();
			dataStoreAux.Clear();
			modelStore.Clear();
			modelStoreAux.Clear();
			workflowStore.Clear();
			workflowStoreAux.Clear();
			studyStore.Clear();
			dependencies = new Graph();
			ProjectName = String.Empty;
			ProjectPath = String.Empty;
		}

		private string GraphString(AircadiaComponent component)
		{
			string type = String.Empty;
			if (component is Data)
				type = "Data";
			else if (component is Model)
				type = "Model";
			else if (component is Workflow)
				type = "Workflow";
			else if (component is Study)
				type = "Study";

			return $"{component?.Id ?? String.Empty} {type}";
		}

		private AircadiaComponent GraphComponent(string graphName)
		{
			int count = graphName.Length;
			if (graphName.Contains(" Data"))
			{
				count -= " Data".Length;
				graphName = graphName.Substring(0, count);
				return GetData(graphName);
			}
			else if (graphName.Contains(" Model"))
			{
				count -= " Model".Length;
				graphName = graphName.Substring(0, count);
				return GetModel(graphName);
			}
			else if (graphName.Contains(" Workflow"))
			{
				count -= " Workflow".Length;
				graphName = graphName.Substring(0, count);
				return GetWorkflow(graphName);
			}
			else if (graphName.Contains(" Study"))
			{
				count -= " Study".Length;
				graphName = graphName.Substring(0, count);
				return GetStudy(graphName);
			}

			return null;
		}


		public event ComponentChangedEventHandler<Data> DataChanged;

		public event ComponentChangedEventHandler<Data> DataAuxChanged;

		public event ComponentChangedEventHandler<Model> ModelChanged;

		public event ComponentChangedEventHandler<Model> ModelAuxChanged;

		public event ComponentChangedEventHandler<Workflow> WorkflowChanged;

		public event ComponentChangedEventHandler<Workflow> WorkflowAuxChanged;

		public event ComponentChangedEventHandler<Study> StudyChanged;

		private void OnHandler<T>(ComponentChangedEventHandler<T> handler, T component, ChangeType type = ChangeType.Added) where T : AircadiaComponent => handler?.Invoke(null, new ComponentChangedEventArgs<T>(component, type));

		public event ComponentRenamedEventHandler<AircadiaComponent> ComponentRenamed;

		private void OnRenamedHandler(AircadiaComponent component, string oldName) => ComponentRenamed?.Invoke(null, new ComponentRenamedEventArgs<AircadiaComponent>(component, oldName));
	}

	public enum DuplicateMode { UserDecides, Replace, Ignore }

	public delegate void ComponentChangedEventHandler<T>(object sender, ComponentChangedEventArgs<T> e) where T : AircadiaComponent;

	public class ComponentChangedEventArgs<T> : EventArgs where T : AircadiaComponent
	{
		public T Component { get;  }
		public ChangeType ChangeType { get;  }

		public ComponentChangedEventArgs(T component, ChangeType changeType)
		{
			Component = component;
			ChangeType = changeType;
		}
	}

	public enum ChangeType { Added, Updated, Deleted}

	public delegate void ComponentRenamedEventHandler<T>(object sender, ComponentRenamedEventArgs<T> e) where T : AircadiaComponent;

	public class ComponentRenamedEventArgs<T> where T : AircadiaComponent
	{
		public T Component { get; }
		public string OldName { get; }

		public ComponentRenamedEventArgs(T component, string oldName)
		{
			Component = component;
			OldName = oldName;
		}
	}
}
