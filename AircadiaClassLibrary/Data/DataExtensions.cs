using System;
using System.Collections.Generic;

namespace Aircadia.ObjectModel.DataObjects
{
	public static class DataExtensions
	{
		public static double GetValueAsDouble(this Data data)
		{
			switch (data.Value)
			{
				case double d:
					return d;
				case int i:
					return i;
				default:
					throw new InvalidOperationException("The underlying data type '{TypeName}' cannot provide its value as a double");
			}
		}

		// By Atif. Modified Sergio
		public static T[] GetValues<T>(this List<Data> dataObjects) => dataObjects.ConvertAll(d => (T)Convert.ChangeType(d.Value, typeof(T))).ToArray();
		public static void SetValues<T>(this List<Data> dataObjects, T[] dataValues)
		{
			if (dataValues.Length != dataObjects.Count)
			{
				//Console.WriteLine("Incorrect lenght of input");
				throw new ArgumentException("Incorrect lenght of input");
			}
			int i = 0;
			foreach (Data data in dataObjects)
				data.Value = dataValues[i++];
		}

		public static object[] GetValues(this List<Data> dataObjects) => GetValues<object>(dataObjects);
		public static double[] GetValuesDouble(this List<Data> dataObjects) => GetValues<double>(dataObjects);
		public static void SetValuesDouble(this List<Data> dataObjects, double[] dataValues) => SetValues(dataObjects, dataValues);
		public static double[] GetValuesDouble(this List<Data> dataObjects, int[] whichval)
		{
			double[] dataValues = new double[whichval.Length];
			for (int i = 0; i < whichval.Length; i++)
			{
				dataValues[i] = (double)(dataObjects[whichval[i]].Value);
			}
			return dataValues;
		}

		// By Sergio
		public static bool IsSuported(this Type type) => type == typeof(int) || type == typeof(double) || type == typeof(string) || type == typeof(int[]) || type == typeof(double[]) || type == typeof(double[,]);

		public static (List<Data> group1, List<Data> group2) Classify(this IEnumerable<Data> list, Predicate<Data> predicate)
		{
			var group1 = new List<Data>();
			var group2 = new List<Data>();
			foreach (Data data in list)
			{
				if (predicate(data))
				{
					group1.Add(data);
				}
				else
				{
					group2.Add(data);
				}
			}

			return (group1, group2);
		}
	}
}
