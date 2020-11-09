using Aircadia.ObjectModel.DataObjects;
using Aircadia.ObjectModel.Models;
using Aircadia.ObjectModel.Workflows;
using Aircadia.Services.Serializers;
using Aircadia.Utilities;
using Aircadia.WorkflowManagement;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Aircadia.ObjectModel.Studies.Missions
{
	public class Mission
	{
		readonly AircadiaProject project = AircadiaProject.Instance;

		public List<WorkflowComponent> Components { get; }

		public List<Data> Data { get; }
		public HashSet<Data> VariableData { get; }
		public HashSet<Data> ConstantData { get; }

		[SerializeEnumerable("Segments")]
		public List<MissionSegment> Segments => segments.Values.ToList();

		[SerializeDictionary("Assignments", "Assignment")]
		public Dictionary<string, string> Assignments { get
			{
				var asssignments = new Dictionary<string, string>();
				foreach (var component in componentsSegmentsGraph.Payloads1.Values)
				{
					var segments = componentsSegmentsGraph.GetChildren(component)
						.Aggregate(String.Empty, (t, s) => t += $"{s.Name} ", t => t.TrimEnd(' '));
					asssignments.Add(component.Name, segments);
				} 
				return asssignments;
			}
		}

		[Serialize("Workflow", SerializationType.Reference)]
		public WorkflowComponent Workflow { get; }

	    private readonly BipartiteGraph<WorkflowComponent, MissionSegment> componentsSegmentsGraph = new BipartiteGraph<WorkflowComponent, MissionSegment>();

		private readonly BipartiteGraph<Data, MissionSegment> dataSegmentsGraph = new BipartiteGraph<Data, MissionSegment>();

		private readonly Dictionary<string, MissionSegment> segments = new Dictionary<string, MissionSegment>();

		private readonly IDependencyAnalysis dependencyAnalysis;

		private readonly List<Data> independentVariables;

		public Graph SegmentsGraph { get;  }
		

		[DeserializeConstructor]
		public Mission(WorkflowComponent workflow, List<MissionSegment> segments, Dictionary<string, string> assignments) : this(workflow)
		{
			var sortedComponents = Workflow is Workflow wf
				? (wf.ScheduledComponents as IEnumerable<WorkflowComponent>).Reverse().ToArray()
				: new[] { Workflow };

			var segmentsDict = segments.ToDictionary(s => s.Name);

			foreach (WorkflowComponent component in sortedComponents)
			{
				if (assignments.TryGetValue(component.Name, out string assignment))
				{
					foreach (MissionSegment segment in GetSegments(assignment))
					{
						AddSegment(segment, component);

						// Remove dependencies
						foreach (var dependent in DependencyComponents(component))
						{
							RemoveAssignment(segment, dependent);
						}
					}
				}
				else
				{
					throw new KeyNotFoundException($"The component '{component.Name}' is not present in the assignments");
				}

				List<MissionSegment> GetSegments(string assignmentString)
				{
					var missionSegments = new List<MissionSegment>();
					foreach (var segmentString in assignmentString.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
					{
						if (segmentsDict.TryGetValue(segmentString, out MissionSegment segment))
						{
							missionSegments.Add(segment);
						}
						else
						{
							throw new KeyNotFoundException($"The segment '{segmentString}' is not present in the assignments of '{component.Name}'");
						}
					}
					return missionSegments;
				}

				void RemoveAssignment(MissionSegment segment, WorkflowComponent dependent)
				{
					if (assignments.TryGetValue(dependent.Name, out string assgnment))
					{
						assignments[dependent.Name] = assgnment.Replace(segment.Name, String.Empty);
					}
					else
					{
						throw new KeyNotFoundException($"The component '{component.Name}' is not present in the assignments");
					}
				}
			}
		}

		public Mission(WorkflowComponent workflow)
		{
			this.Workflow = workflow;
			Components = new List<WorkflowComponent>();
			if (workflow is Model model)
			{
				Components.Add(model);
			}
			else if (workflow is Workflow wf)
			{
				Components.AddRange(wf.ScheduledComponents);
			}

			dependencyAnalysis = new GraphBasedDependencyAnalysis(Components);
			componentsSegmentsGraph.AddVertices(Components);

			Data = Components.GetAllData();
			VariableData = new HashSet<Data>(Data);
			ConstantData = new HashSet<Data>();
			dataSegmentsGraph.AddVertices(Data);
			independentVariables = workflow.ModelDataInputs;

			SegmentsGraph = new Graph();
		}

		public bool AddSegment(MissionSegment segment, WorkflowComponent component)
		{
			if (!segments.ContainsKey(segment.Name))
			{
				segments[segment.Name] = segment;
			}
			else
			{
				segments[segment.Name].AddRange(segment.Parameters);
			}

			segment = segments[segment.Name];
			//foreach (MissionParameter parameter in segment.Parameters)
			//{
			//	parameters[parameter.Name] = parameter;
			//}

			// Ensure all variables have been defined
			var inSegment = new HashSet<string>(segment.Parameters.Select(p => p.Data).GetNames());
			var dependencies = DependencyData(component, onlyIndependent: false).GetNames();
			if (dependencies.Intersect(inSegment).Count() != dependencies.Count())
			{
				return false;
			}

			// Add Segment to the components-segments and data-segments graphs
			componentsSegmentsGraph.AddVertex(segment);
			dataSegmentsGraph.AddVertex(segment);

			List<WorkflowComponent> components = DependencyComponentsHash(component, false);
			foreach (WorkflowComponent dependency in components)
			{
				componentsSegmentsGraph.AddUndirectedEdge(dependency, segment);
			}

			foreach (Data dependency in components.GetAllData())
			{
				dataSegmentsGraph.AddUndirectedEdge(dependency, segment);
			}

			return true;
		}

		public bool RemoveSegment(MissionSegment segment, WorkflowComponent component)
		{
			if (!segments.TryGetValue(segment.Name, out segment))
			{
				return true;
			}

			// Remove Segment to the components-segments and data-segments graphs
			HashSet<WorkflowComponent> components = dependencyAnalysis.ForwardTrace(component);
			foreach (WorkflowComponent dependency in components)
			{
				componentsSegmentsGraph.RemoveUndirectedEdge(dependency, segment);
			}
			components.Remove(component);

			var variables = new HashSet<Data>(component.ModelDataInputs.Intersect(independentVariables).Union(components.GetAllData()));
			foreach (Data dependency in variables)
			{
				dataSegmentsGraph.RemoveUndirectedEdge(dependency, segment);
			}

			segment.RemoveRange(segment.Parameters.Where(p => variables.Contains(p.Data)));

			if (segment.Parameters.Count == 0)
			{
				componentsSegmentsGraph.RemoveVertex(segment);
				dataSegmentsGraph.RemoveVertex(segment);
				return true;
			}

			return false;
		}

		public List<Data> UnnasignedData(string segmentName, WorkflowComponent component, bool onlyIndependent = true) => UnnasignedData(segmentName, Child(component), onlyIndependent);
	
		public List<Data> UnnasignedData(string segmentName, Data data, bool onlyIndependent = true)
		{
			HashSet<Data> dependencies = DependencyDataHash(data, onlyIndependent);

			if (!onlyIndependent)
			{
				dependencies.UnionWith(Provider(data).ModelDataOutputs);
			}

			// If the segment is present filter out existing data
			if (segments.TryGetValue(segmentName, out MissionSegment segment))
			{
				IEnumerable<Data> existing = segment.Parameters.Select(p => p.Data);
				dependencies.ExceptWith(existing);

			}
			return dependencies.ToList();
		}

		public List<WorkflowComponent> UnnasignedComponents()
		{
			var components = Components.ToDictionary(c => c.Name);
			return componentsSegmentsGraph.Vertices1.Where(v => v.Adjacent.Count == 0).Select(v => components[v.Id.Split(':').Last()]).ToList();
		}

		public bool IsComplete() => UnnasignedComponents().Count == 0;

		public MissionSegment Segment(string name) => segments[name];

		// Traceability
		public List<MissionSegment> MissionSegments(Data data)
		{
			if (data == null)
			{
				return new List<MissionSegment>();
			}
			return dataSegmentsGraph.GetChildren(data);
		}

		public List<MissionSegment> MissionSegments(WorkflowComponent component)
		{
			if (component == null)
			{
				return new List<MissionSegment>();
			}
			return componentsSegmentsGraph.GetChildren(component);
		}

		public List<WorkflowComponent> ComponentsRelatedToSegment(MissionSegment segment) => componentsSegmentsGraph.GetChildren(segment);

		public List<Data> DataRelatedToSegment(MissionSegment segment) => dataSegmentsGraph.GetChildren(segment);

		public List<WorkflowComponent> DependencyComponents(Data data) => DependencyComponentsHash(Provider(data), false);

		public List<WorkflowComponent> DependencyComponents(WorkflowComponent component) => DependencyComponentsHash(component, true);

		private List<WorkflowComponent> DependencyComponentsHash(WorkflowComponent component, bool removeComponent)
		{
			if (component == null)
			{
				return new List<WorkflowComponent>();
			}

			HashSet<WorkflowComponent> dependencies = dependencyAnalysis.BackwardTrace(component);
			if (removeComponent)
			{
				dependencies.Remove(component);
			}
			return dependencies.ToList();
		}

		public List<Data> DependencyData(Data data, bool onlyIndependent = true)
		{
			HashSet<Data> dependencies = DependencyDataHash(data, onlyIndependent);
			return dependencies.ToList();
		}

		public List<Data> DependencyData(WorkflowComponent component, bool onlyIndependent = true) => DependencyData(Child(component), onlyIndependent);

		private HashSet<Data> DependencyDataHash(Data data, bool onlyIndependent)
		{
			HashSet<Data> dependencies = onlyIndependent
							? dependencies = dependencyAnalysis.BackwardTrace(data, independentVariables)
							: dependencies = dependencyAnalysis.BackwardTrace(data);
			dependencies.Remove(data);
			return dependencies;
		}

		public WorkflowComponent Provider(Data data) => dependencyAnalysis.Provider(data);

		public Data Child(WorkflowComponent component) => component.ModelDataOutputs.First();

		// Variables / constants
		public void SetDataAsConstant(Data data) => Transfer(data, VariableData, ConstantData);

		public void SetDataAsVariable(Data data) => Transfer(data, ConstantData, VariableData);

		private void Transfer(Data data, HashSet<Data> from, HashSet<Data> to)
		{
			from.Remove(data);
			to.Add(data);
		}

		public void LinkSegments(IEnumerable<MissionSegment> parents, MissionSegment child)
		{
			foreach (var parent in parents)
			{
				LinkSegments(parent, child);
			}
		}
		public void LinkSegments(MissionSegment parent, IEnumerable<MissionSegment> children)
		{
			foreach (var child in children)
			{
				LinkSegments(parent, child);
			}
		}
		public void LinkSegments(MissionSegment parent, MissionSegment child)
		{
			bool addedParent = !SegmentsGraph.Vertices.ContainsKey(parent.Name);
			SegmentsGraph.AddVertex(parent.Name);

			bool addedChild = !SegmentsGraph.Vertices.ContainsKey(parent.Name);
			SegmentsGraph.AddVertex(child.Name);

			SegmentsGraph.AddEdge(parent.Name, child.Name);

			if (!SegmentsGraph.IsAcyclic())
			{
				SegmentsGraph.RemoveEdge(parent.Name, child.Name);
				if (addedParent)
				{
					SegmentsGraph.RemoveVertex(parent.Name);
				}
				if (addedChild)
				{
					SegmentsGraph.RemoveVertex(child.Name);
				}

				throw new ArgumentException($"Adding the edge ({parent.Name} -> {child.Name}) causes a cycle in the graph.");
			}
		}
	}
}
