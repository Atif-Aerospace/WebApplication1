using System;
using System.Collections.Generic;

namespace ExeModelTextFileManager.DataLocations
{
	[Serializable()]
	public class ScalarRelativeLineRelativeColumn : LocationBase, IRelativeColumn, IRelativeLine
	{
		public ScalarRelativeLineRelativeColumn(string anchorText, int skipLines, int position, char[] separators, Types type = Types.String, string format = "") : this(new List<string>() { anchorText }, skipLines, position, separators) { }

		public ScalarRelativeLineRelativeColumn(List<string> anchorTexts, int skipLines, int position, char[] separators, Types type = Types.String, string format = "") : base(true, true, type, format)
		{
			AnchorTexts = anchorTexts;
			SkipLines = skipLines;
			Position = position;
			Separators = separators;
		}

		public List<string> AnchorTexts { get; protected set; }
		public int SkipLines { get; protected set; }
		public int Position { get; protected set; }
		public char[] Separators { get; protected set; }

		public override void Update(TextFileManager tfm, object input) => tfm.UpdateWord(AnchorTexts, SkipLines, Position, Separators, input, Format);

		protected override object Read_(TextFileManager tfm, Type t) => tfm.ReadWord<object>(t, AnchorTexts, SkipLines, Position, Separators);
	}
}
