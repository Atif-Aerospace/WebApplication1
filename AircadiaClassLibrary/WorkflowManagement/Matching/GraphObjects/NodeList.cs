using System;
using System.Collections;
using System.Collections.Generic;

namespace Aircadia.WorkflowManagement
{//NodeList is class to store the list of nodes in a graph
	[Serializable]
    public class NodeList : ICollection<Node>
    {
		private Dictionary<string, Node> nodes = new Dictionary<string, Node>();

		public int Count => nodes.Count;

		public bool IsReadOnly => false;

        public Node FindByValue(string value)
        {
			nodes.TryGetValue(value, out Node node);
			return node;
        }

		public void Add(Node item) => nodes.Add(item.Value, item);

		public void Clear() => nodes.Clear();

		public bool Contains(Node item) => nodes.ContainsKey(item.Value) && nodes[item.Value] == item;

		public void CopyTo(Node[] array, int arrayIndex) => throw new NotImplementedException();

		public bool Remove(Node item) => nodes.Remove(item.Value);

		public IEnumerator<Node> GetEnumerator() => nodes.Values.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
