using Aircadia.Services.Serializers;
using System;
using System.Collections.Generic;

namespace ExeModelTextFileManager.DataLocations
{
	[Serializable()]
	public class ArrayRelativeLineRelativeColumnLocation : LocationBase, IRelativeColumn, IRelativeLine, IArrayStopLocation
	{
		public ArrayRelativeLineRelativeColumnLocation(string anchorText, string stopText, int skipLines, int position, char[] separators,int frequency = 1, Types type = Types.String, string format = "", bool removeEmpty = true) : this(new List<string> { anchorText }, stopText, skipLines, position, separators, frequency, type, format, removeEmpty) { }

		[DeserializeConstructor]
		public ArrayRelativeLineRelativeColumnLocation(List<string> anchorTexts, string stopText, int skipLines, int position, char[] separators, int frequency = 1, Types type = Types.String, string format = "", bool removeEmpty = true) : base(true, true, type, format)
		{
			AnchorTexts = anchorTexts;
			StopText = stopText;
			SkipLines = skipLines;
			Position = position;
			Separators = separators;
			Frequency = frequency;
		}

		[Serialize]
		public List<string> AnchorTexts { get; protected set; }
		[Serialize]
		public string StopText { get; protected set; }
		[Serialize]
		public int SkipLines { get; protected set; }
		[Serialize]
		public int Position { get; protected set; }
		[Serialize]
		public char[] Separators { get; protected set; }
		[Serialize]
		public int Frequency { get; protected set; }
		[Serialize]
		public bool RemoveEmpty { get; protected set; }

		public override void Update(TextFileManager tfm, object input) => tfm.UpdateArray(AnchorTexts, SkipLines, Position, input as object[], Separators, Frequency, Format);

		public override object Read(TextFileManager tfm)
		{
			switch (Type)
			{
				case Types.Int:
					return tfm.ReadArray<int>(AnchorTexts, StopText, SkipLines, Position, Frequency, Separators, RemoveEmpty);
				case Types.Double:
					return tfm.ReadArray<double>(AnchorTexts, StopText, SkipLines, Position, Frequency, Separators, RemoveEmpty);
				case Types.String:
					return tfm.ReadArray<string>(AnchorTexts, StopText, SkipLines, Position, Frequency, Separators, RemoveEmpty);
				case Types.Bool:
					return tfm.ReadArray<bool>(AnchorTexts, StopText, SkipLines, Position, Frequency, Separators, RemoveEmpty);
				default:
					return tfm.ReadArray<object>(AnchorTexts, StopText, SkipLines, Position, Frequency, Separators, RemoveEmpty);
			}
		}
	}
}
