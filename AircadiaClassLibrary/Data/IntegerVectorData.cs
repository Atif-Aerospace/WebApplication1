using Aircadia.Services.Serializers;
using System;
using System.Text;

namespace Aircadia.ObjectModel.DataObjects
{
	[Serializable]
    public class IntegerVectorData : VectorData
    {
		public override string GetDataType() => "IntegerVector";
		public override string ValueAsString
		{
			get => ValueToString(Value as int[]);
			set => Value = StringToValue(value);
		}

		public static int[] StringToValue(string stringValue)
		{
			if (String.IsNullOrWhiteSpace(stringValue))
			{
				return new int[0];
			}

			string[] stringValues = stringValue.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
			int n = stringValues.Length;
			int[] array = new int[n];
			for (int i = 0; i < n; i++)
			{
				array[i] = Convert.ToInt32(stringValues[i]);
			}
			return array;
		}

		internal static string ValueToString(int[] value)
		{
			if (value == null || value.Length == 0) return String.Empty;

			var str = new StringBuilder(String.Empty, value.Length * 8); // Guess of how mwany chars per int
			foreach (int n in value)
			{
				str.Append(Convert.ToString(n) + ",");
			}
			str.Remove(str.Length - 1, 1);
			return str.ToString();
		}

		public override bool IsValidSerialization(string serializedData, out string reason)
		{
			serializedData = serializedData.TrimStart('[');
			serializedData = serializedData.TrimEnd(']');
			string[] elements = serializedData.Split(',');
			reason = String.Empty;

			foreach (string number in elements)
			{
				if (Int32.TryParse(number, out int IntNumber) == false)
				{
					reason = $"The provided data value is not a Int (Incorrect element: {serializedData}). Please provide a value consistent with the specified data type.";
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

			}

			return true;
		}


		public IntegerVectorData(string name)
            : this(name, null, null, null)
        {
        }

		[DeserializeConstructor]
		public IntegerVectorData(string name, string description, int[] value, string unit, bool isAuxiliary = false, string parentName = "", string displayName = "")
            : base(name, description, unit, isAuxiliary, parentName: parentName, displayName: displayName)
        {
            Value = value ?? new int[1];
			Min = Double.NegativeInfinity;
			Max = Double.PositiveInfinity;
		}

		public override Data Copy(string id, string name = null, string parentName = null)
			=> new IntegerVectorData(id, Description, Value as int[], Unit, IsAuxiliary, parentName ?? parentName, name ?? Name);

	}
}
