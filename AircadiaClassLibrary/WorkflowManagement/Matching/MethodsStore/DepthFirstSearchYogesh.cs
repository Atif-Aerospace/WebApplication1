using System.Collections.Generic;

namespace Aircadia.WorkflowManagement
{
	public class DepthFirstSearchYogesh
    {
        public List<string> functionsList = new List<string>();
        public List<string> solutionsList = new List<string>();

        public void findAffectedFunctionsAndSolutions(BipartiteGraph bg, GraphNode gn)
        {
            dfs_iter(gn);
        }

        public void dfs_iter(GraphNode n)
        {
            foreach (GraphNode nod in n.FromNeighbors)
            {
                if (nod.NodeType == GraphNode.Type.Type1)
                {
                    this.functionsList.Add(nod.Value);
                }
                if (nod.NodeType == GraphNode.Type.Type2)
                {
                    this.solutionsList.Add(nod.Value);
                }
                dfs_iter(nod);

            }
        }


    }
}
