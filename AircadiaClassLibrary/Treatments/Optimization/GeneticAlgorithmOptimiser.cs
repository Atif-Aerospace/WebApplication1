using System;

using Aircadia.ObjectModel.Treatments.Optimisers.GA.Selection;
using Aircadia.ObjectModel.Treatments.Optimisers.GA.Crossover;
using Aircadia.ObjectModel.Treatments.Optimisers.GA.Mutation;

namespace Aircadia.ObjectModel.Treatments.Optimisers.GA
{
	[Serializable()]
    public abstract class GeneticAlgorithmOptimiser : Optimiser
    {
		public GAParameters GAParameters { get; set; }

		//total number of genetic algorithm generations
		protected int NoOfGenerations;
        //size of the population
        protected int PopulationSize;












        //selection operator
        protected SelectionOpr selectionOpr;
        //crossover operator
        protected CrossoverOpr crossoverOpr;
        //mutation operator
        protected MutationOpr mutationOpr;

        public GeneticAlgorithmOptimiser(string name, string description)
            : base(name, description)
        {
        }
        public GeneticAlgorithmOptimiser(string name, string description, GAParameters gaParameters)
            : base(name, description)
        {
			GAParameters = gaParameters;

            /*
            this.d2xOptimResult.OptimAllResults.Columns.Add(new DataColumn("Generation ID"));
            this.d2xOptimResult.OptimGenResults.Columns.Add(new DataColumn("Generation ID"));
            for (int i = 0; i < template.DesignVariables.Count(); i++)
            {
                this.d2xOptimResult.OptimAllResults.Columns.Add(new DataColumn("Design Variable " + i));
                this.d2xOptimResult.OptimGenResults.Columns.Add(new DataColumn("Design Variable " + i));
            }
            for (int i = 0; i < template.Objectives.Count(); i++)
            {
                this.d2xOptimResult.OptimAllResults.Columns.Add(new DataColumn("Objective " + i));
                this.d2xOptimResult.OptimGenResults.Columns.Add(new DataColumn("Objective " + i));
            }
            for (int i = 0; i < template.Constraints.Count(); i++)
            {
                this.d2xOptimResult.OptimAllResults.Columns.Add(new DataColumn("Constraint " + i));
                this.d2xOptimResult.OptimGenResults.Columns.Add(new DataColumn("Constraint " + i));
            }
            */
        }



    }
}
