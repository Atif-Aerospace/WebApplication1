using System;
using System.Collections.Generic;
namespace Aircadia.WorkflowManagement

{ //YHB
	[Serializable]
    public class Edge : IEquatable<Edge>, IEqualityComparer<Edge>//This class is created for edges of the graph
    {
        public Edge(GraphNode frm, GraphNode to, int cost, int cost2) //Constructor which creates edge with 'from' and 'to' node of the edge as input/arguments
        {
			FromNode = frm;
			ToNode = to;
			Cost = cost;
			Cost2 = cost2;
        }

		public GraphNode FromNode { get; set; }
		public GraphNode ToNode { get; set; }

		public int Cost { get; set; }
		//to store weight related to input or output
		public int Cost2 { get; set; }

		public string Value => $"{FromNode.Value}-{ToNode.Value}";

		public bool Equals(Edge other) => Equals(this, other);
		public bool Equals(Edge e1, Edge e2) => (e1.FromNode, e1.ToNode).Equals((e2.FromNode, e2.ToNode));
		public int GetHashCode(Edge e) => (e.FromNode, e.ToNode).GetHashCode();
		public override string ToString() => Value;
	}
}
