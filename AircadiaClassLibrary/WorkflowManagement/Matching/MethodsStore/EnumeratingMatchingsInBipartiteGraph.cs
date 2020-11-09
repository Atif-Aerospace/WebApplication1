using System;
using System.Collections.Generic;
using System.Linq;
using Aircadia.WorkflowManagement;
using AirCADiaArchitect.Yogesh.GraphObjects;

namespace AirCADiaArchitect.Yogesh.MethodsStore
{
	class EnumeratingMatchingsInBipartiteGraph<T>
    {

        public static void Enum_Maximum_Matching_Iter(BipartiteGraph G, EdgeList M)
        {
            //Step-1: If G has no edge, STOP
            if (G.Edges.Count == 0)
                return;

            var obj = new CycleInDirectedGraph(G);
            if (!obj.HasCycle)
            {
                return;
            }
            //Step-2: Choose an edge e.
            Edge e = chooseEdge(M, ToEdges(G, obj.Cycle));

        }

        public static EdgeList ToEdges(BipartiteGraph g, List<GraphNode> cycle) 
        {
            var elist = new EdgeList();
            for (int i = 1; i < cycle.Count; i++)
            {
                Edge edg = g.Edges.FindByNodes(cycle[cycle.Count-i], cycle[cycle.Count -i - 1]);
                elist.Add(edg);
            }
            return elist;
        }

        public static Edge chooseEdge(EdgeList M, EdgeList cycle) //Choosing edge which is common in both cycle and matching of the graph
        {
            foreach (Edge edgeM in M)
            {
                foreach (Edge edgeC  in cycle)
                {
                    if((edgeM.FromNode == edgeC.FromNode && edgeM.ToNode == edgeC.ToNode) || (edgeM.ToNode == edgeC.FromNode && edgeM.FromNode == edgeC.ToNode))
                    {
                        return edgeM;
                    }
                }
                
            }

            return null;
        }

        public static void Enum_Maximum_Matching(Dictionary<string, List<string>> G)
        {
            //creating adjecency matrix with its values as integers which will be given as input to maximum HopcroftKarp method.
            var matchingInput = new List<int[]>(); //it is the input to the maximum matching method(hopcroftKarp) It contains adjecency matrix in the form of integers
            var modelIntMap = new Dictionary<string, int>();
            int count = 0;
            int t = 0;
            foreach (var elem in G)
            {
                int[] temp = new int[elem.Value.Count];
                foreach (string mod in elem.Value)
                {
                    if (!modelIntMap.Keys.Contains(mod))
                    {
                        modelIntMap.Add(mod, count++);
                    }
                    temp[t++] = modelIntMap[mod]; //here array is created i.e. row of adjecancy matrix
                }
                matchingInput.Add(temp); //here the array created above is added to the adjecency list( a representation of adjecency matrix representation)
                t = 0;
            } // at the end of this for loop there is a adjecency list containing integers as its elements and a dictionary of mapping is created to store information of 
              //these integers and to which model these intergers are mapped to "modelIntMap< name of model, integer associated to model>"

            var varIntMap = new Dictionary<string, int>();
            count = modelIntMap.Count; //reassigned value of "count" variable
            foreach (var elem in G)
            {
                if (!varIntMap.Keys.Contains(elem.Key))
                {
                    varIntMap.Add(elem.Key, count++);
                }
            }

            //Step1: Finding a maximum matching M of G and output M.
            var matching = HopcroftKarp.GetMatching(matchingInput, modelIntMap.Count); //here "matching" stores the matched variables on right of bipartite graph
                                                                                       /* Here matching obtained above is used to create matched edges as list containing two integers ..first int is source location while second destination for a link/edge*/

            /* List<List<int>> matchingEdges = new List<List<int>>();
             int inc = 0;
             foreach (var match in matching)
             {
                 List<int> temp = new List<int>();
                 temp.Add(match);
                 temp.Add(varIntMap.Values.ElementAt(inc));
                 matchingEdges.Add(temp);
             }*/
            var matchingEdges = new List<string>();
            int inc = 0;
            foreach (var match in matching)
            {
                if (!(match < 0))
                    matchingEdges.Add(varIntMap.Values.ElementAt(inc++).ToString() + match.ToString());
            }

            Dictionary<int, List<int>> UnDirectBGInt = UndirectedGraphStringsToInt(G, modelIntMap, varIntMap);
            int[,] DirectedBGEdges = DirectedGraph(UnDirectBGInt, matchingEdges);
            var g = new Tarjan(DirectedBGEdges);
            var SCCs = g.GetStronglyConnectedComponents();

            //g = new Tarjan(new[,] {
            //    {1, 2}, {2, 3}, {2, 4}, {2, 5}, {3, 1}, {4, 1}, {4, 6}, {4, 8}, {5, 6}, {6, 7}, {7, 5}, {8, 6}
            //});
            //g = new Tarjan(new[,] {
            //    {1, 2}, {2,3 }, {3, 1}, {3, 4}, {4, 5}, {5, 6}, {6, 3}
            //});
            g = new Tarjan(new[,] {
                {1, 2}, {2,3 }, {3, 1}, {4, 3}, {5, 4}, {6, 5}, {3, 6}
            });
            SCCs = g.GetStronglyConnectedComponents();

            //Step2: Trim unnecessary arcs from D(G,M) by a strongly connected component decomposition algorithm.


            //Step:3 Call Enum_Maximum_Matching_Iter(G,M,D(G,M))


        }

        static Dictionary<int, List<int>> UndirectedGraphStringsToInt(Dictionary<string, List<string>> UGs, Dictionary<string, int> modIntMapping, Dictionary<string, int> varIntMapping) //this method converts the string nodes into int values for convienience in future
        {
            var returnUGint = new Dictionary<int, List<int>>(); //BG in the form of integers in it
            foreach (var pair in UGs)
            {
                var temp = new List<int>();
                foreach (string modelString in pair.Value)
                {
                    temp.Add(modIntMapping[modelString]);              //here modelIntMapping file is used to get int values of strings in adjecency list dictionary
                }
                returnUGint.Add(varIntMapping[pair.Key], temp);         //here varIntMapping file is used to get int values of strings in adjecency list dictionary
            }
            return returnUGint;
        }


        static int[,] DirectedGraph(Dictionary<int, List<int>> UGint, List<string> MEdges) //create and retrun directed graph (D) of the given graph G using matching M i.e. Graph D(G,M)
        {
            var DG_edgesList = new List<List<int>>();
            foreach (var pair in UGint)
            {
                foreach (int modint in pair.Value)
                {
                    if (MEdges.Contains(pair.Key.ToString() + modint.ToString()))
                    {
                        DG_edgesList.Add(new List<int> { modint, pair.Key });
                    }
                    else
                    {
                        DG_edgesList.Add(new List<int> { pair.Key, modint });
                    }

                }
            }

            return To2dArray(DG_edgesList);
        }

        static void TrimUnnecessaryEdges() //Trimming unnecessary edges to speed up the algorithm
        {

        }

        static void Enum_Maximum_Matchings_Iter()
        {

        }

        public static void ConvertGraphAdjecencyListToMatrixRepresentation() //takes adjecency list as input and returns matrix representation of the the graph
        {
            int[,] t = new int[,] { { 1, 2 }, { 2, 3 }, { 2, 4 }, { 2, 5 }, { 3, 1 }, { 4, 1 }, { 4, 6 }, { 4, 7 }, { 5, 6 }, { 6, 7 }, { 7, 5 }, { 5, 6 } };
            IEnumerable<int> ttt = t.Cast<int>();
            int tt = t.Cast<int>().Max();
        }

        public static int[,] To2dArray(List<List<int>> list) //public static int[,] To2dArray(this List<List<int>> list)
        {
            if (list.Count == 0 || list[0].Count == 0)
                throw new ArgumentException("The list must have non-zero dimensions.");

            var result = new int[list.Count, list[0].Count];
            for (int i = 0; i < list.Count; i++)
            {
                for (int j = 0; j < list[i].Count; j++)
                {
                    if (list[i].Count != list[0].Count)
                        throw new InvalidOperationException("The list cannot contain elements (lists) of different sizes.");
                    result[i, j] = list[i][j];
                }
            }

            return result;
        }

    }

}
