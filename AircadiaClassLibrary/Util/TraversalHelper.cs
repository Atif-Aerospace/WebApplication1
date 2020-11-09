using System;
using System.Collections.Generic;
using System.Linq;


namespace Aircadia.Utilities
{
	public static class TraversalHelper
	{
		public static void Preorder<TNode>(TNode initial, Func<TNode, IEnumerable<TNode>> childrenSelector, Action<TNode> action)
		{
			var visited = new HashSet<TNode>();
			Recurse(initial);
			void Recurse(TNode _node)
			{
				action(_node);
				visited.Add(_node);
				foreach (var child in childrenSelector(_node).Where(n => !visited.Contains(n)))
				{
					Recurse(child);
				}
			}
		}

		public static TNode PreorderSearch<TNode>(TNode initial, Func<TNode, IEnumerable<TNode>> childrenSelector, Predicate<TNode> predicate)
		{
			{
				var visited = new HashSet<TNode>();
				TNode result = default(TNode);
				Recurse(initial);
				return result;

				bool Recurse(TNode _node)
				{
					if (predicate(_node))
					{
						result = _node;
						return true;
					}
					visited.Add(_node);
					var children = childrenSelector(_node) ?? Enumerable.Empty<TNode>();
					foreach (var child in children.Where(n => !visited.Contains(n)))
					{
						if (Recurse(child))
						{
							break;
						}
					}
					return false;
				}
			}
			throw new NotImplementedException();
		}
	}
}
