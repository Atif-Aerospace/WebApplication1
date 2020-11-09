using System;
using Aircadia.ObjectModel.DataObjects;
using Aircadia.Services.Serializers;

namespace Aircadia.ObjectModel.Treatments.Optimisers.Formulation
{
	[Serializable()]
    public abstract class DesignVariable : INamedComponent
	{
		[Serialize(Type= SerializationType.Reference)]
		public ScalarData Data { get; }
		public string Name => Data.Name;
		public double Value { get => Data.GetValueAsDouble(); set => Data.Value = value; }
		public DesignVariableType Type => (Data is IntegerData) ? DesignVariableType.Integer : DesignVariableType.Double;

		public DesignVariable(ScalarData data)
		{
			Data = data;
		}
    }

	[Serializable()]
	public class BoundedDesignVariableNoInital : DesignVariable
	{
		[Serialize]
		public double LowerBound { get; protected set; }
		[Serialize]
		public double UpperBound { get; protected set; }

		public BoundedDesignVariableNoInital(ScalarData data)
			: this(data, data.Min, data.Max)
		{
		}

		[DeserializeConstructor]
		public BoundedDesignVariableNoInital(ScalarData data, double lowerBound, double upperBound) : base(data)
		{
			LowerBound = lowerBound;
			UpperBound = upperBound;
		}

		public BoundedDesignVariableNoInital(string name, ScalarData wrappedData, double lowerBound, double upperBound) : base(wrappedData)
		{
			LowerBound = lowerBound;
			UpperBound = upperBound;
		}

	}

	[Serializable()]
	public class BoundedDesignVariable : DesignVariable
	{
		[Serialize]
		public double InitialValue { get; }
		[Serialize]
		public double LowerBound { get; }
		[Serialize]
		public double UpperBound { get; }

		public BoundedDesignVariable(ScalarData data)
			: this(data, data.GetValueAsDouble(), data.Min, data.Max)
		{
		}

		[DeserializeConstructor]
		public BoundedDesignVariable(ScalarData data, double initialValue, double lowerBound, double upperBound) : base(data)
		{
			InitialValue = initialValue;
			LowerBound = lowerBound;
			UpperBound = upperBound;
		}
	}

	[Serializable()]
	public class UnboundedDesignVariable : DesignVariable
	{
		[Serialize]
		public double InitialValue { get; }

		public UnboundedDesignVariable(ScalarData data)
			: this(data, data.GetValueAsDouble())
		{
		}

		[DeserializeConstructor]
		public UnboundedDesignVariable(ScalarData data, double initialValue) : base(data)
		{
			InitialValue = initialValue;
		}
	}
}
