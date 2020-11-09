using System;

using Aircadia.ObjectModel.DataObjects;
using Aircadia.ObjectModel.Treatments.Optimisers.Formulation;

namespace Aircadia.ObjectModel.Treatments.Optimisers.GA.Chromosome
{
	[Serializable()]
    public abstract class Chromosome
    {
        //optimisation problem template
        protected OptimisationTemplate Template;
        //genetic algorithm settings
        protected GAParameters gaParameters;

        //an array of double representing chromosome/'individual solution' (combination of genes)
        //one gene represents a solution for one design variable
        protected double[] genes;




        protected double[] constraintValues;
        //*****the values for these three variables will be assigned by evaluateConstraint() method*****
        //stores the violation values for all constraints
        //(length of constraintViolationVal will be equal to the number of constraints, i.e. will not be initialised if there are no constraints)
        protected double[] constraintViolationValues;
        //number of constraint violations for this chromosome/individual, will be initialised to 0 upon object creation
        protected int noOfConstraintViolations;
        //penalty for this chromosome/'individual solution', will be initialised to 0.0 upon object creation
        protected double penalty;

        public Chromosome(OptimisationTemplate Template, GAParameters gaParameters, bool empty)
        {
            this.Template = Template;
            this.gaParameters = gaParameters;


            int noOfDesignVariables = Template.DesignVariables.Count;
			genes = new double[noOfDesignVariables];
            for (int i = 0; i < noOfDesignVariables; i++)
            {
                if (Template.DesignVariables[i].Type == DesignVariableType.Integer)
                {
                    if (!empty)
						genes[i] = GARandom.BoundedRandomInteger((int)Template.DesignVariables[i].LowerBound, (int)Template.DesignVariables[i].UpperBound);
                    else
						genes[i] = 0.0;
                }
                if (Template.DesignVariables[i].Type == DesignVariableType.Double)
                {
                    if (!empty)
						genes[i] = GARandom.BoundedRandomDouble(Template.DesignVariables[i].LowerBound, Template.DesignVariables[i].UpperBound);
                    else
						genes[i] = 0.0;
                }
            }
			constraintValues = new double[Template.Constraints.Count];
			constraintViolationValues = new double[Template.Constraints.Count];
        }

        //***************
        //*****genes*****
        //***************

        public double[] Genes
		{
			get => genes;
			set
			{
				for (int i = 0; i < genes.Length; i++)
					genes[i] = value[i];
			}
		}

		//indexer for genes
		public double this[int index]
		{
			get => genes[index];
			set => genes[index] = value;
		}



		//*********************
		//*****constraints*****
		//*********************

		//calculating and assigning constraint violation values, number of constraint violations and penalty
		public void EvaluateConstraints()
        {
			//first, setting penalty equal to 0
			penalty = 0.0;

            int noOfConstraints = Template.Constraints.Count;
            if (noOfConstraints > 0)
            {
                for (int i = 0; i < Template.Constraints.Count; i++)
                {
                    if (Template.Constraints[i].Type == ConstraintType.LessThanOrEqual)
                    {
                        Data data = Template.ExecutableComponent.ModelDataOutputs.Find(delegate(Data d) { return d.Name == Template.Constraints[i].Name; });
                        if (data is DoubleData)
							constraintValues[i] = (double)data.Value;
						constraintViolationValues[i] = constraintValues[i] - Template.Constraints[i].Value;
                    }
                    else if (Template.Constraints[i].Type == ConstraintType.GreatorThanOrEqual)
                    {
                        Data data = Template.ExecutableComponent.ModelDataOutputs.Find(delegate(Data d) { return d.Name == Template.Constraints[i].Name; });
                        if (data is DoubleData)
							constraintValues[i] = (double)data.Value;
						constraintViolationValues[i] = -constraintValues[i] + Template.Constraints[i].Value;
                    }
                }



                for (int i = 0; i < noOfConstraints; i++)
                {
                    if (constraintViolationValues[i] <= 0.0)
                        constraintViolationValues[i] = 0.0;
                }
                for (int i = 0; i < noOfConstraints; i++)
                {
                    //assigning value to noOfConstraintViolations for current chromosome
                    if (constraintViolationValues[i] > 0)
						noOfConstraintViolations++;
                    //calculating and assigning value to penalty for current chromosome
                    if (gaParameters.ConstraintHandlingMethod == ConstraintHandlingMethods.LinearPenalty)
						penalty += gaParameters.PenaltyWeights[i] * Math.Abs(constraintViolationValues[i]);
                    else if (gaParameters.ConstraintHandlingMethod == ConstraintHandlingMethods.QuadraticPenalty)
						penalty += gaParameters.PenaltyWeights[i] * Math.Pow(constraintViolationValues[i], 2);
                    else if (gaParameters.ConstraintHandlingMethod == ConstraintHandlingMethods.Tournament)
						penalty += Math.Abs(constraintViolationValues[i]);
                }
            }
        }

        public double[] ConstraintValues
		{
			get => constraintValues;
			set => constraintValues = value;
		}

		public double[] ConstraintViolationValues
		{
			get => constraintViolationValues;
			set => constraintViolationValues = value;
		}

		public int NoOfConstraintViolations
		{
			get => noOfConstraintViolations;
			set => noOfConstraintViolations = value;
		}

		public double Penalty
		{
			get => penalty;
			set => penalty = value;
		}


		//****************************
		//*****abstract functions*****
		//****************************

		//returns true if the calling cromosome is better than the chromosome 'crom' passed as argument
		public abstract bool IsBetterThan(Chromosome crom);







        public virtual void Copy(Chromosome crom)
        {
            for (int i = 0; i < Template.DesignVariables.Count; i++)
				genes[i] = crom.genes[i];
            for (int i = 0; i < Template.Constraints.Count; i++)
            {
				constraintValues[i] = crom.constraintValues[i];
				constraintViolationValues[i] = crom.constraintViolationValues[i];
            }
			noOfConstraintViolations = crom.NoOfConstraintViolations;
			penalty = crom.Penalty;
        }


    }
}
