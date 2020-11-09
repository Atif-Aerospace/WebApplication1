using System;
using System.Collections.Generic;

namespace AirCADiaArchitect.Yogesh.OtherObjectTest
{
	public class TarjanCycleDetect_duplicate
    {
        private List<List<Vertex>> output;
        private Stack<Vertex> nodeStatck;
        private int index;

        public List<List<Vertex>> DetectCycle(DepGraph g)
        {
            output = new List<List<Vertex>>();
            index = 0;
            nodeStatck = new Stack<Vertex>();
            foreach (Vertex v in g.Vertices)
            {
                if (v.index < 0)
                {
                    StrongConnect(v);
                }
            }
            return output;
        }

        private void StrongConnect(Vertex v)
        {
            v.index = index;
            v.lowlink = index;
            index++;
            nodeStatck.Push(v);

            foreach (Vertex w in v.Dependencies)
            {
                if (w.index < 0)
                {
                    StrongConnect(w);
                    v.lowlink = Math.Min(v.lowlink, w.lowlink);
                }
                else if (nodeStatck.Contains(w))
                {
                    v.lowlink = Math.Min(v.lowlink, w.index);
                }
            }

            if (v.lowlink == v.index)
            {
                var scc = new List<Vertex>();
                Vertex w;
                do
                {
                    w = nodeStatck.Pop();
                    scc.Add(w);
                } while (v != w);
                output.Add(scc);
            }
        }
    }



    public class Vertex
    {
        public int ID = -1;
        public int index = -1;
        public int lowlink = -1;
        public List<Vertex> Dependencies = new List<Vertex>();
    }


    public class DepGraph
    {
        public readonly List<Vertex> Vertices = new List<Vertex>();

        public static DepGraph CreateGraph(int nodeCount)
        {
            var graph = new DepGraph();
            for (int i = 1; i <= nodeCount; i++)
            {
                var v = new Vertex();
                v.ID = i;
                graph.Vertices.Add(v);
            }
            return graph;
        }
    }
}
