using System;

using Aircadia.ObjectModel.DataObjects;
using Aircadia.ObjectModel.Distributions;
using Aircadia.Services.Serializers;

namespace Aircadia.ObjectModel.Treatments.Optimisers.Formulation
{
	[Serializable()]
    public class Objective : INamedComponent
	{
		[Serialize]
		public ObjectiveType Type { get; }

		[Serialize(Type = SerializationType.Reference)]
		public Data Data { get; }
		public string Name => Data.Name;
		public double Value { get => Data.GetValueAsDouble(); set => Data.Value = value; }

		[DeserializeConstructor]
		public Objective(Data data, ObjectiveType type)
		{
			Type = type;
			Data = data;
		}
    }

	public class SingleObjective : Objective
	{
		[Serialize]
		public double Weight { get; }

		[DeserializeConstructor]
		public SingleObjective(Data data, double weight, ObjectiveType type) : base(data, type)
		{
			Weight = weight;
		}
	}

	public class MeanObjective : Objective
	{
		[DeserializeConstructor]
		public MeanObjective(Data data, ObjectiveType type) : base(data, type)
		{
		}
	}

	public class WeightLossFunctionObjective : Objective
	{
		[Serialize]
		public double Weight { get; protected set; }
		[Serialize]
		public int Sign { get; protected set; }

		[DeserializeConstructor]
		public WeightLossFunctionObjective(Data data, ObjectiveType type, double weight, int sign) : base(data, type)
		{
			Weight = weight;
			Sign = sign;
		}
	}

	public class DistributionLossFunctonObjective : WeightLossFunctionObjective
	{
		[Serialize]
		public IRoustOptimizationDistribution Distribution { get; }
		[Serialize]
		public double Probability { get; }

		[DeserializeConstructor]
		public DistributionLossFunctonObjective(IRoustOptimizationDistribution distribution, ObjectiveType type, int sign, double probability) : base(distribution.Data, type, distribution.GetK(probability), sign)
		{
			Distribution = distribution;
			Probability = probability;
		}
	}
}
