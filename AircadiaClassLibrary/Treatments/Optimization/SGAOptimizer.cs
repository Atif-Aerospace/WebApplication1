using System;
using Aircadia.ObjectModel.Treatments.Optimisers.GA.Chromosome;
using Aircadia.ObjectModel.Treatments.Optimisers.GA.Population;
using Aircadia.ObjectModel.Treatments.Optimisers.GA.Selection;
using Aircadia.ObjectModel.Treatments.Optimisers.GA.Crossover;
using Aircadia.ObjectModel.Treatments.Optimisers.GA.Mutation;
using Aircadia.ObjectModel.Treatments.Optimisers.GA.Elitism;

using System.IO;

namespace Aircadia.ObjectModel.Treatments.Optimisers.GA.SGA
{
	[Serializable()]
    public class SGAOptimizer : SingleObjectiveGeneticAlgorithmOptimiser
    {
        public SGAOptimizer(string name, string description, GAParameters gaParameters)
            : base(name, description, gaParameters)
        {

        }


		public override bool ApplyOn(ExecutableComponent ec) => ApplyOn();
		public override bool ApplyOn()
        {
            //initialising selection operator
            if (GAParameters.SelectionOprMethod == SelectionOprMethods.TournamentSelectionWithoutReplacement)
            {
				selectionOpr = new TournamentSelectionOprWithoutReplacement(OptimisationTemplate, GAParameters);
            }
            else if (GAParameters.SelectionOprMethod == SelectionOprMethods.TournamentSelectionWithReplacement)
            {
            }

            //initialising crossover operator
            if (GAParameters.CrossoverOprMethod == CrossoverOprMethods.OnePointCrossover)
            {
				crossoverOpr = new OnePointCrossoverOpr(OptimisationTemplate, GAParameters);
            }
            else if (GAParameters.CrossoverOprMethod == CrossoverOprMethods.TwoPointCrossover)
            {
            }
            else if (GAParameters.CrossoverOprMethod == CrossoverOprMethods.UniformCrossover)
            {
            }
            else if (GAParameters.CrossoverOprMethod == CrossoverOprMethods.SimulatedBinaryCrossover)
            {
				crossoverOpr = new SimulatedBinaryCrossoverOpr(OptimisationTemplate, GAParameters);
            }

            //initialising mutation operator
            if (GAParameters.MutationOprMethod == MutationOprMethods.SelectiveMutation)
            {
				mutationOpr = new SelectiveMutationOpr(OptimisationTemplate, GAParameters);
            }
            else if (GAParameters.MutationOprMethod == MutationOprMethods.GenewiseMutation)
            {
            }
            else if (GAParameters.MutationOprMethod == MutationOprMethods.PolynomialMutation)
            {
				mutationOpr = new PolynomialMutationOpr(OptimisationTemplate, GAParameters);
            }

            //initialising elitism operator
            if (GAParameters.ElitismOprMethod == ElitismOprMethods.ProportionalElitism)
            {
                //this.elitismOpr = new ProportionalElitismOpr(Template, gaParameters);
            }
            else if (GAParameters.ElitismOprMethod == ElitismOprMethods.UnproportionalElitism)
            {
                //this.elitismOpr = new UnproportionalElitismOpr(Template, gaParameters);
            }



            var bestChromosome = new SingleObjectiveChromosome(OptimisationTemplate, GAParameters, false);
            string result = "";
            //for (int j = 0; j < this.BestChromosome.Genes.Length; j++)
            //result += this.BestChromosome.Genes[j] + "\t";
            //result += this.BestChromosome.ObjectiveValue + "\t";
            //for (int j = 0; j < this.BestChromosome.ConstraintViolationValues.Length; j++)
            //result += this.BestChromosome.ConstraintViolationValues[j] + "\t";
            System.Console.WriteLine(result);



			parentPopulation = new SingleObjectivePopulation(OptimisationTemplate, GAParameters, false);
			childPopulation = new SingleObjectivePopulation(OptimisationTemplate, GAParameters, false);
































            TextWriter solutionsFileWriter = new StreamWriter(GAParameters.EvaluatedSolutionsFile);


            int generationID = 0;

            parentPopulation.Evaluate();
            /*
            System.Console.WriteLine("after evaluating population");
            System.Console.WriteLine(this.parentPopulation);
            System.Console.WriteLine(this.childPopulation);
            */



            //writing to console and text file
            System.Console.WriteLine("Generation: " + generationID);
            double[] x;
            double y;
            double[] c;
            result = "";


            for (int i = 0; i < parentPopulation.Size; i++)
            {
                x = ((SingleObjectiveChromosome)(parentPopulation[i])).Genes;
                y = ((SingleObjectiveChromosome)(parentPopulation[i])).ObjectiveValue;
                c = ((SingleObjectiveChromosome)(parentPopulation[i])).ConstraintViolationValues;
                result = "";
                result += y;
                System.Console.WriteLine(result);
                result = "";
                for (int j = 0; j < x.Length; j++)
                    result += x[j] + "\t";
                result += y + "\t";
                for (int j = 0; j < c.Length; j++)
                    result += c[j] + "\t";
                solutionsFileWriter.WriteLine(result);
                System.Console.WriteLine(result);
            }
            System.Console.WriteLine("");

            while (generationID < GAParameters.NoOfGenerations)
            {
                //selection
                int[] matingPoolIndex = selectionOpr.ApplySelectionOpr(parentPopulation);
                /*
                for (int i = 0; i < matingPoolIndex.Length; i++)
                    System.Console.Write(matingPoolIndex[i] + "***");
                */
                for (int i = 0; i < parentPopulation.Size; i++)
					childPopulation[i] = (SingleObjectiveChromosome)parentPopulation[matingPoolIndex[i]];
                /*
                System.Console.WriteLine("after selection");
                System.Console.WriteLine(this.parentPopulation);
                System.Console.WriteLine(this.childPopulation);
                */

                //crossover
                for (int i = 0; i < parentPopulation.Size; i += 2)
                {
                    if (GARandom.Flip(GAParameters.CrossoverProbability))
                        crossoverOpr.ApplyCrossoverOpr(childPopulation[i], childPopulation[i + 1]);
                }
                /*
                System.Console.WriteLine("after crossover");
                System.Console.WriteLine(this.parentPopulation);
                System.Console.WriteLine(this.childPopulation);
                */

                //mutation
                for (int i = 0; i < parentPopulation.Size; i++)
                {
                    //if (GARandom.coinFlip(gaSet.getMutationProbability())) {
                    mutationOpr.ApplyMutationOpr(childPopulation[i]);
                    //}
                }
				/*
                System.Console.WriteLine("after mutation");
                System.Console.WriteLine(this.parentPopulation);
                System.Console.WriteLine(this.childPopulation);
                */

				childPopulation.Evaluate();
                /*
                System.Console.WriteLine("after evaluating childPopulation");
                System.Console.WriteLine(this.parentPopulation);
                System.Console.WriteLine(this.childPopulation);
                */

                //this.newPopulation.computeObjectiveStatistics();
                //this.newPopulation.computeFitnessStatistics();


                for (int i = 0; i < childPopulation.Size; i++)
                {
                    x = ((SingleObjectiveChromosome)(childPopulation[i])).Genes;
                    y = ((SingleObjectiveChromosome)(childPopulation[i])).ObjectiveValue;
                    c = ((SingleObjectiveChromosome)(childPopulation[i])).ConstraintViolationValues;
                    result = "";
                    for (int j = 0; j < x.Length; j++)
                        result += x[j] + "\t";
                    result += y + "\t";
                    for (int j = 0; j < c.Length; j++)
                        result += c[j] + "\t";
                    solutionsFileWriter.WriteLine(result);
                }



                bool useElitism = true;
                if (useElitism)
                {
                    var elitismOpr = new ProportionalElitismOpr(OptimisationTemplate, GAParameters, parentPopulation, childPopulation);
                    elitismOpr.ApplyElitismOpr();
                }
                else
                {
                }
				/*
                System.Console.WriteLine("after elitism operator");
                System.Console.WriteLine(this.parentPopulation);
                System.Console.WriteLine(this.childPopulation);
                */

				parentPopulation.Evaluate();
                /*
                System.Console.WriteLine("after elitism operator and then evaluating");
                System.Console.WriteLine(this.parentPopulation);
                System.Console.WriteLine(this.childPopulation);
                */

                //this.parentPopulation.ComputeObjectiveStatistics();
                //this.parentPopulation.ComputeFitnessStatistics();
                /*
                generationID++;
                System.Console.WriteLine("Generation: " + generationID);
                fileWriter.WriteLine("Generation: " + generationID);
                System.Console.WriteLine("Best Solution:");
                fileWriter.WriteLine("Best Solution:");
                parentPopulation.ComputeBestChromosome();
                System.Console.WriteLine(parentPopulation.BestChromosome);
                fileWriter.WriteLine(parentPopulation.BestChromosome);
                */
                generationID++;
                //writing to console
                System.Console.WriteLine("Generation: " + generationID);

                // Best chromosome
                System.Console.WriteLine("Best Solution:");
                parentPopulation.ComputeBestChromosome();
				parentPopulation.GetBestChromosome().EvaluateObjective();
				parentPopulation.GetBestChromosome().EvaluateConstraints();
                x = parentPopulation.GetBestChromosome().Genes;
                y = parentPopulation.GetBestChromosome().ObjectiveValue;
                c = parentPopulation.GetBestChromosome().ConstraintViolationValues;
                result = "";
                for (int j = 0; j < x.Length; j++)
                    result += x[j] + "\t";
                result += y + "\t";
                for (int j = 0; j < c.Length; j++)
                    result += c[j] + "\t";
                System.Console.WriteLine(result);
                System.Console.WriteLine("");

            }

            solutionsFileWriter.Close();
            return true;
        }
    }
}
