
using System;
using System.Collections.Generic;

namespace Aircadia.WorkflowManagement
{//YHB:Modified for my own GraphDataSructure
 /*In this post I'll cover finding paths in a directed graph. We'll be using the Digraph data structure from my previous post.
 The code below will allow us to find paths from a source vertex s to any other connected vertex v as well as be able to tell us if v is not connected to s.
 This code uses a depth-first approach. Basically we start from a source vertex (s) and move out to all connected vertices and then move out from all those connected vertices until we've found all vertices connected to s. 
 We're careful not to "double back" or to go to a vertex more than once.
 Here's the code for depth first search for a directed graph in C#:  */

	/*
    * Use a depth first search method to find a path from source vertex s to target vertex v in a directed
    * graph (Digraph).  See a previous post for code for Digraph.
    * */
	public class DepthFirstDirectedPaths
    {
        private Dictionary<String, bool> _marked;  //keep track of whether vertex v is reachable from s
        private Dictionary<String, GraphNode> _edgeTo;  //keep track of the last edge on path s to v
        private readonly GraphNode _s; //source vertex

        public DepthFirstDirectedPaths(BipartiteGraph G, GraphNode s)
        {
            _marked = new Dictionary<string, bool>();//create dictionary containing vertex, statur(true/false) wehter marked or not for all vertices
            foreach (GraphNode item in G)
            {
                _marked.Add(item.Value.ToString(), false);
            }
            _edgeTo = new Dictionary<String, GraphNode>(); //create a boolean array for all vertices
			_s = s; //mark the source vertex
            DFS(G, s);
        }

        /*
        * A recursive function to do depth first search.
        * We start with the source vertex s, find all vertices connected to it and recursively call
        * DFS as move out from s to connected vertices.  We avoid "going backwards" or needlessly looking
        * at all paths by keeping track of which vertices we've already visited using the _marked[] array.
        * We keep track of how we're moving through the graph (from s to v) using _edgeTo[].
        */
        private void DFS(BipartiteGraph G, GraphNode v)
        { 
            _marked[v.Value.ToString()] = true;

            foreach (GraphNode w in v.Neighbors)
            {

                if (!_marked[w.Value.ToString()])
                {
                    _edgeTo[w.Value.ToString()] = v;
                    DFS(G, w);
                }
            }
        }

        /*
        * In the DFS method we've kept track of vertices connected to the source s
        * using the _marked[] array.
        * */
        public Boolean HasPathTo(GraphNode v)
        {
            //if(v == this._s) //added by YHB
            //{
            //    return false;
            //}
            return _marked[v.Value.ToString()];
        }

        /*
        * We can find the path from s to v working backwards using the _edgeTo array.
        * For example, if we want to find the path from 3 to 0.  We look at _edgeTo[0] which gives us
        * a vertex, say 2.  We then look at _edgeTo[2] and so on until _edgeTo[x] equals 3 (our 
        * source vertex).
        * */
        public IEnumerable<GraphNode> PathTo(GraphNode v) //this mehtods just returns the vertices in _edgeTo in the sequence till hte vertex to which path is to find out
        {
            if (!HasPathTo(v)) return null;
            var path = new Stack<GraphNode>();
            for (GraphNode x = v; x != _s; x = _edgeTo[x.Value.ToString()])
                path.Push(x);
            path.Push(_s);
            return path;
        }
    }
}
