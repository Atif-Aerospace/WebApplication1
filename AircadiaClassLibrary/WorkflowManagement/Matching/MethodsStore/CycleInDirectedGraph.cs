using System.Collections.Generic;
using System.Linq;

namespace Aircadia.WorkflowManagement
{
	//YHB: This code initially was giving only wether cycle exist in graph or not. But a modified to get that cycle.
	// code is working fine(checked for some cases 10 cases checked) but may need thorough testing. This is used in enumerating matchings
	class CycleInDirectedGraph 
    {   
        private readonly Stack<GraphNode> stack = new Stack<GraphNode>();//YHB
		public bool HasCycle { get; }
        public List<GraphNode> Cycle { get; } = new List<GraphNode>();//YHB stores cycle in sequence

		public CycleInDirectedGraph(BipartiteGraph graph)
		{
			var whiteSet = new HashSet<GraphNode>(graph.Nodes.Select(n => n as GraphNode)); //unvisited
			var graySet = new HashSet<GraphNode>(); //visiting
			var blackSet = new HashSet<GraphNode>(); //visite4

			HasCycle = false;
			while (whiteSet.Count > 0)
			{
				stack.Clear();
				GraphNode current = whiteSet.First(); //ElementAt(random.Next(whiteSet.Count));

				if (DepthFirstSearch(current, whiteSet, graySet, blackSet))
				{
					Cycle.Add(stack.Pop());
					Cycle.AddRange(stack.TakeWhile(n => n != Cycle[0]));
					Cycle.Add(Cycle[0]);
					HasCycle =  true;
					break;
				}
			}
		}


        private bool DepthFirstSearch(GraphNode current, HashSet<GraphNode> whiteSet, HashSet<GraphNode> graySet, HashSet<GraphNode> blackSet)
        {
            stack.Push(current);//YHB
            //move current to gray set from white set and tehn explore it.
            MoveVertex(current, whiteSet, graySet);
            foreach (GraphNode neighbor in current.Neighbors)
            {
                //if in black set means already explored so continue.
                if (blackSet.Contains(neighbor))
                {                        
                    continue;
                }
                //if in gray set then cylce found.
                if (graySet.Contains(neighbor))
                {
                    stack.Push(neighbor);//YHB
                    return true;
                }
                if (DepthFirstSearch(neighbor, whiteSet, graySet, blackSet))
                {
                    return true;
                }
            }
            //move vertex from gray set to black set when done exploring
            MoveVertex(current, graySet, blackSet);
            stack.Pop();//YHB
            return false;
        }

        private void MoveVertex(GraphNode vertex, HashSet<GraphNode> sourceSet, HashSet<GraphNode> destinationSet)
        {
            sourceSet.Remove(vertex);
            destinationSet.Add(vertex);
        }


    }
}
