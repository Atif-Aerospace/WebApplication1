using System;
using System.Collections.Generic;

namespace ExeModelTextFileManager.DataLocations
{
	[Serializable()]
	public class ScalarRelativeLineFixedColumnLocation : LocationBase, IFixedColumn, IRelativeLine
	{
		public ScalarRelativeLineFixedColumnLocation(string anchorText, int skipLines, int startColumn, int endColumn, Types type = Types.String, string format = "") : this( new List<string>() { anchorText }, skipLines, startColumn, endColumn) { }

		public ScalarRelativeLineFixedColumnLocation(List<string> anchorTexts, int skipLines, int startColumn, int endColumn, Types type = Types.String, string format = "") : base(true, true, type, format)
		{
			AnchorTexts = anchorTexts;
			SkipLines = skipLines;
			StartColumn = startColumn;
			EndColumn = endColumn;
		}

		public List<string> AnchorTexts { get; protected set; }
		public int SkipLines { get; protected set; }
		public int StartColumn { get; protected set; }
		public int EndColumn { get; protected set; }

		public override void Update(TextFileManager tfm, object input) => tfm.UpdateWord(AnchorTexts, SkipLines, StartColumn, EndColumn, input, Format);

		protected override object Read_(TextFileManager tfm, Type t) => tfm.ReadWord<object>(t, AnchorTexts, SkipLines, StartColumn, EndColumn);
	}
}
