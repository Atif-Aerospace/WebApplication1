using System;
using System.Collections.Generic;

namespace Aircadia.WorkflowManagement
{ // base class for GraphNode to create the graphNode
	[Serializable]
    public class Node : IEquatable<Node>, IEqualityComparer<Node>
    {
		public Node() { }
        public Node(string data) : this(data, null) { }
        public Node(string data, NodeList neighbors)
        {
			Value = data;
			Neighbors = neighbors ?? new NodeList();
        }

		public string Value { get; }

		public NodeList Neighbors { get; }
		public NodeList FromNeighbors { get; }
		public NodeList ToNeighbors { get; }

		public bool Equals(Node other) => Equals(this, other);
		public bool Equals(Node n1, Node n2) => n1.Value.Equals(n2.Value);
		public int GetHashCode(Node n) => n.Value.GetHashCode();
		public override string ToString() => Value;
	}
}



