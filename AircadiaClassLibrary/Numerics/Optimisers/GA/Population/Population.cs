using System;

using Aircadia.ObjectModel.Treatments.Optimisers.Formulation;

namespace Aircadia.ObjectModel.Treatments.Optimisers.GA.Population
{
	[Serializable()]
    public abstract class Population
    {
        protected OptimisationTemplate Template;
        protected GAParameters gaParameters;

        //size of the population
        protected int size;

        //chromosomes of the population
        protected Chromosome.Chromosome[] chromosomes;


        public Population(OptimisationTemplate Template, GAParameters gaParameters)
            : this(Template, gaParameters, gaParameters.PopulationSize)
        {
        }

        public Population(OptimisationTemplate Template, GAParameters gaParameters, int size)
        {
            this.Template = Template;
            this.gaParameters = gaParameters;
            this.size = size;
        }


		//*************************
		//*****population size*****
		//*************************

		//the populationSize can not be assigned a value, i.e. the populationSize can not be changed once population object has been created with certain populationSize
		public int Size => size;


		//***************************************
		//*****chromosomes of the population*****
		//***************************************

		//indexer for chromosomes
		public Chromosome.Chromosome this[int index]
		{
			get => chromosomes[index];
			set
			{
				for (int i = 0; i < chromosomes[index].Genes.Length; i++)
					(chromosomes[index])[i] = value[i];
			}
		}

	}
}
