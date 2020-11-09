using System;
using System.Collections.Generic;
using System.Reflection;

namespace ExeModelTextFileManager.DataLocations
{
	[Serializable()]
	public class ArrayRelativeLineFixedColumnLocation : LocationBase, IFixedColumn, IRelativeLine, IArrayStopLocation
	{
		public ArrayRelativeLineFixedColumnLocation(string anchorText, string stopText, int skipLines, int startColumn, int endColumn, int frequency, Types type = Types.String, string format = "", bool removeEmpty = true) : this(new List<string> { anchorText }, stopText, skipLines, startColumn, endColumn, frequency, type, format, removeEmpty) { }
		public ArrayRelativeLineFixedColumnLocation(List<string> anchorTexts, string stopText, int skipLines, int startColumn, int endColumn, int frequency, Types type = Types.String, string format = "", bool removeEmpty = true) : base(true, false, type, format)
		{
			AnchorTexts = anchorTexts;
			StopText = stopText;
			SkipLines = skipLines;
			StartColumn = startColumn;
			EndColumn = endColumn;
			Frequency = frequency;
			RemoveEmpty = removeEmpty;
		}

		private void GetMethodInfo()
		{

		}

		public List<string> AnchorTexts { get; protected set; }
		public string StopText { get; protected set; }
		public int SkipLines { get; protected set; }
		public int StartColumn { get; protected set; }
		public int EndColumn { get; protected set; }
		public int Frequency { get; protected set; }
		public bool RemoveEmpty { get; protected set; }

		private MethodInfo mi;

		public override void Update(TextFileManager tfm, object input) => tfm.UpdateArray(AnchorTexts, SkipLines, StartColumn, EndColumn, input as object[], Frequency, Format);

		protected override object Read_(TextFileManager tfm, Type t)
		{
			if (mi == null)
			{
				mi = typeof(TextFileManagerExtensions).GetMethod("ReadArray", new Type[] { typeof(TextFileManager), typeof(Type), typeof(List<string>), typeof(string), typeof(int), typeof(int), typeof(int), typeof(int), typeof(bool) }).MakeGenericMethod(new Type[] { t });
			}
			return mi.Invoke(null, new object[] { tfm, t, AnchorTexts, StopText, SkipLines, StartColumn, EndColumn, Frequency, RemoveEmpty });
		}
	}
}
