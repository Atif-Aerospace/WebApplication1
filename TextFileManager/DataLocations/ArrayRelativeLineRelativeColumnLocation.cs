using System;
using System.Collections.Generic;
using System.Reflection;

namespace ExeModelTextFileManager.DataLocations
{
	[Serializable()]
	public class ArrayRelativeLineRelativeColumnLocation : LocationBase, IRelativeColumn, IRelativeLine, IArrayStopLocation
	{
		public ArrayRelativeLineRelativeColumnLocation(string anchorText, string stopText, int skipLines, int position, char[] separators,int frequency = 1, Types type = Types.String, string format = "", bool removeEmpty = true) : this(new List<string> { anchorText }, stopText, skipLines, position, separators, frequency, type, format, removeEmpty) { }

		public ArrayRelativeLineRelativeColumnLocation(List<string> anchorTexts, string stopText, int skipLines, int position, char[] separators, int frequency = 1, Types type = Types.String, string format = "", bool removeEmpty = true) : base(true, true, type, format)
		{
			AnchorTexts = anchorTexts;
			StopText = stopText;
			SkipLines = skipLines;
			Position = position;
			Separators = separators;
			Frequency = frequency;
		}

		public List<string> AnchorTexts { get; protected set; }
		public string StopText { get; protected set; }
		public int SkipLines { get; protected set; }
		public int Position { get; protected set; }
		public char[] Separators { get; protected set; }
		public int Frequency { get; protected set; }
		public bool RemoveEmpty { get; protected set; }

		private MethodInfo mi;

		public override void Update(TextFileManager tfm, object input) => tfm.UpdateArray(AnchorTexts, SkipLines, Position, input as object[], Separators, Frequency, Format);

		protected override object Read_(TextFileManager tfm, Type t)
		{
			if (mi == null)
			{
				mi = typeof(TextFileManagerExtensions).GetMethod("ReadArray", new Type[] { typeof(TextFileManager), typeof(Type), typeof(List<string>), typeof(string), typeof(int), typeof(int), typeof(int), typeof(char[]), typeof(bool) }).MakeGenericMethod(new Type[] { t });
			}
			return mi.Invoke(null, new object[] { tfm, t, AnchorTexts, StopText, SkipLines, Position, Frequency, Separators, RemoveEmpty });
		}
	}
}
