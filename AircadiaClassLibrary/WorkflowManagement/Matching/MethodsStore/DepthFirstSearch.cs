using System;
using System.Collections.Generic;

namespace Aircadia.WorkflowManagement
{
	//YHB: modified for my GraphDataStructure
	/*Algorithms In C#: Graphs, Depth-First Search
    By Ron Kahr26.October 2011 09:53
    Solving most graph problems requires the examination of every edge and every vertex of a graph. Ignoring even
    a single ege may cause us to falsely conclude that a graph is not connected. In this and the next blog post I
    examine two basic search algorithms.In these we maintain a set of vertices in the following manner: intially 
    all vertices are unmarked except the source vertex.On each iteration of the algorithm, a vertex v from the set S is removed.
    For each of v's edges to other vertices (u) if u is unmarked we mark it then add it to S. This process continues until S becomes empty. 
    Any vertex that remains unmarked cannot be reached from a path that starts from s. 
    If we treat S as a queue, always removing the vertex that has mremained in S the longest, then the vertices in V will be searched from s in a breadth-first fashion. 
    That is, all vertices adjacent to s will be searched first, then the vertices adjacent to these will be searched. 
    If S is treated as a stack, always removing the most recently added vertex, then the vertices in V will be searched from s in a depth-first fashion. 
    Here is the code for an implementation of a Depth-First Search for a graph in C#:  
    */

	public class DepthFirstSearch
    {
        private Dictionary<String, bool> _marked; //keep track of which vertices have been visited
        private int _count;
        public int _path2Count = 0; // To get lenth 2 path of the graph for 'MaximumMatchings' class
        public List<String> length2Path = new List<string>();
        NodeList unmatchedVertices = new NodeList();

        /*
            * Find vertices connected to vertex s.
            * */
        public DepthFirstSearch(BipartiteGraph G, GraphNode s, NodeList unmatchedVertices)
        {
            this.unmatchedVertices = unmatchedVertices;
            _marked = new Dictionary<string, bool>();
            DFS(G, s);

        }

        // A recursive function to find which vertices are connected.
        private void DFS(BipartiteGraph G, GraphNode v)
        {

            length2Path.Add(v.Value);
            if (_path2Count++ == 2)
            {
                return;
            }
            _count++;
            //mark this vertex as being visited
            _marked.Add(v.Value.ToString(), true);
            /*
             * for each vertex w in the linked list for vertex v 
             * there exists an edge between v and w
             * if it is not in _marked it hasn't been visited 
             * yet so mark it then check all the vertices
             * in it's linked list (it has edges to).
             * */
            foreach (GraphNode w in v.Neighbors)
            {
                if (!_marked.ContainsKey(w.Value.ToString()))
                {
                    if (_path2Count < 2)
                    {
                        DFS(G, w);
                    }
                    else if (_path2Count == 2)
                    {
                        if (unmatchedVertices.Contains(w) || unmatchedVertices.Contains(G.GetNode(length2Path[0])))
                        {
                            DFS(G, w);
                        }
                        else
                        {
                            goto nextNeighbour;
                        }
                    }

                }
                else
                {
                    goto nextNeighbour;
                }

            nextNeighbour:;
            }
            return;
        }

        public Boolean Marked(GraphNode v) //not really necessary as already dictionarry is fulfilling its purpose
        {
            return _marked[v.Value.ToString()];
        }

        public int Count()
        {
            return _count;
        }
    }
}
