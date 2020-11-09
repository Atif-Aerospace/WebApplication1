using Aircadia.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Aircadia.Utilities
{
	public class BipartiteGraph<T1, T2>
		where T1 : INamedComponent
		where T2 : INamedComponent
	{
		private readonly Graph graph;
		private readonly string t1Name;
		private readonly string t2Name;
		private readonly Dictionary<string, object> payloads = new Dictionary<string, object>();

		public List<Vertex> Vertices1 => graph.Vertices.Where(v => IsType1(v.Key)).Select(kvp => kvp.Value).ToList();
		public List<Vertex> Vertices2 => graph.Vertices.Where(v => IsType2(v.Key)).Select(kvp => kvp.Value).ToList();

		public Dictionary<string, T1> Payloads1 => graph.Vertices.Keys.Where(k => IsType1(k)).ToDictionary(k => k, v => (T1)payloads[v]);
		public Dictionary<string, T2> Payloads2 => graph.Vertices.Keys.Where(k => IsType2(k)).ToDictionary(k => k, v => (T2)payloads[v]);


		protected BipartiteGraph(Graph transposed, Dictionary<string, object> payloads) : this()
		{
			graph = transposed;
			this.payloads = new Dictionary<string, object>(payloads);
		}

		public BipartiteGraph()
		{
			graph = new Graph();
			// Add 1 or 2 in case is the same type
			t1Name = typeof(T1).Name + "1"; 
			t2Name = typeof(T2).Name + "2";
		}

		private string KeyOf(T1 t1) => KeyOf(t1Name, t1);
		private string KeyOf(T2 t2) => KeyOf(t2Name, t2);
		private string KeyOf(string tName, INamedComponent t) => $"{tName}:{t?.Name ?? throw new ArgumentNullException("name")}";

		private bool IsType1(string key) => key.Split(':').FirstOrDefault() == t1Name;
		private bool IsType2(string key) => key.Split(':').FirstOrDefault() == t2Name;

		private string AddPayload(T1 t1)
		{
			string key = KeyOf(t1: t1);
			if (!payloads.ContainsKey(key))
			{
				payloads.Add(key, t1);
			}
			return key;
		}
		private string AddPayload(T2 t2)
		{
			string key = KeyOf(t2: t2);
			if (!payloads.ContainsKey(key))
			{
				payloads.Add(key, t2);
			}
			return key;
		}

		private string RemovePayload(T1 t1)
		{
			string key = KeyOf(t1);
			payloads.Remove(key);
			return key;
		}
		private string RemovePayload(T2 t2)
		{
			string key = KeyOf(t2);
			payloads.Remove(key);
			return key;
		}

		public void AddVertex(T1 t1Object) => graph.AddVertex(AddPayload(t1Object));
		public void AddVertex(T2 t2Object) => graph.AddVertex(AddPayload(t2Object));

		public void RemoveVertex(T1 t1Object) => graph.RemoveVertex(RemovePayload(t1Object));
		public void RemoveVertex(T2 t2Object) => graph.RemoveVertex(RemovePayload(t2Object));

		public void AddVertices(IEnumerable<T1> t1Objects)
		{
			foreach (T1 t1 in t1Objects)
				AddVertex(t1);
		}
		public void AddVertices(IEnumerable<T2> t2Objects)
		{
			foreach (T2 t2 in t2Objects)
				AddVertex(t2);
		}

		public void AddEdge(T1 source, T2 sink) => graph.AddEdge(KeyOf(source), KeyOf(sink));
		public void AddEdge(T2 source, T1 sink) => graph.AddEdge(KeyOf(source), KeyOf(sink));

		public void RemoveEdge(T1 source, T2 sink) => graph.RemoveEdge(KeyOf(source), KeyOf(sink));
		public void RemoveEdge(T2 source, T1 sink) => graph.RemoveEdge(KeyOf(source), KeyOf(sink));

		public void AddUndirectedEdge(T1 source, T2 sink)
		{
			AddEdge(source, sink);
			AddEdge(sink, source);
		}
		public void AddUndirectedEdge(T2 source, T1 sink) => AddUndirectedEdge(sink, source);
		public void RemoveUndirectedEdge(T1 source, T2 sink)
		{
			RemoveEdge(source, sink);
			RemoveEdge(sink, source);
		}
		public void RemoveUndirectedEdge(T2 source, T1 sink) => RemoveUndirectedEdge(sink, source);

		public BipartiteGraph<T1, T2> Transpose() => new BipartiteGraph<T1, T2>(graph.Transpose(), payloads);

		public List<T2> GetChildren(T1 t1Object)
		{
			if (graph.Vertices.TryGetValue(KeyOf(t1Object), out Vertex vertex))
			{
				return vertex.Adjacent.Select(a => payloads[a.Id]).Cast<T2>().ToList();
			}
			return new List<T2>();
		}

		public List<T1> GetChildren(T2 t2Object)
		{
			if (graph.Vertices.TryGetValue(KeyOf(t2Object), out Vertex vertex))
			{
				return vertex.Adjacent.Select(a => payloads[a.Id]).Cast<T1>().ToList();
			}
			return new List<T1>();
		}

		public T2 GetChild(T1 t1Object) => GetChildren(t1Object).FirstOrDefault();
		public T1 GetChild(T2 t2Object) => GetChildren(t2Object).FirstOrDefault();

		public List<object> DepthFirstSearch(T1 t1Object) => DepthFirstSearch(KeyOf(t1Object));
		public List<object> DepthFirstSearch(T2 t2Object) => DepthFirstSearch(KeyOf(t2Object));
		private List<object> DepthFirstSearch(string name) => graph.DepthFirstSearch(name).Select(s => payloads[s]).ToList();

		public List<Tout> DepthFirstSearch<Tout>(T1 t1Object) where Tout : INamedComponent => DepthFirstSearch<Tout>(KeyOf(t1Object));
		public List<Tout> DepthFirstSearch<Tout>(T2 t2Object) where Tout : INamedComponent => DepthFirstSearch<Tout>(KeyOf(t2Object));
		private List<Tout> DepthFirstSearch<Tout>(string name) where Tout : INamedComponent => DepthFirstSearch(name).Where(pl => pl is Tout).Cast<Tout>().ToList();
	}
}
