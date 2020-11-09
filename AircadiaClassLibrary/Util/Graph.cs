using System;
using System.Collections.Generic;
using System.Linq;

namespace Aircadia.Utilities
{
	public class Graph
	{
		public Dictionary<string, Vertex> Vertices { get; }

		public List<Edge> Edges
		{
			get
			{
				var edges = new List<Edge>();
				foreach (Vertex source in Vertices.Values)
					foreach (Vertex sink in source.Adjacent)
						edges.Add(new Edge(source, sink));
				return edges;
			}
		}

		public Graph() => Vertices = new Dictionary<string, Vertex>();

		public void AddVertex(string name)
		{
			if (!Vertices.ContainsKey(name))
				Vertices.Add(name, new Vertex(name));
		}

		public void RemoveVertex(string name)
		{
			if (Vertices.ContainsKey(name))
				Vertices.Remove(name);
		}

		public void AddVertices(IEnumerable<string> names)
		{
			foreach (string name in names)
				AddVertex(name);
		}

		public void AddEdge(string source, string sink)
		{
			Vertex sourceVertex = Vertices.ContainsKey(source) ? Vertices[source] : null;
			Vertex sinkVertex = Vertices.ContainsKey(sink) ? Vertices[sink] : null;
			if (sourceVertex != null && sinkVertex != null)
				sourceVertex.AddAdajactent(Vertices[sink]);
		}

		public void AddEdge(Edge edge) => AddEdge(edge.Source, edge.Sink);

		public void AddEdge(Vertex source, Vertex sink) => AddEdge(source.Id, sink.Id);

		public void RemoveEdge(Edge edge) => RemoveEdge(edge.Source, edge.Sink);

		public void RemoveEdge(Vertex source, Vertex sink) => RemoveEdge(source.Id, sink.Id);

		public void RemoveEdge(string source, string sink)
		{
			Vertex sourceVertex = Vertices.ContainsKey(source) ? Vertices[source] : null;
			Vertex sinkVertex = Vertices.ContainsKey(sink) ? Vertices[sink] : null;
			if (sourceVertex != null && sinkVertex != null)
				sourceVertex.RemoveAdajactent(Vertices[sink]);
		}

		public bool IsAcyclic()
		{
			var hash = new HashSet<string>();
			foreach (Vertex vertex in Vertices.Values)
			{
				if (!IsAcyclic_(vertex, hash))
				{
					return false;
				}
			}

			return true;
		}

		private bool IsAcyclic_(Vertex sourceVertex, HashSet<string> hash)
		{
			if (sourceVertex == null)
			{
				throw new ArgumentNullException(nameof(sourceVertex));
			}

			if (hash.Contains(sourceVertex.Id))
			{
				return true;
			}

			var stack = new Stack<Vertex>();
			stack.Push(sourceVertex);

			while (stack.Count > 0)
			{
				Vertex vertex = stack.Pop();
				hash.Add(vertex.Id);

				foreach (Vertex v in vertex.Adjacent)
				{
					if (hash.Contains(v.Id))
					{
						return false;
					}
					else
					{
						stack.Push(v);
					}
				}
			}

			return true;
		}
	}

	public class Vertex
	{
		public HashSet<Vertex> Adjacent { get; } = new HashSet<Vertex>();
		public string Id { get; }

		public Vertex(string id) => Id = id;

		public void AddAdajactent(Vertex adjacent) => Adjacent.Add(adjacent);
		public void RemoveAdajactent(Vertex adjacent) => Adjacent.Remove(adjacent);

		public override string ToString() => Id;
	}

	public class Edge
	{
		public Vertex Source { get; }
		public Vertex Sink { get; }

		public Edge(Vertex source, Vertex sink)
		{
			Source = source;
			Sink = sink;
		}

		public override string ToString() => $"{Source} -> {Sink}";
	}

	public class Vertex<T>
	{
		public HashSet<Vertex<T>> Adjacent { get; } = new HashSet<Vertex<T>>();
		public T Id { get; }

		public Vertex(T id) => Id = id;

		public void AddAdajactent(Vertex<T> adjacent) => Adjacent.Add(adjacent);
		public void RemoveAdajactent(Vertex<T> adjacent) => Adjacent.Remove(adjacent);

		public override string ToString() => Id.ToString();
		public override bool Equals(object obj) => obj is Vertex<T> v && Id.Equals(v.Id);
		public override int GetHashCode() => Id.GetHashCode();
	}

	public class Edge<T>
	{
		public Vertex<T> Source { get; }
		public Vertex<T> Sink { get; }

		public Edge(Vertex<T> source, Vertex<T> sink)
		{
			Source = source;
			Sink = sink;
		}

		public override string ToString() => $"{Source} -> {Sink}";
		public override bool Equals(object obj) => obj is Edge<T> v && Source.Id.Equals(v.Source.Id) && Sink.Id.Equals(v.Sink.Id);
		public override int GetHashCode() => (Source.Id, Sink.Id).GetHashCode();
	}

	public class Graph<T>
	{
		public Dictionary<T, Vertex<T>> Vertices { get; }

		public IEnumerable<Edge<T>> Edges => Vertices.Values.SelectMany(source => source.Adjacent.Select(sink => new Edge<T>(source, sink)));

		public Graph() => Vertices = new Dictionary<T, Vertex<T>>();

		public void AddVertex(T item)
		{
			if (!Vertices.ContainsKey(item))
			{
				Vertices.Add(item, new Vertex<T>(item));
			}
		}

		public void RemoveVertex(T item)
		{
			if (Vertices.ContainsKey(item))
			{
				Vertices.Remove(item);
			}
		}

		public void AddVertices(IEnumerable<T> names)
		{
			foreach (T name in names)
			{
				AddVertex(name);
			}
		}

		public Edge<T> AddEdge(T source, T sink)
		{
			if (Vertices.TryGetValue(source, out Vertex<T> sourceVertex) && Vertices.TryGetValue(sink, out Vertex<T> sinkVertex))
			{
				sourceVertex.AddAdajactent(sinkVertex);
				return new Edge<T>(sourceVertex, sinkVertex);
			}
			return null;
		}

		public void AddEdge(Edge<T> edge) => AddEdge(edge.Source, edge.Sink);

		public void AddEdge(Vertex<T> source, Vertex<T> sink) => AddEdge(source.Id, sink.Id);

		public void RemoveEdge(Edge<T> edge) => RemoveEdge(edge.Source, edge.Sink);

		public void RemoveEdge(Vertex<T> source, Vertex<T> sink) => RemoveEdge(source.Id, sink.Id);

		public void RemoveEdge(T source, T sink)
		{
			if (Vertices.TryGetValue(source, out Vertex<T> sourceVertex) && Vertices.TryGetValue(sink, out Vertex<T> sinkVertex))
			{
				sourceVertex.RemoveAdajactent(sinkVertex);
			}
		}
	}

	public static class GraphExtensions
	{
		public static Graph Transpose(this Graph g)
		{
			var transposed = new Graph();
			transposed.AddVertices(g.Vertices.Keys);
			foreach (Edge edge in g.Edges)
			{
				transposed.AddEdge(edge.Sink, edge.Source);
			}
			return transposed;
		}

		public static Graph<T> Transpose<T>(this Graph<T> g)
		{
			var transposed = new Graph<T>();
			transposed.AddVertices(g.Vertices.Keys);
			foreach (Edge<T> edge in g.Edges)
			{
				transposed.AddEdge(edge.Sink, edge.Source);
			}
			return transposed;
		}

		public static HashSet<string> DepthFirstSearch(this Graph g, string name)
		{
			var hash = new HashSet<string>();
			g.DepthFirstSearch_(name, hash);
			return hash;
		}

		public static HashSet<string> DepthFirstSearch(this Graph g, IEnumerable<string> names)
		{
			var hash = new HashSet<string>();
			foreach (string name in names)
				g.DepthFirstSearch_(name, hash);
			return hash;
		}

		public static HashSet<string> DepthFirstSearch(this Graph g, IEnumerable<string> names, HashSet<string> hash)
		{
			var returnHash = new HashSet<string>();
			foreach (string name in names)
				g.DepthFirstSearch_(name, hash, returnHash);
			return returnHash;
		}


		private static void DepthFirstSearch_(this Graph g, string name, HashSet<string> hash, HashSet<string> returnHash)
		{
			Vertex sourceVertex = g.Vertices.ContainsKey(name) ? g.Vertices[name] : null;
			if (sourceVertex != null)
			{
				var stack = new Stack<Vertex>();
				//if (!hash.Contains(sourceVertex.Name))
				stack.Push(sourceVertex);

				while (stack.Count > 0)
				{
					Vertex vertex = stack.Pop();
					hash.Add(vertex.Id);
					returnHash.Add(vertex.Id);

					foreach (Vertex v in vertex.Adjacent)
						if (!hash.Contains(v.Id))
							stack.Push(v);
				}

			}
		}

		private static void DepthFirstSearch_(this Graph g, string name, HashSet<string> hash)
		{
			if (g.Vertices.TryGetValue(name, out Vertex sourceVertex))
			{
				var stack = new Stack<Vertex>();
				//if (!hash.Contains(sourceVertex.Name))
				stack.Push(sourceVertex);

				while (stack.Count > 0)
				{
					Vertex vertex = stack.Pop();
					hash.Add(vertex.Id);

					foreach (Vertex v in vertex.Adjacent)
						if (!hash.Contains(v.Id))
							stack.Push(v);
				}
			}
			else
			{
				throw new KeyNotFoundException($"Variable '{name}' is not present in the workkflow, dependency analysis it's not possible");
			}
		}

		public static HashSet<string> BreadthFirstSearch(this Graph g, string name)
		{
			var hash = new HashSet<string>();
			g.BreadthFirstSearch_(name, hash);
			return hash;
		}

		public static HashSet<string> BreadthFirstSearch(this Graph g, IEnumerable<string> names)
		{
			var hash = new HashSet<string>();
			foreach (string name in names)
				g.BreadthFirstSearch_(name, hash);
			return hash;
		}

		private static void BreadthFirstSearch_(this Graph g, string name, HashSet<string> hash)
		{
			Vertex sourceVertex = g.Vertices.ContainsKey(name) ? g.Vertices[name] : null;
			if (sourceVertex != null)
			{
				var queue = new Queue<Vertex>();
				if (!hash.Contains(sourceVertex.Id))
					queue.Enqueue(sourceVertex);

				while (queue.Count > 0)
				{
					Vertex vertex = queue.Dequeue();
					hash.Add(vertex.Id);

					foreach (Vertex v in vertex.Adjacent)
						if (!hash.Contains(v.Id))
							queue.Enqueue(v);
				}

			}
		}

		public static List<string> TopologicalSort(this Graph g)
		{
			// Call DFS
			DepthFirstSearch_Times(g.Vertices.Values, out Dictionary<string, int> discoveryTimes, out Dictionary<string, int> finishTimes);
			// As each vertex is finished, insert it onto the front of a linked list
			IOrderedEnumerable<KeyValuePair<string, int>> orderedPairs = finishTimes.OrderByDescending(kvp => kvp.Value);
			// Retturn list of vertices
			return orderedPairs.Select(kvp => kvp.Key).ToList();
		}

		public static List<List<string>> StronglyConnectedComponents(this Graph g)
		{
			// Call DFS
			DepthFirstSearch_Times(g.Vertices.Values, out Dictionary<string, int> discoveryTimes, out Dictionary<string, int> finishTimes);

			var transposedGraph = new Graph();
			transposedGraph.AddVertices(g.Vertices.Keys);
			foreach (Edge edge in g.Edges)
				transposedGraph.AddEdge(edge.Sink, edge.Source);

			DepthFirstSearch_Forest(finishTimes.OrderByDescending(kvp => kvp.Value).Select(kvp => transposedGraph.Vertices[kvp.Key]), out List<List<string>> forest);

			return forest;
		}

		private static HashSet<string> DepthFirstSearch_Forest(IEnumerable<Vertex> vertices, out List<List<string>> trees)
		{
			var hash = new HashSet<string>();
			trees = new List<List<string>>();
			foreach (Vertex v in vertices)
			{
				if (!hash.Contains(v.Id))
				{
					var tree = new List<string>();
					DepthFirstSearch_ForestVisit(v, hash, tree);
					trees.Add(tree);
				}
			}
			return hash;
		}

		private static void DepthFirstSearch_ForestVisit(Vertex vertex, HashSet<string> hash, List<string> tree)
		{
			// Discovery
			hash.Add(vertex.Id);
			tree.Add(vertex.Id);

			// Process Childs
			foreach (Vertex v in vertex.Adjacent)
				if (!hash.Contains(v.Id))
					DepthFirstSearch_ForestVisit(v, hash, tree);
		}

		private static HashSet<string> DepthFirstSearch_Times(IEnumerable<Vertex> vertices, out Dictionary<string, int> discoveryTimes, out Dictionary<string, int> finishTimes)
		{
			var hash = new HashSet<string>();
			discoveryTimes = new Dictionary<string, int>();
			finishTimes = new Dictionary<string, int>();
			int time = 0;
			foreach (Vertex v in vertices)
				if (!hash.Contains(v.Id))
					DepthFirstSearch_TimesVisit(v, hash, discoveryTimes, finishTimes, ref time);
			return hash;
		}

		private static void DepthFirstSearch_TimesVisit(Vertex vertex, HashSet<string> hash, Dictionary<string, int> discoveryTimes, Dictionary<string, int> finishTimes, ref int time)
		{
			// Discovery
			time++;
			discoveryTimes[vertex.Id] = time;
			hash.Add(vertex.Id);

			// Process Childs
			foreach (Vertex v in vertex.Adjacent)
				if (!hash.Contains(v.Id))
					DepthFirstSearch_TimesVisit(v, hash, discoveryTimes, finishTimes, ref time);

			time++;
			finishTimes[vertex.Id] = time;
		}



		public static (HashSet<Vertex<T>> reachable, Dictionary<Vertex<T>, int> distances, Dictionary<Vertex<T>, Vertex<T>> parents) BreadthFirstSearch<T>(this Graph<T> g, T s)
		{
			var reachable = new HashSet<Vertex<T>>();
			var (distances, parents) = g._BreadthFirstSearch(s, ref reachable);
			return (reachable, distances, parents);
		}
		private static (Dictionary<Vertex<T>, int> distances, Dictionary<Vertex<T>, Vertex<T>> parents) _BreadthFirstSearch<T>(this Graph<T> g, T s, ref HashSet<Vertex<T>> reachable)
		{
			if (g.Vertices.TryGetValue(s, out Vertex<T> sourceVertex))
			{
				var distances = new Dictionary<Vertex<T>, int>();
				var parents = new Dictionary<Vertex<T>, Vertex<T>>();
				foreach (Vertex<T> vertex in g.Vertices.Values)
				{
					distances[vertex] = Int32.MaxValue;
					parents[vertex] = null;
				}
				distances[sourceVertex] = 0;
				parents[sourceVertex] = null;

				var Q = new Queue<Vertex<T>>();

				Q.Enqueue(sourceVertex);
				reachable.Add(sourceVertex);

				while (Q.Count > 0)
				{
					Vertex<T> u = Q.Dequeue();
					foreach (Vertex<T> v in u.Adjacent)
					{
						if (!reachable.Contains(v))
						{
							reachable.Add(v);
							distances[v] = distances[u] + 1;
							parents[v] = u;
							Q.Enqueue(v);
						}
					}
				}

				return (distances, parents);
			}
			else
			{
				throw new KeyNotFoundException($"Item '{s}' is not present in the graph");
			}
		}

		public static (Dictionary<Vertex<T>, int> startTimes, Dictionary<Vertex<T>, int> finsihTimes, Dictionary<Vertex<T>, Vertex<T>> parents)
			DepthFirstSearch<T>(this Graph<T> g)
		{
			var reachable = new HashSet<Vertex<T>>();
			var startTimes = new Dictionary<Vertex<T>, int>();
			var finishTimes = new Dictionary<Vertex<T>, int>();
			var parents = new Dictionary<Vertex<T>, Vertex<T>>();
			foreach (Vertex<T> vertex in g.Vertices.Values)
			{
				parents[vertex] = null;
			}
			int time = 0;
			foreach (Vertex<T> vertex in g.Vertices.Values)
			{
				if (!reachable.Contains(vertex))
				{
					time = g._DepthFirstSearch(vertex, reachable, time, startTimes, finishTimes, parents);
				}
			}
			return (startTimes, finishTimes, parents);
		}

		public static (HashSet<Vertex<T>> reachable, Dictionary<Vertex<T>, int> startTimes, Dictionary<Vertex<T>, int> finsihTimes, Dictionary<Vertex<T>, Vertex<T>> parents)
			DepthFirstSearch<T>(this Graph<T> g, T s)
		{
			var reachable = new HashSet<Vertex<T>>();
			var startTimes = new Dictionary<Vertex<T>, int>();
			var finishTimes = new Dictionary<Vertex<T>, int>();
			var parents = new Dictionary<Vertex<T>, Vertex<T>>();
			foreach (Vertex<T> vertex in g.Vertices.Values)
			{
				parents[vertex] = null;
			}

			if (g.Vertices.TryGetValue(s, out Vertex<T> sourceVertex))
			{
				g._DepthFirstSearch(sourceVertex, reachable, 0, startTimes, finishTimes, parents);
				return (reachable, startTimes, finishTimes, parents);
			}
			else
			{
				throw new KeyNotFoundException($"Item '{s}' is not present in the graph");
			}
		}

		private static int _DepthFirstSearch<T>(this Graph<T> g, Vertex<T> s, HashSet<Vertex<T>> reachable, int time,
			Dictionary<Vertex<T>, int> startTimes, Dictionary<Vertex<T>, int> finishTimes, Dictionary<Vertex<T>, Vertex<T>> parents)
		{
			Visit(s);
			return time;

			void Visit(Vertex<T> u)
			{
				time++;
				reachable.Add(u);
				startTimes[u] = time;
				foreach (Vertex<T> v in u.Adjacent)
				{
					if (!reachable.Contains(v))
					{
						parents[v] = u;
						Visit(v);
					}
				}
				time++;
				finishTimes[u] = time;
			}
		}

		public static List<Vertex<T>> TopologicalSort<T>(this Graph<T> g)
			=> g.DepthFirstSearch().finsihTimes.OrderByDescending(kvp => kvp.Value).Select(kvp => kvp.Key).ToList();

		public static List<List<Vertex<T>>> StronglyConnectedComponents<T>(this Graph<T> g)
		{
			var (startTiems, finishTimes, _) = g.DepthFirstSearch();

			Graph<T> gt = g.Transpose();

			return DepthFirstSearch_Forest(finishTimes.OrderByDescending(kvp => kvp.Value).Select(kvp => gt.Vertices[kvp.Key.Id]));
		}

		private static List<List<Vertex<T>>> DepthFirstSearch_Forest<T>(IEnumerable<Vertex<T>> vertices)
		{
			var hash = new HashSet<Vertex<T>>();
			var trees = new List<List<Vertex<T>>>();
			foreach (Vertex<T> v in vertices)
			{
				if (!hash.Contains(v))
				{
					var tree = new List<Vertex<T>>();
					DepthFirstSearchForestVisit(v, tree);
					trees.Add(tree);
				}
			}
			return trees;

			void DepthFirstSearchForestVisit(Vertex<T> vertex, List<Vertex<T>> tree)
			{
				// Discovery
				hash.Add(vertex);
				tree.Add(vertex);

				// Process Childs
				foreach (Vertex<T> v in vertex.Adjacent)
				{
					if (!hash.Contains(v))
					{
						DepthFirstSearchForestVisit(v, tree);
					}
				}
			}
		}

		/// <summary>
		/// Edmonds-Karp algorithm (Ford-Fulkerson + finding augmwnting path with BFS)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="g"></param>
		/// <param name="s"></param>
		/// <param name="t"></param>
		/// <param name="capacities"></param>
		/// <returns></returns>
		public static (int MaxFlow, Dictionary<Edge<T>, int> Flows) MaxFlow<T>(this Graph<T> g, Vertex<T> s, Vertex<T> t, Dictionary<Edge<T>, int> capacities)
		{
			var flows = g.Edges.ToDictionary(e => e, e => 0);

			(List<Edge<T>> path, int capacity) = PathInResidualNetwork(g, s, t, flows, capacities);
			while (path.Count > 0)
			{
				foreach (Edge<T> edge in path)
				{
					Vertex<T> u = g.Vertices[edge.Source.Id];
					Vertex<T> v = g.Vertices[edge.Sink.Id];
					if (u.Adjacent.Contains(v)) // (u,v) in E
					{
						var e = new Edge<T>(u, v);
						flows[e] = flows[e] + capacity;
					}
					else
					{
						var e = new Edge<T>(v, u);
						flows[e] = flows[e] - capacity;
					}
				}
				(path, capacity) = PathInResidualNetwork(g, s, t, flows, capacities);
			}

			int maxFlow = 0;
			foreach (var edge in g.Edges)
			{
				if (edge.Source == s)
				{
					maxFlow += flows[edge];
				}
				else if (edge.Sink == s)
				{
					maxFlow -= flows[edge];
				}
			}

			return (maxFlow, flows);
		}

		private static (List<Edge<T>> path, int capacity) PathInResidualNetwork<T>(Graph<T> g, Vertex<T> s, Vertex<T> t, Dictionary<Edge<T>, int> flows, Dictionary<Edge<T>, int> capacities)
		{
			// Create Gf
			var gf = new Graph<T>();
			var capacitiesf = new Dictionary<Edge<T>, int>();
			foreach (Vertex<T> vertex in g.Vertices.Values)
			{
				gf.AddVertex(vertex.Id);
			}
			foreach (Edge<T> e in g.Edges)
			{
				int flow = flows[e];
				int capacity = capacities[e];
				if (flow < capacity)
				{
					// Add (u,v) with cf(u,v) = c(u,v) - f(u,v)
					Edge<T> ef = gf.AddEdge(e.Source.Id, e.Sink.Id);
					capacitiesf[ef] = capacity - flow;
				}
				if (flow > 0)
				{
					// Add (v,u) with cf(v,u) = f(u,v)
					Edge<T> ef = gf.AddEdge(e.Sink.Id, e.Source.Id);
					capacitiesf[ef] = flow;
				}
			}

			// find path in Gf
			var (_, _, parents) = gf.BreadthFirstSearch(s.Id);
			var path = new List<Edge<T>>();
			int pathCapacity = Int32.MaxValue;
			Vertex<T> v = gf.Vertices[t.Id];
			while (parents[v] is Vertex<T> u)
			{
				var pe = new Edge<T>(u, v);
				path.Add(pe);
				v = u;
				pathCapacity = Math.Min(pathCapacity, capacitiesf[pe]);
			}

			return (path, pathCapacity);
		}
	}
}