using System;
using System.Collections.Generic;
using System.Linq;

namespace Aircadia.WorkflowManagement
{//Graph class contains list of nodes, list of edges and methods to add, remove edges or nodes from the graph; also contains other methods such as checking wether node contains or not in graph etc.
	[Serializable]
    public class BipartiteGraph : IEnumerable<Node>
    {
		public const string DuplicateIdentifier = "Duplicate_";

		public Dictionary<string, int> edgeNodesCost = new Dictionary<string, int>(); //YHB
        public Dictionary<string, int> modelAndItsOutputs = new Dictionary<string, int>(); //stores models and its number of outputs

        Dictionary<string, int> modNodeIntMap = new Dictionary<string, int>();

		public NodeList VariablesSet { get; } = new NodeList();
		public NodeList ModelsSet { get; } = new NodeList();
		public NodeList Nodes { get; }
		public EdgeList Edges { get; }
		//YHB

		public int Count => Nodes.Count;

		public BipartiteGraph() : this(null, null) { } //here this class constructor is called with inputs as null nodeSet and edgeSet. Here NodeList and EdgeList are created if they are null
        public BipartiteGraph(NodeList nodeSet, EdgeList edgeSet)
        {
			Nodes = nodeSet ?? new NodeList();
			Edges = edgeSet ?? new EdgeList();
        }

        // adds a node to the graph
        public void AddNode(GraphNode node)
        {
            Nodes.Add(node);
            if (node.NodeType == GraphNode.Type.Type2)
            {
				ModelsSet.Add(node);
            }
            if (node.NodeType == GraphNode.Type.Type1)
            {
				VariablesSet.Add(node);
            }
        }

		// adds a node to the graph
		public void AddNode(string value) => Nodes.Add(new GraphNode(value));

        // adds a node to the graph
		public void AddNode(string value, GraphNode.Type type) //for bipartite graph
        {
            var t = new GraphNode(value, type);
            Nodes.Add(t);
            if (type == GraphNode.Type.Type2)
            {
				ModelsSet.Add(t);
            }
            if (type == GraphNode.Type.Type1)
            {
				VariablesSet.Add(t);
            }

        }

		public void AddDirectedEdge(string from, string to, int cost, int cost2) => AddDirectedEdge(GetNode(from), GetNode(to), cost, cost2);
		public void AddDirectedEdge(GraphNode from, string to, int cost, int cost2) => AddDirectedEdge(from, GetNode(to), cost, cost2);
		public void AddDirectedEdge(string from, GraphNode to, int cost, int cost2) => AddDirectedEdge(GetNode(from), to, cost, cost2);
        public void AddDirectedEdge(GraphNode from, GraphNode to, int cost, int cost2)
        {
			from = Nodes.FindByValue(from.Value) as GraphNode;
			to = Nodes.FindByValue(to.Value) as GraphNode;
			if (from == null || to == null)
				return;

            from.Neighbors.Add(to);

			Edges.Add(new Edge(from, to, cost, cost2));   //YHB
			edgeNodesCost.Add($"{from.Value}-{to.Value}", cost);
        }

		public void RemoveDirectedEdge(string from, string to) => RemoveDirectedEdge(GetNode(from), GetNode(to));
		public void RemoveDirectedEdge(GraphNode from, string to) => RemoveDirectedEdge(from, GetNode(to));
		public void RemoveDirectedEdge(string from, GraphNode to) => RemoveDirectedEdge(GetNode(from), to);
		public void RemoveDirectedEdge(GraphNode from, GraphNode to)
		{
			Edge edge = Edges.FindByNodes(from, to);
			if (edge != null)
			{
				from = edge.FromNode;
				to = edge.ToNode;

				Edges.Remove(edge);

				if (from.Neighbors.Contains(to))
				{
					from.Neighbors.Remove(to);
				}

				if (to.Neighbors.Contains(from))
				{
					to.Neighbors.Remove(from);
				}
			}
        }

        public void AddUndirectedEdge(GraphNode from, GraphNode to, int cost, int cost2) //not in use...
        {
            from.Neighbors.Add(to);
            to.Neighbors.Add(from);

			Edges.Add(new Edge(from, to, cost, cost2));    //YHB
        }

		public bool Contains(string value) => Nodes.FindByValue(value) != null;

		//YHB: returns true if edge exists in a graph
		public bool Contains(GraphNode from, GraphNode to) => Edges.FindByNodes(from, to) != null;

		public bool Remove(GraphNode node) => Remove(node.Value);
		public bool Remove(string value)
        {
            // first remove the node from the nodeset
            var nodeToRemove = (GraphNode)Nodes.FindByValue(value);
            if (nodeToRemove == null)
                // node wasn't found
                return false;

            // otherwise, the node was found
            Nodes.Remove(nodeToRemove); //removing the node from the node of the graph
            if (nodeToRemove.NodeType == GraphNode.Type.Type2) //removing the same node from modeSet if its type is 'model'
            {
				ModelsSet.Remove(nodeToRemove);
            }
            if (nodeToRemove.NodeType == GraphNode.Type.Type1)//removing hte same node from varSet if its type is 'variable'
            {
				VariablesSet.Remove(nodeToRemove);
            }


            // enumerate through each node in the nodeSet, removing edges to this node
            foreach (GraphNode gnode in Nodes)
            {//not deleting properly the edges while deleting the node... index is not found properly 27-12-2016...check edge deletion works or not
                if (gnode.Neighbors.Remove(nodeToRemove))
					Edges.Remove(Edges.FindByNodes(gnode, nodeToRemove)); //YHB: nodeToRemove is 'toNode' in the edge whereas 'fromNode' is a node(here gnode) in whos neighbors contains nodeToRemove
				if (nodeToRemove.Neighbors.Contains(gnode))
					Edges.Remove(Edges.FindByNodes(nodeToRemove, gnode));//added on 27-12-2016

            }

            return true;
        }

		//YHB: this method was not there in original code. this was required to solve errors of IEnumeration
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
		//YHB: this method was not there in original code. this was required to solve errors of IEnumeration
		public IEnumerator<Node> GetEnumerator() => Nodes.GetEnumerator();

		//YHB: this mehtod returns the GraphNode object with given value
		public GraphNode GetNode(string value) => (GraphNode)Nodes.FindByValue(value);

		//For graph
		public List<NodeList> GetLinkedListRepresentation() => Nodes.Select(n => (n as GraphNode).Neighbors).ToList();

		

        public void ReverseEdgeDirection(Edge edge) //reversing the edge after matching involves deleting that edge and adding new edge with reverse direction as below
        {
            RemoveDirectedEdge(edge.FromNode, edge.ToNode); //removing exisitng edge
            AddDirectedEdge(edge.ToNode, edge.FromNode, edge.Cost, edge.Cost2); //adding edge inbetween the same nodes with direction opposite at that of initial edge direction
        }

		//taken from enumeratingmatchinginbipartiteGraph class.... decide where to keep this method here or in the 'enumeratingmathcihgin...' class
        public EdgeList ToEdges(List<GraphNode> cycle) 
        {
            var elist = new EdgeList();
            for (int i = 1; i < cycle.Count; i++)
            {
                Edge edg = Edges.FindByNodes(cycle[cycle.Count - i], cycle[cycle.Count - i - 1]);
                elist.Add(edg);
            }
            return elist;
        }

		//taken from enumeratingmatchinginbipartiteGraph class.... decide where to keep this method here or in the 'enumeratingmathcihgin...' class
        public List<string> ToEdgesAsStrings(List<GraphNode> cycle) 
        {
            var elist = new List<string>();
            for (int i = 0; i < cycle.Count - 1; i++)
            {
				string edg = cycle[i].Value.ToString() + '-' + cycle[i + 1].Value.ToString();
                elist.Add(edg);
            }
            return elist;
        }

		//Here matching edges nodes are checked if it is model or variable
        //And then directed graph is created, where matching edges are oriented from model to variable whereas,
        //other graph edges(original graph edges) are from variable to model.
        public BipartiteGraph GetDirectedGraphAsPerMatching(EdgeList M)
        {
			foreach (Edge edge in M)
            {
				GraphNode from = edge.FromNode;
				GraphNode to = edge.ToNode;
				if (from != null && to != null)
                {
                    if (from.NodeType == GraphNode.Type.Type2)
                    {
                        if (Contains(from, to))
                        {
							RemoveDirectedEdge(to, from);
                        }
                        else if (Contains(to, from))
                        {
							RemoveDirectedEdge(to, from);
							AddDirectedEdge(from, to, 0, 0);
                        }
                    }
                    else
                    {
                        if (Contains(from, to))
                        {
							RemoveDirectedEdge(from, to);
							AddDirectedEdge(to, from, 0, 0);
                        }
                        else if (Contains(to, from))
                        {
							RemoveDirectedEdge(from, to);
                        }
                    }
                }
            }
            return this;
        }

        public NodeList GetUnmatchedVertices(List<string> M)
        {
            var matchedNodesAsStrings = new HashSet<string>();
            foreach (string str in M)
            {
				string[] s = str.Split('-');
                matchedNodesAsStrings.Add(s[0]);
                matchedNodesAsStrings.Add(s[1]);
            }
            var unmatchedVertices = new NodeList();
            foreach (GraphNode gn in Nodes)
            {
                if (!matchedNodesAsStrings.Contains(gn.Value))
                {
                    unmatchedVertices.Add(gn);
                }
            }
            return unmatchedVertices;
        }

        public Dictionary<string, string> duplicateModToModel = new Dictionary<string, string>();

		public void UpdateAddingDuplicateNodes()
        {
			var incidentEdges = new Dictionary<string, List<Edge>>();
			foreach (Edge edge in Edges)
			{
				string modelName = edge.ToNode.Value;
				if (incidentEdges.ContainsKey(modelName))
				{
					incidentEdges[modelName].Add(edge);
				}
				else
				{
					incidentEdges.Add(modelName, new List<Edge> { edge });
				}
			}

            foreach (KeyValuePair<string, int> M_Os in modelAndItsOutputs)
            {
				AddDuplicateNode(M_Os.Key, M_Os.Value, incidentEdges);
            }
        }

		//This method adds the duplicate(which is temperary) nodes in the original graph to take care of multiple outputs from a model
        public void AddDuplicateNode(string name, int Nout, Dictionary<string, List<Edge>> incidentEdges)
        {
			//number of duplicate nodes that will be added will be less than the number of outputs of the model
            var addedDuplicateNodes = new GraphNode[Nout - 1]; 
            for (int i = 0; i < Nout - 1; i++)
			{
				var duplicate = new GraphNode(DuplicateName(name, i + 1), GraphNode.Type.Type2);
				duplicateModToModel.Add(DuplicateName(name, i + 1), name);
				AddNode(duplicate);
				addedDuplicateNodes[i] = duplicate;
			}

            foreach (Edge edge in incidentEdges[name]) //adding links to duplicate nodes similar to original node
            {
                for (int i = 0; i < Nout - 1; i++)
                {
					AddDirectedEdge(edge.FromNode, addedDuplicateNodes[i], edge.Cost, edge.Cost2);
                }
            }
        }

		private static string DuplicateName(string name, int i) => $"{DuplicateIdentifier}{name}_{i}";

		/*public List<List<String>> MatchingToEdgeListAsValues()
        {
            this.GetAdjListInTheFormOfIntegers();
            IList<int> match = HopcroftKarp.GetMatching(this.AdjList, this.ModSet.Count);
            List<List<String>> MatchingEdgeList = new List<List<String>>();
            int i = 0;
            foreach (int item in match)
            {
                if (item != -1)
                {
                    MatchingEdgeList.Add(new List<String> { varSet[i++].Value, this.GetNode(this.modNodeIntMap.FirstOrDefault(x => x.Value == item).Key).Value});                                                                                                                                      // 'variables set' to 'model set'
                }
                else
                {
                    /*variable associated at that location is unmatched///Implement to store unmatched variable information if required
                }
            }

            return MatchingEdgeList;
        } */

		//     public void plotMethod(GoView goView1, List<String> M, Dictionary<string, string> dmToM)
		//     {
		//         goView1.Document.Clear();
		//         var nodesAdded = new Dictionary<string, GoBasicNode>();
		//         foreach (GraphNode gn in Nodes)
		//         {//Adding graphical objects on canvas associated with each node in the graph
		//             if (gn.NodeType == "Variable") //variable type of nodes are represented by ovals
		//             {
		//                 var nod = new GoBasicNode();
		//                 nod.Shape = new GoEllipse();
		//                 nod.LabelSpot = GoObject.Middle;
		//                 nod.Text = gn.Value.ToString();
		//                 nod.Label.Editable = true;
		//                 nod.Label.Multiline = true;
		//                 nod.Shape.FillShapeHighlight(Color.FromArgb(255, 0, 0), Color.FromArgb(255, 0, 0));
		//                 nodesAdded.Add(gn.Value.ToString(), nod);
		//                 goView1.Document.Add(nod);
		//             }

		//             if (gn.NodeType == "Model") //model type of nodes are represented by rectangle
		//             {
		//                 var nod = new GoBasicNode();
		//                 nod.Shape = new GoRectangle();
		//                 nod.LabelSpot = GoObject.Middle;
		//                 nod.Text = gn.Value.ToString();
		//                 nod.Label.Editable = true;
		//                 nod.Label.Multiline = true;
		//                 nod.Shape.FillShapeHighlight(Color.FromArgb(123, 104, 238), Color.FromArgb(123, 104, 238));
		//                 goView1.Document.Add(nod);
		//                 nodesAdded.Add(gn.Value.ToString(), nod);
		//                 goView1.Document.Add(nod);

		//             }

		//         }


		//         foreach (GraphNode gn in Nodes)
		//         {//links are created here

		//             foreach (GraphNode gnNeighbor in gn.Neighbors)
		//             {
		//                 var lnk = new GoLink();
		//                 lnk.Style = GoStrokeStyle.Bezier;
		//                 lnk.ToArrow = true;
		//                 if (gn.NodeType == "model")
		//                 {
		//                     lnk.ToArrow = true;
		//                     //lnk.PenColor = Color.Yellow;
		//                     //lnk.Highlight = true;
		//                     //lnk.PenWidth = 5;
		//                     lnk.FromPort = nodesAdded[gn.Value.ToString()].Port;
		//                     lnk.ToPort = nodesAdded[gnNeighbor.Value.ToString()].Port;
		//                 }
		//                 else
		//                 {
		//                     lnk.ToArrow = true;//lnk.FromArrow = true;
		//                     //lnk.PenColor = Color.Yellow;
		//                     //lnk.Highlight = true;
		//                     //lnk.PenWidth = 5;
		//                     lnk.ToPort = nodesAdded[gn.Value.ToString()].Port;
		//                     lnk.FromPort = nodesAdded[gnNeighbor.Value.ToString()].Port;
		//                 }
		//                 lnk.ToArrow = false;//Added on 06-01-2017
		//                 lnk.FromArrow = true;//Added on 06-01-2017
		//                 goView1.Document.Add(lnk);
		//             }
		//         }

		//         if (M != null)
		//         {
		//             foreach (String eString in M)
		//             {
		//                 String[] eNodes = eString.Split('-');
		//                 GoBasicNode gn1;
		//                 GoBasicNode gn2;
		//                 if (dmToM.ContainsKey(eNodes[0]))
		//                 {
		//                     gn1 = (GoBasicNode)goView1.Document.FindNode(dmToM[eNodes[0]]);
		//                 }
		//                 else
		//                 {
		//                     gn1 = (GoBasicNode)goView1.Document.FindNode(eNodes[0]);
		//                 }
		//                 if (dmToM.ContainsKey(eNodes[1]))
		//                 {
		//                     gn2 = (GoBasicNode)goView1.Document.FindNode(dmToM[eNodes[0]]);
		//                 }
		//                 else
		//                 {
		//                     gn2 = (GoBasicNode)goView1.Document.FindNode(eNodes[1]);
		//                 }


		//                 foreach (GoLink lnk in (gn2 as GoNode).Links)
		//                 {
		//                     if (lnk.FromNode == gn1 && lnk.ToNode == gn2)
		//                     {
		//                         //lnk.ToPort = gn2.Port;
		//                         //lnk.FromPort = gn1.Port;
		//                         lnk.ToArrow = true;
		//                         lnk.FromArrow = false;
		//                         lnk.PenColor = Color.Red;
		//                         lnk.Highlight = true;
		//                         lnk.PenWidth = 5;
		//                         break;

		//                     }
		//                 }
		//             }
		//         }

		//// This code makes the model node red if it is overconstrained
		////if (M != null)
		////{
		////    foreach (GraphNode gn in this.getUnmatchedVerticesOfGraph(M))
		////    {
		////        if (gn.NodeType == "Model")
		////        {
		////            GoBasicNode gnRed = (GoBasicNode)goView1.Document.FindNode(gn.Value);
		////            gnRed.Shape.FillShapeHighlight(Color.Red, Color.Red);
		////        }
		////    }
		////}

		//ForceAction(goView1);
		//customlayout(goView1);
		//     }

		//     void customlayout(GoView goView1)
		//     {
		//         var layout = new GoLayoutLayeredDigraph();
		//         layout.Document = goView1.Document;
		//         layout.DirectionOption = GoLayoutDirection.Left; //Down, Top, Left
		//         layout.SetsPortSpots = false;  // for nicer looking links near single-Port nodes when not Orthogonal
		//         layout.ColumnSpacing = 15;
		//         layout.LayerSpacing = 50;
		//         layout.ArrangementOrigin = new PointF(goView1.DocExtentCenter.X, goView1.DocExtentCenter.Y - 300);
		//         layout.PerformLayout();

		//     }


		//     void LayerAction(GoView goView1)
		//     {
		//         var layout = new GoLayoutLayeredDigraph();
		//         layout.Document = goView1.Document;
		//         // maybe set other properties too . . .

		//         //layout.DirectionOption = GoLayoutDirection.Down; // it is working direction is down , same as that of default setting in this program(as AirCADia Explorer)

		//         layout.PerformLayout();
		//     }

		//     void ForceAction(GoView goView1)
		//     {
		//         var layout = new GoLayoutForceDirected();
		//         layout.Document = goView1.Document;
		//         // maybe set other properties too . . .
		//         layout.PerformLayout();
		//     }

		//     void TreeAction(GoView goView1)
		//     {
		//         var layout = new GoLayoutTree();
		//         layout.Document = goView1.Document;
		//         // maybe set other properties too . . .
		//         layout.PerformLayout();
		//     }
	}
}
