using System;

using Aircadia.ObjectModel.Treatments.Optimisers.Formulation;

using Aircadia.ObjectModel.Treatments.Optimisers.GA.Chromosome;
using Aircadia.ObjectModel.Treatments.Optimisers.GA.Population;

namespace Aircadia.ObjectModel.Treatments.Optimisers.GA.Elitism
{
	[Serializable()]
    public class ProportionalElitismOpr : ElitismOpr
    {
        public ProportionalElitismOpr(OptimisationTemplate Template, GAParameters gaParameters, Population.Population parentPopulation, Population.Population childPopulation)
            : base(Template, gaParameters, parentPopulation, childPopulation)
        {
        }

        //
        public void ApplyElitismOpr()
        {
            if (parentPopulation is SingleObjectivePopulation && childPopulation is SingleObjectivePopulation)
            {

                int populationSize = parentPopulation.Size;
                if (childPopulation.Size != populationSize)
                {
                    System.Console.WriteLine("The population size must be same for both populations.");
                    System.Environment.Exit(1);
                }

                if (gaParameters.ElitismProportion == 0.0) //not using elitism
                {
                    for (int i = 0; i < populationSize; i++)
                    {
                        parentPopulation[i] = childPopulation[i];
                    }
                }
                else //using elitism
                {
                    int[] parentSortedIndex = new int[populationSize];
                    int[] childSortedIndex = new int[populationSize];
                    for (int i = 0; i < populationSize; i++)
                    {
                        parentSortedIndex[i] = i;
                        childSortedIndex[i] = i;
                    }

                    //sorting the populations, sorted indices will be stored in parentSortedIndex and childSortedIndex
                    PopulationSorting(parentPopulation, parentSortedIndex, 0, populationSize);
                    PopulationSorting(childPopulation, childSortedIndex, 0, populationSize);
                    /*
                    System.Console.WriteLine("sorted parent population");
                    for (int i = 0; i < populationSize; i++)
                        System.Console.WriteLine(parentPopulation.getChromosomeAt(parentSortedIndex[i]));
                    System.Console.WriteLine("sorted child population");
                    for (int i = 0; i < populationSize; i++)
                        System.Console.WriteLine(childPopulation.getChromosomeAt(childSortedIndex[i]));
                    */
                    //number of elite chromosomes copied from parent population
                    int noOfParentElitesCopied = (int)(gaParameters.ElitismProportion * populationSize);
                    //System.Console.WriteLine("noOfParentElitesCopied: " + noOfParentElitesCopied);

                    //population object to store elites of parent population and child population
                    var elitePopulation = new SingleObjectivePopulation(Template, gaParameters, true);
                    for (int i = 0; i < noOfParentElitesCopied; i++)
                        elitePopulation[i] = parentPopulation[parentSortedIndex[i]];
                    for (int i = 0; i < (populationSize - noOfParentElitesCopied); i++)
                        elitePopulation[i + noOfParentElitesCopied] = childPopulation[childSortedIndex[i]];
                    /*
                    System.Console.WriteLine("elite population");
                    for (int i = 0; i < populationSize; i++)
                        System.Console.WriteLine(elitePopulation[i]);
                    */
                    for (int i = 0; i < populationSize; i++)
                        parentPopulation[i] = elitePopulation[i];

                }

            } //if (parentPopulation is SingleObjectivePopulation && childPopulation is SingleObjectivePopulation)
            else if (parentPopulation is MultiObjectivePopulation && childPopulation is MultiObjectivePopulation)
            {
            }

        }


        //implements 'quick sort' sorting algorithm
        //pop is the single objective population to be sorted
        //sortedIndex array will store the indices of sorted population
        private void PopulationSorting(Population.Population pop, int[] sortedIndex, int left, int right)
        {
            if (right > left)
            {
                var target = (SingleObjectiveChromosome)pop[sortedIndex[right - 1]];//this.popChromosomes[sortedCromList[right-1]]);
                int i = left - 1;
                int j = right - 1;
                while (true)
                {
                    while (((SingleObjectiveChromosome)pop[sortedIndex[++i]]).IsBetterThan(target))
                    {
                        if (i >= right - 1)
                            break;
                    }
                    if (i >= j)
                        break;
                    while (!((SingleObjectiveChromosome)pop[sortedIndex[--j]]).IsBetterThan(target))
                    {
                        if (j <= 0)
                            break;
                    }
                    if (i >= j)
                        break;
					//swaping
					Swap(ref sortedIndex[i], ref sortedIndex[j]);
                }
				//swaping
				Swap(ref sortedIndex[i], ref sortedIndex[right - 1]);

                PopulationSorting(pop, sortedIndex, left, i);
                PopulationSorting(pop, sortedIndex, i + 1, right);
            }
        }

        private void Swap(ref int a, ref int b)
        {
            int temp = a;
            a = b;
            b = temp;
        }
    }
}
