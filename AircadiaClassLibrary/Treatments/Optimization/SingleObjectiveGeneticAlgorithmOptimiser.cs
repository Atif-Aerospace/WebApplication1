using System;

using Aircadia.ObjectModel.Treatments.Optimisers.GA.Chromosome;
using Aircadia.ObjectModel.Treatments.Optimisers.GA.Population;

namespace Aircadia.ObjectModel.Treatments.Optimisers.GA
{
	[Serializable()]
    public abstract class SingleObjectiveGeneticAlgorithmOptimiser : GeneticAlgorithmOptimiser
    {
        //original/parent population (before applying GA operators)
        protected SingleObjectivePopulation parentPopulation;
        //new/child population (after applying GA operators)
        protected SingleObjectivePopulation childPopulation;

        public SingleObjectiveChromosome BestChromosome;

        //variables for storing change in objectives and fitnesses in subsequent generations
        protected double bestObjectiveChange;
        protected double averageObjectiveChange;
        protected double maximumFitnessChange;
        protected double averageFitnessChange;
        protected double fitnessVarianceChange;

        public SingleObjectiveGeneticAlgorithmOptimiser(string name, string description, GAParameters gaParameters)
            : base(name, description, gaParameters)
        {
        }

        public SingleObjectivePopulation ParentPopulation
		{
			get => parentPopulation;
			set => parentPopulation = value;
		}

		public SingleObjectivePopulation ChildPopulation => childPopulation;

	}
}
