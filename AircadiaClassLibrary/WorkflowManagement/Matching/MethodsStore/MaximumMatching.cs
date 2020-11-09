using Aircadia.ObjectModel;
using Aircadia.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Aircadia.WorkflowManagement
{
	public static class MaximumMatching
	{
		public static EdgeList Get(BipartiteGraph bipartiteGraph)
		{
			NodeList models = bipartiteGraph.ModelsSet;
			if (models is null)
			{
				throw new ArgumentException("Graph has no models", nameof(bipartiteGraph));
			}

			NodeList variables = bipartiteGraph.VariablesSet;
			if (variables is null)
			{
				throw new ArgumentException("Graph has no variables", nameof(bipartiteGraph));
			}

			//Assigning int values to each node in the graph starting from 0.
			var modNodeIntMap = models.Select((m, i) => new KeyValuePair<string, int>(m.Value, i)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

			//Creating Adjacency List
			var adjacencyList = new List<int[]>(variables.Count);
			foreach (GraphNode variable in variables)
			{
				adjacencyList.Add(variable.Neighbors.Select(n => modNodeIntMap[n.Value]).ToArray());
			}

			// Get Match
			IList<int> match = new HopcroftKarpClass(adjacencyList, models.Count).GetMatching();

			// Format the match
			var MatchingEdgeList = new EdgeList();
			int idx = 0;
			foreach (Node variable in variables)
			{
				int matchedModelIndex = match[idx];
				if (matchedModelIndex != -1)
				{
					string modelValue = modNodeIntMap.FirstOrDefault(x => x.Value == matchedModelIndex).Key;
					//matching edge list contains edges having direction from
					MatchingEdgeList.Add(bipartiteGraph.Edges.FindByNodes(variable, bipartiteGraph.GetNode(modelValue)));
					// 'variables set' to 'model set'
					idx++;
				}
				else
				{
					idx++;/*variable associated at that location is unmatched*///Implement to store unmatched variable information if required
				}
			}

			return MatchingEdgeList;
		}

		public static List<(T1, T2)> Get<T1, T2>(BipartiteGraph<T1, T2> bipartiteGraph)
			where T1 : INamedComponent
			where T2 : INamedComponent
		{
			var vertices1 = bipartiteGraph.Payloads1.Values.ToList();
			if (vertices1 is null)
			{
				throw new ArgumentException($"Graph has no vertices of type {typeof(T1).Name}", nameof(bipartiteGraph));
			}

			var vertices2 = bipartiteGraph.Payloads2.Values.ToList();
			if (vertices2 is null)
			{
				throw new ArgumentException($"Graph has no vertices of type {typeof(T2).Name}", nameof(bipartiteGraph));
			}

			//Assigning int values to each node in the graph starting from 0.
			Dictionary<T1, int> vertex1toInt = vertices1.Select((v, i) => new KeyValuePair<T1, int>(v, i)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
			Dictionary<int, T1> intToVertex1 = vertex1toInt.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);

			//Creating Adjacency List
			var adjacencyList = new List<int[]>(vertices2.Count);
			foreach (T2 vertex2 in vertices2)
			{
				adjacencyList.Add(bipartiteGraph.GetChildren(t2Object: vertex2).Select(v => vertex1toInt[v]).ToArray());
			}

			// Get Match
			IList<int> match = new HopcroftKarpClass(adjacencyList, vertices1.Count).GetMatching();

			// Format the match
			var MatchingEdgeList = new List<(T1, T2)>();
			for (int i = 0; i < vertices2.Count; i++)
			{
				int matchedModelIndex = match[i];
				if (matchedModelIndex != -1)
				{
					//matching edge list contains edges having direction from
					T1 vertex1 = intToVertex1[matchedModelIndex];
					MatchingEdgeList.Add((vertex1, vertices2[i]));
				}
			}

			return MatchingEdgeList;
		}
	}
}
