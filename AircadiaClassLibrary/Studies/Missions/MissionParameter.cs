using Aircadia.ObjectModel.DataObjects;
using Aircadia.ObjectModel.Util;
using MathNet.Numerics;
using MathNet.Numerics.Interpolation;
using System;
using System.Linq;
using Aircadia.Services.Serializers;

namespace Aircadia.ObjectModel.Studies.Missions
{
	public abstract class MissionParameter : INamedComponent
	{
		public MissionParameter(Data data, int samples, double start, double duration)
		{
			Data = data;
			Start = start;
			Duration = duration;
			Samples = samples;
		}

		public override string ToString() => Name;

		[Serialize(Type = SerializationType.Reference)]
		public Data Data { get; set; }

		public string Name => Data.Name;

		[Serialize]
		public virtual double Start { get; set; }
		[Serialize]
		public virtual double Duration { get; set; }
		[Serialize]
		public virtual int Samples { get; set; }
	}

	public class ConstantMissionParameter : MissionParameter
	{
		[DeserializeConstructor]
		public ConstantMissionParameter(Data data, int samples, double start, double duration, object value) 
			: base(data, samples, start, duration)
		{
			Value = value;
			Time = start + duration / 2;
		}

		public virtual void Update(object value) => Data.Value = value;

		public virtual object Value { get; protected set; } // => Values?[0] ?? Double.NaN;

		public double Time { get; }
	}

	public class ConstantOutputMissionParameter : ConstantMissionParameter
	{
		[DeserializeConstructor]
		public ConstantOutputMissionParameter(Data data, int samples, double start, double duration)
			: base(data, samples, start, duration, data.Value) { } 

		public override void Update(object value) => Value = Math.Max(Convert.ToDouble(Value), Convert.ToDouble(value));
	}


	public class VariableMissionParameter : MissionParameter
	{
		public VariableMissionParameter(Data data, int samples, double start, double duration) : base(data, samples, start, duration)
		{
			values = new double[Samples];
			UserValues = new double[0];
			UserTimes = new double[0];
		}

		[DeserializeConstructor]
		public VariableMissionParameter(Data data, int samples, double start, double duration, double[] userTimes, double[] userValues, bool calculate = false) : this(data, samples, start, duration)
		{
			UserValues = userValues ?? new double[0];
			UserTimes = userTimes ?? new double[0];

			if (calculate) Calculate();
		}

		public virtual void Calculate()
		{
			values = UserValues;
			Times = UserTimes;
		}

		public virtual void Update(object value, int index) {
			if (values.Length == 0)
				values = new double[Samples];

			values[index] = Convert.ToDouble(value);
		}

		protected double[] values;
		public object[] Values => values.Cast<object>().ToArray();
		public double[] Times { get; protected set; }

		[Serialize]
		public double[] UserValues { get; protected set; }
		[Serialize]
		public double[] UserTimes { get; protected set; }

	}

	public class LinearSplineVariableMissionParameter : VariableMissionParameter
	{
		[DeserializeConstructor]
		public LinearSplineVariableMissionParameter(Data data, int samples, double start, double duration, double[] userTimes, double[] userValues) : base(data, samples, start, duration, userTimes, userValues, true) { }

		public override void Calculate()
		{
			Times = Generate.LinearSpaced(base.Samples, base.Start, base.Start + base.Duration);

			LinearSpline interpolator = (UserTimes.Length < 2) 
				? LinearSpline.InterpolateSorted(new double[] { UserTimes[0] + 1, UserTimes[0] }, new double[] { UserValues[0], UserValues[0] })
				: LinearSpline.InterpolateSorted(UserTimes, UserValues);
			values = Times.Select(t => interpolator.Interpolate(t)).ToArray();
		}
	}

	public class PolyFitVariableMissionParameter : VariableMissionParameter
	{
		[DeserializeConstructor]
		public PolyFitVariableMissionParameter(Data data, int samples, double start, double duration, double[] userTimes, double[] userValues, int order) : base(data, samples, start, duration, userTimes, userValues, false)
		{
			Order = order;
			Calculate();
		}

		public override void Calculate()
		{
			Times = Generate.LinearSpaced(base.Samples, base.Start, base.Start + base.Duration);

			PolyFit.FitAndGetPoints(UserTimes, UserValues, Order, Samples, Times, out double[] caculatedVariables);
			values = caculatedVariables;
		}

		[Serialize]
		public int Order { get; }
	}

}
