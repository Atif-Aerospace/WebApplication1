using Aircadia.ObjectModel.DataObjects;
using Aircadia.ObjectModel.Workflows;
using Combinatorics.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Aircadia.WorkflowManagement
{
	using Combination = HashSet<Data>;
	using Set = HashSet<Data>;

	public class GuidingReversal : IEnumerable<Combination>
	{
		public Data[] RequestedOutputsToReverse { get; }
		public Data[] RequestedInputsToReverse { get; }
		public Data[] IndependentInputs { get; }
		private readonly Workflow workflow;
		private readonly IDependencyAnalysis dependencyAnalysis;

		private int NinRequest => RequestedInputsToReverse.Length;
		private int NoutRequest => RequestedOutputsToReverse.Length;

		public int NewNumberOfOutputs { get; private set; } = 0;
		public ReversalType Type { get; }

		public IEnumerable<Set> FixesByRemoving { get; }
		public IEnumerable<Set> FixesByAdding { get; }

		public GuidingReversal(Data[] requestedOutputsToReverse, Data[] requestedInputsToReverse, Data[] independentVariables, Workflow workflow)
		{
			RequestedOutputsToReverse = requestedOutputsToReverse;
			RequestedInputsToReverse = requestedInputsToReverse;
			IndependentInputs = independentVariables;
			this.workflow = workflow;
			dependencyAnalysis = workflow.DependencyAnalysis;

			if (NoutRequest > NinRequest)
			{
				Type = ReversalType.Outputs;
			}
			else
			{
				Type = ReversalType.Inputs;
			}

			FixesByRemoving = FixRequestByRemoving();
			FixesByAdding = FixRequestByAdding();
		}

		/*This method actually solves using the removing resolution scheme both determined
		* and undetermined types of incorrect reversal requests. */
		private IEnumerable<Combination> FixRequestByRemoving()
		{
			if (Type == ReversalType.Outputs)
			{
				List<Set> outputsDependencies = GetDependencies();
				int numberOfVariablesNeeded = GetNumberOfInputsToBeRemoved(outputsDependencies);
				var combinations = new Combinations<Data>(RequestedInputsToReverse, numberOfVariablesNeeded);

				return combinations.Select(c => new Set(c)).Where(c => GetNumberOfInputsToBeRemoved(outputsDependencies, c) == 0);
			}
			else
			{
				List<Set> inputsDependencies = GetDependencies();
				int numberOfVariablesNeeded = GetNumberOfOutputsToBeRemoved(inputsDependencies);
				var combinations = new Combinations<Data>(RequestedOutputsToReverse, numberOfVariablesNeeded);

				return combinations.Select(c => new Set(c)).Where(c => GetNumberOfOutputsToBeRemoved(inputsDependencies, c) == 0);
			}
		}

		/*This method actually solves using the adding resolution scheme both determined
         * and undetermined types of incorrect reversal requests. */
		private IEnumerable<Combination> FixRequestByAdding()
		{
			//set of dependencies for each output
			List<Set> dependencySets = Type == ReversalType.Inputs ? GetOutputDependencies(IndependentInputs) : GetInputDependencies();
			Data[] dependencyTargets = Type == ReversalType.Inputs ? RequestedInputsToReverse : RequestedOutputsToReverse;
			IEnumerable<Combination> combinations = GetCombinations(dependencySets, dependencyTargets);

			//// Do guided reversal using cumulative dep set for inputs and outputs
			//// needs to be filtered to include inputs in ReqRevInps
			//List<Combination> globalReversalCombinations = GetGlobalReversalCombinations(IndependentInputs, combinations);

			//// Filter reversals such that each rev comb entry must include all outputs from RequestedOutputsToReverse
			//var filteredGlobalReversalCombinations = globalReversalCombinations.Where(c => AllOutputsPresentFilter(c));

			//// Check determinacy such that each output links to atleast one input
			//var checkedGlobalReversalCombinations = filteredGlobalReversalCombinations.Where(c => AllInputsMappedFilter(c));

			//// Check at leaset one inpput is mapped for each of the original requested outputs in the reversal combination
			////create cumulative dependencies for outputs and inputs requested 
			//Set cumulativeDependencies = Accumulate(dependencySets);
			////Set inputCumulativeDependencies = Accumulate(inputsDependencySets);
			//var outputCheckedReversalCombinations = checkedGlobalReversalCombinations.Where(c => OutputDependenciesMappedFilter(c, cumulativeDependencies));

			//var reversalCombinations = outputCheckedReversalCombinations.Where(c => AllAddedVariablesDependenciesMappedFilter(c));

			//return reversalCombinations;

			return combinations;
		}

		/*This method actually solves using the adding resolution scheme both determined
         * and undetermined types of incorrect reversal requests. */
		private IEnumerable<Combination> GetCombinationsByAdding()
		{
			//set of dependencies for each output
			List<Set> outputsDependencySets = GetOutputDependencies(IndependentInputs);
			List<Set> inputsDependencySets = GetInputDependencies();

			//create cumulative dependencies for outputs and inputs requested 
			Set outputCumulativeDependencies = Accumulate(outputsDependencySets);
			Set inputCumulativeDependencies = Accumulate(inputsDependencySets);

			IEnumerable<Combination> outCombinations = GetCombinations(inputsDependencySets, RequestedOutputsToReverse);
			IEnumerable<Combination> inCombinations = GetCombinations(outputsDependencySets, RequestedInputsToReverse);

			// Do guided reversal using cumulative dep set for inputs and outputs
			// needs to be filtered to include inputs in ReqRevInps
			List<Combination> globalReversalCombinations = GetGlobalReversalCombinations(IndependentInputs, outCombinations);

			// Filter reversals such that each rev comb entry must include all outputs from RequestedOutputsToReverse
			var filteredGlobalReversalCombinations = globalReversalCombinations.Where(c => AllOutputsPresentFilter(c));

			// Check determinacy such that each output links to atleast one input
			var checkedGlobalReversalCombinations = filteredGlobalReversalCombinations.Where(c => AllInputsMappedFilter(c));

			// Check at leaset one inpput is mapped for each of the original requested outputs in the reversal combination
			var outputCheckedReversalCombinations = checkedGlobalReversalCombinations.Where(c => OutputDependenciesMappedFilter(c, outputCumulativeDependencies));

			var reversalCombinations = outputCheckedReversalCombinations.Where(c => AllAddedVariablesDependenciesMappedFilter(c));

			return reversalCombinations;
		}

		private IEnumerable<Combination> GetCombinations(List<Set> dependencySets, Data[] dependencyTargets)
		{
			///////////////////////NEW PART ADDED ON 22 Feb 2012////////////////////////////////
			List<int> nMappedToTargets = CountElementsInSets(dependencyTargets, dependencySets);
			int nUnmapped = nMappedToTargets.Count(i => i == 0);
			////////////////////////////////////////////////////////////////////////////////////

			IEnumerable<Set> unmappedSets = dependencySets.Where((s, i) => nMappedToTargets[i] == 0);

			var indices = Accumulate(unmappedSets).Select((data, i) => new { data, i }).ToDictionary(d => d.data, d => Convert.ToChar(d.i));
			var dependencies = indices.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
			char[] outsToAddCharVersion = indices.Values.ToArray();

			IEnumerable<Combination> combinations = new Combinations<char>(outsToAddCharVersion, nUnmapped)
				.Select(comb => new Combination(comb.Select(c => dependencies[c])))
				.Where(comb => CountElementsInSets(comb, dependencySets).Count(i => i == 0) == 0);
			return combinations;
		}

		private int GetNumberOfOutputsToBeRemoved(List<Set> outputsDependencies, IEnumerable<Data> removedOutputs = null)
		{
			// Number of requested output to reverse in each requested input dependency set
			IEnumerable<Data> outputs = removedOutputs is null ? RequestedOutputsToReverse : RequestedOutputsToReverse.Except(removedOutputs);
			List<int> requestedOutputsPerSet = CountElementsInSets(outputs, outputsDependencies);

			// Check that at least Nin outputs map to at lest one input
			IEnumerable<Data> unmmapedVariables = RequestedInputsToReverse.Where((d, i) => requestedOutputsPerSet[i] == 0);

			return unmmapedVariables.Count() - NinRequest + outputs.Count();
		}

		private int GetNumberOfInputsToBeRemoved(List<Set> outputsDependencies, IEnumerable<Data> removedInputs = null)
		{
			// Number of requested input to reverse in each requested output dependency set
			IEnumerable<Data> inputs = removedInputs is null ? RequestedInputsToReverse : RequestedInputsToReverse.Except(removedInputs);
			List<int> requestedInputsPerSet = CountElementsInSets(inputs, outputsDependencies);

			// Check that at least Nin outputs map to at lest one input
			IEnumerable<Data> unmmapedVariables = RequestedOutputsToReverse.Where((d, i) => requestedInputsPerSet[i] == 0);

			return unmmapedVariables.Count() - NoutRequest + inputs.Count();
		}

		private List<Set> GetOutputDependencies(Data[] independentVariables) => RequestedOutputsToReverse.Select(o => dependencyAnalysis.BackwardTrace(o, independentVariables)).ToList();

		private List<Set> GetInputDependencies() => RequestedOutputsToReverse.Select(i =>
		{
			HashSet<Data> set = dependencyAnalysis.BackwardTrace(i);
			set.Remove(i); // removes element traced
			return set;
		}).ToList();

		private List<Combination> GetGlobalReversalCombinations(Data[] independentVariables, IEnumerable<Combination> combinations)
		{
			var globalReversalCombinations = new List<Combination>();

			// then need to change below for loop to add the combination each time instead of just 'inpDep'
			//****************************************************
			foreach (Combination combination in combinations)
			{
				int foundCounter = CountElementsInSet(RequestedOutputsToReverse, combination);
				if (foundCounter == combination.Count())
				{
					//create the set to have it's cumalitive dep analysed
					var set = new List<Data>();
					foreach (Data output in RequestedOutputsToReverse)
					{
						set.Add(output);
					}
					foreach (Data output in combination)
					{
						set.Add(output);// this (indep)should be adding a set of 1 or more, depending on how many inputs were orignally not linked
					}
					set.Sort();

					////get and store cum dep results
					//List<Data> combinationDependencies = AccumulateDependencies(set.ToArray(), independentVariables);
					//combinationDependencies.Sort();

					//calculate reversal combinations of variables in cumaltive dep set (tempCumDep)
					Data[] requestedOutputs = set.ToArray(); // set.Aggregate(String.Empty, (s, d) => s += d.Name, s => s.TrimEnd(',', ' '));
					Data[] requestedInputs = new Data[0]; // "";

					var reversalCombinations = new ReversalCombinations(requestedOutputs, requestedInputs, workflow);
					//reversalCombinations.GetReversalCombinations();
					//List<Combination> revCombinations = reversalCombinations.Reversals;

					foreach (Combination combinatn in reversalCombinations)
					{
						globalReversalCombinations.Add(new Combination(requestedOutputs.Concat(combinatn)));
					}
				}
			}

			return globalReversalCombinations;
		}

		private bool AllOutputsPresentFilter(Combination combination) => CountElementsInSet(RequestedOutputsToReverse, combination) == RequestedOutputsToReverse.Length;

		private bool AllInputsMappedFilter(Combination combination)
		{
			var combinationOuts = combination.Take(NewNumberOfOutputs);
			var combinationInps = combination.Skip(NewNumberOfOutputs);

			List<int> counters = CountElementsInSets(combinationInps, combinationOuts.Select(o => dependencyAnalysis.BackwardTrace(o, IndependentInputs)));
			return !counters.Contains(0);
		}

		private bool OutputDependenciesMappedFilter(Combination combination, Set cumulativeDependency) => CountElementsInSet(cumulativeDependency, combination) == RequestedOutputsToReverse.Length;

		private bool AllAddedVariablesDependenciesMappedFilter(Combination combination)
		{
			var addedVariables = new List<Data>();
			foreach (Data element in combination)
			{
				if (!RequestedOutputsToReverse.Contains(element) && !IndependentInputs.Contains(element))
				{
					addedVariables.Add(element);
				}
			}

			List<Data> addedVariablesDependencies = AccumulateDependencies(addedVariables.ToArray(), IndependentInputs);

			int foundCounter = CountElementsInSet(addedVariablesDependencies, combination);
			return foundCounter >= addedVariables.Count;
		}

		private static Set Accumulate(IEnumerable<Set> sets)
		{
			var cumulativeSet = new Set();
			foreach (Set set in sets)
			{
				cumulativeSet.UnionWith(set);
			}

			return cumulativeSet;
		}

		private List<Data> AccumulateDependencies(Data[] variables, Data[] independentVariables)
		{
			var dependencies = new HashSet<Data>();
			foreach (Data variable in variables)
			{
				dependencies.UnionWith(dependencyAnalysis.BackwardTrace(variable, independentVariables));
			}

			return dependencies.ToList();
		}

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

		private List<int> CountElementsInSets(IEnumerable<Data> elements, IEnumerable<Set> sets) => sets.Select(set => CountElementsInSet(elements, set)).ToList();

		private int CountElementsInSet(IEnumerable<Data> elements, Set set) => elements.Count(e => set.Contains(e));

		public IEnumerator<Set> GetEnumerator() => GetCombinationsByAdding().GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
