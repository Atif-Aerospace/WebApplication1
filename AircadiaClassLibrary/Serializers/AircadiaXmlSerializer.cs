using Aircadia.ObjectModel.DataObjects;
using Aircadia.ObjectModel.Models;
using Aircadia.ObjectModel.Studies;
using Aircadia.ObjectModel.Workflows;
using ExeModelTextFileManager.DataLocations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Aircadia.Numerics;
using Aircadia.Numerics.Solvers;
using Aircadia.ObjectModel.Treatments.Optimisers.Formulation;
using Aircadia.ObjectModel.Treatments.Optimisers.GA;
using Aircadia.ObjectModel.Treatments.Optimisers.GA.NSGAII;

namespace Aircadia.Services.Serializers
{
	public static class AircadiaXmlSerializer
	{
		static AircadiaProject Project => AircadiaProject.Instance;

		public static void SaveProjectXML(string fileName)
		{
			string p = Path.Combine(Project.ProjectPath, fileName + ".explorer");

			var xmlDocument = new XDocument();

			var projectElement = new XElement("Project");

			projectElement.Add(new XAttribute("Version", 1.0));

			#region Data
			XElement dataRepositoryElement = GetDataElement("DataRepository", Project.DataStore);
			projectElement.Add(dataRepositoryElement);

			dataRepositoryElement = GetDataElement("DataRepositoryAux", Project.AuxiliaryDataStore);
			projectElement.Add(dataRepositoryElement);
			#endregion Data

			#region Models
			XElement modelRepositoryElement = GetModelsElement("ModelRepository", Project.ModelStore);
			projectElement.Add(modelRepositoryElement);

			modelRepositoryElement = GetModelsElement("ModelRepositoryAux", Project.AuxiliaryModelStore);
			projectElement.Add(modelRepositoryElement);
			#endregion Models

			#region Workflows
			XElement workflowRepositoryElement = GetWorkflowsElement("WorkflowRepository", Project.WorkflowStore);
			projectElement.Add(workflowRepositoryElement);

			workflowRepositoryElement = GetWorkflowsElement("WorkflowRepositoryAux", Project.AuxiliaryWorkflowStore);
			projectElement.Add(workflowRepositoryElement);
			#endregion Workflows

			#region Studies
			XElement studyRepositoryElement = GetStudiesElement("StudyRepository");
			projectElement.Add(studyRepositoryElement);
			#endregion Studies

			xmlDocument.Add(projectElement);

			xmlDocument.Save(p);
		}

		private static XElement GetStudiesElement(string Name)
		{
			var studyRepositoryElement = new XElement(Name);
			foreach (Study study in Project.StudyStore)
			{
				var studyElement = new XElement("Study");
				// Name
				studyElement.Add(new XAttribute("Name", study.Name));
				// Type
				studyElement.Add(new XAttribute("Type", study.StudyType));
				// Description
				studyElement.Add(new XAttribute("Description", study.Description));

				studyElement.Add(new XAttribute("Component", study.StudiedComponent.Name));

				//if (study is FullFactorialDOEStudy ffs)
				//{
				//	var element = new XElement("Factors");
				//	foreach (Data factor in (ffs.Treatment as FullFactorial).factors)
				//		element.Add(new XElement("Factor", factor.Name));
				//	studyElement.Add(element);

				//	element = new XElement("StartingValues");
				//	foreach (decimal value in ffs.startingValues)
				//		element.Add(new XElement("Value", value));
				//	studyElement.Add(element);

				//	element = new XElement("StepSizes");
				//	foreach (decimal value in ffs.stepSizes)
				//		element.Add(new XElement("Size", value));
				//	studyElement.Add(element);

				//	element = new XElement("NumberOfLevels");
				//	foreach (int value in ffs.noOfLevels)
				//		element.Add(new XElement("Number", value));
				//	studyElement.Add(element);

				//	element = new XElement("Responses");
				//	foreach (Data response in (ffs.Treatment as FullFactorial).responses)
				//		element.Add(new XElement("Response", response.Name));
				//	studyElement.Add(element);
				//}
				/*else*/ if (study is FullFactorialDOEStudySergio ffss)
				{
					var element = new XElement("Factors");
					foreach (BoundedDesignVariableNoInitial factor in ffss.Factors)
					{
						var factorElement = new XElement("Factor");
						factorElement.Add(new XAttribute("Name", factor.Name));
						factorElement.Add(new XAttribute("LowerBound", factor.LowerBound));
						factorElement.Add(new XAttribute("Step", factor.Step));
						factorElement.Add(new XAttribute("UpperBound", factor.UpperBound));
						factorElement.Add(new XAttribute("Levels", factor.Levels));
						element.Add(factorElement);
					}
					studyElement.Add(element);

					element = new XElement("Responses");
					foreach (Data response in ffss.Responses)
					{
						var responseElement = new XElement("Response");
						responseElement.Add(new XAttribute("Name", response.Name));
						element.Add(responseElement);
					}
					studyElement.Add(element);
				}
				else if (study is RandomDOEStudy rds)
				{
					studyElement.Add(new XAttribute("Strategy", rds.Strategy));

					studyElement.Add(new XAttribute("Samples", rds.Samples));

					var element = new XElement("Factors");
					foreach (Factor factor in rds.Factors)
					{
						var factorElement = new XElement("Factor");
						factorElement.Add(new XAttribute("Name", factor.Name));
						factorElement.Add(new XAttribute("LowerBound", factor.LowerBound));
						factorElement.Add(new XAttribute("UpperBound", factor.UpperBound));
						element.Add(factorElement);
					}
					studyElement.Add(element);

					element = new XElement("Responses");
					foreach (Data response in rds.Responses)
					{
						var responseElement = new XElement("Response");
						responseElement.Add(new XAttribute("Name", response.Name));
						element.Add(responseElement);
					}
					studyElement.Add(element);
				}
                else if (study is DetOptStudy detOptStd)
                {
                    var formulation = new XElement("Formulation");
                    foreach (BoundedDesignVariableNoInital designVariable in detOptStd.OptimisationTemplate.DesignVariables)
                    {
                        var factorElement = new XElement("DesignVariable");
                        factorElement.Add(new XAttribute("Name", designVariable.Name));
                        factorElement.Add(new XAttribute("LowerBound", designVariable.LowerBound));
                        factorElement.Add(new XAttribute("UpperBound", designVariable.UpperBound));
                        formulation.Add(factorElement);
                    }

                    foreach (Objective objective in detOptStd.OptimisationTemplate.Objectives)
                    {
                        var responseElement = new XElement("Objective");
                        responseElement.Add(new XAttribute("Name", objective.Name));
                        responseElement.Add(new XAttribute("Type", objective.Type));
                        responseElement.Add(new XAttribute("Value", objective.Value));
                        formulation.Add(responseElement);
                    }

                    foreach (Constraint constraint in detOptStd.OptimisationTemplate.Constraints)
                    {
                        var responseElement = new XElement("Constraint");
                        responseElement.Add(new XAttribute("Name", constraint.Name));
                        responseElement.Add(new XAttribute("Type", constraint.Type));
                        responseElement.Add(new XAttribute("Value", constraint.Value));
                        formulation.Add(responseElement);
                    }

                    studyElement.Add(formulation);
                }

                studyRepositoryElement.Add(studyElement);
			}

			return studyRepositoryElement;
		}

		private static XElement GetWorkflowsElement(string name, IEnumerable<Workflow> workflowStore)
		{
			var workflowRepositoryElement = new XElement(name);
			foreach (Workflow workflow in workflowStore)
			{
				var workflowElement = new XElement("Workflow");
				// Name
				workflowElement.Add(new XAttribute("Name", workflow.Name));
				// Type
				workflowElement.Add(new XAttribute("Type", workflow.WorkflowType));
				// Description
				workflowElement.Add(new XAttribute("Description", SerializeLines(workflow.Description)));
				// IsAuxiliary
				workflowElement.Add(new XAttribute("IsAuxiliary", workflow.IsAuxiliary));

				#region Inputs
				var inputsElement = new XElement("Inputs");
				foreach (Data inputData in workflow.ModelDataInputs)
					inputsElement.Add(GetNamedElement("Input", inputData.Name));
				workflowElement.Add(inputsElement);
				#endregion Inputs

				#region Outputs
				var outputsElement = new XElement("Outputs");
				foreach (Data outputData in workflow.ModelDataOutputs)
					outputsElement.Add(GetNamedElement("Output", outputData.Name));
				workflowElement.Add(outputsElement);
				#endregion Outputs

				#region Models Sequence
				var modelsElement = new XElement("Components");
				foreach (WorkflowComponent component in workflow.Components)
				{
					var modelElement = new XElement("Model");
					modelElement.Add(new XAttribute("Name", component.Name));
					if (component is Model) // Computational model
					{
						modelElement.Add(new XAttribute("Type", "ComputationalModel"));
					}
					else if (component is WorkflowGlobal)
					{
						modelElement.Add(new XAttribute("Type", "GloballySolvedWorkflow"));
					}
					else if (component is WorkflowReversedModel)
					{
						modelElement.Add(new XAttribute("Type", "ReversedModel"));
					}
					//else if (component is WorkflowSCCSergioOld)
					//{
					//	modelElement.Add(new XAttribute("Type", "StronglyConnectedComponent"));
					//}
					else if (component is Workflow)
					{
						modelElement.Add(new XAttribute("Type", "Workflow"));
					}
					modelsElement.Add(modelElement);
				}
				workflowElement.Add(modelsElement);

				var scheduledComponentsElement = new XElement("ScheduledComponents");
				foreach (WorkflowComponent component in workflow.ScheduledComponents)
				{
					var modelElement = new XElement("Model");
					modelElement.Add(new XAttribute("Name", component.Name));
					if (component is Model) // Computational model
					{
						modelElement.Add(new XAttribute("Type", "ComputationalModel"));
					}
					else if (component is WorkflowGlobal)
					{
						modelElement.Add(new XAttribute("Type", "GloballySolvedWorkflow"));
					}
					else if (component is WorkflowReversedModel)
					{
						modelElement.Add(new XAttribute("Type", "ReversedModel"));
					}
					//else if (component is WorkflowSCCSergioOld)
					//{
					//	modelElement.Add(new XAttribute("Type", "StronglyConnectedComponent"));
					//}
					else if (component is Workflow)
					{
						modelElement.Add(new XAttribute("Type", "Workflow"));
					}
					scheduledComponentsElement.Add(modelElement);
				}
				workflowElement.Add(scheduledComponentsElement);
				#endregion Models Sequence

				// Mode
				if (!String.IsNullOrWhiteSpace(workflow.ScheduleMode))
					workflowElement.Add(new XAttribute("ScheduleMode", workflow.ScheduleMode));

				#region Solvers
				if (workflow is INumericallyTreatedWorkflow ntw)
				{
					var solversElement = new XElement("Solvers");
					foreach (ISolver solver in ntw.Solvers)
						solversElement.Add(GetSolverElement(solver));
					workflowElement.Add(solversElement);
				}
				#endregion Solvers


				workflowRepositoryElement.Add(workflowElement);
			}
			return workflowRepositoryElement;
		}

		private static XElement GetModelsElement(string name, IEnumerable<Model> modelStore)
		{
			var modelRepositoryElement = new XElement(name);
			foreach (Model model in modelStore)
			{
				var modelElement = new XElement("Model");
				// Name
				modelElement.Add(new XAttribute("Name", model.Name));
				// Type
				modelElement.Add(new XAttribute("Type", model.ModelType));
				// Description
				modelElement.Add(new XAttribute("Description", SerializeLines(model.Description)));
				// Inputs
				var inputsElement = new XElement("Inputs");
				foreach (Data inputData in model.ModelDataInputs)
					inputsElement.Add(GetNamedElement("Input", inputData.Name));
				modelElement.Add(inputsElement);
				// Outputs
				var outputsElement = new XElement("Outputs");
				foreach (Data outputData in model.ModelDataOutputs)
					outputsElement.Add(GetNamedElement("Output", outputData.Name));
				modelElement.Add(outputsElement);
				// IsAuxiliary
				modelElement.Add(new XAttribute("IsAuxiliary", model.IsAuxiliary));

				switch (model)
				{
					//case ModelCDll mod:
					//	// Dll Path
					//	modelElement.Add(new XAttribute("CDllPath", SerializePath(mod.DllPath)));
					//	// Method Name
					//	modelElement.Add(new XAttribute("MethodName", mod.MethodName));
					//	break;
					//case ModelCppDll mod:
					//	// Dll Path
					//	modelElement.Add(new XAttribute("CppDllPath", SerializePath(mod.DllPath)));
					//	// Method Name
					//	modelElement.Add(new XAttribute("MethodName", mod.MethodName));
					//	break;
					//case ModelCSharpLegacy mod:
					//	// Code
					//	modelElement.Add(new XAttribute("CSharpCode", SerializeLines(mod.CSharpCode)));
					//	break;
					//case ModelCSharpDllLegacy mod:
					//	// Dll Path
					//	modelElement.Add(new XAttribute("Code", SerializePath(mod.DllPath)));
					//	// Method Name
					//	modelElement.Add(new XAttribute("Signature", mod.DllEntry));
					//	break;
					//case ModelFortranDll mod:
					//	// Dll Path
					//	modelElement.Add(new XAttribute("FortranDllPath", SerializePath(mod.DllPath)));
					//	// Method Name
					//	modelElement.Add(new XAttribute("MethodName", mod.MethodName));
					//	break;
					//case IModelMatlabLegacy mod:
					//	// Dll Path
					//	modelElement.Add(new XAttribute("MatlabDllPath", SerializePath(mod.DllPath)));
					//	// Method Name
					//	modelElement.Add(new XAttribute("MethodName", mod.MethodName));
					//	break;
					case ModelCSharp mod:
						// Function Body
						modelElement.Add(new XAttribute("Body", SerializeLines(mod.FunctionBody)));
                        modelElement.Add(new XElement("Body", mod.FunctionBody));
                        break;
					case ModelDotNetDll mod:
						// Assembly Name
						modelElement.Add(new XAttribute("AssemblyName", SerializePath(mod.AssemblyName)));
						// Type Name
						modelElement.Add(new XAttribute("TypeName", mod.TypeName));
						// Method Body
						modelElement.Add(new XAttribute("Method", mod.Method));
						break;
					case ModelExe mod:
						// Working Directory
						modelElement.Add(new XAttribute("WorkingDirectory", SerializePath(mod.WorkingDirectory)));
						// Path to the Exe File
						modelElement.Add(new XAttribute("ExecutableFileName", SerializePath(mod.ExecutableFilePath)));
						// Working Directory
						modelElement.Add(new XAttribute("ExecutableFileArguments", SerializePath(mod.ExecutableFileArguments)));

						// Input File Instructions
						var inputFileInstructionsElement = new XElement("InputFilesInstructionsSets");
						foreach (InstructionsSet inputFileInstructionSet in mod.InputInstructions)
						{
							inputFileInstructionsElement.Add(GetLocationElement(inputFileInstructionSet));
						}
						modelElement.Add(inputFileInstructionsElement);

						// Output files Instructions
						var outputFileInstructionsElement = new XElement("OutputFilesInstructionsSets");
						foreach (InstructionsSet outputFileInstructionSet in mod.OutputInstructions)
						{
							outputFileInstructionsElement.Add(GetLocationElement(outputFileInstructionSet));
						}
						modelElement.Add(outputFileInstructionsElement);
						break;
					case ModelMatlab mod:
						// File Path
						modelElement.Add(new XAttribute("FilePath", SerializePath(mod.FilePath)));
						// Function Name
						modelElement.Add(new XAttribute("FunctionName", mod.FunctionName));
						break;
                    case ModelWebService mod:
                        // End Point
                        modelElement.Add(new XAttribute("EndPoint", SerializePath(mod.EndPoint)));
                        break;
                    default:
						break;
				}

				modelRepositoryElement.Add(modelElement);
			}

			return modelRepositoryElement;
		}

		private static XElement GetNamedElement(string type, string name)
		{
			var element = new XElement(type);
			element.Add(new XAttribute("Name", name));
			return element;
		}

		private static XElement GetDataElement(string name, IEnumerable<Data> dataStore)
		{
			var dataRepositoryElement = new XElement(name);
			foreach (Data data in dataStore)
			{
				var dataElement = new XElement("Data");
				// Name
				dataElement.Add(new XAttribute("Name", data.Name));
				// Type
				dataElement.Add(new XAttribute("Type", data.GetDataType()));
				// Description
				dataElement.Add(new XAttribute("Description", SerializeLines(data.Description)));
                // Parent Name
                dataElement.Add(new XAttribute("ParentName", data.ParentName));
                // Min
                dataElement.Add(new XAttribute("Min", (data.Min == Double.NegativeInfinity) ? "-Infinity" : data.Min.ToString()));
				// Value
				dataElement.Add(new XAttribute("Value", data.ValueAsString));
				// Max
				dataElement.Add(new XAttribute("Max", (data.Max == Double.PositiveInfinity) ? "Infinity" : data.Max.ToString()));
				if (data.GetDataType() == "Double")
				{
					var doubleData = data as DoubleData;
					dataElement.Add(new XAttribute("DecimalPlaces", doubleData.DecimalPlaces));
					dataElement.Add(new XAttribute("Unit", doubleData.Unit));
				}
				else if (data.GetDataType() == "DoubleVector")
				{
					var doubleVectorData = data as DoubleVectorData;
					dataElement.Add(new XAttribute("DecimalPlaces", doubleVectorData.DecimalPlaces));
					dataElement.Add(new XAttribute("Unit", doubleVectorData.Unit));
				}
				else if (data.GetDataType() == "DoubleMatrix")
				{
					var doubleMatrixData = data as DoubleMatrixData;
					dataElement.Add(new XAttribute("DecimalPlaces", doubleMatrixData.DecimalPlaces));
					dataElement.Add(new XAttribute("Unit", doubleMatrixData.Unit));
				}
				if (data.GetDataType() == "Integer")
				{
					var integerData = data as IntegerData;
					dataElement.Add(new XAttribute("Unit", integerData.Unit));
				}
				else if (data.GetDataType() == "IntegerVector")
				{
					var integerVectorData = data as IntegerVectorData;
					dataElement.Add(new XAttribute("Unit", integerVectorData.Unit));
				}
				dataRepositoryElement.Add(dataElement);
			}

			return dataRepositoryElement;
		}


		public static void OpenProjectXML(string path)
		{
			if (!File.Exists(path))
			{
				throw new FileNotFoundException("File \"" + path + "\" does not exists");
			}

			//initialiseProject(Path.GetFileNameWithoutExtension(path), Path.GetDirectoryName(path));

			XElement projectElement = XDocument.Load(path).Element("Project");
			if (projectElement == null)
			{
				throw new ArgumentException("The .xml \"Project\" element is null. Please make sure the .xml file contains an Aircadia project definition");
			}

			string version = projectElement.Attribute("Version")?.Value;


			#region Data Repository
			IEnumerable<XElement> dataelements = projectElement.Element("DataRepositoryAux")?.Descendants("Data") ?? new List<XElement>();
			foreach (XElement dataElement in dataelements)
				GetData(dataElement, true);

			dataelements = projectElement.Element("DataRepository")?.Descendants("Data") ?? new List<XElement>();
			foreach (XElement dataElement in dataelements)
				GetData(dataElement);
			#endregion Data Repository

			#region Model Repository
			IEnumerable<XElement> modelelements = projectElement.Element("ModelRepositoryAux")?.Descendants("Model") ?? new List<XElement>();
			foreach (XElement modelElement in modelelements)
				GetModel(modelElement, true);

			modelelements = projectElement.Element("ModelRepository")?.Descendants("Model") ?? new List<XElement>();
			foreach (XElement modelElement in modelelements)
				GetModel(modelElement);
			#endregion Model Repository

			#region Worflow Repository
			List<XElement> workflowElements = projectElement.Element("WorkflowRepositoryAux")?.Descendants("Workflow").ToList() ?? new List<XElement>();
			workflowElements.AddRange(projectElement.Element("WorkflowRepository")?.Descendants("Workflow") ?? new List<XElement>());

			var workflowDict = workflowElements.ToDictionary(w => GetValue(w, "Name"));

			foreach (XElement workflowElement in workflowElements)
				GetWorkflow(workflowElement, workflowDict);

			#endregion Workflow Repository

			// Clean-up
			//var hash = new HashSet<WorkflowComponent>(Project.WorkflowStoreAux.Values);
			//foreach (var component in Project.WorkflowStore)
			//{
			//	foreach (var subcomp in component.ScheduledComponents)
			//	{
			//		if (hash.Contains(subcomp))
			//			hash.Remove(subcomp);
			//	}
			//}
			//var old = new List<Workflow>(Project.WorkflowStoreAux.Values);
			//Project.WorkflowStoreAux.Clear();
			//foreach (var item in old)
			//{
			//	if (!hash.Contains(item))
			//		Project.WorkflowStoreAux.Add(item.Name, item);
			//}

			#region Study Repository
			IEnumerable<XElement> studyElements = projectElement.Element("StudyRepository")?.Descendants("Study") ?? new List<XElement>();
			foreach (XElement studyElement in studyElements)
				GetStudy(studyElement);

			#endregion
		}

		private static void GetStudy(XElement studyElement)
		{
			Study study = null;

			string name = GetValue(studyElement, "Name");
			string studyType = GetValue(studyElement, "Type");
			string description = GetValue(studyElement, "Description");

			WorkflowComponent workflow = Project.GetComponent(GetValue(studyElement, "Component"));
			if (workflow == null)
				workflow = Project.GetComponent(GetValue(studyElement, "Workflow"));

			if (studyType == "FullFactorialDOEStudy")
			{
				var factorObjs = new List<BoundedDesignVariableNoInitial>();
				try
				{
					// Factors
					var factors = new List<Data>();
					IEnumerable<XElement> factorElements = studyElement.Element("Factors").Descendants("Factor");
					foreach (XElement factorElement in factorElements)
						factors.Add(Project.GetData(factorElement.Value));

					// StartingValues
					var startingValues = new List<decimal>();
					IEnumerable<XElement> startingValueElements = studyElement.Element("LowerBound").Descendants("Value");
					foreach (XElement startingValueElement in startingValueElements)
						startingValues.Add(Convert.ToDecimal(startingValueElement.Value));

					// StepSizes
					var stepSizes = new List<decimal>();
					IEnumerable<XElement> stepSizeElements = studyElement.Element("StepSizes").Descendants("Size");
					foreach (XElement stepSizeElement in stepSizeElements)
						stepSizes.Add(Convert.ToDecimal(stepSizeElement.Value));

					// NumberOfLevels
					var noOfLevels = new List<int>();
					IEnumerable<XElement> noOfLevelElements = studyElement.Element("NumberOfLevels").Descendants("Number");
					foreach (XElement noOfLevelElement in noOfLevelElements)
						noOfLevels.Add(Convert.ToInt32(noOfLevelElement.Value));

					for (int i = 0; i < factors.Count; i++)
					{
						factorObjs.Add(new BoundedDesignVariableNoInitial(factors[i], startingValues[i], noOfLevels[i], stepSizes[i]));
					}
				}
				catch (Exception)
				{
					// Factors
					IEnumerable<XElement> factorElements = studyElement.Element("Factors").Descendants("Factor");
					foreach (XElement factorElement in factorElements)
					{
						Data factorData = Project.GetData(GetValue(factorElement, "Name"));
						decimal lower = Convert.ToDecimal(GetValue(factorElement, "LowerBound"));
						int levels = Convert.ToInt32(GetValue(factorElement, "Levels"));
						decimal upper = Convert.ToDecimal(GetValue(factorElement, "UpperBound"));
						factorObjs.Add(new BoundedDesignVariableNoInitial(factorData, lower, levels, upper));
					}
				}

				// Responses
				var responses = new List<Data>();
				IEnumerable<XElement> responseElements = studyElement.Element("Responses").Descendants("Response");
				foreach (XElement responseElement in responseElements)
					responses.Add(Project.GetData(GetValueOrInner(responseElement, "Name")));

				//study = new FullFactorialDOEStudy(name, description, workflow, factors, startingValues, stepSizes, noOfLevels, responses);
				if (workflow != null)
					study = new FullFactorialDOEStudySergio(name, description, workflow, factorObjs, responses, false);
			}
			else if (studyType == "RandomDOEStudy")
			{
				// Factors
				var factors = new List<Factor>();
				IEnumerable<XElement> factorElements = studyElement.Element("Factors").Descendants("Factor");
				foreach (XElement factorElement in factorElements)
				{
					Data factorData = Project.GetData(GetValue(factorElement, "Name"));
					decimal lower = Convert.ToDecimal(GetValue(factorElement, "LowerBound"));
					decimal upper = Convert.ToDecimal(GetValue(factorElement, "UpperBound"));
					factors.Add(new Factor(factorData, lower, upper));
				}

				// Responses
				var responses = new List<Data>();
				IEnumerable<XElement> responseElements = studyElement.Element("Responses").Descendants("Response");
				foreach (XElement responseElement in responseElements)
					responses.Add(Project.GetData(GetValueOrInner(responseElement, "Name")));

				// Samples
				long samples = Convert.ToInt64(GetValue(studyElement, "Samples"));

				//Strategy
				var strategy = (SamplingStrategy)Enum.Parse(typeof(SamplingStrategy), GetValue(studyElement, "Strategy"));

				if (workflow != null)
					study = new RandomDOEStudy(name, description, workflow, factors, responses, samples, strategy, false);
			}
            else if (studyType == "DeterministicOptimisation")
            {
                // Design Variables
                List<BoundedDesignVariableNoInital> designVariables = new List<BoundedDesignVariableNoInital>();
                IEnumerable<XElement> designVariableElements = studyElement.Element("Formulation").Descendants("DesignVariable");
                foreach (XElement designVariableElement in designVariableElements)
                {
                    Data designVariableData = Project.GetData(GetValue(designVariableElement, "Name"));
                    double lowerBound = Convert.ToDouble(GetValue(designVariableElement, "LowerBound"));
                    double upperBound = Convert.ToDouble(GetValue(designVariableElement, "UpperBound"));
                    BoundedDesignVariableNoInital designVariable = new BoundedDesignVariableNoInital(designVariableData as ScalarData, lowerBound, upperBound);
                    designVariables.Add(designVariable);
                }

                // Constants

                // Objectives
                List<Objective> objectives = new List<Objective>();
                IEnumerable<XElement> responseElements = studyElement.Element("Formulation").Descendants("Objective");
                foreach (XElement responseElement in responseElements)
                {
                    Data data = Project.GetData(GetValueOrInner(responseElement, "Name"));
                    ObjectiveType type = ObjectiveType.Minimise;
                    string typeString = GetValueOrInner(responseElement, "Type");
                    if (typeString == "Minimise")
                        type = ObjectiveType.Minimise;
                    else if (typeString == "Maximise")
                        type = ObjectiveType.Maximise;
                    Objective objective = new Objective(data, type);
                    objectives.Add(objective);
                }

                // Constraints
                List<Constraint> constraints = new List<Constraint>();
                IEnumerable<XElement> constraintElements = studyElement.Element("Formulation").Descendants("Constraint");
                foreach (XElement constraintElement in constraintElements)
                {
                    Data data = Project.GetData(GetValueOrInner(constraintElement, "Name"));
                    ConstraintType type = ConstraintType.LessThanOrEqual;
                    string typeString = GetValueOrInner(constraintElement, "Type");
                    if (typeString == "LessThanOrEqual")
                        type = ConstraintType.LessThanOrEqual;
                    else if (typeString == "GreatorThanOrEqual")
                        type = ConstraintType.GreatorThanOrEqual;
                    double value = Convert.ToDouble(GetValueOrInner(constraintElement, "Value"));
                    Constraint constraint = new Constraint(data, value, type);
                    constraints.Add(constraint);
                }

                OptimisationTemplate gaTemplate = new OptimisationTemplate(designVariables, new List<Parameter>(), objectives, constraints, workflow);

                GAParameters gaParameters = new GAParameters(gaTemplate);

                NSGAIIOptimiser nsga2 = new NSGAIIOptimiser("nsga2", "", gaTemplate, gaParameters);

                // Create deterministic optimisation study
                study = new DetOptStudy(name, description, workflow, gaTemplate, nsga2);
                //FormPresenter.ShowForm("Optimizer options", new OptimizerOptionsSelectionUI(gaParameters, study), false);

            }


            // Add study to repository
            Project.Add(study);
		}

		private static void GetWorkflow(XElement workflowElement, Dictionary<string, XElement> elementsDict)
		{
			Workflow workflow = null;
			string name = GetValue(workflowElement, "Name");
			string workflowType = GetValue(workflowElement, "Type");
			string description = DeserializeLines(GetValue(workflowElement, "Description"));
			bool isAux = Convert.ToBoolean(GetValue(workflowElement, "IsAuxiliary"));

			// Inputs
			var inputDataList = new List<Data>();
			IEnumerable<XElement> inputElements = workflowElement.Element("Inputs").Descendants("Input");
			foreach (XElement inputElement in inputElements)
			{
				string inputName = GetValueOrInner(inputElement, "Name");
				Data inputData = Project.GetData(inputName);
				if (inputData != null)
					inputDataList.Add(inputData);
			}

			// Outputs
			var outputDataList = new List<Data>();
			IEnumerable<XElement> outputElements = workflowElement.Element("Outputs").Descendants("Output");
			foreach (XElement outputElement in outputElements)
			{
				string outputDataName = GetValueOrInner(outputElement, "Name");
				Data outputData = Project.GetData(outputDataName);
				if (outputData != null)
					outputDataList.Add(outputData);
			}

			// Original Components Sequence
			var componentsList = new List<WorkflowComponent>();
			IEnumerable<XElement> componentElements = workflowElement.Element("Components").Descendants("Model");
			foreach (XElement componentElement in componentElements)
			{
				string modelName = GetValue(componentElement, "Name");
				string modelType = GetValue(componentElement, "Type");
				WorkflowComponent model = Project.GetComponent(modelName);

				// If the workflow has a dependency (workflow) that has not been deserialied yet,
				// look for it in the dictionary and deserialize it first so it can be added
				if (model == null)
				{
					if (elementsDict.TryGetValue(modelName, out XElement compElement))
					{
						GetWorkflow(compElement, elementsDict);
						model = Project.GetComponent(modelName);
					}
				}

				if (model != null)
					componentsList.Add(model);
			}

			// Scheduled Sequence
			var scheduledComponentsList = new List<WorkflowComponent>();
			IEnumerable<XElement> scheduledComponentsElements = workflowElement.Element("ScheduledComponents").Descendants("Model");
			foreach (XElement componentElement in scheduledComponentsElements)
			{
				string modelName = GetValue(componentElement, "Name");
				string modelType = GetValue(componentElement, "Type");
				WorkflowComponent model = Project.GetComponent(modelName);
				if (model != null)
					scheduledComponentsList.Add(model);
			}

			string mode = GetValue(workflowElement, "ScheduleMode");
			mode = String.IsNullOrWhiteSpace(mode) ? "GroupNonReversibleOnly" : mode;

			List<ISolver> solvers = GetSolvers(workflowElement.Element("Solvers"));

			// Select right kind of workflow
			if (workflowType == "Workflow")
			{
				workflow = new Workflow(name, description, inputDataList, outputDataList, componentsList, scheduledComponentsList, isAux, mode);
			}
			else if (workflowType == "WorkflowGlobal")
			{
				workflow = new WorkflowGlobal(name, description, inputDataList, outputDataList, componentsList, scheduledComponentsList, solvers, isAux, mode);
			}
			else if (workflowType == "ReversedModel")
			{
				workflow = new WorkflowReversedModel(name, description, inputDataList, outputDataList, componentsList.FirstOrDefault() as Model, solvers);
			}
			else if (workflowType == "StronglyConnectedComponent")
			{
				workflow = new WorkflowSCC(name, description, inputDataList, outputDataList, componentsList, scheduledComponentsList, solvers);
			}

			workflow.IsAuxiliary = isAux;
			// Add model to repository
			Project.Add(workflow, DuplicateMode.Ignore);
			//if (Project.WorkflowStore.FirstOrDefault(w => w.Name == name) == null)
			//	Project.WorkflowStore.Add(workflow);
		}

		private static void GetData(XElement dataElement, bool isAux = false)
		{
			string type = GetValue(dataElement, "Type");
			string name = GetValue(dataElement, "Name");
            string parentName = GetValue(dataElement, "ParentName");

            string description = DeserializeLines(GetValue(dataElement, "Description"));
			string value = GetValue(dataElement, "Value");

			string min = GetValue(dataElement, "Min");
			string max = GetValue(dataElement, "Max");
			Data data = null;
			if (type == "Double")
			{
				string decimalPlaces = GetValue(dataElement, "DecimalPlaces");
				string unit = GetValue(dataElement, "Unit");
				data = new DoubleData(name, description, Convert.ToDouble(value), Convert.ToInt32(decimalPlaces), unit, parentName: parentName);
			}
			else if (type == "DoubleVector")
			{
				string decimalPlaces = GetValue(dataElement, "DecimalPlaces");
				string unit = GetValue(dataElement, "Unit");
				double[] valueObject = DoubleVectorData.StringToValue(value);
				data = new DoubleVectorData(name, description, valueObject, Convert.ToInt32(decimalPlaces), unit);
			}
			else if (type == "Integer")
			{
				string unit = GetValue(dataElement, "Unit");
				data = new IntegerData(name, description, Convert.ToInt32(value), unit);
			}
			else if (type == "IntegerVector")
			{
				string unit = GetValue(dataElement, "Unit");
				int[] valueObject = IntegerVectorData.StringToValue(value);
				data = new IntegerVectorData(name, description, valueObject, unit);
			}
			else if (type == "DoubleMatrix")
			{
				string decimalPlaces = GetValue(dataElement, "DecimalPlaces");
				string unit = GetValue(dataElement, "Unit");
				double[,] valueObject = DoubleMatrixData.StringToValue(value);
				if (valueObject != null)
					data = new DoubleMatrixData(name, description, valueObject, Convert.ToInt32(decimalPlaces));
			}
			else if (type == "String")
			{
				string decimalPlaces = GetValue(dataElement, "DecimalPlaces");
				string unit = GetValue(dataElement, "Unit");
				data = new StringData(name, description, value);
			}
			data.Min = (min == "-Infinity" || min == "-INF") ? Double.NegativeInfinity : Convert.ToDouble(min);
			data.Max = (max == "Infinity" || min == "INF") ? Double.PositiveInfinity : Convert.ToDouble(max);

			data.IsAuxiliary = isAux;
			// Add data to repository
			Project.Add(data, DuplicateMode.Ignore);
			//if (!isAux && !Project.DataStore.Contains(data))
			//	Project.DataStore.Add(data);
			//else if (!Project.DataStoreAux.ContainsKey(data.Name))
			//	Project.DataStoreAux.Add(data.Name, data);
		}

		private static void GetModel(XElement modelElement, bool isAux = false)
		{
			string type = GetValue(modelElement, "Type");
			string name = GetValue(modelElement, "Name");
			string description = DeserializeLines(GetValue(modelElement, "Description"));

			// Inputs
			var inputDataList = new List<Data>();
			IEnumerable<XElement> inputElements = modelElement.Element("Inputs").Descendants("Input");
			foreach (XElement inputElement in inputElements)
			{
				string inputName = GetValueOrInner(inputElement, "Name");
				Data inputData = Project.GetData(inputName);
				if (inputData != null)
					inputDataList.Add(inputData);
			}
			// Outputs
			var outputDataList = new List<Data>();
			IEnumerable<XElement> outputElements = modelElement.Element("Outputs").Descendants("Output");
			foreach (XElement outputElement in outputElements)
			{
				string outputDataName = GetValueOrInner(outputElement, "Name");
				Data outputData = Project.GetData(outputDataName);
				if (outputData != null)
					outputDataList.Add(outputData);
			}

			Model model = null;
			switch (type)
			{
				//case "CDll":
				//	model = new ModelCDll(name, description, inputDataList, outputDataList, DeserializePath(GetValue(modelElement, "CDllPath")), GetValue(modelElement, "MethodName"));
				//	break;
				//case "CppDll":
				//	model = new ModelCppDll(name, description, inputDataList, outputDataList, DeserializePath(GetValue(modelElement, "CppDllPath")), GetValue(modelElement, "MethodName"));
				//	break;
				//case "CSharpLegacy":
				//	// Code
				//	model = new ModelCSharpLegacy(name, description, inputDataList, outputDataList, DeserializeLines(GetValue(modelElement, "CSharpCode")));
				//	break;
				//case "CSharpDllLegacy":
				//	// Dll Path
				//	model = new ModelCSharpDllLegacy(name, description, inputDataList, outputDataList, DeserializePath(GetValue(modelElement, "DllPath")), GetValue(modelElement, "DllEntry"));
				//	break;
				//case "FortranDll":
				//	model = new ModelFortranDll(name, description, inputDataList, outputDataList, DeserializePath(GetValue(modelElement, "FortranDllPath")), GetValue(modelElement, "MethodName"));
				//	break;
				//case "MatlabDll_7_3":
				//	model = new ModelMatlabDll_7_3(name, description, inputDataList, outputDataList, DeserializePath(GetValue(modelElement, "MatlabDllPath")), GetValue(modelElement, "MethodName"));
				//	break;
				//case "MatlabDll_7_7":
				//	model = new ModelMatlabDll_7_7(name, description, inputDataList, outputDataList, DeserializePath(GetValue(modelElement, "MatlabDllPath")), GetValue(modelElement, "MethodName"));
				//	break;
				//case "MatlabDll_7_8":
				//	model = new ModelMatlabDll_7_8(name, description, inputDataList, outputDataList, DeserializePath(GetValue(modelElement, "MatlabDllPath")), GetValue(modelElement, "MethodName"));
				//	break;
				//case "MatlabDll_7_13":
				//	model = new ModelMatlabDll_7_13(name, description, inputDataList, outputDataList, DeserializePath(GetValue(modelElement, "MatlabDllPath")), GetValue(modelElement, "MethodName"));
				//	break;
				//case "MatlabDll_7_15":
				//	model = new ModelMatlabDll_7_15(name, description, inputDataList, outputDataList, DeserializePath(GetValue(modelElement, "MatlabDllPath")), GetValue(modelElement, "MethodName"));
				//	break;
				case "CSharp":
                    string functionBody = "";
                    if (modelElement.Element("Body") != null)
                        functionBody = modelElement.Element("Body").Value;
                    if (functionBody != String.Empty)
                        model = new ModelCSharp(name, description, inputDataList, outputDataList, functionBody: functionBody);
                    else
                        model = new ModelCSharp(name, description, inputDataList, outputDataList, functionBody: DeserializeLines(GetValue(modelElement, "Body")));
					break;
				case "DotNetDll":
					model = new ModelDotNetDll(name, description, inputDataList, outputDataList, DeserializePath(GetValue(modelElement, "AssemblyName")), GetValue(modelElement, "TypeName"), GetValue(modelElement, "Method"));
					break;
				case "Exe":
					string executableFileName = DeserializePath(GetValue(modelElement, "ExecutableFileName"));
					string executableFileArguments = GetValue(modelElement, "ExecutableFileArguments");
					string workingDirectory = DeserializePath(GetValue(modelElement, "WorkingDirectory") ?? Path.GetDirectoryName(executableFileName));

					// Instructions sets for each input file
					var inputFilesInstructionsSets = new List<InstructionsSet>();
					IEnumerable<XElement> inputFilesInstructionsSetElements = modelElement.Element("InputFilesInstructionsSets").Descendants("InstructionsSet");
					foreach (XElement instructionSetElement in inputFilesInstructionsSetElements)
					{
						InstructionsSet instructionsSet = GetInstructionsSet(instructionSetElement);
						inputFilesInstructionsSets.Add(instructionsSet);
					}

					// Instructions sets for each output file
					var outputFilesInstructionsSets = new List<InstructionsSet>();
					IEnumerable<XElement> outputFilesInstructionsSetElements = modelElement.Element("OutputFilesInstructionsSets").Descendants("InstructionsSet");
					foreach (XElement instructionSetElement in outputFilesInstructionsSetElements)
					{
						InstructionsSet instructionsSet = GetInstructionsSet(instructionSetElement);
						outputFilesInstructionsSets.Add(instructionsSet);
					}

					model = new ModelExe(name, description, inputDataList, outputDataList, workingDirectory, executableFileName, executableFileArguments, inputFilesInstructionsSets, outputFilesInstructionsSets);
					break;
				case "Matlab":
					model = new ModelMatlab(name, description, inputDataList, outputDataList, DeserializePath(GetValue(modelElement, "FilePath")), GetValue(modelElement, "FunctionName"));
					break;
                case "WebService":
                    model = new ModelWebService(name, description, inputDataList, outputDataList, DeserializeLines(GetValue(modelElement, "EndPoint")), "");
                    break;
                default:
					break;
			}

			model.IsAuxiliary = isAux;
			// Add model to repository
			Project.Add(model, DuplicateMode.Ignore);
			//if (!isAux && Project.ModelStore.FirstOrDefault(m => m.Name == name) == null && modobj != null)
			//	Project.ModelStore.Add(modobj);
			//else if (modobj != null && !Project.ModelStoreAux.ContainsKey(name))
			//	Project.ModelStoreAux.Add(name, modobj);
		}



		private static XElement GetLocationElement(InstructionsSet instructionsSet)
		{
			var instructionSetElement = new XElement("InstructionsSet");

			// File path
			instructionSetElement.Add(new XAttribute("FileName", SerializePath(instructionsSet.FilePath)));

			// Data name and location
			foreach (KeyValuePair<string, LocationBase> kvp in instructionsSet.Instructions)
			{
				var instructionElement = new XElement("Instruction");

				// Data name
				instructionElement.Add(new XAttribute("DataName", kvp.Key));

				// Data location
				var locationElement = new XElement("DataLocation");
				locationElement.Add(new XAttribute("LocationType", kvp.Value.GetType().Name));
				foreach (PropertyInfo property in kvp.Value.GetType().GetProperties())
				{
					if (property.PropertyType == typeof(List<string>))
					{
						string value = String.Empty;
						foreach (string item in property.GetValue(kvp.Value) as List<string>)
						{
							value += item + '\n';
						}
						value = value.TrimEnd('\n');
						locationElement.Add(new XAttribute(property.Name, SerializeLines(value)));
					}
					else if (property.PropertyType == typeof(char[]))
					{
						string value = String.Empty;
						foreach (char item in property.GetValue(kvp.Value) as char[])
						{
							value += $"{item}'";
						}
						value = value.TrimEnd('\'');
						locationElement.Add(new XAttribute(property.Name, value));
					}
					else
					{
						locationElement.Add(new XAttribute(property.Name, property.GetValue(kvp.Value)));
					}
				}

				//// Old Style
				//if (kvp.Value is WordFixedLocation)
				//{
				//	WordFixedLocation l = kvp.Value as WordFixedLocation;
				//	locationElement.Add(new XElement("Type", "ScalarFixedRowFixedColumn"));
				//	locationElement.Add(new XElement("Line", l.Line));
				//	locationElement.Add(new XElement("StartColumn", l.StartColumn));
				//	locationElement.Add(new XElement("EndColumn", l.EndColumn));
				//	locationElement.Add(new XElement("DataType", l.Type));
				//	locationElement.Add(new XElement("DataFormat", l.Format));
				//}
				//else if (kvp.Value is WordFixedColumnLocation)
				//{
				//	WordFixedColumnLocation l = kvp.Value as WordFixedColumnLocation;
				//	locationElement.Add(new XElement("Type", "ScalarRelativeRowFixedColumn"));
				//	XElement anchorTextsElement = new XElement("AnchorTexts");
				//	foreach (string anchorText in l.AnchorTexts)
				//	{
				//		anchorTextsElement.Add(new XElement("AnchorText", anchorText));
				//	}
				//	locationElement.Add(anchorTextsElement);
				//	locationElement.Add(new XElement("AnchorSkipLines", l.SkipLines));
				//	locationElement.Add(new XElement("StartColumn", l.StartColumn));
				//	locationElement.Add(new XElement("EndColumn", l.EndColumn));
				//	locationElement.Add(new XElement("DataType", l.Type));
				//	locationElement.Add(new XElement("DataFormat", l.Format));
				//}
				//else if (kvp.Value is ArrayFixedLocationFixedCount)
				//{
				//	ArrayFixedLocationFixedCount l = kvp.Value as ArrayFixedLocationFixedCount;
				//	locationElement.Add(new XElement("Type", "VectorFixedRowFixedColumnFixedCount"));

				//	locationElement.Add(new XElement("Line", l.Line));
				//	locationElement.Add(new XElement("Count", l.Count));
				//	locationElement.Add(new XElement("Frequency", l.Frequency));
				//	locationElement.Add(new XElement("StartColumn", l.StartColumn));
				//	locationElement.Add(new XElement("EndColumn", l.EndColumn));
				//	locationElement.Add(new XElement("DataType", l.Type));
				//	locationElement.Add(new XElement("DataFormat", l.Format));
				//}
				//else if (kvp.Value is ArrayFixedColumnLocation)
				//{
				//	ArrayFixedColumnLocation l = kvp.Value as ArrayFixedColumnLocation;
				//	locationElement.Add(new XElement("Type", "VectorRelativeRowFixedColumn"));

				//	XElement anchorTextsElement = new XElement("AnchorTexts");
				//	foreach (string anchorText in l.AnchorTexts)
				//	{
				//		anchorTextsElement.Add(new XElement("AnchorText", anchorText));
				//	}
				//	locationElement.Add(anchorTextsElement);
				//	locationElement.Add(new XElement("AnchorSkipLines", l.SkipLines));
				//	locationElement.Add(new XElement("Frequency", l.Frequency));
				//	locationElement.Add(new XElement("StartColumn", l.StartColumn));
				//	locationElement.Add(new XElement("EndColumn", l.EndColumn));
				//	locationElement.Add(new XElement("StopText", l.StopText));
				//	locationElement.Add(new XElement("DataType", l.Type));
				//	locationElement.Add(new XElement("DataFormat", l.Format));
				//}
				//else
				//{
				//	throw new NotImplementedException();
				//}
				instructionElement.Add(locationElement);

				instructionSetElement.Add(instructionElement);
			}

			return instructionSetElement;
		}

		private static InstructionsSet GetInstructionsSet(XElement instructionSetElement)
		{
			// File name (including project-relative path)
			string fileName = DeserializePath(GetValue(instructionSetElement, "FileName"));

			var instructions = new Dictionary<string, LocationBase>();
			IEnumerable<XElement> instructionsElements = instructionSetElement.Descendants("Instruction");
			foreach (XElement instructionElement in instructionsElements)
			{
				string dataName = GetValue(instructionElement, "DataName");

				XElement locationElement = instructionElement.Element("DataLocation");
				string locationTypeName = GetValue(locationElement, "LocationType");
				LocationBase locationBase = null;

				Type locationType = Assembly.GetAssembly(typeof(LocationBase)).GetType(locationsNamespaceName + locationTypeName);
				ConstructorInfo constructor = locationType.GetConstructors().Count() > 1 ? locationType.GetConstructors()[1] : locationType.GetConstructors()[0];
				// Get parameters
				ParameterInfo[] parameterInfos = constructor.GetParameters();
				object[] parameters = new object[parameterInfos.Length];
				for (int i = 0; i < parameterInfos.Length; i++)
				{
					string name = parameterInfos[i].Name;
					name = name.Substring(0, 1).ToUpper() + name.Substring(1);

					Type type = parameterInfos[i].ParameterType;

					string valueString = GetValue(locationElement, name);
					if (type == typeof(Types))
					{
						Types value = Types.Double;
						switch (valueString)
						{
							case "Int":
								value = Types.Int;
								break;
							case "Double":
								value = Types.Double;
								break;
							case "String":
								value = Types.String;
								break;
							case "Bool":
								value = Types.Bool;
								break;
							default:
								break;
						}
						parameters[i] = value;
					}
					else if (type == typeof(List<string>))
					{
						valueString = DeserializeLines(valueString);
						var valueList = new List<string>();
						foreach (string item in valueString.Split('\n'))
							valueList.Add(item);
						parameters[i] = valueList;
					}
					else if (type == typeof(char[]))
					{
						valueString = DeserializeLines(valueString);
						var valueList = new List<char>();
						foreach (string item in valueString.Split('\n'))
							valueList.Add(item[0]);
						parameters[i] = valueList.ToArray();
					}
					else
					{
						parameters[i] = Convert.ChangeType(valueString, type);
					}
				}
				locationBase = constructor.Invoke(parameters) as LocationBase;

				//// Old Style
				//if (locationTypeName == "ScalarFixedRowFixedColumn")
				//{
				//	int line = Convert.ToInt32(locationElement.Element("Line").Value);
				//	int startColumn = Convert.ToInt32(locationElement.Element("StartColumn").Value);
				//	int endColumn = Convert.ToInt32(locationElement.Element("EndColumn").Value);
				//	Types type = (Types)(Enum.Parse(typeof(Types), locationElement.Element("DataType").Value));
				//	string format = locationElement.Element("DataFormat").Value;
				//	locationBase = new WordFixedLocation(line, startColumn, endColumn, type, format);
				//}
				//else if (locationTypeName == "ScalarRelativeRowFixedColumn")
				//{
				//	List<string> anchorTexts = new List<string>();
				//	var anchorTextElements = locationElement.Element("AnchorTexts").Descendants("AnchorText");
				//	foreach (XElement anchorTextElement in anchorTextElements)
				//	{
				//		anchorTexts.Add(anchorTextElement.Value);
				//	}
				//	int anchorSkipLines = Convert.ToInt32(locationElement.Element("AnchorSkipLines").Value);
				//	int startColumn = Convert.ToInt32(locationElement.Element("StartColumn").Value);
				//	int endColumn = Convert.ToInt32(locationElement.Element("EndColumn").Value);
				//	Types type = (Types)(Enum.Parse(typeof(Types), locationElement.Element("DataType").Value));
				//	string format = locationElement.Element("DataFormat").Value;
				//	locationBase = new WordFixedColumnLocation(anchorTexts, anchorSkipLines, startColumn, endColumn, type, format);
				//}
				//else if (locationTypeName == "VectorFixedRowFixedColumnFixedCount")
				//{
				//	int line = Convert.ToInt32(locationElement.Element("Line").Value);
				//	int count = Convert.ToInt32(locationElement.Element("Count").Value);
				//	int frequency = Convert.ToInt32(locationElement.Element("Frequency").Value);
				//	int startColumn = Convert.ToInt32(locationElement.Element("StartColumn").Value);
				//	int endColumn = Convert.ToInt32(locationElement.Element("EndColumn").Value);
				//	Types type = (Types)(Enum.Parse(typeof(Types), locationElement.Element("DataType").Value));
				//	string format = locationElement.Element("DataFormat").Value;
				//	locationBase = new ArrayFixedLocationFixedCount(line, count, startColumn, endColumn, frequency, type, format);
				//}
				//else if (locationTypeName == "VectorRelativeRowFixedColumn")
				//{
				//	List<string> anchorTexts = new List<string>();
				//	var anchorTextElements = locationElement.Element("AnchorTexts").Descendants("AnchorText");
				//	foreach (XElement anchorTextElement in anchorTextElements)
				//	{
				//		anchorTexts.Add(anchorTextElement.Value);
				//	}
				//	int anchorSkipLines = Convert.ToInt32(locationElement.Element("AnchorSkipLines").Value);
				//	int startColumn = Convert.ToInt32(locationElement.Element("StartColumn").Value);
				//	int endColumn = Convert.ToInt32(locationElement.Element("EndColumn").Value);
				//	string stopText = locationElement.Element("StopText").Value;
				//	int frequency = Convert.ToInt32(locationElement.Element("Frequency").Value);
				//	Types type = (Types)(Enum.Parse(typeof(Types), locationElement.Element("DataType").Value));
				//	string format = locationElement.Element("DataFormat").Value;
				//	locationBase = new ArrayFixedColumnLocation(anchorTexts, stopText, anchorSkipLines, startColumn, endColumn, frequency, type, format);
				//}
				//else
				//{
				//	throw new NotImplementedException();
				//}

				instructions.Add(dataName, locationBase);
			}

			var instructionsSet = new InstructionsSet(fileName, instructions);
			return instructionsSet;
		}

		private static string SerializePath(string path) => path.Contains(Project.ProjectPath) ? path.Replace(Project.ProjectPath, "*") : path;

		private static string DeserializePath(string path) => path.StartsWith("*") ? path.Replace("*", Project.ProjectPath) : path;

		private static XElement GetSolverElement(ISolver solver)
		{
			var solverElement = new XElement(solver.GetType().Name);
			var solverOptions= new NumericalMethodOptions(solver);
			//solverElement.Add(new XElement("Type", solver.Name));
			//XElement options = new XElement("Options");

			foreach (string key in solverOptions.Names)
			{
				solverElement.Add(new XAttribute(key, solverOptions[key]));
			}
			//solverElement.Add(options);
			return solverElement;
		}

		private static List<ISolver> GetSolvers(XElement solversElement)
		{
			if (solversElement == null)
				return null;
			var solvers = new List<ISolver>();
			foreach (XElement solverElement in solversElement?.Descendants())
			{
				var solverType = Type.GetType(SolversNamespaceName + solverElement.Name.LocalName);
				if (solverType == null) continue;

				var solver = Activator.CreateInstance(solverType) as ISolver;

				var solverOptions = new NumericalMethodOptions(solver);
				foreach (XAttribute optionElement in solverElement.Attributes())
					solverOptions[optionElement.Name.LocalName] = optionElement.Value;

				solvers.Add(solver);
			}
			return solvers;
		}

		private static string GetValue(XElement element, string key) => element.Attribute(key)?.Value ?? element.Element(key)?.Value ?? String.Empty;

		private static string GetValueOrInner(XElement element, string key) => element.Attribute(key)?.Value ?? element.Value ?? String.Empty;

		private const string locationsNamespaceName = "ExeModelTextFileManager.DataLocations.";

		private static readonly string SolversNamespaceName = "Aircadia.Numerics.Solvers.";

		private static string SerializeLines(string lines) => lines.Replace("\n", "# \\n #");

		private static string DeserializeLines(string lines) => lines.Replace("# \\n #", "\n");
	}
}
