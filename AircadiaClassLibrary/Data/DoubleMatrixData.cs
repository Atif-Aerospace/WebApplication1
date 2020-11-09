using Aircadia.Services.Serializers;
using System;
using System.Text;

namespace Aircadia.ObjectModel.DataObjects
{
	[Serializable]
    public class DoubleMatrixData : MatrixData
    {
		private const int DefaultDecimalPlaces = 4;

		public override string GetDataType() => "DoubleMatrix";
		public override string ValueAsString
		{
			get => ValueToString(Value as double[,], DecimalPlaces);
			set => Value = StringToValue(value);
		}
        
        public static string ValueToString(double[,] value, int decimalPlaces = DefaultDecimalPlaces)
		{
			if (value == null || value.Length == 0) return String.Empty;

			var sss = new StringBuilder(String.Empty, value.Length * (decimalPlaces + 1));
			double[,] arr = value;
			string format = $"F{decimalPlaces}";
			for (int i = 0; i < arr.GetLength(0); i++)
			{
				for (int j = 0; j < arr.GetLength(1); j++)
				{
					sss.Append(arr[i, j].ToString(format) + ",");
				}
				sss = sss.Remove(sss.Length - 1, 1);
				sss.Append(";");
			}
			sss = sss.Remove(sss.Length - 1, 1);

			return sss.ToString();
		}

		public static double[,] StringToValue(string stringValue)
		{
			if (String.IsNullOrWhiteSpace(stringValue))
			{
				return new double[0, 0];
			}

			string[] MatrixRows = stringValue.Split(';');
			string[] FirstRowColumnElements = MatrixRows[0].Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
			int NoFirstRowColumnElements = FirstRowColumnElements.Length;
			double[,] MatrixData = new double[MatrixRows.GetLength(0), NoFirstRowColumnElements];
			for (int j_Row = 0; j_Row < MatrixRows.GetLength(0); j_Row++)
			{
				string[] j_RowColumnElements = MatrixRows[j_Row].Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
				int NoMatrixColumnElements = j_RowColumnElements.Length;
				if (NoMatrixColumnElements != NoFirstRowColumnElements)
				{
					throw new Exception("Data not created correctly. The matrix was not specified in the correct format.");
				}
				else
				{
					for (int i_ColumnElement = 0; i_ColumnElement < NoMatrixColumnElements; i_ColumnElement++)
					{
						MatrixData[j_Row, i_ColumnElement] = Convert.ToDouble(j_RowColumnElements[i_ColumnElement]);
					}
				}
			}
			return MatrixData;

			//// Sergio
			//List<double[]> doubleVectorData = new List<double[]>();

			////Splitting the input string of values into its constituent rows:
			//InputString = Value.TrimStart('[');
			//InputString = InputString.TrimEnd(']');
			//InputStringElements = InputString.Split(';');

			//int nn = InputStringElements.Length; int mm = InputStringElements[0].Split(',').Length;
			//double[,] arr = new double[nn, mm];
			//for (int row = 0; row < nn; row++)
			//{
			//	string[] Ithrow_InputStringElements = InputStringElements[row].Split(',');
			//	for (int column = 0; column < mm; column++)
			//		arr[row, column] = Convert.ToDouble(Ithrow_InputStringElements[column]);
			//}
		}

        public override string ToString()
        {
            return ValueAsString;
        }


        //public string Dimensions { get; set; }

        [Serialize]
		public int DecimalPlaces { get; set; }

		public DoubleMatrixData(string name)
			: this(name, null, null, DefaultDecimalPlaces)
		{
        }

		[DeserializeConstructor]
		public DoubleMatrixData(string name, string description, double[,] value, int decimalPlaces, bool isAuxiliary = false, string parentName = "", string displayName = "")
            : base(name, description, isAuxiliary, parentName: parentName, displayName: displayName)
        {
            Value = value ?? new double[1, 1] { { 0 } };
            DecimalPlaces = decimalPlaces;
        }


    
		public override bool IsValidSerialization(string serializedData, out string reason)
		{
			//Splitting the input string of values into its constituent rows:
			serializedData = serializedData.TrimStart('[');
			serializedData = serializedData.TrimEnd(']');
			string[] rowElements = serializedData.Split(';');
			reason = String.Empty;
			int NFirstRow = -1;

			//Splitting the i-th row input string of values into its constituent elements:
			for (int Ithrow = 0; Ithrow < rowElements.GetLength(0); Ithrow++)
			{
				string Ithrow_InputString = rowElements[Ithrow];
				string[] Ithrow_InputStringElements = Ithrow_InputString.Split(',');

				if (NFirstRow == -1)
				{
					NFirstRow = Ithrow_InputStringElements.Length;
				}
				else if (NFirstRow != Ithrow_InputStringElements.Length)
				{
					reason = $"Row number {Ithrow} has a number of elements different then the first row. Please provide a value consistent with the specified data type.";
					return false;
				}


				foreach (string number in Ithrow_InputStringElements)
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
			}
			return true;
		}

		public override Data Copy(string id, string name = null, string parentName = null)
			=> new DoubleMatrixData(id, Description, Value as double[,], DecimalPlaces, IsAuxiliary, parentName ?? ParentName, name ?? Name);
	}
}
