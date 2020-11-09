using System;
using System.Collections.Generic;
using System.Linq;
using Aircadia.ObjectModel;
using Aircadia.ObjectModel.DataObjects;
using Aircadia.ObjectModel.Workflows;
using Combinatorics.Collections;
using System.Collections;
using Aircadia.Utilities;

namespace Aircadia.WorkflowManagement
{
	using Set = HashSet<Data>;
	using Combination = HashSet<Data>;

	public class ReversalCombinations : IEnumerable<Combination>
	{
		private readonly IDependencyAnalysis dependencyAnalysis;
		private readonly Workflow workflow;

		public Data[] RequestedInputsToReverse { get; }
		public Data[] RequestedOutputsToReverse { get; }
		public List<Data> IndependentInputs { get; }

		private int NinRequest => RequestedInputsToReverse.Length;
		private int NoutRequest => RequestedOutputsToReverse.Length;
		private int Nin => IndependentInputs.Count;
		private int Nout => workflow.ModelDataOutputs.Count;

		public ReversalType Type { get; private set; }

		public ReversalCombinations(Data[] requestedOutputsToReverse, Data[] requestedInputsToReverse, Workflow workflow)
		{
			RequestedOutputsToReverse = requestedOutputsToReverse;
			RequestedInputsToReverse = requestedInputsToReverse;
			this.workflow = workflow;
			dependencyAnalysis = workflow.DependencyAnalysis;
			IndependentInputs = workflow.ModelDataInputs;

			CheckReversalRequestCorrectness();
		}

		private void CheckReversalRequestCorrectness()
		{
			// Check there are not more requested inputs than workflow inputs or outputa
			if (NinRequest > Nin)
			{
				throw new ArgumentException($"The number of requested inputs is {NinRequest}, but the workflow contains only {Nin} inputs");
			}
			if (NinRequest > Nout)
			{
				throw new ArgumentException($"The number of requested inputs is {NinRequest}, but the workflow contains only {Nout} outputs");
			}

			// Check reversed inputs are amongst the workfolw inputs
			var inputsHash = new HashSet<string>(workflow.ModelDataInputs.GetNames());
			foreach (var data in this.RequestedInputsToReverse.GetNames())
			{
				if (!inputsHash.Contains(data))
				{
					throw new ArgumentException($"The requested input {data} is not amongst the workflow inputs");
				}
			}

			// Check there are not more requested outputs than workflow workfolw outputs
			if (NoutRequest > Nout)
			{
				throw new ArgumentException($"The number of requested outputs is {NoutRequest}, but the workflow contains only {Nout} outputs");
			}
			if (NoutRequest > Nin)
			{
				throw new ArgumentException($"The number of requested outputs is {NoutRequest}, but the workflow contains only {Nin} inputs");
			}

			// Check reversed outputs are amongst the workfolw outputs
			var outputsHash = new HashSet<string>(workflow.ModelDataOutputs.GetNames());
			foreach (var data in this.RequestedOutputsToReverse.GetNames())
			{
				if (!outputsHash.Contains(data))
				{
					throw new ArgumentException($"The requested input {data} is not amongst the workflow outputs");
				}
			}


			if (NoutRequest > NinRequest)
			{
				Type = ReversalType.Outputs;

				// Validate determined reversal
				List<Set> outputsDependencies = GetDependencies();

				// Number of requested input to reverse in each requested output dependency set
				List<int> requestedInputsPerSet = CountElementsInSets(RequestedInputsToReverse, outputsDependencies);

				// Check that at least Nin outputs map to at lest one input
				IEnumerable<Data> unmmapedVariables = RequestedOutputsToReverse.Where((d, i) => requestedInputsPerSet[i] == 0);
				if (unmmapedVariables.Count() > NoutRequest - NinRequest)
				{
					string unmmapedString = unmmapedVariables.Aggregate(String.Empty, (t, l) => t += l.Id + ", ", t => t.TrimEnd(' ', ','));
					throw new ArgumentException($"Only {requestedInputsPerSet.Count - unmmapedVariables.Count()} outputs map to an input variable" +
						$", however, {RequestedInputsToReverse.Length} outputs are needed.\n\r {unmmapedString} are the unmapped ones. Try fixing guidance");
				}
			}
			else
			{
				Type = ReversalType.Inputs;

				// Validate determined reversal
				List<Set> outputsDependencies = GetDependencies();

				// Number of requested output to reverse in each requested input dependency set
				List<int> requestedOutputsPerSet = CountElementsInSets(RequestedOutputsToReverse, outputsDependencies);

				// Check that at least Nin outputs map to at lest one input
				IEnumerable<Data> unmmapedVariables = RequestedInputsToReverse.Where((d, i) => requestedOutputsPerSet[i] == 0);
				if (unmmapedVariables.Count() > NinRequest - NoutRequest)
				{
					string unmmapedString = unmmapedVariables.Aggregate(String.Empty, (t, l) => t += l.Id + ", ", t => t.TrimEnd(' ', ','));
					throw new ArgumentException($"Only {requestedOutputsPerSet.Count - unmmapedVariables.Count()} inputs map to an output variable" +
						$", however, {RequestedOutputsToReverse.Length} inputs are needed.\n\r {unmmapedString} are the unmapped ones. Try fixing guidance");
				}
			}
		}

		private IEnumerable<Combination> GetCombinationsEnumerable()
		{
			List<Set> dependencySets = GetDependencies();
			Set accumulatedDependencies = AccumulateSets(dependencySets);
			accumulatedDependencies.OrderBy(d => d);

			// List<int> counters = GetNumberOfElementsInEachSet(dependencySets, Type);

			// Create the reversal sets, nCr approach needed
			int numberOfVariablesNeeded = Math.Abs(NinRequest - NoutRequest);
			//(Type == ReversalType.Inputs)
			//? RequestedInputsToReverse.Length
			//: RequestedOutputsToReverse.Length;

			// Get combinations
			foreach (var data in (Type == ReversalType.Inputs) ? RequestedOutputsToReverse : RequestedInputsToReverse)
			{
				accumulatedDependencies.Remove(data);
			}

			var indices = accumulatedDependencies.Select((data, i) => new { data, i }).ToDictionary(d => d.data, d => Convert.ToChar(d.i));
			var dependencies = indices.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);

			char[] inputSetTest = accumulatedDependencies.Select(d => indices[d]).ToArray();
			var combinations = new Combinations<char>(inputSetTest, numberOfVariablesNeeded);

			foreach (IList<char> combination in combinations)
			{
				// A COMBINATION IS OBTAIN BUT IS YET TO APPLY VARIOUS FILTERINGS
				Set c = new Set(combination.Select(e => dependencies[e]));
				foreach (var data in (Type == ReversalType.Inputs) ? RequestedOutputsToReverse : RequestedInputsToReverse)
				{
					c.Add(data);
				}

				// i.e. that at least one dependency exists between recommended reversals.
				// i.e. also check determinacy when considering reversal that may cause under-/over-determined models.
				// see section 2.3.1. of report.
				if (!AllSetMappedCheck(c))
				{
					continue;
				}

				// Now need to filter results if: i.e. rev combs for outputs, but some inputs must be included
				// and vice versa.
				//if (!AllInputsOutputsMappedCheck(c, Type))
				//{
				//	continue;
				//}

				
				
				yield return c;
			}
		}

		private bool AllSetMappedCheck(Combination combination)
		{
			if (Type == ReversalType.Inputs)
			{
				BipartiteGraph<Data, Data> G = GraphBuilder.BipartiteFromTwoSets(RequestedInputsToReverse, combination,
				i => GetDependenciesFromInput(i), i => GetDependenciesFromInput(i));
				List<(Data, Data)> Matching = MaximumMatching.Get(G);
				return Matching.Count == NinRequest;
			}
			else
			{
				BipartiteGraph<Data, Data> G = GraphBuilder.BipartiteFromTwoSets(RequestedOutputsToReverse, combination,
					o => GetDependenciesFromOutput(o), o => GetDependenciesFromOutput(o));
				List<(Data, Data)> Matching = MaximumMatching.Get(G);
				return Matching.Count == NoutRequest;
			}
		}

		//private bool AllInputsOutputsMappedCheck(Combination combination, ReversalType type)
		//{
		//	if (type == ReversalType.Outputs && NinRequest > 0)
		//	{
		//		// filter rev combs by checking INPUTS to reverse supplied are included in reversal combinations.
		//		return CountElementsInSet(RequestedInputsToReverse, combination) == NinRequest;
		//	}
		//	else if (type == ReversalType.Inputs && NoutRequest > 0)
		//	{
		//		// filter rev combs by checking OUTPUTS to reverse supplied are included in reversal combinations.
		//		return CountElementsInSet(RequestedOutputsToReverse, combination) == NoutRequest;
		//	}
		//	else
		//	{
		//		return true;
		//	}
		//}

		private static Set AccumulateSets(List<Set> VariablesDependencies)
		{
			var accumulatedDependencies = new Set();
			foreach (Set set in VariablesDependencies)
				accumulatedDependencies.UnionWith(set);

			return accumulatedDependencies;
		}

		private List<int> CountElementsInSets(IEnumerable<Data> elements, IEnumerable<Set> sets) => sets.Select(set => CountElementsInSet(elements, set)).ToList();

		private int CountElementsInSet(IEnumerable<Data> elements, Set set) => elements.Count(e => set.Contains(e));

		private List<Set> GetDependencies()
		{
			//get the output dependencies
			if (Type == ReversalType.Outputs)
			{
				return RequestedOutputsToReverse.Select(o => GetDependenciesFromOutput(o)).ToList();
			}
			else
			{
				return RequestedInputsToReverse.Select(i => GetDependenciesFromInput(i)).ToList();
			}
		}

		private Set GetDependenciesFromOutput(Data data) => dependencyAnalysis.BackwardTrace(data, IndependentInputs);

		private Set GetDependenciesFromInput(Data data)
		{
			Set set = dependencyAnalysis.ForwardTrace(data);
			set.Remove(data);
			return set;
		}

		public IEnumerator<Set> GetEnumerator() => GetCombinationsEnumerable().GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}

	public enum ReversalType { Inputs, Outputs }
}
