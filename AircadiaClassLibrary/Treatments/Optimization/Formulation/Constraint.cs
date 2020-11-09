using System;

using Aircadia.ObjectModel.DataObjects;
using Aircadia.ObjectModel.Distributions;
using Aircadia.Services.Serializers;

namespace Aircadia.ObjectModel.Treatments.Optimisers.Formulation
{
	[Serializable()]
	public class Constraint : INamedComponent
	{
		[Serialize]
		public ConstraintType Type { get; set; }
		[Serialize]
		public double Value { get; set; }
		[Serialize(Type = SerializationType.Reference)]
		public Data Data { get; }
		public string Name => Data.Name;

		[DeserializeConstructor]
		public Constraint(Data data, double constraintValue, ConstraintType type)
		{
			Type = type;
			Value = constraintValue;
			Data = data;
		}

		public Constraint(string name, ConstraintType type, double constraintValue, Data data)
		{
			Type = type;
			Value = constraintValue;
			Data = data;
		}
	}

	public class MeanConstraint : Constraint
	{
		[DeserializeConstructor]
		public MeanConstraint(Data data, double constraintValue, ConstraintType type) : base(data, constraintValue, type)
		{
		}
	}

	public class WeightLossFunctionConstraint : Constraint
	{
		[Serialize]
		public double Weight { get; protected set; }
		[Serialize]
		public int Sign { get; protected set; }

		[DeserializeConstructor]
		public WeightLossFunctionConstraint(Data data, double constraintValue, ConstraintType type, double weight, int sign) : base(data, constraintValue, type)
		{
			Weight = weight;
			Sign = sign;
		}
	}

	public class DistributionLossFunctonConstraint : WeightLossFunctionConstraint
	{
		[Serialize]
		public IRoustOptimizationDistribution Distribution { get; }
		[Serialize]
		public double Probability { get; }

		[DeserializeConstructor]
		public DistributionLossFunctonConstraint(IRoustOptimizationDistribution distribution, double constraintValue, ConstraintType type, int sign, double probability) : base(distribution.Data, constraintValue, type, distribution.GetK(probability), sign)
		{
			Distribution = distribution;
			Probability = probability;
		}
	}
}
