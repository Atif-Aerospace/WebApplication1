using System;

using Aircadia.ObjectModel.DataObjects;
using Aircadia.ObjectModel.Treatments.Optimisers.Formulation;

namespace Aircadia.ObjectModel.Treatments.Optimisers.GA.Chromosome
{
	[Serializable()]
    public class MultiObjectiveChromosome : Chromosome
    {
        //*****the values for these two variables will be assigned by evaluateObjective() and evaluateFitness() methods*****
        //stores the objective values of all objectives for the chromosome/'individual solution'
        protected double[] objectiveValues;
        //stores the fitness values of all objectives for the chromosome/'individual solution'
        protected double[] fitnessValues;

        //*****the values for these two variables will be assigned by setRanks() and setCrowdingDistances() methods
        protected int rank; //will be initialised to 0 upon object creation
        protected double crowdingDistance; //will be initialised to 0.0 upon object creation


        public MultiObjectiveChromosome(OptimisationTemplate Template, GAParameters gaParameters, bool empty)
            : base(Template, gaParameters, empty)
        {
			objectiveValues = new double[Template.Objectives.Count];
			fitnessValues = new double[Template.Objectives.Count];
        }


        //*********************************************
        //*****objective values and fitness values*****
        //*********************************************

        public void EvaluateObjectives()
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

            // Evaluate Objectives
            for (int i = 0; i < Template.Objectives.Count; i++)
            {
                // Objective Value
                Data data = Template.ExecutableComponent.ModelDataOutputs.Find(delegate(Data d) { return d.Name == Template.Objectives[i].Name; });
                if (data is IntegerData)
					objectiveValues[i] = (int)(data.Value);
                else if (data is DoubleData)
					objectiveValues[i] = (double)(data.Value);
                /*
                // Objective Fitness
                if (Template.Objectives[i].Type == ObjectiveType.Minimise)
                    this.fitnessValues[i] = -this.objectiveValues[i];
                else if (Template.Objectives[i].Type == ObjectiveType.Maximise)
                    this.fitnessValues[i] = this.objectiveValues[i];
                */
            }
            /*
            // Evaluate Constraints
            for (int i = 0; i < Template.Constraints.Length; i++)
            {
                Data data = Template.Workflow.DataOutputs.Find(delegate(Data d) { return d.Name == Template.Constraints[i].Name; });
                if (data is IntegerData)
                    this.ConstraintValues[i] = ((IntegerData)data).Value;
                else if (data is DoubleData)
                    this.ConstraintValues[i] = ((DoubleData)data).Value;
            }
            */
        }

        public void EvaluateFitnesses()
        {
            //initialising and assigning fitness values
            for (int i = 0; i < Template.Objectives.Count; i++)
            {
                if (Template.Objectives[i].Type == ObjectiveType.Minimise)
					fitnessValues[i] = -objectiveValues[i];
                else if (Template.Objectives[i].Type == ObjectiveType.Maximise)
					fitnessValues[i] = objectiveValues[i];
            }
        }

        public double[] ObjectiveValues
		{
			get => objectiveValues;
			set => objectiveValues = value;
		}

		public double[] FitnessValues
		{
			get => fitnessValues;
			set => fitnessValues = value;
		}

		//************************************
		//*****rank and crowding distance*****
		//************************************

		//set will be used by multi-objective population object to set rank
		public int Rank
		{
			get => rank;
			set => rank = value;
		}

		//set will be used by population object to set crowdingDistance
		public double CrowdingDistance
		{
			get => crowdingDistance;
			set => crowdingDistance = value;
		}

		//****************************************************************
		//*****domination test for finding rank and crowding distance*****
		//****************************************************************

		//performs a domination test
		//returns 1, if the calling chromosome dominates the chromosome 'crom' (passed as argument)
		//returns -1, if the chromosome 'crom' (passed as argument) dominates the calling chromosome
		//returns 0, if neither calling chromosome nor chromosome 'crom' (passed as argument) dominates the other
		public int DominationTest(MultiObjectiveChromosome crom)
        {
            bool dominates = false, dominatedBy = false;
            if (Template.Constraints.Count == 0 || gaParameters.ConstraintHandlingMethod == ConstraintHandlingMethods.LinearPenalty || gaParameters.ConstraintHandlingMethod == ConstraintHandlingMethods.QuadraticPenalty)
            {
                for (int i = 0; i < Template.Objectives.Count; i++)
                {
                    if (fitnessValues[i] > crom.FitnessValues[i])
                        dominates = true;
                    else if (fitnessValues[i] < crom.FitnessValues[i])
                        dominatedBy = true;
                }
            }
            //if constraint handling method is tournament constraint handling method
            else if (gaParameters.ConstraintHandlingMethod == ConstraintHandlingMethods.Tournament)
            {
                //if both individuals (chromosomes) are feasible, i.e. penalty for both is 0.0
                //if both individuals (chromosomes) are infeasible, i.e. penalty for both is greater than 0.0, but with same amount of infeasibility
                if (penalty == crom.penalty)
                {
                    for (int i = 0; i < Template.Objectives.Count; i++)
                    {
                        if (fitnessValues[i] > crom.FitnessValues[i])
                            dominates = true;
                        else if (fitnessValues[i] < crom.FitnessValues[i])
                            dominatedBy = true;
                    }
                }
                //if one individual (chromosome) is feasible and the other individual (chromosome) is infeasible
                //if both individuals (chromosomes) are infeasible, but with different amount of infeasibility
                if (penalty < crom.penalty)
                    dominates = true;
                else if (penalty > crom.penalty)
                    dominatedBy = true;
            }
            if (dominates == true && dominatedBy == false)
                return 1;
            else if (dominates == false && dominatedBy == true)
                return -1;
            else
                return 0;
        }

        //********************************************
        //*****implementation of abstract methods*****
        //********************************************


        public override bool IsBetterThan(Chromosome crom)
        {
            bool result = false;
            var mCrom = (MultiObjectiveChromosome)crom;
            if (rank < mCrom.rank || ((rank == mCrom.rank) && (CrowdingDistance > mCrom.crowdingDistance)))
                result = true;
            return result;
        }







        public override void Copy(Chromosome crom)
        {
            var mCrom = (MultiObjectiveChromosome)crom;
            for (int i = 0; i < Template.DesignVariables.Count; i++)
				genes[i] = mCrom.Genes[i];
            for (int i = 0; i < Template.Objectives.Count; i++)
            {
				objectiveValues[i] = mCrom.ObjectiveValues[i];
				fitnessValues[i] = mCrom.FitnessValues[i];
            }
            for (int i = 0; i < Template.Constraints.Count; i++)
            {
				constraintValues[i] = crom.ConstraintValues[i];
				constraintViolationValues[i] = crom.ConstraintViolationValues[i];
            }
			noOfConstraintViolations = mCrom.NoOfConstraintViolations;
			penalty = crom.Penalty;
			rank = mCrom.Rank;
			crowdingDistance = mCrom.CrowdingDistance;
        }







        //*************************************
        //*****toString() overriden method*****
        //*************************************

        public override string ToString()
        {
			string tempString = ""; //string to be returned
            for (int i = 0; i < Template.DesignVariables.Count; i++)
            {
                tempString = tempString + "Var" + i + genes[i].ToString("F6") + "  ";
            }
            for (int i = 0; i < Template.Objectives.Count; i++)
            {
                tempString = tempString + "Obj" + i + objectiveValues[i].ToString("F6") + "  " + fitnessValues[i].ToString("F6") + "  ";
            }
            for (int i = 0; i < Template.Constraints.Count; i++)
            {
                tempString = tempString + "Ctr" + i + constraintValues[i].ToString("F6") + " " + constraintViolationValues[i].ToString("F6") + "  ";
            }
            return tempString;
        }
    }
}
