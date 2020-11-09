using System;
using System.Collections.Generic;
using System.Linq;

using Aircadia.ObjectModel.Treatments.Optimisers.Formulation;

using Aircadia.ObjectModel.Treatments.Optimisers.GA.Chromosome;


namespace Aircadia.ObjectModel.Treatments.Optimisers.GA.Population
{
	[Serializable()]
    public class MultiObjectivePopulation : Population
    {
        protected int noOfFronts;
        protected List<List<int>> fronts;
        protected List<int> noOfCromInAllFronts;

        //values for these seven variables will be assigned by ComputeObjectiveStatistics() and ComputeFitnessStatistics functions
        //can also be assigned by separate methods, i.e. by
        //ComputeBestObjective(), ComputeWorstObjective(), ComputeAverageObjective()
        //and ComputeMaximumFitness(), ComputeMinimumFitness(), ComputeAverageFitness(), ComputeFitnessVariance()
        protected double[] bestObjectives;
        protected double[] worstObjectives;
        protected double[] averageObjectives;
        protected double[] maximumFitnesses;
        protected double[] minimumFitnesses;
        protected double[] averageFitnesses;
        protected double[] fitnessVariances;


        public MultiObjectivePopulation(OptimisationTemplate Template, GAParameters gaParameters, bool empty)
            : this(Template, gaParameters, gaParameters.PopulationSize, empty)
        {
        }

        public MultiObjectivePopulation(OptimisationTemplate Template, GAParameters gaParameters, int populationSize, bool empty)
            : base(Template, gaParameters, populationSize)
        {
			//initialising population chromosomes
			chromosomes = new MultiObjectiveChromosome[size];
            for (int i = 0; i < size; i++)
            {
				chromosomes[i] = new MultiObjectiveChromosome(Template, gaParameters, empty);
            }

            //initialising the seven statistics variables
            int noOfObjectives = Template.Objectives.Count;
            bestObjectives = new double[noOfObjectives];
            worstObjectives = new double[noOfObjectives];
            averageObjectives = new double[noOfObjectives];
            maximumFitnesses = new double[noOfObjectives];
            minimumFitnesses = new double[noOfObjectives];
            averageFitnesses = new double[noOfObjectives];
            fitnessVariances = new double[noOfObjectives];
        }


        //********************************************************
        //*****ranks and crowding distances, including fronts*****
        //********************************************************

        //setting ranks of all the chromosomes of population
        public void ComputeRanks()
        {
            //***for every unique pair of individuals (i,j), check if 'i' dominates 'j' or 'j' dominates 'i'
            //number of times the individual 'i' dominated others (when compared with all other individuals in the population)
            int[] domOthersCount = new int[size];
            for (int i = 0; i < size; i++)
                domOthersCount[i] = 0;
            //number of times the individual 'i' was dominated by others (when compared with all other individuals in the population)
            int[] domByOthersCount = new int[size];
            for (int i = 0; i < size; i++)
                domByOthersCount[i] = 0;
            //dynamic arrays (ArrayList) to store the indices of the individuals that were dominated by individual 'i'
            //e.g. domOthersList[0] will store the indices of all individuals in the population that were dominated by individual at index 0 in the population
            var domOthersList = new List<int>[size];
            //initialising domOtherList (ArrayList)
            for (int i = 0; i < size; i++)
                domOthersList[i] = new List<int>();
            //initialising variable to store domination result
            int dominationTestResult;
            //comparing all chromosomes with all other chromosomes
            for (int i = 0; i < size - 1; i++)
            {
                for (int j = i + 1; j < size; j++)
                {
                    dominationTestResult = ((MultiObjectiveChromosome)this[i]).DominationTest((MultiObjectiveChromosome)this[j]);
                    if (dominationTestResult == 1)
                    {
                        domOthersCount[i]++;
                        domOthersList[i].Add(j);
                        domByOthersCount[j]++;
                    }
                    if (dominationTestResult == -1)
                    {
                        domOthersCount[j]++;
                        domOthersList[j].Add(i);
                        domByOthersCount[i]++;
                    }
                }
            }

            //***initialising a dynamic array (ArrayList) for fronts, (stores the indices of the chromosomes that are present in a particular front)
            fronts = new List<List<int>>();
            noOfCromInAllFronts = new List<int>();
            noOfFronts = 0;

            //***initialising the first front (index is 0 for the first front)
            var tempFront = new List<int>();
            for (int i = 0; i < size; i++)
            {
                if (domByOthersCount[i] == 0)
                {
                    tempFront.Add(i);
                    ((MultiObjectiveChromosome)this[i]).Rank = 0;
                }
            }
            fronts.Insert(0, tempFront);
            noOfFronts++;
            noOfCromInAllFronts.Add(tempFront.Count());

            //***initialising the rest of the fronts
            int frontID = 0;
            while (true)
            {
                //now tempFront will store the indices of individuals of the front represented by frontID+1 (removing all elements)
                tempFront = new List<int>();
                //number of individuals in front represented by frontID
                for (int i = 0; i < noOfCromInAllFronts[frontID]; i++)
                {
                    //index of the individuals in the front represented by frontID
                    int indIndex = (fronts[frontID])[i];
                    for (int j = 0; j < domOthersCount[indIndex]; j++)
                    {
                        domByOthersCount[(domOthersList[indIndex])[j]]--;
                        if (domByOthersCount[(domOthersList[indIndex])[j]] == 0)
                        {
                            tempFront.Add((domOthersList[indIndex])[j]);
                            ((MultiObjectiveChromosome)this[(domOthersList[indIndex])[j]]).Rank = frontID + 1;
                        }
                    }
                }
                if (tempFront.Count != 0)
                {
                    frontID++;
                    fronts.Insert(frontID, tempFront);
                    noOfFronts++;
                    noOfCromInAllFronts.Add(tempFront.Count());
                }
                else
                    break;
            }
        }

        //setting crowdingDistances of all the chromosomes of population
        public void ComputeCrowdingDistances()
        {
            //an array to stores the crowding distances of all individuals in the population (crowding distance w.r.t individuals in a particular front)
            double[] crowdingDistances = new double[size];
            for (int i = 0; i < size; i++)
                crowdingDistances[i] = 0.0;

            int noOfCrom; //to store number of individuals in a particular front
            int[] frontSortedCromList; //to store indices of sorted individuals of a paerticular front
            for (int i = 0; i < noOfFronts; i++)
            {
                //number of individuals in the front represented by 'i'
                noOfCrom = noOfCromInAllFronts[i];
                //initialising the array for storing the indices of sorted individuals for the current front (represented by 'i') in the ascending order
                frontSortedCromList = new int[noOfCrom];
                for (int j = 0; j < Template.Objectives.Count; j++)
                {
                    //copying the indices of individuals from the current front (represented by 'i') to sortedIndList
                    for (int k = 0; k < noOfCrom; k++)
                    {
                        frontSortedCromList[k] = (fronts[i])[k];
                    }
                    //now sorting frontSortedIndList, i.e. sorting the current front (represented by 'i') in the ascending order
                    FrontSorting(frontSortedCromList, 0, noOfCrom, j);
                    //first and last element of frontSortedIndList contain the indices of most extreme individuals of the current front (represented by 'i')
                    //assigning the crowding distances of both these individuals to infinity
                    crowdingDistances[frontSortedCromList[0]] = Double.PositiveInfinity;
                    crowdingDistances[frontSortedCromList[noOfCrom - 1]] = Double.PositiveInfinity;
                    //assigning the crowding distances of the remaining individuals of the current front (represented by 'i')
                    for (int k = 1; k < noOfCrom - 1; k++)
                    {
                        crowdingDistances[frontSortedCromList[k]] += Math.Abs((((MultiObjectiveChromosome)this[frontSortedCromList[k + 1]]).FitnessValues[j] - ((MultiObjectiveChromosome)this[frontSortedCromList[k - 1]]).FitnessValues[j]) / (1.0 + maximumFitnesses[j]));
                    }
                }
            }
            //now assigning the crowding distances to all individuals (chromosomes)
            for (int i = 0; i < size; i++)
                ((MultiObjectiveChromosome)this[i]).CrowdingDistance = crowdingDistances[i];

        }

        //quick sort method used by computeCrowdingDistances() method
        protected void FrontSorting(int[] frontSortedCromList, int left, int right, int objectiveID)
        {
            if (right > left)
            {
                double target = ((MultiObjectiveChromosome)this[frontSortedCromList[right - 1]]).FitnessValues[objectiveID];
                int i = left - 1;
                int j = right - 1;
                while (true)
                {
                    while (((MultiObjectiveChromosome)this[frontSortedCromList[++i]]).FitnessValues[objectiveID] < target)
                    {
                        if (i >= right - 1)
                            break;
                    }
                    if (i >= j)
                        break;
                    while (((MultiObjectiveChromosome)this[frontSortedCromList[--j]]).FitnessValues[objectiveID] > target)
                    {
                        if (j <= 0)
                            break;
                    }
                    if (i >= j)
                        break;
					//swaping
					Swap(ref frontSortedCromList[i], ref frontSortedCromList[j]);
                }
				//swaping
				Swap(ref frontSortedCromList[i], ref frontSortedCromList[right - 1]);

                FrontSorting(frontSortedCromList, left, i, objectiveID);
                FrontSorting(frontSortedCromList, i + 1, right, objectiveID);
            }
        }

        //function to swap two integers, used by FrontSorting() function
        private void Swap(ref int a, ref int b)
        {
            int temp = a;
            a = b;
            b = temp;
        }

		public int NoOfFronts => noOfFronts;

		public List<List<int>> Fronts => fronts;

		public List<int> NoOfCromInAllFronts => noOfCromInAllFronts;

		//******************************
		//*****statistics variables*****
		//******************************

		public void ComputeObjectiveStatistics()
        {
            for (int i = 0; i < Template.Objectives.Count; i++)
            {
				bestObjectives[i] = worstObjectives[i] = averageObjectives[i] = ((MultiObjectiveChromosome)this[0]).ObjectiveValues[i];
                for (int j = 1; j < size; j++)
                {
					averageObjectives[i] += ((MultiObjectiveChromosome)this[j]).ObjectiveValues[i];
                    if (Template.Objectives[i].Type == ObjectiveType.Minimise)
                    {
                        if (bestObjectives[i] > ((MultiObjectiveChromosome)this[j]).ObjectiveValues[i])
							bestObjectives[i] = ((MultiObjectiveChromosome)this[j]).ObjectiveValues[i];
                        if (worstObjectives[i] < ((MultiObjectiveChromosome)this[j]).ObjectiveValues[i])
							worstObjectives[i] = ((MultiObjectiveChromosome)this[j]).ObjectiveValues[i];
                    }
                    else if (Template.Objectives[i].Type == ObjectiveType.Maximise)
                    {
                        if (bestObjectives[i] < ((MultiObjectiveChromosome)this[j]).ObjectiveValues[i])
							bestObjectives[i] = ((MultiObjectiveChromosome)this[j]).ObjectiveValues[i];
                        if (worstObjectives[i] > ((MultiObjectiveChromosome)this[j]).ObjectiveValues[i])
							worstObjectives[i] = ((MultiObjectiveChromosome)this[j]).ObjectiveValues[i];
                    }
                }
				averageObjectives[i] /= size;
            }
        }

        public void ComputeBestObjective()
        {
            for (int i = 0; i < Template.Objectives.Count; i++)
            {
				bestObjectives[i] = ((MultiObjectiveChromosome)this[0]).ObjectiveValues[i];
                for (int j = 1; j < size; j++)
                {
                    if (Template.Objectives[i].Type == ObjectiveType.Minimise)
                    {
                        if (bestObjectives[i] > ((MultiObjectiveChromosome)this[j]).ObjectiveValues[i])
							bestObjectives[i] = ((MultiObjectiveChromosome)this[j]).ObjectiveValues[i];
                    }
                    else if (Template.Objectives[i].Type == ObjectiveType.Maximise)
                    {
                        if (bestObjectives[i] < ((MultiObjectiveChromosome)this[j]).ObjectiveValues[i])
							bestObjectives[i] = ((MultiObjectiveChromosome)this[j]).ObjectiveValues[i];
                    }
                }
            }
        }

        public void ComputeWorstObjective()
        {
            for (int i = 0; i < Template.Objectives.Count; i++)
            {
				worstObjectives[i] = ((MultiObjectiveChromosome)this[0]).ObjectiveValues[i];
                for (int j = 1; j < size; j++)
                {
                    if (Template.Objectives[i].Type == ObjectiveType.Minimise)
                    {
                        if (worstObjectives[i] < ((MultiObjectiveChromosome)this[j]).ObjectiveValues[i])
							worstObjectives[i] = ((MultiObjectiveChromosome)this[j]).ObjectiveValues[i];
                    }
                    else if (Template.Objectives[i].Type == ObjectiveType.Maximise)
                    {
                        if (worstObjectives[i] > ((MultiObjectiveChromosome)this[j]).ObjectiveValues[i])
							worstObjectives[i] = ((MultiObjectiveChromosome)this[j]).ObjectiveValues[i];
                    }
                }
            }
        }

        public void ComputeAverageObjective()
        {
            for (int i = 0; i < Template.Objectives.Count; i++)
            {
				averageObjectives[i] = ((MultiObjectiveChromosome)this[0]).ObjectiveValues[i];
                for (int j = 1; j < size; j++)
					averageObjectives[i] += ((MultiObjectiveChromosome)this[j]).ObjectiveValues[i];
				averageObjectives[i] /= size;
            }
        }

        public void ComputeFitnessStatistics()
        {
            for (int i = 0; i < Template.Objectives.Count; i++)
            {
				maximumFitnesses[i] = minimumFitnesses[i] = averageFitnesses[i] = ((MultiObjectiveChromosome)this[0]).FitnessValues[i];
				fitnessVariances[i] = ((MultiObjectiveChromosome)this[0]).FitnessValues[i] * ((MultiObjectiveChromosome)this[0]).FitnessValues[i];
                for (int j = 1; j < size; j++)
                {
					averageFitnesses[i] += ((MultiObjectiveChromosome)this[j]).FitnessValues[i];
					fitnessVariances[i] += ((MultiObjectiveChromosome)this[j]).FitnessValues[i] * ((MultiObjectiveChromosome)this[j]).FitnessValues[i];
                    if (maximumFitnesses[i] < ((MultiObjectiveChromosome)this[j]).FitnessValues[i])
						maximumFitnesses[i] = ((MultiObjectiveChromosome)this[j]).FitnessValues[i];
                    if (minimumFitnesses[i] > ((MultiObjectiveChromosome)this[j]).FitnessValues[i])
						minimumFitnesses[i] = ((MultiObjectiveChromosome)this[j]).FitnessValues[i];
                }
				averageFitnesses[i] /= size;
				fitnessVariances[i] = (fitnessVariances[i] / size) - averageFitnesses[i];
            }
        }

        public void ComputeMaximumFitness()
        {
            for (int i = 0; i < Template.Objectives.Count; i++)
            {
				maximumFitnesses[i] = ((MultiObjectiveChromosome)this[0]).FitnessValues[i];
                for (int j = 1; j < size; j++)
                {
                    if (maximumFitnesses[i] < ((MultiObjectiveChromosome)this[j]).FitnessValues[i])
						maximumFitnesses[i] = ((MultiObjectiveChromosome)this[j]).FitnessValues[i];
                }
            }
        }

        public void ComputeMinimumFitness()
        {
            for (int i = 0; i < Template.Objectives.Count; i++)
            {
				minimumFitnesses[i] = ((MultiObjectiveChromosome)this[0]).FitnessValues[i];
                for (int j = 1; j < size; j++)
                {
                    if (minimumFitnesses[i] > ((MultiObjectiveChromosome)this[j]).FitnessValues[i])
						minimumFitnesses[i] = ((MultiObjectiveChromosome)this[j]).FitnessValues[i];
                }
            }
        }

        public void ComputeAverageFitness()
        {
            for (int i = 0; i < Template.Objectives.Count; i++)
            {
				averageFitnesses[i] = ((MultiObjectiveChromosome)this[0]).FitnessValues[i];
                for (int j = 1; j < size; j++)
					averageFitnesses[i] += ((MultiObjectiveChromosome)this[j]).FitnessValues[i];
				averageFitnesses[i] /= size;
            }
        }

        public void ComputeFitnessVariance()
        {
            for (int i = 0; i < Template.Objectives.Count; i++)
            {
				fitnessVariances[i] = ((MultiObjectiveChromosome)this[0]).FitnessValues[i] * ((MultiObjectiveChromosome)this[0]).FitnessValues[i];
                for (int j = 1; j < size; j++)
					fitnessVariances[i] += ((MultiObjectiveChromosome)this[j]).FitnessValues[i] * ((MultiObjectiveChromosome)this[j]).FitnessValues[i];
				fitnessVariances[i] = (fitnessVariances[i] / size) - averageFitnesses[i];
            }
        }

		public double[] BestObjectives => bestObjectives;

		public double[] WorstObjectives => worstObjectives;

		public double[] AverageObjectives => averageObjectives;

		public double[] MaximumFitnesses => maximumFitnesses;

		public double[] MinimumFitnesses => minimumFitnesses;

		public double[] AverageFitnesses => averageFitnesses;

		public double[] FitnessVariances => fitnessVariances;

		//*****************************************************************************************************************************
		//*****combined function for evaluating objectives, fitnesses and constraints of all chromosomes of the population*****
		//*****************************************************************************************************************************

		public void Evaluate()
        {
            for (int i = 0; i < size; i++)
            {
                ((MultiObjectiveChromosome)this[i]).EvaluateObjectives();
                ((MultiObjectiveChromosome)this[i]).EvaluateFitnesses();
                this[i].EvaluateConstraints();
            }
        }


        //*************************************
        //*****ToString() overriden method*****
        //*************************************

        public override string ToString()
        {
			string tempString = "";
            for (int i = 0; i < size; i++)
            {
                tempString = tempString + ((MultiObjectiveChromosome)this[i]).ToString() + ((MultiObjectiveChromosome)this[i]).Rank + "\t" + ((MultiObjectiveChromosome)this[i]).CrowdingDistance.ToString("F3") + "\n";
            }
            return tempString;
        }


    }
}
