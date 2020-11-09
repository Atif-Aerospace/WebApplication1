using System;
using System.Collections.Generic;
using System.Reflection;

namespace ExeModelTextFileManager.DataLocations
{
	[Serializable()]
	public class ArrayFixedLineRelativeColumnLocation : LocationBase, IFixedLine, IRelativeColumn, IArrayStopLocation
	{
		public ArrayFixedLineRelativeColumnLocation(int line, string stopText, int position, char[] separators, int frequency = 1, Types type = Types.String, string format = "", bool removeEmpty = true) : base(true, true, type, format)
		{
			Line = line;
			StopText = stopText;
			Position = position;
			Separators = separators;
			Frequency = frequency;
		}

		public int Line { get; protected set; }
		public string StopText { get; protected set; }
		public int Position { get; protected set; }
		public char[] Separators { get; protected set; }
		public int Frequency { get; protected set; }
		public bool RemoveEmpty { get; protected set; }
		public override void Update(TextFileManager tfm, object input) => tfm.UpdateArray(Line, Position, input as object[], Separators, Frequency, Format);

		private MethodInfo mi;

		protected override object Read_(TextFileManager tfm, Type t)
		{
			if (mi == null)
			{
				mi = typeof(TextFileManagerExtensions).GetMethod("ReadArray", new Type[] { typeof(TextFileManager), typeof(Type), typeof(int), typeof(string), typeof(int), typeof(int), typeof(char[]), typeof(bool) }).MakeGenericMethod(new Type[] { t });
			}
			return mi.Invoke(null, new object[] { tfm, t, Line, StopText, Position, Frequency, Separators, RemoveEmpty });
		}
	}
}
