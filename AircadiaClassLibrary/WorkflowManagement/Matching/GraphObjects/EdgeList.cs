using System;
using System.Collections;
using System.Collections.Generic;

namespace Aircadia.WorkflowManagement
{//YHB
	[Serializable]
    public class EdgeList : ICollection<Edge>
    {
		private Dictionary<string, Edge> edges = new Dictionary<string, Edge>();


		public int Count => edges.Count;

		public bool IsReadOnly => true;

		public void Add(Edge item) => edges.Add(item.Value, item);

		public void Clear() => edges.Clear();

		public bool Contains(Edge item) => edges.ContainsKey(item.Value) && edges[item.Value] == item;

		public void CopyTo(Edge[] array, int arrayIndex) => throw new NotImplementedException();

		public Edge FindByNodes(Node from, Node to) // method to find the particular edge from the list i.e. EdgeList, given the 'from' and 'to' node of the edge
        {
            if (from != null & to != null)
            {
				string value = $"{from.Value}-{to.Value}";
				// search the list for the value
				edges.TryGetValue(value, out Edge edge);
				return edge;
			}
            // if we reached here, we didn't find a matching node
            return null;
        }

		public IEnumerator<Edge> GetEnumerator() => edges.Values.GetEnumerator();

		public bool Remove(Edge item) => edges.Remove(item.Value);

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
