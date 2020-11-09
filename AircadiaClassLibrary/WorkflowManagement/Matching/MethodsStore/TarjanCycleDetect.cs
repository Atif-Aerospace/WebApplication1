using System;
using System.Collections.Generic;

namespace Aircadia.WorkflowManagement
{
	public class TarjanCycleDetect //TarjanCycleDetect is misleading:- It does not give the cycle in graph, it gives SCC in a graph.
    {//More details of this algoirthm can be found on https://en.wikipedia.org/wiki/Tarjan%27s_strongly_connected_components_algorithm
        //YHB: gives strongly connected component i.e. SCC (not cycle) in a graph
        private static List<List<GraphNode>> StronglyConnectedComponents;
        private static Stack<GraphNode> S;
        private static int index;
        private static BipartiteGraph dg; //graph directed graph(dg)
        public static List<List<GraphNode>> DetectCycle(BipartiteGraph g)
        {
            StronglyConnectedComponents = new List<List<GraphNode>>();
            index = 0;
            S = new Stack<GraphNode>();
            dg = g;
            foreach (GraphNode v in g.Nodes)
            {
                if (v.index < 0)
                {
                    strongconnect(v);
                }
            }
            return StronglyConnectedComponents;
        }

        private static void strongconnect(GraphNode v)
        {
            v.index = index;
            v.lowlink = index;
            index++;
            S.Push(v);

            foreach (GraphNode w in v.Neighbors)
            {
                if (w.index < 0)
                {
                    strongconnect(w);
                    v.lowlink = Math.Min(v.lowlink, w.lowlink);
                }
                else if (S.Contains(w))
                {
                    v.lowlink = Math.Min(v.lowlink, w.index);
                }
            }

            if (v.lowlink == v.index)
            {
                var scc = new List<GraphNode>();
                GraphNode w;
                do
                {
                    w = S.Pop();
                    scc.Add(w);
                } while (v != w);
                StronglyConnectedComponents.Add(scc);
            }

        }
    }
}

    //Note a DepGraph is just a list of Vertex. and Vertex has a list of other Vertex which represent the edges. Also index and lowlink are initialized to -1

    //EDIT: This is working...I just misinterpreted the results