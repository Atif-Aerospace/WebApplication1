using System;

using Aircadia.ObjectModel.Treatments.Optimisers.Formulation;

using Aircadia.ObjectModel.Treatments.Optimisers.GA.Chromosome;

namespace Aircadia.ObjectModel.Treatments.Optimisers.GA.Population
{
	[Serializable()]
    public class SingleObjectivePopulation : Population
    {
        //values for these five variables will be assigned by ComputeObjectiveStatistics() and ComputeFitnessStatistics functions (not constructor)
        protected double bestObjective;
        protected double averageObjective;
        protected double bestFitness;
        protected double averageFitness;
        protected double fitnessVariance;
        //value for bestChromosome will be assigned by ComputeObjectiveStatistics() and ComputeFitnessStatistics functions (not constructor)
        protected SingleObjectiveChromosome bestChromosome;

        public SingleObjectivePopulation(OptimisationTemplate Template, GAParameters gaParameters, bool empty)
            : this(Template, gaParameters, gaParameters.PopulationSize, empty)
        {
        }

        public SingleObjectivePopulation(OptimisationTemplate Template, GAParameters gaParameters, int populationSize, bool empty)
            : base(Template, gaParameters, populationSize)
        {
			//initialising population chromosomes
			chromosomes = new SingleObjectiveChromosome[populationSize];
            for (int i = 0; i < Size; i++)
            {
				chromosomes[i] = new SingleObjectiveChromosome(Template, gaParameters, empty);
            }
			//initialising best chromosome
			bestChromosome = new SingleObjectiveChromosome(Template, gaParameters, true);
        }


        //******************************
        //*****statistics variables*****
        //******************************

        public void ComputeObjectiveStatistics()
        {
            int bestChromosomeID = 0;
			averageObjective = ((SingleObjectiveChromosome)chromosomes[0]).ObjectiveValue;
            for (int i = 1; i < Size; i++)
            {
				averageObjective += ((SingleObjectiveChromosome)chromosomes[i]).ObjectiveValue;
                if (!((chromosomes[bestChromosomeID]).IsBetterThan((chromosomes[i]))))
                    bestChromosomeID = i;
            }
			bestObjective = ((SingleObjectiveChromosome)chromosomes[bestChromosomeID]).ObjectiveValue;
			averageObjective /= Size;
			GetBestChromosome().Genes = this[bestChromosomeID].Genes;
			bestChromosome.EvaluateObjective();
			bestChromosome.EvaluateFitness();
			bestChromosome.EvaluateConstraints();
        }

        public void ComputeBestObjective()
        {
            int bestChromosomeID = 0;
            for (int i = 1; i < Size; i++)
            {
                if (!((chromosomes[bestChromosomeID]).IsBetterThan((chromosomes[i]))))
                    bestChromosomeID = i;
            }
			bestObjective = ((SingleObjectiveChromosome)chromosomes[bestChromosomeID]).ObjectiveValue;
        }

        public void ComputeAverageObjective()
        {
			averageObjective = ((SingleObjectiveChromosome)chromosomes[0]).ObjectiveValue;
            for (int i = 1; i < Size; i++)
				averageObjective += ((SingleObjectiveChromosome)chromosomes[i]).ObjectiveValue;
			averageObjective /= Size;
        }

        public void ComputeBestChromosome()
        {
            int bestChromosomeID = 0;
            for (int i = 1; i < Size; i++)
            {
                if (!((chromosomes[bestChromosomeID]).IsBetterThan((chromosomes[i]))))
                    bestChromosomeID = i;
            }
			GetBestChromosome().Genes = this[bestChromosomeID].Genes;
			bestChromosome.EvaluateObjective();
			bestChromosome.EvaluateFitness();
			bestChromosome.EvaluateConstraints();
        }

        public void ComputeFitnessStatistics()
        {
            int bestChromosomeID = 0;
			averageFitness = ((SingleObjectiveChromosome)chromosomes[0]).FitnessValue;
			fitnessVariance = ((SingleObjectiveChromosome)chromosomes[0]).FitnessValue * ((SingleObjectiveChromosome)chromosomes[0]).FitnessValue;
            for (int i = 1; i < Size; i++)
            {
				averageFitness += ((SingleObjectiveChromosome)chromosomes[i]).FitnessValue;
				fitnessVariance += ((SingleObjectiveChromosome)chromosomes[i]).FitnessValue * ((SingleObjectiveChromosome)chromosomes[i]).FitnessValue;
                if (!((chromosomes[bestChromosomeID]).IsBetterThan((chromosomes[i]))))
                    bestChromosomeID = i;
            }
			bestFitness = ((SingleObjectiveChromosome)chromosomes[bestChromosomeID]).FitnessValue;
			averageFitness /= Size;
			fitnessVariance = (fitnessVariance / Size) - averageFitness;
        }

        public void ComputeBestFitness()
        {
            int bestChromosomeID = 0;
            for (int i = 1; i < Size; i++)
            {
                if (!((chromosomes[bestChromosomeID]).IsBetterThan((chromosomes[i]))))
                    bestChromosomeID = i;
            }
			bestFitness = ((SingleObjectiveChromosome)chromosomes[bestChromosomeID]).FitnessValue;
        }

        public void ComputeAverageFitness()
        {
			averageFitness = ((SingleObjectiveChromosome)chromosomes[0]).FitnessValue;
            for (int i = 1; i < Size; i++)
				averageFitness += ((SingleObjectiveChromosome)chromosomes[i]).FitnessValue;
			averageFitness /= Size;
        }

        public void ComputeFitnessVariance()
        {
			fitnessVariance = ((SingleObjectiveChromosome)chromosomes[0]).FitnessValue * ((SingleObjectiveChromosome)chromosomes[0]).FitnessValue;
            for (int i = 1; i < Size; i++)
				fitnessVariance += ((SingleObjectiveChromosome)chromosomes[i]).FitnessValue * ((SingleObjectiveChromosome)chromosomes[i]).FitnessValue;
			fitnessVariance = (fitnessVariance / Size) - averageFitness;
        }

		public double BestObjective => bestObjective;

		public double GetAverageObjective() => averageObjective;

		public SingleObjectiveChromosome GetBestChromosome() => bestChromosome;

		public double GetBestFitness() => bestFitness;

		public double GetAverageFitness() => averageFitness;

		public double GetFitnessVariance() => fitnessVariance;

		//********************************************************************************************************************
		//*****combined function for evaluating objective, fitness and constraint(s) of all chromosomes of the population*****
		//********************************************************************************************************************

		public void Evaluate()
        {
            for (int i = 0; i < Size; i++)
            {
                ((SingleObjectiveChromosome)chromosomes[i]).EvaluateObjective();
                ((SingleObjectiveChromosome)chromosomes[i]).EvaluateFitness();
				chromosomes[i].EvaluateConstraints();
            }
        }


        //*************************************
        //*****ToString() overriden method*****
        //*************************************

        public override string ToString()
        {
			string tempString = "";

            for (int i = 0; i < Template.DesignVariables.Count; i++)
            {
                tempString = tempString + "Var  # " + i + "\t";
            }
            for (int i = 0; i < Template.Constraints.Count; i++)
            {
                tempString = tempString + "Cntr # " + i + "\t";
            }
            tempString = tempString + "Penlty  " + "\t" + "Obj Val " + "\t" + "Fitness " + "\n";

            for (int i = 0; i < Size; i++)
            {
                tempString = tempString + ((SingleObjectiveChromosome)chromosomes[i]).ToString() + "\n";
            }
            return tempString;
        }
    }
}
