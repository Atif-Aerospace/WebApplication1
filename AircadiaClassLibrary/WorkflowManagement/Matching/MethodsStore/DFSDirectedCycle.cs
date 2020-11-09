using System;
using System.Collections.Generic;

namespace Aircadia.WorkflowManagement
{ 
	public class DFSDirectedCycle<T>
    {//YHB: Returns wether graph has cycle or not (given starting node). Does not give the cycle.

        private Dictionary<String, bool> marked;
        private Dictionary<String, bool> onStack;
        private GraphNode s;
        private BipartiteGraph g;

        private bool hasCycle;

        public bool HasCycle
        {
            get { return hasCycle; }
           // set { hasCycle = value; }
        }


        public DFSDirectedCycle(BipartiteGraph g, GraphNode s)
        {
            this.g = g;
            this.s = s;

            marked = new Dictionary<string, bool>();
            onStack = new Dictionary<string, bool>();

            foreach (GraphNode item in g.Nodes)
            {
                marked.Add(item.Value.ToString(), false);
                onStack.Add(item.Value.ToString(), false);
            }
            findCycle(g, s);
        }

        //public bool hasCycle()
        //{
        //    return hasCycle;
        //}

        public void findCycle(BipartiteGraph g, GraphNode v)
        {

            marked[v.Value.ToString()]= true;
            onStack[v.Value.ToString()] = true;

            foreach (GraphNode w in v.Neighbors)
            {
                if (!marked[w.Value.ToString()])
                {
                    findCycle(g, w);
                }
                else if (onStack[w.Value.ToString()])
                {
                    hasCycle = true;
                    return;
                }
            }

            onStack[v.Value.ToString()] = false;
        }
    }


    #region "For Undirected Graph: Cycle Detection Code"

    //public class DFSCycle
    //{

    //    private boolean marked[];
    //    private int s;
    //    private Graph g;
    //    private boolean hasCycle;

    //    // s - starting node
    //    public DFSCycle(Graph g, int s)
    //    {
    //        this.g = g;
    //        this.s = s;
    //        marked = new boolean[g.vSize()];
    //        findCycle(g, s, s);
    //    }

    //    public boolean hasCycle()
    //    {
    //        return hasCycle;
    //    }

    //    public void findCycle(Graph g, int v, int u)
    //    {

    //        marked[v] = true;

    //        for (int w : g.getAdjacentNodes(v))
    //        {
    //            if (!marked[w])
    //            {
    //                marked[w] = true;
    //                findCycle(g, w, v);
    //            }
    //            else if (v != u)
    //            {
    //                hasCycle = true;
    //                return;
    //            }
    //        }

    //    }
    //}
    #endregion "For Undirected Graph: Cycle Detection Code" 
}
