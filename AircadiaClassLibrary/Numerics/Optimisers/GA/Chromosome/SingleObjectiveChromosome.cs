using System;

using Aircadia.ObjectModel.DataObjects;
using Aircadia.ObjectModel.Treatments.Optimisers.Formulation;


namespace Aircadia.ObjectModel.Treatments.Optimisers.GA.Chromosome
{
	[Serializable()]
    public class SingleObjectiveChromosome : Chromosome
    {
		public double ObjectiveValue { get; set; }
		public double FitnessValue { get; set; }

		public SingleObjectiveChromosome(OptimisationTemplate Template, GAParameters gaParameters, bool empty)
            : base(Template, gaParameters, empty)
        {
        }


        //*******************************************
        //*****objective value and fitness value*****
        //*******************************************

        public void EvaluateObjective()
        {
            //initialising and assigning objective values
            for (int i = 0; i < Template.DesignVariables.Count; i++)
            {
                Template.DesignVariables[i].Value = genes[i];
                Data data = Template.ExecutableComponent.ModelDataInputs.Find(delegate(Data d) { return d.Name == Template.DesignVariables[i].Name; });
                if (data is IntegerData)
                    data.Value = (int)genes[i];
                else if (data is DoubleData)
                    data.Value = (double)genes[i];
            }

            Template.ExecutableComponent.Execute();


            Data objectiveData = Template.ExecutableComponent.ModelDataOutputs.Find(delegate(Data d) { return d.Name == Template.Objectives[0].Name; });
            if (objectiveData is IntegerData)
				ObjectiveValue = (int)objectiveData.Value;
            else if (objectiveData is DoubleData)
				ObjectiveValue = (double)objectiveData.Value;

        }

        public void EvaluateFitness()
        {
            if (Template.Objectives[0].Type == ObjectiveType.Minimise)
            {
                switch (gaParameters.ConstraintHandlingMethod)
                {
                    case ConstraintHandlingMethods.LinearPenalty:
                    case ConstraintHandlingMethods.QuadraticPenalty:
						FitnessValue = -ObjectiveValue - penalty;
                        break;
                    case ConstraintHandlingMethods.Tournament:
						FitnessValue = -ObjectiveValue;
                        break;
                }
            }
            else if (Template.Objectives[0].Type == ObjectiveType.Maximise)
                switch (gaParameters.ConstraintHandlingMethod)
                {
                    case ConstraintHandlingMethods.LinearPenalty:
                    case ConstraintHandlingMethods.QuadraticPenalty:
						FitnessValue = ObjectiveValue - penalty;
                        break;
                    case ConstraintHandlingMethods.Tournament:
						FitnessValue = ObjectiveValue;
                        break;
                }
        }




        //********************************************
        //*****implementation of abstract methods*****
        //********************************************

        //returns true, if the calling chromosome is better than the chromosome provided as argument by considering constraint handling method
        public override bool IsBetterThan(Chromosome crom)
        {
            var sCrom = (SingleObjectiveChromosome)crom;

            bool result = false;

            //there are three constraint handling methods

            //if there are no constraints, then comparing fitness values
            if (Template.Constraints.Count == 0)
            {
                if (FitnessValue > sCrom.FitnessValue)
                    result = true;
                return result;
            }
            //if there are constraints, then comparing according to the constraint handling method
            else
            {
                switch (gaParameters.ConstraintHandlingMethod)
                {
                    //if constraint handling method is either linear or quadratic penalty constraint handling method, then comparing fitness values
                    case ConstraintHandlingMethods.LinearPenalty:
                    case ConstraintHandlingMethods.QuadraticPenalty:
                        if (FitnessValue > sCrom.FitnessValue)
                            result = true;
                        break;
                    //if constraint handling method is tournament constraint handling method, then comparing using constrained tournament selection/comparison operator
                    case ConstraintHandlingMethods.Tournament:
                        if (ConstrainedTournamentComparison(crom))
                            result = true;
                        break;
                }
            }
            return result;
        }

        //'constraint tournament selection/comparison operator' (for single objective problem)
        //returns 1, if crom1 is better than crom2
        //returns -1, if crom2 is better than crom1
        //returns 0, if none of the chromosomes/'individual solutions' is better than the other
        public static int ConstrainedTournamentComparison(Chromosome crom1, Chromosome crom2)
        {
            var sCrom1 = (SingleObjectiveChromosome)crom1;
            var sCrom2 = (SingleObjectiveChromosome)crom2;

            int result = 2;
            //case 1: when crom1 is feasible and crom2 is infeasible
            if (sCrom1.Penalty == 0.0 && sCrom2.Penalty > 0.0)
                result = 1;
            //case 2: when crom1 is infeasible and crom2 is feasible
            else if (sCrom2.Penalty == 0.0 && sCrom1.Penalty > 0.0)
                result = -1;
            //case 3: when both (crom1 and crom2) are feasible
            //case 4: when both (crom1 and crom2) are infeasible, but with same amount of infeasibility
            else if (sCrom1.Penalty == sCrom2.Penalty)
            {
                //if the fitness of crom1 is better than fitness of crom2
                if (sCrom1.FitnessValue > sCrom2.FitnessValue)
                    result = 1;
                //if the fitness of crom2 is better than fitness of crom1
                else if (sCrom2.FitnessValue > sCrom1.FitnessValue)
                    result = -1;
                //the only remaining case, when both (crom1 and crom2) have same fitness (i.e. both are same, none is better than the other)
                else
                    result = 0;
            }
            //case 4: the only remaining case, when both (crom1 and crom2) are infeasible but with different amount of infeasibility
            else
            { //(sCrom1.getPenalty() > 0.0 && sCrom2.getPenalty() > 0.0 && sCrom1.getPenalty() != sCrom2.getPenalty())
                //both are infeasible, but infeasibility of crom1 is less than infeasibility of crom2
                if (sCrom1.Penalty < sCrom2.Penalty)
                    result = 1;
                //both are infeasible, but infeasibility of crom2 is less than infeasibility of crom1
                else if (sCrom2.Penalty < sCrom1.Penalty)
                    result = -1;
            }

            if (result == 2)
            {
                //System.out.println("Error in constrainedTournamentComparison() method");
                //System.exit(1);
            }
            return result;
        }





        //'constraint tournament selection/comparison operator' (for single objective)
        //returns true, if the calling chromosome is better than the chromosome provided as argument, otherwise false
        public bool ConstrainedTournamentComparison(Chromosome crom)
        {
            var sCrom = (SingleObjectiveChromosome)crom;

            bool result = false;

            //there are 4 cases when the calling chromosome is better than the chromosome provided as argument

            //case 1: when calling chromosome is feasible and chromosome provided as argument is infeasible
            if (Penalty == 0.0 && sCrom.Penalty > 0.0)
                result = true;
            //case 2: when both chromosomes are feasible
            //case 3: when both chromosomes are infeasible with same amount of infeasibility (i.e. with same penalty)
            else if (Penalty == sCrom.Penalty)
            {
                //if the fitness of crom1 is better than fitness of crom2
                if (FitnessValue > sCrom.FitnessValue)
                    result = true;
            }
            //case 4: when both chromosomes are infeasible, but the infeasibility of the calling chromosome is less than the chromosome provided as argument
            else if (Penalty < sCrom.Penalty)
                result = true;

            return result;
        }




        //*************************************
        //*****ToString() overriden method*****
        //*************************************


        public override string ToString()
        {
			string tempString = ""; //string to be returned
            for (int i = 0; i < Template.DesignVariables.Count; i++)
            {
                tempString = tempString + genes[i].ToString("F6") + "\t";
            }
            for (int i = 0; i < Template.Constraints.Count; i++)
            {
                tempString = tempString + constraintViolationValues[i].ToString("F6") + "\t";
            }
            tempString = tempString + penalty.ToString("F6") + "\t" + ObjectiveValue.ToString("E1") + "\t" + FitnessValue.ToString("E1");
            return tempString;
        }
    }
}
