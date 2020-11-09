using System;
using System.Collections.Generic;
using System.Linq;

namespace Aircadia.WorkflowManagement
{//Implementation for enumerating perfect matchings works fine(has been tested for 3-4 cases). But needs to clean the code(i.e. removing the unwanted methods and their calls). 
	class PerfectMatchings
    {
        public BipartiteGraph G = new BipartiteGraph();
        public List<EdgeList> list_PerfectMatchings = new List<EdgeList>(); //stores all the perfect matchings in the graph
        public static List<List<string>> list_AllPerfectMatchings = new List<List<string>>();

        public List<string> cycleToStringList(List<GraphNode> cycle) //taken from enumeratingmatchinginbipartiteGraph class.... decide where to keep this method here or in the 'enumeratingmathcihgin...' class
        {
            var elist = new List<string>();
            for (int i = 0; i < cycle.Count - 1; i++)
            {//Converts cycle into edges as strings of values in the form i.e. 'model.value - variable.value'
			 //String form of the matching will be used for performing set operations on it.
				string edg;
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

        public EdgeList matchingStringsToEdgeList(BipartiteGraph G, List<string> M_String)
        {
            var M_to_EdgeListM = new EdgeList();
            GraphNode nd1, nd2;
            foreach (string edg in M_String)
            {
                nd1 = G.GetNode(edg.Split('-')[0]);
                nd2 = G.GetNode(edg.Split('-')[1]);
                if (G.Contains(nd1,nd2))
                {
                    M_to_EdgeListM.Add(G.Edges.FindByNodes(nd1,nd2));
                }
                else if (G.Contains(nd2,nd1))
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
        public List<string> matchingToStringList(EdgeList M)
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

        public PerfectMatchings(BipartiteGraph G)
        {
            this.G = G;
			ENUM_PERFECT_MATCHINGS(G.Clone());
        }

        private void ENUM_PERFECT_MATCHINGS(BipartiteGraph G)
        {
			/*-------------------------------------------------------------------------------
            Step1: Find a perfect matching M of G and output M. If M is not found, stop.
            ---------------------------------------------------------------------------------*/

			//////G.GetAdjListInTheFormOfIntegers(); //Graph is converted to AdjList containing integers which are assigned to each model type node in a graph
			//////IList<int> match = HopcroftKarp.GetMatching(G.AdjList, G.ModSet.Count); //Max. matching algorithm is called here with above AdjList of graph and number of model type nodes in a graph as input
			//This returns matching as IList of integers associated with models

			throw new NotImplementedException();
			EdgeList M = null; //G.MatchingToEdgeList(); //Edges of Matching
                                                 //// List<List<T>> M_String = G.MatchingToEdgeListAsValues();//Matching Edges as Strings( "Node - Node")

            list_AllPerfectMatchings.Add(matchingToStringList(M));
            /*-----------------------------------------------------------------------------------------------------------
            Step2: Trim unnecessary edges from G by a strongly connected component decomposition algorithm with D(G,M)
            -------------------------------------------------------------------------------------------------------------*/

            ////Graph D_GM = Get_D_GM(G.Clone(), M); //Directed graph is obtained here with reversing edges of matchingEdges.
            ////List<List<GraphNode>> sCCs = TarjanCycleDetect.DetectCycle(D_GM); //Need further action on this to trim edges after finding SCCs in a D(G,M). Here only SCCs are found. Edges are not trimmed yet.


            /*----------------------------------------
            Step3: Call ENUM_PERFECT_MATCHINGS_ITER
            ------------------------------------------*/
            ENUM_PERFECT_MATCHINGS_ITER(G.Clone(), M, matchingToStringList(M));
            //list_AllPerfectMatchings.Add(M_String);



        }

        private void ENUM_PERFECT_MATCHINGS_ITER(BipartiteGraph G, EdgeList M, List<string> M_String)
        {
            /*-------------------------------
            Step1: If G has no edge, stop.
            ---------------------------------*/

            if (G.Edges.Count == 0)
            {
                return;
            }
            /*-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
            Step2 & Step3: Choosing Edge-e and Finding Cycle. //Cycle is found using DFS algorithm and edge-e is chosen such that it is in cycle and in matching M ( and not in M'(which is found next)) of the directed graph(DG)
            ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/

            BipartiteGraph d_G = G.Clone().GetDirectedGraphAsPerMatching(M);
            var CIDG = new CycleInDirectedGraph(d_G); //Class object containing Method to detect and get cycle in a grah
			if (!CIDG.HasCycle)
            {
                return;
            }
            List<GraphNode> cycleInDG = CIDG.Cycle; //returns here cycle in a directed graph DG
            //Cycle: Vertices can not repeate and Edges can not repeat
            EdgeList cycleInDG_Edges = d_G.ToEdges(cycleInDG); //cycleInDG is list of nodes in a Cycle . Here this list is converted into edges in the Cycle.
            List<string> cycleInDG_EdgesAsStrings = d_G.ToEdgesAsStrings(cycleInDG);
            EdgeList M_corresp = CorrespondingListForTheGraph(d_G, M);
            Edge e = chooseEdge(M_corresp, cycleInDG_Edges); //Edge - e is chosen here.
            ////String e_string = chooseEdgeStringVersion(M_String, cycleInDG_EdgesAsStrings); //Edge - e is chosen here.

            /*---------------------------------------------------------------------------------
            Step4: Find a perfect matching M' by exchanging edges along the cycle. Output M'.
            ------------------------------------------------------------------------------------*/

            EdgeList M_dash = ExchangingEdgesAlongCycle(M_corresp, cycleInDG_Edges); /* Write code here to find M' here */
            List<string> M_dash_String = ExchangingEdgesAlongCycleStringVersion(M_String, cycleToStringList(cycleInDG));//this.matchingToStringList(M)

            //list_PerfectMatchings.Add(M_dash);
            list_AllPerfectMatchings.Add(M_dash_String);



            /*-----------------------------------------------------------------------------------------
            Step5: Find G+(e), trim unnecessary edges of G+(e) using SCC decomposition algorithm.
            ------------------------------------------------------------------------------------------*/

            //Graph<T> G_plus = G_plus_e(DG.Clone(), e); //G+(e) is obtained here
            BipartiteGraph G_plus = G_plus_e(G.Clone(), e);
            List<List<GraphNode>> sCCs_G_plus = TarjanCycleDetect.DetectCycle(G_plus); // /SCCs in G+(e) to trim unnecessary edges
                                                                                       /* Write code for trimming edges here*/

            /*-----------------------------------------------------------------------------------------------------------
            Step6: Enumerate all perfect matchings including e by ENUM_PERFECT_MATCHING_ITER with obtained graph and M
            ------------------------------------------------------------------------------------------------------------*/

           ENUM_PERFECT_MATCHINGS_ITER(G_plus, M, M_String); //recursive call with G+(e) and M  // !! here G+(e) graph will not have edge which is there in matching

            /*-----------------------------------------------------------------------------------------
            Step7: Find G-(e), trim unnecessary edges of G-(e) using SCC decomposition algorithm.
            -----------------------------------------------------------------------------------------*/

            BipartiteGraph G_minus = G_minus_e(G.Clone(), e); //G+(e) is obtained here
            ////G_minus.plotMethod(this.goView1);
            List<List<GraphNode>> sCCs_G_minus = TarjanCycleDetect.DetectCycle(G_minus); // SCCs in G-(e) to trim unnecessary edges
            /* Write code for trimming edges here*/


            /*---------------------------------------------------------------------------------------------------------------
            Step8: Enumerate all perfect matchings not including e by ENUM_PERFECT_MATCHING_ITER with obtained graph and M'
            ---------------------------------------------------------------------------------------------------------------*/

            ENUM_PERFECT_MATCHINGS_ITER(G_minus, matchingStringsToEdgeList(G_minus,M_dash_String), M_dash_String); //recursive call with G+(e) and M


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

        private static List<string> ExchangingEdgesAlongCycleStringVersion(List<string> M, List<string> cycle)
        {//Now working propery 28-12-2016

            var union = M.Union(cycle).ToList();
            var intersection = (List<string>)M.Intersect(cycle).ToList();
            return (List<string>)union.Except(intersection).ToList();


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


        public static string chooseEdgeStringVersion(List<List<string>> M, List<string> cycle) //Choosing edge which is common in both cycle and matching of the graph
        {
            foreach (List<string> edgeM in M)
            {
                foreach (string edgeC in cycle)
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
