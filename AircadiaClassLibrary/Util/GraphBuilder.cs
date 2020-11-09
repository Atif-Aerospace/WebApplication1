using Aircadia.ObjectModel;
using System;
using System.Collections.Generic;

namespace Aircadia.Utilities
{
	public static class GraphBuilder
	{

		public static Graph FromTwoSets<T1, T2>(IEnumerable<T1> set1, IEnumerable<T2> set2, Func<T1, IEnumerable<T2>> from1to2 = default(Func<T1, IEnumerable<T2>>), Func<T1, IEnumerable<T2>> to1from2 = default(Func<T1, IEnumerable<T2>>), Func<T1, string> name1 = default(Func<T1, string>), Func<T2, string> name2 = default(Func<T2, string>))
			where T1 : INamedComponent 
			where T2 : INamedComponent
		{
			if (name1 == default(Func<T1, string>))
				name1 = m => m.Name;

			if (name2 == default(Func<T2, string>))
				name2 = m => m.Name;

			if (from1to2 == default(Func<T1, IEnumerable<T2>>))
				from1to2 = m => new T2[0];

			if (to1from2 == default(Func<T1, IEnumerable<T2>>))
				to1from2 = m => new T2[0];

			var graph = new Graph();

			foreach (T1 member in set1)
				graph.AddVertex(name1(member));

			foreach (T2 member in set2)
				graph.AddVertex(name2(member));

			foreach (T1 member1 in set1)
			{
				foreach (T2 member2 in from1to2(member1))
					graph.AddEdge(name1(member1), name2(member2));

				foreach (T2 member2 in to1from2(member1))
					graph.AddEdge(name2(member2), name1(member1));
			}

			return graph;
		}

		public static BipartiteGraph<T1, T2> BipartiteFromTwoSets<T1, T2>(IEnumerable<T1> set1, IEnumerable<T2> set2, Func<T1, IEnumerable<T2>> from1to2 = default(Func<T1, IEnumerable<T2>>), Func<T1, IEnumerable<T2>> to1from2 = default(Func<T1, IEnumerable<T2>>))
			where T1 : INamedComponent
			where T2 : INamedComponent
		{
			if (from1to2 == default(Func<T1, IEnumerable<T2>>))
				from1to2 = m => new T2[0];

			if (to1from2 == default(Func<T1, IEnumerable<T2>>))
				to1from2 = m => new T2[0];

			var graph = new BipartiteGraph<T1, T2>();

			foreach (T1 member in set1)
				graph.AddVertex(t1Object: member);

			foreach (T2 member in set2)
				graph.AddVertex(t2Object: member);

			foreach (T1 member1 in set1)
			{
				foreach (T2 member2 in from1to2(member1))
					graph.AddEdge(member1, member2);

				foreach (T2 member2 in to1from2(member1))
					graph.AddEdge(member2, member1);
			}

			return graph;
		}
	}
}
