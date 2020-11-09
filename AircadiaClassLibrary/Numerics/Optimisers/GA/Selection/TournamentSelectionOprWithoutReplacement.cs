using System;

using Aircadia.ObjectModel.Treatments.Optimisers.Formulation;

namespace Aircadia.ObjectModel.Treatments.Optimisers.GA.Selection
{
	[Serializable()]
    public class TournamentSelectionOprWithoutReplacement : TournamentSelectionOpr
    {
        public TournamentSelectionOprWithoutReplacement(OptimisationTemplate Template, GAParameters gaParameters)
            : this(Template, gaParameters, 2)
        {
        }

        public TournamentSelectionOprWithoutReplacement(OptimisationTemplate Template, GAParameters gaParameters, int tournamentSize)
            : base(Template, gaParameters, tournamentSize)
        {
        }

        //returns an array of the indices of selected chromosomes from the population after applying selection operator
        public override int[] ApplySelectionOpr(Population.Population population)
        {
            int populationSize = population.Size;

            //creating an array for storing shuffled indices of the chromosomes of population
            //array initialisation
            int[] shuffledIndex = new int[populationSize];
            //storing all the indices in ascending order (1 to population size) for later shuffling (thats why, the name shuffledIndex)
            for (int i = 0; i < populationSize; i++)
                shuffledIndex[i] = i;

            //initialising an array for storing indices of the chromosomes after selection for mating pool
            int[] matingPoolIndex = new int[populationSize];
            Chromosome.Chromosome winner;
            int populationCounter = 0;
            //if the tournament is between 2 chromosomes, we need to schedule 2 sets of tournaments in order to create mating pool of size equal to population size
            for (int i = 0; i < tournamentSize; i++)
            {
                //shuffling the indices of chromosomes in the population
                shuffleArray(shuffledIndex);


                for (int a = 0; a < shuffledIndex.Length; a++)
                {
                    System.Console.Write(shuffledIndex[a] + "*");
                }
                System.Console.WriteLine("");


                for (int j = 0; j < populationSize; j += tournamentSize)
                {
                    winner = population[shuffledIndex[j]];
                    matingPoolIndex[populationCounter] = shuffledIndex[j];
                    for (int k = 1; k < tournamentSize; k++)
                    {
                        if (winner.IsBetterThan(population[shuffledIndex[j + k]]))
                        {
                            winner = population[shuffledIndex[j]];
                            matingPoolIndex[populationCounter] = shuffledIndex[j];
                        }
                        else
                        {
                            winner = population[shuffledIndex[j + k]];
                            matingPoolIndex[populationCounter] = shuffledIndex[j + k];
                        }
                    }
                    populationCounter++;
                }
            }

            return matingPoolIndex;
        }

    }
}
