using Aircadia.Services.Serializers;
using System;
using System.Text;

namespace Aircadia.ObjectModel.DataObjects
{
	[Serializable]
    public class DoubleVectorData : VectorData
    {
		private const int DefaultDecimalPlaces = 4;

		public override string GetDataType() => "DoubleVector";
		public override string ValueAsString
		{
			get => ValueToString(Value as double[], DecimalPlaces);
			set => Value = StringToValue(value);
		}

		[Serialize]
		public int DecimalPlaces { get; set; }

		public static string ValueToString(double[] value, int decimalPlaces = DefaultDecimalPlaces)
		{
			if (value == null || value.Length == 0) return String.Empty;

			var str = new StringBuilder(String.Empty, value.Length * (decimalPlaces + 1)); 
			string format = $"F{decimalPlaces}";
			foreach (double n in value)
			{
				str.Append(n.ToString(format) + ",");
			}
			str.Remove(str.Length - 1, 1);
			return str.ToString();
		}

		public static double[] StringToValue(string stringValue)
        {
			if (String.IsNullOrWhiteSpace(stringValue))
			{
				return new double[0];
			}

			string[] stringValues = stringValue.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            int n = stringValues.Length;
            double[] array = new double[n];
            for (int i = 0; i < n; i++)
            {
                array[i] = Convert.ToDouble(stringValues[i]);
            }
            return array;
        }

        public override string ToString()
        {
            return ValueAsString;
        }

        //public string Dimensions => (Value as double[]).Length.ToString();

        public DoubleVectorData(string name)
			: this(name, null, null, DefaultDecimalPlaces, null)
		{
        }

		[DeserializeConstructor]
        public DoubleVectorData(string name, string description, double[] value, int decimalPlaces, string unit, bool isAuxiliary = false, string parentName = "", string displayName = "")
            : base(name, description, unit, isAuxiliary, parentName: parentName, displayName: displayName)
        {
            Value = value ?? new double[1];
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
		//	string output = "NAME: " + Name + "\r\n";
		//	output = output + "TYPE: " + "Double" + "\r\n";
		//	output = output + "VALUE: " + ValueAsString + "\r\n";
		//	output = output + "MINIMUM: " + Convert.ToString(Min) + "\r\n";
		//	output = output + "MAXIMUM: " + Convert.ToString(Max) + "\r\n";
		//	output = output + "UNIT: " + Unit;
		//	return output;
		//}

		public override bool IsValidSerialization(string serializedData, out string reason)
		{
			serializedData = serializedData.TrimStart('[');
			serializedData = serializedData.TrimEnd(']');
			string[] elements = serializedData.Split(',');
			reason = String.Empty;

			foreach (string number in elements)
			{
				if (Double.TryParse(number, out double DoubleNumber) == false)
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

			}

			return true;
		}

		public override Data Copy(string id, string name = null, string parentName = null)
			=> new DoubleVectorData(id, Description, Value as double[], DecimalPlaces, Unit, IsAuxiliary, parentName ?? parentName, name ?? Name);
	}
}
