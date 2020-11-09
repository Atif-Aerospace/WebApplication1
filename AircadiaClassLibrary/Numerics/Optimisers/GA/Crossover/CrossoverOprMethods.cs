using System;

namespace Aircadia.ObjectModel.Treatments.Optimisers.GA.Crossover
{
	[Serializable()]
    public enum CrossoverOprMethods
    {
        OnePointCrossover,
        TwoPointCrossover,
        UniformCrossover,
        SimulatedBinaryCrossover,
    }
}
