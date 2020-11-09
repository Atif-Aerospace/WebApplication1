using System;
using Aircadia.ObjectModel.Treatments.Optimisers.GA.Population;

namespace Aircadia.ObjectModel.Treatments.Optimisers.GA
{
	[Serializable()]
    public abstract class MultiObjectiveGeneticAlgorithmOptimiser : GeneticAlgorithmOptimiser
    {
        //original/parent population (before applying GA operators)
        protected MultiObjectivePopulation parentPopulation;
        //new/child population (after applying GA operators)
        protected MultiObjectivePopulation childPopulation;

        public MultiObjectiveGeneticAlgorithmOptimiser(string name, string description)
            : base(name, description)
        {
        }
        public MultiObjectiveGeneticAlgorithmOptimiser(string name, string description, GAParameters gaParameters)
            : base(name, description, gaParameters)
        {
        }

        public MultiObjectivePopulation ParentPopulation
		{
			get => parentPopulation;
			set => parentPopulation = value;
		}

		public MultiObjectivePopulation ChildPopulation
        {
            get
            {
                return childPopulation;
            }
            //read-only, 
        }
    }
}
