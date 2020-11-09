using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;


namespace Aircadia.WorkflowManagement
{
	using MatchingList = List<string>;

	public class MaximumMatchings2
	{
		private const int MaximumRecursionLevel = 10; //~ 2 ^ 12 = 4096 graphs 
		private readonly int minimumAchievableCost;
		public BipartiteGraph G = new BipartiteGraph();
		public List<Matching> OrderedFilteredMaximumMatchings = new List<Matching>();//04102017
		public SortedSet<Matching> SortedMaximumMatchings = new SortedSet<Matching>();//04102017
		public Dictionary<string, int> edgePrims = new Dictionary<string, int>(); //stores weightages of the edges of the graph
		public MatchingList OverConstrainedModels { get; private set; } = new MatchingList();
		public MatchingList UnmappedVariables { get; private set; } = new MatchingList();
		public bool OverConstrained { get; private set; } = false;
		public int MinimumCost => SortedMaximumMatchings.Min.ReversalCost;

		public MaximumMatchings2(BipartiteGraph G, bool getAllMatchings = false)
		{
			this.G = G;
			this.G.UpdateAddingDuplicateNodes();
			edgePrims = this.G.edgeNodesCost;

			minimumAchievableCost = 0;

			ENUM_MAXIMUM_MATCHINGS(this.G.Clone(), getAllMatchings);
		}

		private void FilterDuplicateMatchings()
		{
			//stores the integers of matchings, the interger is calculated by multiplying prims of the edges of the matching
			var addedMatchings = new HashSet<BigInteger>();
			//Filtering duplicate matchings
			foreach (Matching matching in SortedMaximumMatchings)
			{
				BigInteger matchingID = matching.ID;
				if (!addedMatchings.Contains(matchingID) && matchingID != 1)
				{
					OrderedFilteredMaximumMatchings.Add(matching);
					addedMatchings.Add(matchingID);
				}

			}
		}

		private int FindOverconstrainedModelsReversalCost(MatchingList M)
		{
			int reversalCost = 0;//04102017
			OverConstrainedModels = new MatchingList();
			UnmappedVariables = new MatchingList();

			if (M != null) //after filtering the matchings from list_AllMaximumMatchings
			{
				foreach (GraphNode node in G.GetUnmatchedVertices(M)) //after filtering the matchings
				{
					string value = node.Value;
					if (node.NodeType == GraphNode.Type.Type2)
					{
						if (G.duplicateModToModel.ContainsKey(value))
						{
							OverConstrainedModels.Add(G.duplicateModToModel[value]);
						}
						else
						{
							OverConstrainedModels.Add(value);
						}
					}
					else
					{
						UnmappedVariables.Add(value);
					}
				}

				//for finding revCost of this matching
				foreach (string match in M)
				{
					string[] split = match.Split('-');
					string from = split[0];
					string to = split[1];

					Node fromNode = GetNode(from);
					Node toNode = GetNode(to);

					//Node from = this.G.GetNode();
					Edge e = GetEdge(fromNode, toNode);

					reversalCost += e.Cost2;
				}
			}

			return reversalCost;
		}

		//taken from enumeratingmatchinginbipartiteGraph class.... decide where to keep this method here or in the 'enumeratingmathcihgin...' class
		private MatchingList CycleToMatching(List<GraphNode> cycle)
		{
			var matching = new MatchingList();
			//Converts cycle into edges as strings of values in the form i.e. 'model.value - variable.value'
			//String form of the matching will be used for performing set operations on it.
			for (int i = 0; i < cycle.Count - 1; i++)
			{
				string edge;
				if (cycle[i].NodeType == GraphNode.Type.Type2)
				{
					edge = EdgeString(cycle[i].Value, cycle[i + 1].Value);
				}
				else
				{
					edge = EdgeString(cycle[i + 1].Value, cycle[i].Value);
				}
				matching.Add(edge);
			}
			return matching;
		}

		private EdgeList MatchingToEdgeList(BipartiteGraph G, MatchingList M_String)
		{
			var edgeList = new EdgeList();
			foreach (string edg in M_String)
			{
				GraphNode node1 = G.GetNode(edg.Split('-')[0]);
				GraphNode node2 = G.GetNode(edg.Split('-')[1]);
				if (G.Contains(node1, node2))
				{
					edgeList.Add(G.Edges.FindByNodes(node1, node2));
				}
				else if (G.Contains(node2, node1))
				{
					edgeList.Add(G.Edges.FindByNodes(node2, node1));
				}
				//else
				//{
				//    M_to_EdgeListM.Add(new Edge(nd1,nd2,0));
				//}
			}

			return edgeList;
		}

		//Converts matching edge list into strings of values in the form i.e. 'model.value - variable.value'
		//String form of the cycle will be used for performing set operations on it.
		private MatchingList EdgeListToMatching(EdgeList M)
		{
			var matching = new MatchingList();
			foreach (Edge edge in M)
			{
				if (edge.FromNode.NodeType == GraphNode.Type.Type2)
				{
					matching.Add(EdgeString(edge.FromNode.Value, edge.ToNode.Value));
				}
				else
				{
					matching.Add(EdgeString(edge.ToNode.Value, edge.FromNode.Value));
				}
			}
			return matching;
		}

		private void ENUM_MAXIMUM_MATCHINGS(BipartiteGraph G, bool getAllMatchings)
		{
			/*-------------------------------------------------------------------------------------------------------------------
			 * Step1: Find a maximum matching M of G and output M. 
			 * ------------------------------------------------------------------------------------------------------------------*/
			EdgeList M = MaximumMatching.Get(G); //Edges of Matching
			MatchingList matching = EdgeListToMatching(M);
			matching.Sort();

			FindOverconstrainedModelsReversalCost(matching);
			if (OverConstrainedModels.Count > 0)
			{
				OverConstrained = true;
				return;
			}

			AddMatching(matching);
			if (MinimumCost > minimumAchievableCost)
			{
				/*-------------------------------------------------------------------------------------------------------------------
				* Step2: Trim unnecessary arcs from D(G,M) by a strongly connected component decomposition algorithm.
				* ------------------------------------------------------------------------------------------------------------------*/
				//Need further action on this to trim edges after finding SCCs in a D(G,M). Here only SCCs are found. Edges are not trimmed yet.
				//Graph D_GM = Get_D_GM(G.Clone(), M); //Directed graph is obtained here with reversing edges of matchingEdges.
				//List<List<GraphNode>> sCCs = TarjanCycleDetect.DetectCycle(D_GM); 


				/*-------------------------------------------------------------------------------------------------------------------
				* Step3: Call ENUM_MAXIMUM_MATCHINGS_ITER(G, M, D(G,M))
				* ------------------------------------------------------------------------------------------------------------------*/
				if (getAllMatchings)
				{
					ENUM_MAXIMUM_MATCHINGS_ITER(G.Clone(), M, matching, 0);
				}
			}

			FilterDuplicateMatchings();
		}

		//This code has been tested and works fine for test-case under consideration
		private void ENUM_MAXIMUM_MATCHINGS_ITER(BipartiteGraph G, EdgeList M, MatchingList matching, int recursionLevel)
		{
			//System.Console.WriteLine(recursionLevel);			//System.Console.WriteLine(recursionLevel);
			if (recursionLevel >= MaximumRecursionLevel)
			{
				return;
			}

			/*-------------------------------------------------------------------------------------------------------------------
			* Step1: If G has no edge, stop.
			* ------------------------------------------------------------------------------------------------------------------*/
			if (G.Edges.Count <= 1)
			{
				return;
			}


			/*-------------------------------------------------------------------------------------------------------------------
            * Step2: If D(G,M) contains no cylce, Go to Step8.
            * ------------------------------------------------------------------------------------------------------------------*/
			BipartiteGraph directedGraph = G.Clone().GetDirectedGraphAsPerMatching(M);
			var CIDG = new CycleInDirectedGraph(directedGraph); //Class object containing Method to detect and get cycle in a grah
			Edge e;
			MatchingList Mdash;
			BipartiteGraph Gplus, Gminus;
			if (CIDG.HasCycle)
			{
				/*-------------------------------------------------------------------------------------------------------------------
				* Step3 & 4: Choose an edge e as the same manner in ENUM_PERFECT_MATCHINGS_ITER. 
				* Find a cycle containing e by a depth-first-search algorithm.
				* ------------------------------------------------------------------------------------------------------------------*/
				// Cycle is found using DFS algorithm and edge-e is chosen such that it is in cycle and in matching M 
				// ( and not in M'(which is found next)) of the directed graph(DG)
				List<GraphNode> cycle = CIDG.Cycle; //returns here cycle in a directed graph DG //'Cycle': Vertices can not repeate and Edges can not repeat            
				EdgeList cycleEdges = directedGraph.ToEdges(cycle); //cycleInDG is list of nodes in a Cycle . Here this list is converted into edges in the Cycle.
				EdgeList MEdgeList = CorrespondingEdgeList(directedGraph, M);
				e = ChooseEdge(MEdgeList, cycleEdges); //Edge - e is chosen here.


				/*-------------------------------------------------------------------------------------------------------------------
				* Step5: Exchange edges along the cycle and output the obtained maximum matching M'. 
				* ------------------------------------------------------------------------------------------------------------------*/
				Mdash = ExchangeEdgesAlongCycleOrPath(matching, CycleToMatching(cycle));//this.matchingToStringList(M)
				Mdash.Sort();

				AddMatching(Mdash);
				if (MinimumCost > minimumAchievableCost)
				{
					/*-------------------------------------------------------------------------------------------------------------------
							* Step6: Enumerate all maximum matchings including e by ENUM_MAXIMUM_MATCHINGS_ITER with G+(e), M and trimmed D(G+(e), M\e).
							* ------------------------------------------------------------------------------------------------------------------*/
					Gplus = BuildGplus(G, e); //G+(e) is obtained here //!! here G+(e) graph will not have edge which is there in matching
					ENUM_MAXIMUM_MATCHINGS_ITER(Gplus, M, matching, recursionLevel + 1); //recursive call with G+(e) and M  


					/*-------------------------------------------------------------------------------------------------------------------
					* Step7: Enumerate all maximum matchings not including e by ENUM_MAXIMUM_MATCHINGS_ITER with G-(e), M' and trimmed D(G-(e), M'). Stop.
					* ------------------------------------------------------------------------------------------------------------------*/
					Gminus = BuildGminus(G, e); //G-(e) is obtained here
					ENUM_MAXIMUM_MATCHINGS_ITER(Gminus, MatchingToEdgeList(Gminus, Mdash), Mdash, recursionLevel + 1); //recursive call with G-(e) and M 
				}

				return;
			}


			/*----------------------------------------------------------------------------------------------------------------------
			* Step8: Find a feasible path with length 2 and generate a new maximum matching M'. 
			* Let e be the edge of the path not included in M.
			* ------------------------------------------------------------------------------------------------------------------*/
			//'Feasible Path': 
			NodeList unmatchedVertices = directedGraph.GetUnmatchedVertices(matching);
			var path = new MatchingList();
			foreach (GraphNode node in directedGraph)
			{
				if (node.NodeType == GraphNode.Type.Type1 || node.NodeType == GraphNode.Type.Type2 && !unmatchedVertices.Contains(node))
				{
					var dfs = new DepthFirstSearch(directedGraph, node, unmatchedVertices);
					path = dfs.length2Path;
					if (path.Count == 3 && IsFeasiblePath(directedGraph, path, unmatchedVertices))
					{
						break;
					}
				}
			}

			if (path.Count != 3)
			{
				return;
			}

			e = GetEdgeInPathNotInM(G, matching, PathToMatching(G, path));
			Mdash = ExchangeEdgesAlongCycleOrPath(matching, PathToMatching(G, path));
			Mdash.Sort();

			AddMatching(Mdash);
			if (MinimumCost > minimumAchievableCost)
			{
				/*-------------------------------------------------------------------------------------------------------------------
					* Step9: Call ENUM_MAXIMUM+MATCHINGS_ITER(G+(e), M', theta).
					-* ------------------------------------------------------------------------------------------------------------------*/
				Gplus = BuildGplus(G, e); //G+(e) is obtained here
				ENUM_MAXIMUM_MATCHINGS_ITER(Gplus, MatchingToEdgeList(Gplus, Mdash), Mdash, recursionLevel + 1);


				/*-------------------------------------------------------------------------------------------------------------------
				* Step10: Call ENUM_MAXIMUM+MATCHINGS_ITER(G-(e), M, theta).
				* ------------------------------------------------------------------------------------------------------------------*/
				Gminus = BuildGminus(G, e); //G-(e) is obtained here
				ENUM_MAXIMUM_MATCHINGS_ITER(Gminus, M, matching, recursionLevel + 1);
			}
		}

		/// <summary>
		/// Add the mathcing to the collection and if one matching with no reversed models 
		/// is found returns true
		/// </summary>
		/// <param name="Mdash"></param>
		/// <returns>True if no reversed models are present in the matching</returns>
		private void AddMatching(MatchingList matching)
		{
			int cost = FindOverconstrainedModelsReversalCost(matching);
			BigInteger id = GetMatchingID(matching);

			SortedMaximumMatchings.Add(new Matching(cost, id, matching));
		}

		protected BigInteger GetMatchingID(MatchingList matching)
		{
			BigInteger t = 1;
			foreach (string edge in matching)
			{

				if (edgePrims.ContainsKey(edge))
				{
					t = t * edgePrims[edge];
				}
				else
				{
					string[] split = edge.Split('-');
					string reversed = $"{split[1]}-{split[0]}";
					if (edgePrims.ContainsKey(reversed))
					{
						t = t * edgePrims[reversed];
					}
				}
			}
			return t;
		}

		private bool IsFeasiblePath(BipartiteGraph G, MatchingList path, NodeList unmatchedNodes)
		{
			if (unmatchedNodes.Contains(G.GetNode(path[0])) || unmatchedNodes.Contains(G.GetNode(path[2])))
			{
				return true;
			}

			return false;
		}

		private Edge GetEdgeInPathNotInM(BipartiteGraph G, MatchingList M, MatchingList path)
		{
			Edge e = null;
			string edge = path.Except(M).ToList()[0];
			string[] nodes = edge.Split('-');
			GraphNode node1 = G.GetNode(nodes[0]);
			GraphNode node2 = G.GetNode(nodes[1]);
			if (G.Contains(node1, node2))
			{
				e = G.Edges.FindByNodes(node1, node2);
			}
			else if (G.Contains(node2, node1))
			{
				e = G.Edges.FindByNodes(node2, node1);
			}
			return e;
		}

		private MatchingList PathToMatching(BipartiteGraph G, MatchingList path)
		{
			var matching = new MatchingList();
			string edge;
			for (int i = 0; i < path.Count() - 1; i++)
			{
				if (G.GetNode(path[i]).NodeType == GraphNode.Type.Type2)
				{
					edge = EdgeString(path[i], path[i + 1]);
				}
				else
				{
					edge = EdgeString(path[i + 1], path[i]);
				}

				matching.Add(edge);
			}
			return matching;
		}


		//This method find the corresponding edges of matching M(which contains edges from other Graph) for the given Graph G
		private static EdgeList CorrespondingEdgeList(BipartiteGraph G, EdgeList M)
		{
			var corresponding = new EdgeList();
			foreach (Edge edge in M)
			{
				GraphNode from = G.GetNode(edge.FromNode.Value);
				GraphNode to = G.GetNode(edge.ToNode.Value);
				if (from == null && to == null)
				{
					continue;
				}

				if (G.Edges.FindByNodes(from, to) != null)
				{
					corresponding.Add(G.Edges.FindByNodes(from, to));
				}
				else
				{
					corresponding.Add(G.Edges.FindByNodes(to, from));
				}
			}
			return corresponding;
		}

		private static MatchingList ExchangeEdgesAlongCycleOrPath(MatchingList M, MatchingList cycleOrPath)
		{
			var union = M.Union(cycleOrPath).ToList();
			var intersection = M.Intersect(cycleOrPath).ToList();
			return union.Except(intersection).ToList();
		}

		private static Edge ChooseEdge(EdgeList M, EdgeList cycle) //Choosing edge which is common in both cycle and matching of the graph
		{
			var edgesInCycle = new HashSet<Edge>(cycle);

			foreach (Edge edgeM in M)
			{
				if (edgesInCycle.Contains(edgeM))
				{
					return edgeM;
				}
			}

			return null;
		}

		//YHB: this method find the graph G_plus by deleting given edge-e, its end vertices and edges associated with the end vertices
		private static BipartiteGraph BuildGplus(BipartiteGraph G, Edge e)
		{
			BipartiteGraph Gplus = G.Clone();
			Gplus.Remove(e.FromNode.Value);
			Gplus.Remove(e.ToNode.Value);
			return Gplus;
		}

		//YHB: this method find the graph G_minus by deleting given edge-e from the G i.e. given graph
		private static BipartiteGraph BuildGminus(BipartiteGraph G, Edge e)
		{
			BipartiteGraph Gminus = G.Clone();
			GraphNode from = G.GetNode(e.FromNode.Value);
			GraphNode to = G.GetNode(e.ToNode.Value);
			if (G.Contains(from, to))
			{
				Gminus.RemoveDirectedEdge(from, to);
			}
			else
			{
				Gminus.RemoveDirectedEdge(to, from);
			}


			return Gminus;
		}

		private Edge GetEdge(Node fromNode, Node toNode)
		{
			Edge e = G.Edges.FindByNodes(fromNode, toNode);
			if (e == null)
			{
				e = G.Edges.FindByNodes(toNode, fromNode);
			}

			return e;
		}

		private Node GetNode(string from)
		{
			if (G.duplicateModToModel.ContainsKey(from))
			{
				from = G.duplicateModToModel[from];
			}

			return G.GetNode(from);
		}

		private static string EdgeString(string from, string to) => $"{from}-{to}";
	}

	public class Matching : IComparable<Matching>
	{
		public int ReversalCost { get; }

		public BigInteger ID { get; }

		public Dictionary<string, List<string>> Pairs { get; }

		public Matching(int reversalCost, BigInteger id, MatchingList pairs)
		{
			ReversalCost = reversalCost;
			ID = id;
			Pairs = new Dictionary<string, List<string>>(pairs.Count);
			foreach (string pair in pairs)
			{
				string[] fromto = pair.Split('-');

				string model = fromto.First();
				var match = System.Text.RegularExpressions.Regex.Match(model, @"^Duplicate_(.+)_\d+$");
				if (match.Success)
				{
					model = match.Groups[1].Value;
				}

				string variable = fromto.Last();

				if (Pairs.ContainsKey(model))
				{
					Pairs[model].Add(variable);
				}
				else
				{
					Pairs[model] = new List<string>() { variable };
				}
			}
		}

		public void Deconstruct(out int reversalCost, out BigInteger id, out Dictionary<string, List<string>> pairs)
		{
			(reversalCost, id, pairs) = (ReversalCost, ID, Pairs);
		}

		public int CompareTo(Matching other) => ReversalCost.CompareTo(other?.ReversalCost ?? Int32.MaxValue);

		public override string ToString() => $"Matching cost = {ReversalCost}, N = {Pairs.Count}, id = {ID}";
	}
}
