using System;
using System.Collections.Generic;
using System.Reflection;

namespace ExeModelTextFileManager.DataLocations
{
	[Serializable()]
	public class ArrayFixedLineFixedColumnLocation : LocationBase, IFixedLine, IFixedColumn, IArrayStopLocation
	{
		public ArrayFixedLineFixedColumnLocation(int line, string stopText, int startColumn, int endColumn, int frequency, Types type = Types.String, string format = "", bool removeEmpty = true) : base(true, true, type, format)
		{
			Line = line;
			StopText = stopText;
			StartColumn = startColumn;
			EndColumn = endColumn;
			Frequency = frequency;
			RemoveEmpty = removeEmpty;
		}

		public int Line { get; protected set; }
		public string StopText { get; protected set; }
		public int StartColumn { get; protected set; }
		public int EndColumn { get; protected set; }
		public int Frequency { get; protected set; }
		public bool RemoveEmpty { get; protected set; }

		private MethodInfo mi;

		protected override object Read_(TextFileManager tfm, Type t)
		{
			if (mi == null)
			{
				mi = typeof(TextFileManagerExtensions).GetMethod("ReadArray", new Type[] { typeof(TextFileManager), typeof(Type), typeof(int), typeof(string), typeof(int), typeof(int), typeof(int), typeof(bool) }).MakeGenericMethod(new Type[] { t });
			}
			return mi.Invoke(null, new object[] { tfm, t, Line, StopText, StartColumn, EndColumn, Frequency, RemoveEmpty });
		}

		public override void Update(TextFileManager tfm, object input) => tfm.UpdateArray(Line, StartColumn, EndColumn, input as object[], Frequency, Format);
	}
}
