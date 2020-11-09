using Aircadia.Services.Serializers;
using System;
using System.Collections.Generic;

namespace ExeModelTextFileManager.DataLocations
{
	[Serializable()]
	public class ScalarRelativeLineRelativeColumn : LocationBase, IRelativeColumn, IRelativeLine
	{
		public ScalarRelativeLineRelativeColumn(string anchorText, int skipLines, int position, char[] separators, Types type = Types.String, string format = "") : this(new List<string>() { anchorText }, skipLines, position, separators) { }

		[DeserializeConstructor]
		public ScalarRelativeLineRelativeColumn(List<string> anchorTexts, int skipLines, int position, char[] separators, Types type = Types.String, string format = "") : base(true, true, type, format)
		{
			AnchorTexts = anchorTexts;
			SkipLines = skipLines;
			Position = position;
			Separators = separators;
		}

		[Serialize]
		public List<string> AnchorTexts { get; protected set; }
		[Serialize]
		public int SkipLines { get; protected set; }
		[Serialize]
		public int Position { get; protected set; }
		[Serialize]
		public char[] Separators { get; protected set; }

		public override void Update(TextFileManager tfm, object input) => tfm.UpdateWord(AnchorTexts, SkipLines, Position, Separators, input, Format);

		public override object Read(TextFileManager tfm)
		{
			switch (Type)
			{
				case Types.Int:
					return tfm.ReadWord<int>(AnchorTexts, SkipLines, Position, Separators);
				case Types.Double:
					return tfm.ReadWord<double>(AnchorTexts, SkipLines, Position, Separators);
				case Types.String:
					return tfm.ReadWord<string>(AnchorTexts, SkipLines, Position, Separators);
				case Types.Bool:
					return tfm.ReadWord<bool>(AnchorTexts, SkipLines, Position, Separators);
				default:
					return tfm.ReadWord<object>(AnchorTexts, SkipLines, Position, Separators);
			}
		}
	}
}
