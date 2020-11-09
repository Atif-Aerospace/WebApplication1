using Aircadia.Services.Serializers;
using System;

namespace Aircadia.ObjectModel.DataObjects
{
	[Serializable]
    public class DoubleData : ScalarData
    {
		private const int DefaultDecimalPlaces = 4;
		private const double DefaultValue = 0.0;

		public override string GetDataType() => "Double";

		public override string ValueAsString
		{
			get => ((double)Value).ToString($"F{DecimalPlaces}");
			set => Value = Convert.ToDouble(value);
		}

		[Serialize()]
        public int DecimalPlaces { get; set; }

        public DoubleData(string name, bool isAuxiliary = false)
            : this(name, DefaultValue, isAuxiliary)
        {
        }

        public DoubleData(string name, double value, bool isAuxiliary = false)
            : this(name, null, value, DefaultDecimalPlaces, null, isAuxiliary: isAuxiliary)
        {
        }

		[DeserializeConstructor]
        public DoubleData(string name, string description, double value, int decimalPlaces, string unit, 
			Dimension dimension = default(Dimension), bool isAuxiliary = false, string parentName = "", string displayName = "")
            : base(name, description, unit, dimension, isAuxiliary, parentName, displayName)
        {
			Value = value;
			DecimalPlaces = decimalPlaces;
			Min = Double.NegativeInfinity;
			Max = Double.PositiveInfinity;
		}

		///// <summary>
		///// The string returned by this function is the one which will be printed in the properties window of AirCADia for the selected data object.
		///// </summary>
		///// <returns></returns>
		//public override string PropertiesSummaryText()
		//{
		//	string output = "NAME: " + Name + " \r\n";
		//	output = output + "TYPE: " + "Double" + "\r\n";
		//	output = output + "VALUE: " + ValueAsString + "\r\n";
		//	output = output + "MINIMUM: " + Convert.ToString(Min) + "\r\n";
		//	output = output + "MAXIMUM: " + Convert.ToString(Max) + "\r\n";
		//	output = output + "UNIT: " + Unit;
		//	return output;
		//}

		public override bool IsValidSerialization(string serializedData, out string reason)
		{
			reason = String.Empty;
			if (Double.TryParse(serializedData, out double DoubleNumber) == false)
			{
				reason = $"The provided data value is not a Double (Incorrect element: {serializedData}). Please provide a value consistent with the specified data type.";
				return false;
			}

			//Checking Min and Max values:
			if (Min > DoubleNumber)
			{
				reason = "The provided 'Min Value' is greater than the specified data value.";
				return false;
			}

			if (Max < DoubleNumber)
			{
				reason = "The provided 'Max Value' is smaller than the specified data value.";
				return false;
			}

			return true;
		}

		public override Data Copy(string id, string name = null, string parentName = null)
			=> new DoubleData(id, Description, (double)Value, DecimalPlaces, Unit, Dimension, IsAuxiliary, parentName ?? parentName, name ?? Name);
	}
}
