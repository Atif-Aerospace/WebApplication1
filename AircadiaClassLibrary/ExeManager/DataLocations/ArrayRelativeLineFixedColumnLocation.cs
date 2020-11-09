using Aircadia.Services.Serializers;
using System;
using System.Collections.Generic;

namespace ExeModelTextFileManager.DataLocations
{
	[Serializable()]
	public class ArrayRelativeLineFixedColumnLocation : LocationBase, IFixedColumn, IRelativeLine, IArrayStopLocation
	{
		public ArrayRelativeLineFixedColumnLocation(string anchorText, string stopText, int skipLines, int startColumn, int endColumn, int frequency, Types type = Types.String, string format = "", bool removeEmpty = true) : this(new List<string> { anchorText }, stopText, skipLines, startColumn, endColumn, frequency, type, format, removeEmpty) { }

		[DeserializeConstructor]
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

		[Serialize]
		public List<string> AnchorTexts { get; protected set; }
		[Serialize]
		public string StopText { get; protected set; }
		[Serialize]
		public int SkipLines { get; protected set; }
		[Serialize]
		public int StartColumn { get; protected set; }
		[Serialize]
		public int EndColumn { get; protected set; }
		[Serialize]
		public int Frequency { get; protected set; }
		[Serialize]
		public bool RemoveEmpty { get; protected set; }

		public override void Update(TextFileManager tfm, object input) => tfm.UpdateArray(AnchorTexts, SkipLines, StartColumn, EndColumn, input as object[], Frequency, Format);

		public override object Read(TextFileManager tfm)
		{
			switch (Type)
			{
				case Types.Int:
					return tfm.ReadArray<int>(AnchorTexts, StopText, SkipLines, StartColumn, EndColumn, Frequency, RemoveEmpty);
				case Types.Double:
					return tfm.ReadArray<double>(AnchorTexts, StopText, SkipLines, StartColumn, EndColumn, Frequency, RemoveEmpty);
				case Types.String:
					return tfm.ReadArray<string>(AnchorTexts, StopText, SkipLines, StartColumn, EndColumn, Frequency, RemoveEmpty);
				case Types.Bool:
					return tfm.ReadArray<bool>(AnchorTexts, StopText, SkipLines, StartColumn, EndColumn, Frequency, RemoveEmpty);
				default:
					return tfm.ReadArray<object>(AnchorTexts, StopText, SkipLines, StartColumn, EndColumn, Frequency, RemoveEmpty);
			}
		}
	}
}
