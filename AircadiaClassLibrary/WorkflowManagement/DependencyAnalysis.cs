/**************************************************************************************************
AIRCADIA - Multiphysics model-based design tool                                                   *
Engineering Design Group                                                                          *
Department of Aerospace Engineering, School of Engineering, Cranfield University                  *
Cranfield, Bedfordshire, MK43 0AL, UK                                                             *
/**************************************************************************************************/

using Aircadia.ObjectModel.DataObjects;
using Aircadia.ObjectModel.Models;
using Aircadia.Services.Serializers;
using Aircadia.Utilities;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Aircadia.WorkflowManagement
{
	public class MatrixBasedDependencyAnalysis : IDependencyAnalysis
	{
		[SerializeEnumerable("Models", "Model")]
		private List<WorkflowComponent> ModelsList { get; }

		private Matrix<double> IM { get; }

		private Matrix<double> DSM { get; }
		private Matrix<double> DSMT { get; }
		private Dictionary<string, int> ModelIndices { get; }
		private WorkflowComponent[] Models { get; }

		private Matrix<double> VBDSM { get; }
		private Matrix<double> VBDSMT { get; }
		private Dictionary<string, int> DataIndices { get; }
		private Data[] Data { get; }

		[DeserializeConstructor]
		public MatrixBasedDependencyAnalysis(IEnumerable<WorkflowComponent> models)
		{
			ModelsList = models?.ToList() ?? throw new NullReferenceException();

			List<Data> data = models.GetAllData();

			//Model-Based Incidence Matrix
			IM = LibishMatrixCalculators.GetIncidenceMatrix(models.ToList(), data);
			// Model-Based DSM for model forward tracing
			DSM = CreateDSM(IM);
			// Model-Based transposed DSM for model backward tracing
			DSMT = DSM.Transpose();

			Models = models.ToArray();
			ModelIndices = Models.Select((m, i) => new { m.Id, Index = i }).ToDictionary(m => m.Id, m => m.Index);

			// Variable-Based Incidence Matrix
			Matrix<double> VBIM = IM.Transpose();
			// Variable-Based DSM for data forward tracing
			VBDSM = CreateDSM(VBIM);
			// Variable-Based transposed DSM for data backward tracing
			VBDSMT = VBDSM.Transpose();

			DataIndices = LibishMatrixCalculators.GetDataIndices(data);
			var allData = data.ToDictionary(d => d.Id);
			Data = new Data[DataIndices.Count];
			foreach (KeyValuePair<string, int> kvp in DataIndices)
				Data[kvp.Value] = allData[kvp.Key];
		}

		public HashSet<Data> BackwardTrace(Data data) => Hash(Tracer(VBDSMT, Index(data)).Select(i => Data[i]));
		public HashSet<Data> BackwardTrace(Data data, IEnumerable<Data> independentVars) 
			=> Hash(BackwardTrace(data).Intersect(independentVars ?? throw new ArgumentNullException(nameof(independentVars))));
		public HashSet<Data> ForwardTrace(Data data) => Hash(Tracer(VBDSM, Index(data)).Select(i => Data[i]));

		public HashSet<WorkflowComponent> BackwardTrace(WorkflowComponent component) => Hash(Tracer(DSM, Index(component)).Select(i => Models[i]));
		public HashSet<WorkflowComponent> ForwardTrace(WorkflowComponent component) => Hash(Tracer(DSMT, Index(component)).Select(i => Models[i]));

		public WorkflowComponent Provider(Data data)
		{
			List<int> parents = IM.Column(DataIndices[data.Id]).FindLocations(LibishScheduler.OUT);
			if (parents.Count == 1)
			{
				return Models[parents.FirstOrDefault()];
			}
			else
			{
				return null;
			}
		}

		private HashSet<int> Tracer(Matrix<double> VBDSM, int varNoToTrace)
		{
			int start = varNoToTrace;

			// Number of models
			int rows = VBDSM.RowCount;
			// Number of variables
			int columns = VBDSM.ColumnCount;

			//Direct links
			var links = new HashSet<int> { start };

			var linksToBeTraced = new HashSet<int>() { start };
			while (linksToBeTraced.Count > 0)
			{
				// updated list of next variables to search for.
				var newLinks = new HashSet<int>();

				foreach (int link in linksToBeTraced)
				{
					for (int i = 0; i < rows; i++)
					{
						//if variable is already in global links then don't add (i.e. avoid loop)
						if (VBDSM[i, link] != LibishScheduler.NOTHING && !links.Contains(i))
						{
							links.Add(i);
							newLinks.Add(i);
						}
					}
				}

				linksToBeTraced.Clear();
				linksToBeTraced.UnionWith(newLinks);
			}

			return links;
		}

		private Matrix<double> CreateDSM(Matrix<double> IM) => LibishMatrixCalculators.IncidenceMatrixToDesignStructureMatrix(IM);

		private int Index(Data data)
		{
			if (data == null)
			{
				throw new ArgumentNullException(nameof(data));
			}

			try
			{
				return DataIndices[data.Id];
			}
			catch (KeyNotFoundException ex)
			{

				throw new KeyNotFoundException($"Variable '{data.Id}' is not present in the workkflow, dependency analysis it's not possible", ex);
			}
		}

		private int Index(WorkflowComponent component)
		{
			if (component == null)
			{
				throw new ArgumentNullException(nameof(component));
			}

			try
			{
				return ModelIndices[component.Id];
			}
			catch (KeyNotFoundException ex)
			{

				throw new KeyNotFoundException($"Component '{component.Id}' is not present in the workkflow, dependency analysis it's not possible", ex);
			}
		}

		private HashSet<T> Hash<T>(IEnumerable<T> enumerable) => new HashSet<T>(enumerable);
	}

	public class GraphBasedDependencyAnalysis : IDependencyAnalysis
	{
		[SerializeEnumerable("Components", "Component")]
		private List<WorkflowComponent> Components { get; }

		private readonly BipartiteGraph<WorkflowComponent, Data> forwardGraph;
		private readonly BipartiteGraph<WorkflowComponent, Data> backwardGraph;

		[DeserializeConstructor]
		public GraphBasedDependencyAnalysis(IEnumerable<WorkflowComponent> components)
		{
			Components = components?.ToList() ?? throw new NullReferenceException();
			List<Data> allData = components.GetAllData();
			forwardGraph = GraphBuilder.BipartiteFromTwoSets(components, allData, c => c.ModelDataOutputs, c => c.ModelDataInputs);
			backwardGraph = forwardGraph.Transpose();
		}

		public HashSet<Data> BackwardTrace(Data data) => Hash(backwardGraph.DepthFirstSearch<Data>(data));
		public HashSet<Data> BackwardTrace(Data data, IEnumerable<Data> independentVars) 
			=> Hash(backwardGraph.DepthFirstSearch<Data>(data).Intersect(independentVars ?? throw new ArgumentNullException(nameof(independentVars))));
		public HashSet<WorkflowComponent> BackwardTrace(WorkflowComponent component) => Hash(backwardGraph.DepthFirstSearch<WorkflowComponent>(component));
		public HashSet<Data> ForwardTrace(Data data) => Hash(forwardGraph.DepthFirstSearch<Data>(data));
		public HashSet<WorkflowComponent> ForwardTrace(WorkflowComponent component) => Hash(forwardGraph.DepthFirstSearch<WorkflowComponent>(component));
		public WorkflowComponent Provider(Data data) => backwardGraph.GetChild(data);

		private HashSet<T> Hash<T>(IEnumerable<T> enumerable) => new HashSet<T>(enumerable);
	}
}
