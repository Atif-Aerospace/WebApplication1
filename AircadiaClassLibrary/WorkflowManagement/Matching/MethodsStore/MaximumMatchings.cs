using System;
using System.Collections.Generic;
using System.Linq;

namespace Aircadia.WorkflowManagement
{
	public class MaximumMatchings
    {
		private const int MaximumRecursionLevel = 12;

		public BipartiteGraph G = new BipartiteGraph();
        public List<List<String>> list_AllMaximumMatchings = new List<List<string>>();
        public Dictionary<int, int> matchingIndexVsrevCost = new Dictionary<int, int>();//for calculating revCost //04102017
        public List<List<String>> list_FilteredMaximumMatchings = new List<List<string>>();
        public List<List<String>> OrderedFilteredMaximumMatchings = new List<List<string>>();//04102017
        public Dictionary<string, int> edgePrims = new Dictionary<string, int>(); //stores weightages of the edges of the graph


        public void orderFilterdMatchingAsPerRevCost()
        {
			OrderedFilteredMaximumMatchings = 
				(from entry in matchingIndexVsrevCost
				 orderby entry.Value ascending
				 select list_FilteredMaximumMatchings[entry.Key]).ToList();
        }

        public void filterDuplicateMatchings()
        {
            int filteredMatchCounter = 0;
            var added_matchings = new List<int>(); //stores the integers of matchings, the interger is calculated by multiplying prims of the edges of the matching
            foreach (List<string> _match in list_AllMaximumMatchings)
            {//Filtering duplicate matchings
                int t = 1;
                foreach (string strg in _match)
                {

                    if (edgePrims.ContainsKey(strg))
                    {
                        t = t * edgePrims[strg];
                    }
                    else if (edgePrims.ContainsKey(strg.Split('-')[1] + "-" + strg.Split('-')[0]))
                    {
                        t = t * edgePrims[(strg.Split('-')[1] + "-" + strg.Split('-')[0])];
                    }
                }
                if ((!added_matchings.Contains(t)) && t != 1)
                {
                    OverConstrainedModels.Clear();//04102017
					findOverconstrainedModelsReversalCost(_match, out OverConstrainedModels, out unmappedVariables, out revCost); //04102017
                    matchingIndexVsrevCost.Add(filteredMatchCounter++, revCost);//04102017

                    list_FilteredMaximumMatchings.Add(_match);
                    added_matchings.Add(t);
                }
                
            }

            orderFilterdMatchingAsPerRevCost();
        }

        public void findOverconstrainedModelsReversalCost(List<String> M, out List<string> overConstrainedModelsList, out List<string> unmappedVariablesList, out int revCost)
        {
            revCost = 0;//04102017
            overConstrainedModelsList = new List<string>();
            unmappedVariablesList = new List<string>();

            if (M != null) //after filtering the matchings from list_AllMaximumMatchings
            {
                //foreach (GraphNode gn in web1.getUnmatchedVerticesOfGraph(maxT.list_AllMaximumMatchings[n])) //original code
                foreach (GraphNode gn in G.GetUnmatchedVertices(M)) //after filtering the matchings
                {
                    if (gn.NodeType == GraphNode.Type.Type2)
                    {
                        if (G.duplicateModToModel.ContainsKey(gn.Value))
                            overConstrainedModelsList.Add(G.duplicateModToModel[gn.Value]);
                        else
                            overConstrainedModelsList.Add(gn.Value);
                    }
                    else
                    {
                        unmappedVariablesList.Add(gn.Value);
                    }
                }



                //for finding revCost of this matching //04102017
                foreach (string match in M)//04102017
                {//04102017 Entire added on //04102017
                    Node fromNode;
                    Node toNode;
                    string[] split = match.Split('-');
					string from = split[0];
					string to = split[1];

					if (G.duplicateModToModel.ContainsKey(from))
                        fromNode = G.GetNode(G.duplicateModToModel[from]);
                    else
                        fromNode = G.GetNode(from);

					if (G.duplicateModToModel.ContainsKey(to))
                        toNode = G.GetNode(G.duplicateModToModel[to]);
                    else
                        toNode = G.GetNode(to); 
                    
					//Node from = this.G.GetNode();
                    Edge e = G.Edges.FindByNodes(fromNode, toNode);
                    if (e == null)
                        e = G.Edges.FindByNodes(toNode, fromNode); //04102017

					revCost += e.Cost2; //04102017
                }//04102017
            }

        }

        public List<String> cycleToStringList(List<GraphNode> cycle) //taken from enumeratingmatchinginbipartiteGraph class.... decide where to keep this method here or in the 'enumeratingmathcihgin...' class
        {
            var elist = new List<String>();
            for (int i = 0; i < cycle.Count - 1; i++)
            {//Converts cycle into edges as strings of values in the form i.e. 'model.value - variable.value'
             //String form of the matching will be used for performing set operations on it.
                String edg;
                if (cycle[i].NodeType == GraphNode.Type.Type2)
                {
                    edg = cycle[i].Value.ToString() + '-' + cycle[i + 1].Value.ToString();
                }
                else
                {
                    edg = cycle[i + 1].Value.ToString() + '-' + cycle[i].Value.ToString();
                }
                elist.Add(edg);
            }
            return elist;
        }

        public EdgeList matchingStringsToEdgeList(BipartiteGraph G, List<String> M_String)
        {
            var M_to_EdgeListM = new EdgeList();
            GraphNode nd1, nd2;
            foreach (String edg in M_String)
            {
                nd1 = G.GetNode(edg.Split('-')[0]);
                nd2 = G.GetNode(edg.Split('-')[1]);
                if (G.Contains(nd1, nd2))
                {
                    M_to_EdgeListM.Add(G.Edges.FindByNodes(nd1, nd2));
                }
                else if (G.Contains(nd2, nd1))
                {
                    M_to_EdgeListM.Add(G.Edges.FindByNodes(nd2, nd1));
                }
                //else
                //{
                //    M_to_EdgeListM.Add(new Edge(nd1,nd2,0));
                //}
            }

            return M_to_EdgeListM;
        }

        public List<String> matchingToStringList(EdgeList M)
        {//Converts matching edge list into strings of values in the form i.e. 'model.value - variable.value'
            //String form of the cycle will be used for performing set operations on it.
            var M_to_stringM = new List<string>();
            foreach (Edge edg in M)
            {
                if (edg.FromNode.NodeType == GraphNode.Type.Type2)
                {
                    M_to_stringM.Add(edg.FromNode.Value + '-' + edg.ToNode.Value);
                }
                else
                {
                    M_to_stringM.Add(edg.ToNode.Value + '-' + edg.FromNode.Value);
                }
            }
            return M_to_stringM;
        }




        public MaximumMatchings(BipartiteGraph G, bool getAllMatchings = false)
        {
			list_AllMaximumMatchings.Clear();
            this.G = G;
            this.G.UpdateAddingDuplicateNodes();
			edgePrims = this.G.edgeNodesCost;
			ENUM_MAXIMUM_MATCHINGS(this.G.Clone(), getAllMatchings);
        }

        public List<string> OverConstrainedModels = new List<string>();
        public List<string> unmappedVariables = new List<string>();
        public int revCost;
        public bool returnTrue = false;

        private void ENUM_MAXIMUM_MATCHINGS(BipartiteGraph G, bool getAllMatchings)
        {
			/*--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
            Step1: Find a maximum matching M of G and output M. 
            --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/
			
			// The maximum mathing
            EdgeList M = MaximumMatching.Get(G);
            List<string> M_String = matchingToStringList(M);
            M_String.Sort();

			findOverconstrainedModelsReversalCost(M_String, out OverConstrainedModels, out unmappedVariables, out revCost);                     
            if (OverConstrainedModels.Count > 0)
            {
                returnTrue = true;
                return;                                
            }

            list_AllMaximumMatchings.Add(M_String);
            /*--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
            Step2: Trim unnecessary arcs from D(G,M) by a strongly connected component decomposition algorithm.
            --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/
            //Need further action on this to trim edges after finding SCCs in a D(G,M). Here only SCCs are found. Edges are not trimmed yet.

            //Graph D_GM = Get_D_GM(G.Clone(), M); //Directed graph is obtained here with reversing edges of matchingEdges.
            //List<List<GraphNode>> sCCs = TarjanCycleDetect.DetectCycle(D_GM); 


            /*-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
            Step3: Call ENUM_MAXIMUM_MATCHINGS_ITER(G, M, D(G,M))
            --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/
			if (getAllMatchings)
				ENUM_MAXIMUM_MATCHINGS_ITER(G.Clone(), M, matchingToStringList(M), 0);
			//list_AllPerfectMatchings.Add(M_String);

			filterDuplicateMatchings();

        }

        private void ENUM_MAXIMUM_MATCHINGS_ITER(BipartiteGraph G, EdgeList M, List<String> M_String, int recursionLevel)
        {
			if (recursionLevel > MaximumRecursionLevel)
				return;
			
			//This code has been tested and works fine for test-case under consideration
            /*-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
            Step1: If G has no edge, stop.
            --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/
            if (G.Edges.Count <= 1)
            {
                return;
            }




            /*-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
            Step2: If D(G,M) contains no cylce, Go to Step8.
            --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/
            BipartiteGraph d_G = G.Clone().GetDirectedGraphAsPerMatching(M);
            var CIDG = new CycleInDirectedGraph(d_G); //Class object containing Method to detect and get cycle in a grah
			if (!CIDG.HasCycle)
            {
                goto Step8;
            }



            /*--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
            Step3 & 4: Choose an edge e as the same manner in ENUM_PERFECT_MATCHINGS_ITER. Find a cycle containing e by a depth-first-search algorithm.
            ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/
            //Cycle is found using DFS algorithm and edge-e is chosen such that it is in cycle and in matching M ( and not in M'(which is found next)) of the directed graph(DG)
            List<GraphNode> cycleInDG = CIDG.Cycle; //returns here cycle in a directed graph DG //'Cycle': Vertices can not repeate and Edges can not repeat            
            EdgeList cycleInDG_Edges = d_G.ToEdges(cycleInDG); //cycleInDG is list of nodes in a Cycle . Here this list is converted into edges in the Cycle.
            List<String> cycleInDG_EdgesAsStrings = d_G.ToEdgesAsStrings(cycleInDG);
            EdgeList M_corresp = CorrespondingListForTheGraph(d_G, M);
            Edge e = chooseEdge(M_corresp, cycleInDG_Edges); //Edge - e is chosen here.




            /*--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
            Step5: Exchange edges along the cycle and output the obtained maximum matching M'. 
            ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/
            List<String> M_dash_String = ExchangingEdgesAlongCycleOrPathStringVersion(M_String, cycleToStringList(cycleInDG));//this.matchingToStringList(M)
            M_dash_String.Sort();
            list_AllMaximumMatchings.Add(M_dash_String);




            /*-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
            Step6: Enumerate all maximum matchings including e by ENUM_MAXIMUM_MATCHINGS_ITER with G+(e), M and trimmed D(G+(e), M\e).
            --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/
            BipartiteGraph G_plus = G_plus_e(G.Clone(), e);
            ENUM_MAXIMUM_MATCHINGS_ITER(G_plus, M, M_String, ++recursionLevel); //recursive call with G+(e) and M  // !! here G+(e) graph will not have edge which is there in matching




            /*-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
            Step7: Enumerate all maximum matchings not including e by ENUM_MAXIMUM_MATCHINGS_ITER with G-(e), M' and trimmed D(G-(e), M'). Stop.
            --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/
            BipartiteGraph G_minus = G_minus_e(G.Clone(), e); //G-(e) is obtained here
            ENUM_MAXIMUM_MATCHINGS_ITER(G_minus, matchingStringsToEdgeList(G_minus, M_dash_String), M_dash_String,++recursionLevel); //recursive call with G-(e) and M
            return;





        /*-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        Step8: Find a feasible path with length 2 and generate a new maximum matching M'. Let e be the edge of the path not included in M.
        ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/
        //'Feasible Path': 
        Step8:;
            NodeList unmatchedVertices = d_G.GetUnmatchedVertices(M_String);
            var path = new List<String>();
            foreach (GraphNode Gn in d_G)
            {
                if (Gn.NodeType == GraphNode.Type.Type1)
                {
                    var dfs = new DepthFirstSearch(d_G, Gn, unmatchedVertices);
                    path = dfs.length2Path;
                    if (path.Count == 3 && isFeasiblePath(d_G, path, unmatchedVertices))
                        break;
                }
                if ((Gn.NodeType == GraphNode.Type.Type2) && !(unmatchedVertices.Contains(Gn)))
                {
                    var dfs = new DepthFirstSearch(d_G, Gn, unmatchedVertices);
                    path = dfs.length2Path;
                    if (path.Count == 3 && isFeasiblePath(d_G, path, unmatchedVertices))
                        break;
                }


            }

            if (path.Count != 3)
            {
                return;
            }

            e = getEdgeInPathNotInM(G, M_String, pathToEdgesAsStrings(G, path));

            M_dash_String = ExchangingEdgesAlongCycleOrPathStringVersion(M_String, pathToEdgesAsStrings(G, path));
            M_dash_String.Sort();
            list_AllMaximumMatchings.Add(M_dash_String);


            /*--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
            Step9: Call ENUM_MAXIMUM+MATCHINGS_ITER(G+(e), M', theta).
            ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/
            G_plus = G_plus_e(G.Clone(), e);
            ENUM_MAXIMUM_MATCHINGS_ITER(G_plus, matchingStringsToEdgeList(G_plus, M_dash_String), M_dash_String, ++recursionLevel);




            /*--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
            Step10: Call ENUM_MAXIMUM+MATCHINGS_ITER(G-(e), M, theta).
            --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/

            G_minus = G_minus_e(G.Clone(), e); //G-(e) is obtained here
            ENUM_MAXIMUM_MATCHINGS_ITER(G_minus, M, M_String, ++recursionLevel);





        }

        private bool isFeasiblePath(BipartiteGraph G, List<String> path, NodeList unmatchedNodes)
        {
            bool isFeasiblePath = false;
            if (unmatchedNodes.Contains(G.GetNode(path[0])) || unmatchedNodes.Contains(G.GetNode(path[2])))
                isFeasiblePath = true;

            return isFeasiblePath;
        }

        private Edge getEdgeInPathNotInM(BipartiteGraph G, List<String> M, List<String> path)
        {
            Edge e = null;
            String eString = path.Except(M).ToList()[0];
            String[] eNodes = eString.Split('-');
            GraphNode nd1 = G.GetNode(eNodes[0]);
            GraphNode nd2 = G.GetNode(eNodes[1]);
            if (G.Contains(nd1, nd2))
            {
                e = G.Edges.FindByNodes(nd1, nd2);
            }
            else if (G.Contains(nd2, nd1))
            {
                e = G.Edges.FindByNodes(nd2, nd1);
            }
            return e;
        }

        private List<String> pathToEdgesAsStrings(BipartiteGraph G, List<String> path) //04-01-2017 Added for MaximumMatchings
        {
            var pathAsStrings = new List<String>();
            String str;
            for (int i = 0; i < path.Count() - 1; i++)
            {
                if (G.GetNode(path[i]).NodeType == GraphNode.Type.Type2)
                {
                    str = path[i] + '-' + path[i + 1];
                }
                else
                {
                    str = path[i + 1] + '-' + path[i];
                }

                pathAsStrings.Add(str);
            }
            return pathAsStrings;
        }


        private static EdgeList CorrespondingListForTheGraph(BipartiteGraph G, EdgeList M) //This method find the corresponding edges of matching M(which contains edges from other Graph) for the given Graph G
        {
            var M_corresp = new EdgeList();
            foreach (Edge edg in M)
            {
                GraphNode frm = G.GetNode(edg.FromNode.Value);
                GraphNode to = G.GetNode(edg.ToNode.Value);
                if (frm == null && to == null)
                {
                    goto xyz;
                }
                if (G.Edges.FindByNodes(frm, to) != null)
                {
                    M_corresp.Add(G.Edges.FindByNodes(frm, to));
                }
                else
                {
                    M_corresp.Add(G.Edges.FindByNodes(to, frm));
                }
            xyz:;
            }
            return M_corresp;
        }
        private static EdgeList ExchangingEdgesAlongCycle(EdgeList M, EdgeList cycle)
        {//improve this method to exchange edges along the cycle

            foreach (var itemM in M)
            {

                foreach (Edge itemC in cycle)
                {
                    if (itemM.ToNode.Value.ToString() == itemC.FromNode.Value.ToString() && itemM.FromNode.Value.ToString() == itemC.ToNode.Value.ToString())
                    {
                        cycle.Remove(itemC);
                        break;
                    }

                }


            }

            //EdgeList<T> union = (EdgeList<T>)M.Union(cycle);
            //EdgeList<T> intersection = (EdgeList<T>)M.Intersect(cycle);
            //return (EdgeList<T>)union.Except(intersection);

            return cycle;
        }

        private static List<String> ExchangingEdgesAlongCycleOrPathStringVersion(List<String> M, List<String> cycleOrPath)
        {//Now working propery 28-12-2016

            var union = M.Union(cycleOrPath).ToList();
            var intersection = (List<String>)M.Intersect(cycleOrPath).ToList();
            return (List<String>)union.Except(intersection).ToList();


        }

        public static Edge chooseEdge(EdgeList M, EdgeList cycle) //Choosing edge which is common in both cycle and matching of the graph
        {
            foreach (Edge edgeM in M)
            {
                foreach (Edge edgeC in cycle)
                {
                    if (edgeM.FromNode.Value.Equals(edgeC.FromNode.Value) && edgeM.ToNode.Value.Equals(edgeC.ToNode.Value))
                    {//(edgeM.FromNode.Value.Equals(edgeC.FromNode.Value) && edgeM.ToNode.Value.Equals(edgeC.ToNode.Value)) ||
                        //(edgeM.ToNode.Value.Equals(edgeC.FromNode.Value) && edgeM.FromNode.Value.Equals(edgeC.ToNode.Value))
                        return edgeC;
                    }
                }

            }

            return null;
        }


        public static String chooseEdgeStringVersion(List<List<String>> M, List<String> cycle) //Choosing edge which is common in both cycle and matching of the graph
        {
            foreach (List<String> edgeM in M)
            {
                foreach (String edgeC in cycle)
                {
                    if ((edgeM[0] + '-' + edgeM[1]) == edgeC)
                    {//(edgeM.FromNode.Value.Equals(edgeC.FromNode.Value) && edgeM.ToNode.Value.Equals(edgeC.ToNode.Value)) ||
                        //(edgeM.ToNode.Value.Equals(edgeC.FromNode.Value) && edgeM.FromNode.Value.Equals(edgeC.ToNode.Value))
                        return edgeC;
                    }
                }

            }

            return null;
        }


        private static BipartiteGraph G_plus_e(BipartiteGraph G, Edge e)
        {//YHB: this method find the graph G_plus by deleting given edge-e, its end vertices and edges associated with the end vertices
            BipartiteGraph G_plus = G;

            foreach (GraphNode item in G.Nodes)
            {

            }

            G_plus.Remove(e.FromNode.Value);
            G_plus.Remove(e.ToNode.Value);

            return G_plus;
        }

        private static BipartiteGraph G_minus_e(BipartiteGraph G, Edge e)
        {//YHB: this method find the graph G_minus by deleting given edge-e from the G i.e. given graph
            BipartiteGraph G_minus = G;
            if (G.Contains(G.GetNode(e.FromNode.Value), G.GetNode(e.ToNode.Value)))
            {
                G_minus.RemoveDirectedEdge(G.GetNode(e.FromNode.Value), G.GetNode(e.ToNode.Value));
            }
            else
            {
                G_minus.RemoveDirectedEdge(G.GetNode(e.ToNode.Value), G.GetNode(e.FromNode.Value));
            }


            return G_minus;
        }

        private static BipartiteGraph Get_D_GM(BipartiteGraph G, EdgeList M)
        {//Get directed graph from given Graph G and matchings(list of matched edges) M
            BipartiteGraph D_GM = G;


            //D_GM.GetAdjListInTheFormOfIntegers(); //Graph is converted to AdjList containing integers which are assigned to each model type node in a graph
            //IList<int> match = HopcroftKarp.GetMatching(D_GM.AdjList, D_GM.ModSet.Count); //Max. matching algorithm is called here with above AdjList of graph and number of model type nodes in a graph as input

            //EdgeList<T> M = D_GM.MatchingToEdgeList(match); //Edges of Matching



            foreach (var edge in M)
            {
                D_GM.ReverseEdgeDirection(edge); //getting directed graph with matched edges with direction from v1 -> v2 and other edges from v2 -> v1
            }

            return D_GM;
        }
    }
}
