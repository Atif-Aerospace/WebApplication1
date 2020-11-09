using Aircadia.Utilities;
using Aircadia.Numerics.Solvers;
using Aircadia.ObjectModel.DataObjects;
using Aircadia.ObjectModel.Models;
using Aircadia.ObjectModel.Workflows;
using System;
using System.Collections.Generic;
using System.Linq;
using Northwoods.Go;
using Northwoods.Go.Layout;

namespace Aircadia.WorkflowManagement
{
	public class WorkflowScheduler
	{
		public static Func<List<WorkflowComponent>, List<WorkflowComponent>> SccScheduleProvider;

		public static (MatchingDictionary matching, MaximumMatchings2 matchings) FindAllMatchings(List<Data> inputs, List<Data> outputs, List<WorkflowComponent> components)
		{
			var allData = inputs.Concat(outputs).ToList();

			// 0. Sanity checks, check array is not more than 1 output
			NonReversibleSanityCheck(components);

			// 1. Find Model-Variable matching. Which variables are inputs, and wich are outputs for each model
			return FindAllMatches(inputs, allData, components);
		}

		public static Workflow ScheduleWorkflow(string name, string description, List<Data> inputs, List<Data> outputs, 
			List<WorkflowComponent> components, MatchingDictionary matching, ReversalMode mode)
		{
			if (mode == ReversalMode.Global || mode == ReversalMode.Legacy)
			{
				throw new ArgumentException($"Only reversals modes: '{ReversalMode.GroupNonReversibleOnly}' and '{ReversalMode.GroupBoth}' " +
					$"are valid to schedule a workflow from a matching", nameof(mode));
			}

			List<WorkflowComponent> workflowComponents = components.GetAllComponents();
			var allData = inputs.Concat(outputs).ToList();

			// 2. Determine which models are reversed, and create reversed Workflows (Model or Global) to cater for them
			ReverseComponents(name, matching, workflowComponents, out List<WorkflowComponent> matchedComponents, out List<WorkflowComponent> reversedComponents);

			// 3. Cluster reversed components joint by non-reversible variables (e.g. arrays)
			List<WorkflowComponent> reversedClusteredComponents = ClusterReversedModel(name, allData, matchedComponents, reversedComponents, mode, components);

			// 4. Cluster Strongly Connected Components to get an acyclic graph
			List<WorkflowComponent> clusteredComponents = ClusterStronglyConnectedComponents(name, allData, reversedClusteredComponents);

			// 5. Schedule the acyclic graph to get the final order of the components
			List<WorkflowComponent> scheduledComponents = ScheduleAcyclic(allData, clusteredComponents);

			return new Workflow(name, description, inputs, outputs, components, scheduledComponents, false, mode.ToString())
			{
				DependencyAnalysis = new GraphBasedDependencyAnalysis(matchedComponents)
			};
		}

		public static Workflow ScheduleWorkflow(string name, string description, List<Data> inputs, List<Data> outputs, List<WorkflowComponent> components, ReversalMode mode)
		{
			// Redirect to right scheduler method
			if (mode == ReversalMode.Global)
			{
				return ScheduleWorkflowGlobal(name, description, inputs, outputs, components, GlobalReversalMode.Global);
			}
			else if (mode == ReversalMode.Legacy)
			{
				return LibishScheduler.ScheduleWorkflow(name, description, inputs, outputs, components);
			}

			List<WorkflowComponent> workflowComponents = components.GetAllComponents();
			var allData = inputs.Concat(outputs).ToList();

			// 0. Sanity checks, check array is not more than 1 output
			NonReversibleSanityCheck(workflowComponents);

			// 1. Find Model-Variable matching. Which variables are inputs, and wich are outputs for each model
			MatchingDictionary outputsDict = MatchModelsWithVariables(inputs, allData, workflowComponents);

			// 2. Determine which models are reversed, and create reversed Workflows (Model or Global) to cater for them
			ReverseComponents(name, outputsDict, workflowComponents, out List<WorkflowComponent> matchedComponents, out List<WorkflowComponent> reversedComponents);

			// 3. Cluster reversed components joint by non-reversible variables (e.g. arrays)
			List<WorkflowComponent> reversedClusteredComponents = ClusterReversedModel(name, allData, matchedComponents, reversedComponents, mode, components);

			// 4. Cluster Strongly Connected Components to get an acyclic graph
			List<WorkflowComponent> clusteredComponents = ClusterStronglyConnectedComponents(name, allData, reversedClusteredComponents);

			// 5. Schedule the acyclic graph to get the final order of the components
			List<WorkflowComponent> scheduledComponents = ScheduleAcyclic(allData, clusteredComponents);

			return new Workflow(name, description, inputs, outputs, components, scheduledComponents, false, mode.ToString())
			{
				DependencyAnalysis = new GraphBasedDependencyAnalysis(matchedComponents)
			};
		}

		public static Workflow ScheduleWorkflowGlobal(string name, string description, List<Data> inputs, List<Data> outputs, List<WorkflowComponent> components, GlobalReversalMode mode)
		{
			List<WorkflowComponent> workflowComponents = components.GetAllComponents();
			var allData = inputs.Concat(outputs).ToList();

			// 0. Samity checks, check is not more than 1 output
			if (mode == GlobalReversalMode.NoReversedModels)
			{
				if (!AllSanityCheck(workflowComponents))
					mode = GlobalReversalMode.ReverseModelsWhenReversibleVariables;
			}
			else if (mode == GlobalReversalMode.Global)
			{
				if (!AllSanityCheck(workflowComponents))
					throw new ArgumentException($"A default workflow does not exist. Therefore the gloabl workflow cannot be created");
			}

			if (mode == GlobalReversalMode.ReverseModelsWhenReversibleVariables)
				NonReversibleSanityCheck(workflowComponents);

			// 1. Find Model-Variable matching. Which variables are inputs, and wich are outputs for each model
			MatchingDictionary outputsDict = MatchModelsWithVariablesGlobal(allData, components, mode);

			// 2. Determine which models are reversed, and create reversed Workflows (Model or Global) to cater for them
			ReverseComponents(name, outputsDict, workflowComponents, out List<WorkflowComponent> matchedComponents, out List<WorkflowComponent> reversedComponents);

			// 4. Cluster Strongly Connected Components to get an acyclic graph
			List<WorkflowComponent> clusteredComponents = ClusterStronglyConnectedComponents(name, allData, matchedComponents);

			// 5. Schedule the acyclic graph to get the final order of the components
			List<WorkflowComponent> scheduledComponents = ScheduleAcyclic(allData, clusteredComponents);

			Workflow workflow = null;
			if (clusteredComponents.Count < matchedComponents.Count)
			{
				workflow = ScheduleWorkflowSCC(name.Split(':').First(), description, inputs, outputs, workflowComponents, matchedComponents);
			}
			else
			{
				workflow = new WorkflowGlobal(name, description, inputs, outputs, components, scheduledComponents, DeafaultSolvers(), mode != GlobalReversalMode.Global, mode.ToString());
			}
			workflow.DependencyAnalysis = new GraphBasedDependencyAnalysis(matchedComponents);
			return workflow;
		}

		public static Workflow ReverseWorkflow(Workflow workflowToReverse, IEnumerable<Data> inputsToReverse, IEnumerable<Data> outputsToReverse)
		{
			Workflow wf = workflowToReverse;
			var inputs = wf.ModelDataInputs.Except(inputsToReverse).Union(outputsToReverse).ToList();
			var outputs = wf.ModelDataOutputs.Except(outputsToReverse).Union(inputsToReverse).ToList();

			var mode = (ReversalMode)Enum.Parse(typeof(ReversalMode), wf.ScheduleMode);

			return ScheduleWorkflow(wf.Name, wf.Description, inputs, outputs, wf.Components, mode);
		}

		private static WorkflowSCC ScheduleWorkflowSCC(string name, string description, List<Data> inputs, List<Data> outputs, List<WorkflowComponent> components, List<WorkflowComponent> sccComponents)
		{
			string sccName = Workflow.GetSCCWorkflowName(name, components, sccComponents);
			List<WorkflowComponent> sccScheduledComponents = DefaultSccScheduleProvider(sccComponents);
			if (SccScheduleProvider != null)
			{
				sccScheduledComponents = SccScheduleProvider(sccScheduledComponents);
			}
			return new WorkflowSCC(sccName, description, inputs, outputs, sccComponents, sccScheduledComponents, DeafaultSolversSCC());
		}

		private static List<WorkflowComponent> DefaultSccScheduleProvider(List<WorkflowComponent> components)
		{
			var allData = components.GetAllData();


			// Nodes
			var dataNodes = new Dictionary<string, GoBasicNode>();
			GoView tempView = new GoView();
			foreach (Data data in allData)
			{
				var node = new GoBasicNode { Text = data.Id };

				tempView.Document.Add(node);
				dataNodes[data.Id] = node;
			}
			
			foreach (Model model in components)
			{
				var node = new GoBasicNode { Text = model.Id };

				tempView.Document.Add(node);

				// Links
				foreach (Data data in model.ModelDataInputs)
				{
					tempView.Document.Add(new GoLink
					{
						ToArrow = true,
						FromPort = dataNodes[data.Id].Port,
						ToPort = node.Port
					});
				}
				foreach (Data data in model.ModelDataOutputs)
				{
					tempView.Document.Add(new GoLink
					{
						ToArrow = true,
						FromPort = node.Port,
						ToPort = dataNodes[data.Id].Port
					});
				}
			}

			// automatically layout the graph
			GoLayoutLayeredDigraph layout = new GoLayoutLayeredDigraph
			{
				Document = tempView.Document,
				DirectionOption = GoLayoutDirection.Down,
				SetsPortSpots = false,  // for nicer looking links near single-Port nodes when not Orthogonal
				ColumnSpacing = 15,
				LayerSpacing = 10
			};
			layout.PerformLayout();

			var modelLayersDictionary = new Dictionary<WorkflowComponent, int>();
			foreach (GoLayoutLayeredDigraphNode node in layout.Network.Nodes)
			{
				if (node.GoObject is GoBasicNode n && components.FirstOrDefault(c => c.Id == n.Text) is WorkflowComponent wc)
				{
					modelLayersDictionary.Add(wc, node.Layer);
				}
			}

			var scheduledComponents = modelLayersDictionary.OrderBy(kvp => kvp.Value).Select(kvp => kvp.Key).ToList();
			return scheduledComponents;
		}

		private static MatchingDictionary MatchModelsWithVariablesGlobal(List<Data> allData, List<WorkflowComponent> components, GlobalReversalMode mode)
		{

			if (mode == GlobalReversalMode.NoReversedModels || mode == GlobalReversalMode.Global)
			{
				List<WorkflowComponent> originalComponents = components.GetAllComponents();
				Dictionary<string, IOStatus> status = originalComponents.GetInputsOutputsStatus(allData, out List<Data> inputs, out List<Data> outputs);
				foreach (string key in status.Keys)
					if (status[key] == IOStatus.Conflict)
						throw new ArgumentException($"The following varibale \"{key}\" is of non-reversible type and output of more than one model. Therefore the workflow cannot be created");

				var inputsHash = new HashSet<string>(inputs.Select(d => d.Id));

				var bipartite = new BipartiteGraph();
				Primes.Reset();
				var completeMatching = new MatchingDictionary(originalComponents.Select(c => c.Id));

				// 1.1 Add Nodes for the Variables
				// Filter-out the selected inputs and outputs, as they don't belong to the bipartite graph
				foreach (Data data in allData.Where(d => !inputsHash.Contains(d.Id)))
					bipartite.AddNode(data.Id, GraphNode.Type.Type1);

				// 1.2 Add Nodes for the Models, and edges between Variables and Nodes
				foreach (WorkflowComponent component in originalComponents)
				{
					completeMatching.Add(component.Id, new HashSet<string>());

					bipartite.AddNode(CS(component.Id), GraphNode.Type.Type2);
					GraphNode modelNode = bipartite.GetNode(CS(component.Id));

					// Filter-out the selected inputs, as they don't belong to the bipartite graph
					foreach (Data data in component.ModelDataInputs.Where(d => !inputsHash.Contains(d.Id)))
						bipartite.AddDirectedEdge(data.Id, modelNode, Primes.Next(), 1);

					// Filter-out the selected inputs, as they don't belong to the bipartite graph
					foreach (Data data in component.ModelDataOutputs.Where(d => !inputsHash.Contains(d.Id)))
						bipartite.AddDirectedEdge(data.Id, modelNode, Primes.Next(), 0);

					// How many output the model need
					if (component.ModelDataOutputs.Count > 1)
						bipartite.modelAndItsOutputs.Add(CS(component.Id), component.ModelDataOutputs.Count); //signle output models are not stored
				}

				// 1.3 Associate matchings to each model, maping models to set of outputs
				var matches = new MaximumMatchings2(bipartite, true);

				if (matches.OverConstrainedModels.Count > 0)
					throw new ArgumentException($"The inputs conbination is not valid as the following models are overconstraint:\r\n\t" +
						matches.OverConstrainedModels.Aggregate((total, last) => total += "\r\n\t" + last) + "\r\n");

				Matching matching = matches.OrderedFilteredMaximumMatchings.FirstOrDefault()
				?? throw new NullReferenceException("NO suitable matchings were found");

				completeMatching.CompleteWithMatching(matching);

				return completeMatching;
			}
			else
			{
				Dictionary<string, IOStatus> status = components.GetInputsOutputsStatus(allData, out List<Data> inputs, out List<Data> outputs);


				var inputsHash = new HashSet<string>(inputs.Select(d => d.Id));
				var outputsHash = new HashSet<string>();

				List<WorkflowComponent> originalComponents = components.GetAllComponents();
				Dictionary<string, IOStatus> originalStatus = originalComponents.GetDataStatus(allData);

				var nonReversibleData = allData.Where(d => !(d is DoubleData)).ToList();
				Dictionary<string, IOStatus> nonReversibleStatus = components.GetDataStatus(nonReversibleData);
				foreach (string data in nonReversibleStatus.Keys)
				{
					if (nonReversibleStatus[data] == IOStatus.Input)
						inputsHash.Add(data); // Should be already there
					else if (nonReversibleStatus[data] == IOStatus.Output || nonReversibleStatus[data] == IOStatus.Both)
						outputsHash.Add(data);
				}

				var reversedInputsHash = new HashSet<string>();
				var reversedOutputsHash = new HashSet<string>();
				foreach (WorkflowComponent component in components)
				{
					if (component is IReversableWorkflow rw)
					{
						foreach (Data data in rw.ReversedInputs)
							reversedInputsHash.Add(data.Id);

						foreach (Data data in rw.ReversedOutputs)
							reversedOutputsHash.Add(data.Id);

						IEnumerable<Data> NonReversableReversedInputs = rw.ReversedInputs.Where(d => !(d is DoubleData));
						IEnumerable<Data> NonReversableReversedOutputs = rw.ReversedOutputs.Where(d => !(d is DoubleData));

						// If the model id the upper end of a reversal throug non-reversible variables.
						int difference = NonReversableReversedOutputs.Count() - NonReversableReversedInputs.Count();
						while (difference > 0)
						{
							// Assign as many reversible variable to the inputs as reversals end in the component
							IEnumerable<Data> reversableReversedInputs = rw.ReversedInputs.Where(d => d is DoubleData);
							foreach (Data data in reversableReversedInputs.Take(difference))
								inputsHash.Add(data.Id);
						}

					}
				}

				// Relax one input per reversal -> lowerEndOfReversal might have more elements than non-reversible reversal, but those should be already not in inputHash
				IEnumerable<string> lowerEndOfReversals = reversedOutputsHash.Except(reversedInputsHash);
				inputsHash.ExceptWith(lowerEndOfReversals);

				var bipartite = new BipartiteGraph();
				Primes.Reset();
				var completeMatching = new MatchingDictionary(originalComponents.Select(c => c.Id));

				// 1.1 Add Nodes for the Variables
				// Filter-out the selected inputs and outputs, as they don't belong to the bipartite graph
				foreach (Data data in allData.Where(d => !inputsHash.Contains(d.Id) && !outputsHash.Contains(d.Id)))
					bipartite.AddNode(data.Id, GraphNode.Type.Type1);

				// 1.2 Add Nodes for the Models, and edges between Variables and Nodes
				foreach (WorkflowComponent component in originalComponents)
				{
					completeMatching.Add(component.Id, new HashSet<string>());

					// if the component has all its inputs and outputs determined do not add to the graph
					bool addModel = component.ModelDataOutputs.Count > component.ModelDataOutputs.Where(d => inputsHash.Count > -1 && outputsHash.Contains(d.Id)).Count();
					if (addModel)
						bipartite.AddNode(component.Id, GraphNode.Type.Type2);

					GraphNode modelNode = bipartite.GetNode(component.Id);

					int uniqueNonReversibleOutputs = 0;

					// Filter-out the selected inputs, as they don't belong to the bipartite graph
					foreach (Data data in component.ModelDataInputs.Where(d => !inputsHash.Contains(d.Id) && !outputsHash.Contains(d.Id)))
					{
						bipartite.AddDirectedEdge(data.Id, modelNode, Primes.Next(), 1);
					}

					// Filter-out the selected inputs, as they don't belong to the bipartite graph
					foreach (Data data in component.ModelDataOutputs.Where(d => !inputsHash.Contains(d.Id)))
					{
						if (outputsHash.Contains(data.Id))
						{
							uniqueNonReversibleOutputs++;
							completeMatching[component.Id].Add(data.Id);
						}
						else
						{
							bipartite.AddDirectedEdge(data.Id, modelNode, Primes.Next(), 0);
						}
					}

					// How many output the model need
					if (component.ModelDataOutputs.Count - uniqueNonReversibleOutputs > 1 && addModel)
					{
						bipartite.modelAndItsOutputs.Add(component.Id, component.ModelDataOutputs.Count - uniqueNonReversibleOutputs); //signle output models are not stored
					}
				}

				// 1.3 Associate matchings to each model, maping models to set of outputs
				var matches = new MaximumMatchings2(bipartite, true);

				if (matches.OverConstrainedModels.Count > 0)
					throw new ArgumentException($"The inputs conbination is not valid as the following models are overconstraint:\r\n\t" +
						matches.OverConstrainedModels.Aggregate((total, last) => total += "\r\n\t" + last) + "\r\n");

				Matching matching = matches.OrderedFilteredMaximumMatchings.FirstOrDefault()
				?? throw new NullReferenceException("NO suitable matchings were found");

				completeMatching.CompleteWithMatching(matching);

				return completeMatching;
			}
		}

		private static bool AllSanityCheck(List<WorkflowComponent> workflowComponents)
		{
			foreach (var kvp in workflowComponents.GetDataStatus(workflowComponents.GetAllData()))
			{
				if (kvp.Value == IOStatus.Conflict)
				{
					return false;
				}
			}
			
			return true;
		}

		private static void NonReversibleSanityCheck(List<WorkflowComponent> workflowComponents)
		{
			foreach (var kvp in workflowComponents.GetDataStatus(workflowComponents.GetAllData().Where(d => !(d is DoubleData))))
			{
				if (kvp.Value == IOStatus.Conflict)
				{
					throw new ArgumentException($"The following varibale \"{kvp.Key}\" is of non-reversible type and output of more than one model. Therefore the workflow cannot be created");
				}
			}
		}

		private static List<WorkflowComponent> ClusterReversedModel(string name, List<Data> allData, List<WorkflowComponent> matchedComponents, List<WorkflowComponent> reversedComponents, ReversalMode mode, List<WorkflowComponent> components)
		{

			//// Identify Reversed Components
			//foreach (var component in matchedComponents)
			//	if (component is IReversableWorkflow reversable && reversable.ReversedInputs.Count > 0)
			//		reversedComponents.Add(component);

			// 3.1. Graph only with reversed links of non-reversible variables only, both forward and backward (can be thought as undirected)
			var reversedLinksGraph = new Graph();
			foreach (Data data in reversedComponents.GetAllData())
				reversedLinksGraph.AddVertex(data.Id);
			foreach (WorkflowComponent component in reversedComponents)
				AddComponentAndEdgesBoth(reversedLinksGraph, component, mode);

			// 3.2. Forward graph - To trace forward dependencies
			var forwardGraph = new Graph();
			foreach (Data data in allData)
				forwardGraph.AddVertex(data.Id);
			foreach (WorkflowComponent component in matchedComponents)
				AddComponentAndEdgesForward(forwardGraph, component);

			//forwardGraph = GraphBuilder.FromTwoSets(matchedComponents, allData, name1: c => CS(c.Id), from1to2: c => c.ModelDataOutputs, to1from2: c => c.ModelDataInputs);

			// 3.3. Reversed graph - To trace backward dependencies
			var reversedGraph = new Graph();
			foreach (Data data in allData)
				reversedGraph.AddVertex(data.Id);
			foreach (WorkflowComponent component in matchedComponents)
				AddComponentAndEdgesBackWard(reversedGraph, component);

			// 3.4 Group reversed components
			var visitedComponents = new HashSet<string>();
			var reversedComponentsHash = new HashSet<string>(reversedComponents.Select(c => CS(c.Id)));
			var componentsHash = new HashSet<string>(matchedComponents.Select(c => CS(c.Id)));
			var componentsUnderStudy = new Stack<string>();
			var groups = new List<HashSet<string>>();
			HashSet<string> groupForwardHash = null; // Used to reduce unnecesary visits, if node is visited for one component in the group it shouldn't be visited more
			HashSet<string> groupBackwardHash = null; // Used to reduce unnecesary visits, if node is visited for one component in the group it shouldn't be visited more
			foreach (WorkflowComponent component in reversedComponents)
			{
				// 3.4.1. If the component has not been studied start studie and prepare a gropup for the component
				if (!visitedComponents.Contains(CS(component.Id)))
				{
					groups.Add(new HashSet<string>());
					groupForwardHash = new HashSet<string>();
					groupBackwardHash = new HashSet<string>();
					componentsUnderStudy.Push(CS(component.Id));
				}

				while (componentsUnderStudy.Count > 0)
				{
					// 3.4.2. Get the component under study to investigate its dependencies
					string componentUnderStudy = componentsUnderStudy.Pop();

					if (visitedComponents.Contains(componentUnderStudy))
						continue;

					// 3.4.3. Group with adajacent reversed components
					HashSet<string> adjacent = reversedLinksGraph.DepthFirstSearch(componentUnderStudy);
					foreach (string adj in adjacent)
						visitedComponents.Add(adj);

					// If there is more than one component joint by non-reversible variables
					if (adjacent.Where(a => reversedComponentsHash.Contains(a)).Count() > 1)
					{
						// 3.4.4. Extend with others dependencies
						HashSet<string> affectedForward = forwardGraph.DepthFirstSearch(adjacent, groupForwardHash);
						HashSet<string> affectedBackward = reversedGraph.DepthFirstSearch(adjacent, groupBackwardHash);
						affectedForward.IntersectWith(affectedBackward);
						adjacent.UnionWith(affectedForward);
					}

					// 3.4.5. If dependencies include a new reversed model push to the stack to be studied
					foreach (string adj in adjacent)
					{
						// If a component is a reversed component and hasn't been added yet
						if (!visitedComponents.Contains(adj) && reversedComponentsHash.Contains(adj))
							componentsUnderStudy.Push(adj);
						else
							visitedComponents.Add(adj);

						// Add to the group of dependent model from the first reversed model under study
						if (componentsHash.Contains(adj))
							groups.Last().Add(adj);
					}
				}

			}

			// 3.5 Create Globally Reversed Workflows for each group
			var globalReversedComponents = new List<WorkflowComponent>();
			var componentsDict = matchedComponents.ToDictionary(c => CS(c.Id));
			foreach (HashSet<string> group in groups)
			{
				if (group.Count > 1)
				{
					var groupComponents = group.Select(c => componentsDict[c]).ToList(); // new List<WorkflowComponent>();
					List<Data> groupData = groupComponents.GetAllData();
					var (groupInputs, groupOutputs, _) = groupComponents.GetInputsOutputsStatus(groupData);

					GlobalReversalMode globalMode = (mode == ReversalMode.GroupNonReversibleOnly)
						? GlobalReversalMode.ReverseModelsWhenReversibleVariables
						: GlobalReversalMode.NoReversedModels;
					string globalName = WorkflowGlobal.GetGlobalWorkflowName(name, components, groupComponents);
					Workflow globalWorkflow = ScheduleWorkflowGlobal(globalName, "", groupInputs, groupOutputs, groupComponents, globalMode);

					globalReversedComponents.Add(globalWorkflow);
				}
				else
				{
					// If the group consists of one component only, it is already a reversed model and does not have any unfeasible reversal
					globalReversedComponents.Add(componentsDict[group.First()]);
				}
			}

			// 3.6. Add the rest of components
			globalReversedComponents.AddRange(matchedComponents.Where(c => !visitedComponents.Contains(CS(c.Id))));

			return globalReversedComponents;
		}

		private static void ReverseComponents(string name, MatchingDictionary outputsDict, List<WorkflowComponent> workflowComponents, out List<WorkflowComponent> matchedComponents, out List<WorkflowComponent> reversedComponents)
		{
			matchedComponents = new List<WorkflowComponent>();
			reversedComponents = new List<WorkflowComponent>();
			foreach (WorkflowComponent component in workflowComponents)
			{
				WorkflowComponent componentToAdd = component;

				// 2.1 Check if the inputs correspont to the default ones, if not create workflow for reversal
				if (component.IsReversed(outputsDict[component.Id]))
				{
					// 2.2 Classify into inputs and outputs
					HashSet<string> componentOuts = outputsDict[component.Id];
					(List<Data> outpts, List<Data> inpts) = 
						component.GetAllData().Classify(d => componentOuts.Contains(d.Id));

					if (component is Model model) //(modelObjects[nrow] is cModel)
					{
						componentToAdd = model.Reverse(inpts, outpts);
					}
					else if (component is Workflow workflow)
					{
						componentToAdd = ScheduleWorkflowGlobal($"{name}#Global#{workflow.Id}", "", inpts, outpts, workflow.Components, GlobalReversalMode.NoReversedModels);
					}

					reversedComponents.Add(componentToAdd);
				}

				matchedComponents.Add(componentToAdd);
			}
		}

		private static MatchingDictionary MatchModelsWithVariables(List<Data> inputs, List<Data> allData, List<WorkflowComponent> workflowComponents)
		{
			(MatchingDictionary outputsDict, MaximumMatchings2 matches) = FindAllMatches(inputs, allData, workflowComponents);

			if (matches.OverConstrainedModels.Count > 0)
				throw new ArgumentException($"The inputs conbination is not valid as the following models are overconstraint:\r\n\t" +
					matches.OverConstrainedModels.Aggregate((total, last) => total += "\r\n\t" + last) + "\r\n");

			Matching matching = matches.OrderedFilteredMaximumMatchings.FirstOrDefault()
				?? throw new NullReferenceException("No suitable matchings were found");

			outputsDict.CompleteWithMatching(matching);

			return outputsDict;
		}

		private static (MatchingDictionary completeMatching, MaximumMatchings2 matches) FindAllMatches(List<Data> inputs, List<Data> allData, List<WorkflowComponent> workflowComponents)
		{
			var inputsHash = new HashSet<string>(inputs.Select(d => d.Id));
			var outputsHash = new HashSet<string>();

			// 1.0. Check for non-reversible variables that are either only input or output
			var nonReversibleData = allData.Where(d => !(d is DoubleData)).ToList();
			Dictionary<string, IOStatus> nonReversibleStatus = workflowComponents.GetDataStatus(nonReversibleData);

			//var collisionDictionary = new Dictionary<string, char>();
			//foreach (var component in workflowComponents)
			//{
			//	foreach (var input in component.ModelDataInputs)
			//	{
			//		if (!(input is DoubleData))
			//		{
			//			string inName = input.Id;
			//			if (collisionDictionary.ContainsKey(inName))
			//				collisionDictionary[inName] = 'b';
			//			else
			//				collisionDictionary[inName] = 'i';
			//		}
			//	}

			//	foreach (var output in component.ModelDataOutputs)
			//	{
			//		if (!(output is DoubleData))
			//		{
			//			string outName = output.Id;
			//			if (collisionDictionary.ContainsKey(outName))
			//				collisionDictionary[outName] = 'b';
			//			else
			//				collisionDictionary[outName] = 'o';
			//		}
			//	}
			//}

			foreach (string data in nonReversibleStatus.Keys)
			{
				if (nonReversibleStatus[data] == IOStatus.Input)
					inputsHash.Add(data);
				else if (nonReversibleStatus[data] == IOStatus.Output)
					outputsHash.Add(data);
			}


			var bipartite = new BipartiteGraph();
			Primes.Reset();
			var outputsDict = new MatchingDictionary(workflowComponents.Select(c => c.Id));

			// 1.1 Add Nodes for the Variables
			// Filter-out the selected inputs and outputs, as they don't belong to the bipartite graph
			foreach (Data data in allData.Where(d => !inputsHash.Contains(d.Id) && !outputsHash.Contains(d.Id)))
				bipartite.AddNode(data.Id, GraphNode.Type.Type1);

			// 1.2 Add Nodes for the Models, and edges between Variables and Nodes
			foreach (WorkflowComponent component in workflowComponents)
			{
				int uniqueNonReversibleOutputs = 0;

				outputsDict.Add(component.Id, new HashSet<string>());

				bipartite.AddNode(CS(component.Id), GraphNode.Type.Type2);
				GraphNode modelNode = bipartite.GetNode(CS(component.Id));

				// Filter-out the selected inputs, as they don't belong to the bipartite graph
				foreach (Data data in component.ModelDataInputs.Where(d => !inputsHash.Contains(d.Id)))
				{
					if (outputsHash.Contains(data.Id))
					{
						// Shouldn't arrive here
						uniqueNonReversibleOutputs++;
						outputsDict[component.Id].Add(data.Id);
					}
					else
					{
						bipartite.AddDirectedEdge(data.Id, modelNode, Primes.Next(), 1);
					}
				}

				// Filter-out the selected inputs, as they don't belong to the bipartite graph
				foreach (Data data in component.ModelDataOutputs.Where(d => !inputsHash.Contains(d.Id)))
				{
					if (outputsHash.Contains(data.Id))
					{
						uniqueNonReversibleOutputs++;
						outputsDict[component.Id].Add(data.Id);
					}
					else
					{
						bipartite.AddDirectedEdge(data.Id, modelNode, Primes.Next(), 0);
					}
				}
				// How many output the model need
				if (component.ModelDataOutputs.Count - uniqueNonReversibleOutputs > 1)
					bipartite.modelAndItsOutputs.Add(CS(component.Id), component.ModelDataOutputs.Count - uniqueNonReversibleOutputs); //signle output models are not stored
				else if (component.ModelDataOutputs.Count - uniqueNonReversibleOutputs == 0)
					bipartite.Remove(CS(component.Id));
			}

			// 1.3 Associate matchings to each model, maping models to set of outputs
			var matches = new MaximumMatchings2(bipartite, getAllMatchings: true);

			return (outputsDict, matches);
		}

		private static string DeserializeModel(string modelString)
		{
			const string duplicateStart = "Duplicate_";
			int length = duplicateStart.Length;
			if (modelString.StartsWith(duplicateStart))
			{
				modelString = modelString.Substring(length);
			}
			return modelString.Split('#').First();
		}

		private static void DeserializeMatching(string match, out string from, out string to)
		{
			string[] fromto = match.Split('-');
			from = fromto.First();
			const string duplicateStart = "Duplicate_";
			int length = duplicateStart.Length;
			if (from.StartsWith(duplicateStart))
			{
				from = from.Substring(length);
				from = from.Split('#').First();
			}

			to = fromto.Last();
			if (to.StartsWith(duplicateStart))
			{
				to = to.Substring(length);
				to = to.Split('#').First();
			}

			from = from.TrimEnd('#');
			to = to.TrimEnd('#');
		}

		private static List<WorkflowComponent> ClusterStronglyConnectedComponents(string name, List<Data> allData, List<WorkflowComponent> components)
		{
			// 4.1. Graph with reversed components grouped into global elements
			var globalReversedGraph = new Graph();
			foreach (Data data in allData)
				globalReversedGraph.AddVertex(data.Id);
			foreach (WorkflowComponent component in components)
				AddComponentAndEdgesForward(globalReversedGraph, component);

			// 4.2. Obtain Strongly connected components and group them 
			var clusteredComponents = new List<WorkflowComponent>();
			var componentsDict = components.ToDictionary(c => CS(c.Id));
			List<List<string>> resultingGraphSCCs = globalReversedGraph.StronglyConnectedComponents();
			foreach (List<string> scc in resultingGraphSCCs)
			{
				if (scc.Count == 1)
				{
					if (componentsDict.ContainsKey(scc.First()))
						clusteredComponents.Add(componentsDict[scc.First()]);
				}
				else
				{
					var sccComponents = scc.Where(c => componentsDict.ContainsKey(c)).Select(c => componentsDict[c]).ToList();
					List<Data> sccData = sccComponents.GetAllData();
					sccComponents.GetInputsOutputsStatus(sccData, out List<Data> sccInputs, out List<Data> sccOutputs);
					clusteredComponents.Add(ScheduleWorkflowSCC(name, "", sccInputs, sccOutputs, components, sccComponents));
				}
			}

			return clusteredComponents;
		}

		private static List<WorkflowComponent> ScheduleAcyclic(List<Data> allData, List<WorkflowComponent> components)
		{
			// 5.1. Graph with clustered SCCS
			var definitiveGraph = new Graph();
			foreach (Data data in allData)
				definitiveGraph.AddVertex(data.Id);
			foreach (WorkflowComponent component in components)
				AddComponentAndEdgesForward(definitiveGraph, component);

			// 5.2. Topological sort for scheduling the components
			var componentsDict = components.ToDictionary(c => CS(c.Id));
			List<string> topologicalSort = definitiveGraph.TopologicalSort();
			IEnumerable<string> scheduled = topologicalSort.Where(v => componentsDict.ContainsKey(v)); //.Select(v => componentDict[v]).ToList();

			var scheduledComponents = new List<WorkflowComponent>();
			foreach (string comp in scheduled)
				scheduledComponents.Add(componentsDict[comp]);

			return scheduledComponents;
		}

		private static void AddComponentAndEdgesForward(Graph graph, WorkflowComponent component)
		{
			graph.AddVertex(CS(component.Id));
			foreach (Data data in component.ModelDataInputs)
				graph.AddEdge(data.Id, CS(component.Id));
			foreach (Data data in component.ModelDataOutputs)
				graph.AddEdge(CS(component.Id), data.Id);
		}

		private static void AddComponentAndEdgesBackWard(Graph graph, WorkflowComponent component)
		{
			graph.AddVertex(CS(component.Id));
			foreach (Data data in component.ModelDataInputs)
				graph.AddEdge(CS(component.Id), data.Id);
			foreach (Data data in component.ModelDataOutputs)
				graph.AddEdge(data.Id, CS(component.Id));
		}

		private static void AddComponentAndEdgesBoth(Graph graph, WorkflowComponent component, ReversalMode mode)
		{
			graph.AddVertex(CS(component.Id));
			var reversable = component as IReversableWorkflow;
			foreach (Data data in reversable.ReversedInputs)
			{
				if (mode == ReversalMode.GroupNonReversibleOnly && !(data is DoubleData))
				{
					graph.AddEdge(data.Id, CS(component.Id));
					graph.AddEdge(CS(component.Id), data.Id);
				}
				else if (mode == ReversalMode.GroupBoth)
				{
					graph.AddEdge(data.Id, CS(component.Id));
					graph.AddEdge(CS(component.Id), data.Id);
				}

			}
			foreach (Data data in reversable.ReversedOutputs)
			{
				if (mode == ReversalMode.GroupNonReversibleOnly && !(data is DoubleData))
				{
					graph.AddEdge(CS(component.Id), data.Id);
					graph.AddEdge(data.Id, CS(component.Id));
				}
				else if (mode == ReversalMode.GroupBoth)
				{
					graph.AddEdge(data.Id, CS(component.Id));
					graph.AddEdge(CS(component.Id), data.Id);
				}
			}
		}

		private static string CS(string name) => $"{name}#";
		private static string CSI(string name) => name.TrimEnd('#');

		private static List<ISolver> DeafaultSolvers()
		{
			var opts = new NewtonOptions() { MaxIterations = 20, DerivativeStep = new double[] { 0.01 } };
			var solver = new NewtonSolver(opts);
			return new List<ISolver>() { solver };
		}

		private static List<ISolver> DeafaultSolversSCC()
		{
			var solver = new FixedPointSolver() { MaxEvals = 20 };
			var opts = new NewtonOptions() { MaxIterations = 20, DerivativeStep = new double[] { 0.01 } };
			var solver2 = new NewtonSolver(opts);
			return new List<ISolver>() { solver, solver2 };
		}
	}

	public enum ReversalMode
	{
		GroupNonReversibleOnly, GroupBoth, Global, Legacy
	}

	public enum GlobalReversalMode
	{
		NoReversedModels, ReverseModelsWhenReversibleVariables, Global
	}

	public class MatchingDictionary : Dictionary<string, HashSet<string>>
	{
		private readonly HashSet<string> validComponentsHash;

		public MatchingDictionary(IEnumerable<string> validComponents)
		{
			validComponentsHash = new HashSet<string>(validComponents);
		}

		public MatchingDictionary(MatchingDictionary matchingDictionary)
		{
			foreach (KeyValuePair<string, HashSet<string>> kvp in matchingDictionary)
			{
				this[kvp.Key] = kvp.Value;
			}

			validComponentsHash = new HashSet<string>(matchingDictionary.validComponentsHash);
		}

		public void CompleteWithMatching(Matching matching)
		{
			foreach (var kvp in matching.Pairs)
			{
				string model = DeserializeModel(kvp.Key);
				HashSet<string> modelOut = this[model];

				if (validComponentsHash.Contains(model))
				{
					foreach (string variable in kvp.Value)
					{
						modelOut.Add(variable);
					}
				}
			}
		}

		private static string DeserializeModel(string modelString)
		{
			const string duplicateStart = "Duplicate_";
			int length = duplicateStart.Length;
			if (modelString.StartsWith(duplicateStart))
			{
				modelString = modelString.Substring(length);
			}
			return modelString.Split('#').First();
		}
	}
}
