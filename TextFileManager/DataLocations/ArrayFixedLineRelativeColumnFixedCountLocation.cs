using System;
using System.Collections.Generic;
using System.Reflection;

namespace ExeModelTextFileManager.DataLocations
{
	[Serializable()]
	public class ArrayFixedLineRelativeColumnFixedCountLocation : LocationBase, IFixedLine, IRelativeColumn, IArrayCountLocation
	{
		public ArrayFixedLineRelativeColumnFixedCountLocation(int line, int count, int position, char[] separators, int frequency = 1, Types type = Types.String, string format = "", bool removeEmpty = true) : base(true, true, type, format)
		{
			Line = line;
			Count = count;
			Position = position;
			Separators = separators;
			Frequency = frequency;
		}

		public int Line { get; protected set; }
		public int Count { get; protected set; }
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
				mi = typeof(TextFileManagerExtensions).GetMethod("ReadArray", new Type[] { typeof(TextFileManager), typeof(Type), typeof(int), typeof(int), typeof(int), typeof(int), typeof(char[]), typeof(bool) }).MakeGenericMethod(new Type[] { t });
			}
			return mi.Invoke(null, new object[] { tfm, t, Line, Count, Position, Frequency, Separators, RemoveEmpty });
		}
	}
}
