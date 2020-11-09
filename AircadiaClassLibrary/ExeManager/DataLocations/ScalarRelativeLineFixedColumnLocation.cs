using Aircadia.Services.Serializers;
using System;
using System.Collections.Generic;

namespace ExeModelTextFileManager.DataLocations
{
	[Serializable()]
	public class ScalarRelativeLineFixedColumnLocation : LocationBase, IFixedColumn, IRelativeLine
	{
		public ScalarRelativeLineFixedColumnLocation(string anchorText, int skipLines, int startColumn, int endColumn, Types type = Types.String, string format = "") : this( new List<string>() { anchorText }, skipLines, startColumn, endColumn) { }

		[DeserializeConstructor]
		public ScalarRelativeLineFixedColumnLocation(List<string> anchorTexts, int skipLines, int startColumn, int endColumn, Types type = Types.String, string format = "") : base(true, true, type, format)
		{
			AnchorTexts = anchorTexts;
			SkipLines = skipLines;
			StartColumn = startColumn;
			EndColumn = endColumn;
		}

		[Serialize]
		public List<string> AnchorTexts { get; protected set; }
		[Serialize]
		public int SkipLines { get; protected set; }
		[Serialize]
		public int StartColumn { get; protected set; }
		[Serialize]
		public int EndColumn { get; protected set; }

		public override void Update(TextFileManager tfm, object input) => tfm.UpdateWord(AnchorTexts, SkipLines, StartColumn, EndColumn, input, Format);

		public override object Read(TextFileManager tfm)
		{
			switch (Type)
			{
				case Types.Int:
					return tfm.ReadWord<int>(AnchorTexts, SkipLines, StartColumn, EndColumn);
				case Types.Double:
					return tfm.ReadWord<double>(AnchorTexts, SkipLines, StartColumn, EndColumn);
				case Types.String:
					return tfm.ReadWord<string>(AnchorTexts, SkipLines, StartColumn, EndColumn);
				case Types.Bool:
					return tfm.ReadWord<bool>(AnchorTexts, SkipLines, StartColumn, EndColumn);
				default:
					return tfm.ReadWord<object>(AnchorTexts, SkipLines, StartColumn, EndColumn);
			}
		}
	}
}
