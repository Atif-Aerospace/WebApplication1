using Aircadia.Services.Serializers;
using System;

namespace Aircadia.ObjectModel.DataObjects
{
	[Serializable]
    public class IntegerData : ScalarData
    {
		private const int DefaultValue = 0;

		public override string GetDataType() => "Integer";

		public override string ValueAsString
		{
			get => Convert.ToString(Value);
			set => Value = Convert.ToInt32(value);
		}

        public IntegerData(string name, bool isAuxiliary = false)
            : this(name, null, DefaultValue, null, isAuxiliary: isAuxiliary)
        {
        }


		[DeserializeConstructor]
		public IntegerData(string name, string description, int value, string unit,
			Dimension dimension = default(Dimension), bool isAuxiliary = false, string parentName = "", string displayName = "")
            : base(name, description, unit, dimension, isAuxiliary, parentName, displayName)
        {
            Value = value;
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
		//	output = output + "TYPE: " + "Integer" + "\r\n";
		//	output = output + "VALUE: " + ValueAsString + "\r\n";
		//	output = output + "MINIMUM: " + Convert.ToString(Min) + "\r\n";
		//	output = output + "MAXIMUM: " + Convert.ToString(Max) + "\r\n";
		//	output = output + "UNIT: " + Unit;
		//	return output;
		//}

		public override bool IsValidSerialization(string serializedData, out string reason)
		{
			reason = String.Empty;
			if (Int32.TryParse(serializedData, out int IntNumber) == false)
			{
				reason = $"The provided data value is not an Int (Incorrect element: {serializedData}). Please provide a value consistent with the specified data type.";
				return false;
			}

			//Checking Min and Max values:
			if (Min > IntNumber)
			{
				reason = "The provided 'Min Value' is greater than the specified data value.";
				return false;
			}

			if (Max < IntNumber)
			{
				reason = "The provided 'Max Value' is smaller than the specified data value.";
				return false;
			}

			return true;
		}

		public override Data Copy(string id, string name = null, string parentName = null)
			=> new IntegerData(id, Description, (int)Value, Unit, Dimension, IsAuxiliary, parentName ?? parentName, name ?? Name);
	}
}
