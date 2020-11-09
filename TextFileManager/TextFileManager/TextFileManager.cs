using System;
using System.Collections.Generic;
using System.Linq;

namespace ExeModelTextFileManager
{
	public class TextFileManager
	{
		protected List<string> lines;
		protected char[] separators = new char[] { ' ' };
		private char[] defaultSeparator;
		protected FirstIndexMode mode;

		public TextFileManager(IEnumerable<string> fileContents, FirstIndexMode indexMode = FirstIndexMode.Zero)
		{
			lines = new List<string>(fileContents); 
			mode = indexMode;
			defaultSeparator = separators;
		}
		public TextFileManager(IEnumerable<string> fileContents, char[] separators, FirstIndexMode indexMode = FirstIndexMode.Zero) : this(fileContents, indexMode)
		{
			this.separators = separators;
			defaultSeparator = separators;
		}
		public TextFileManager(List<string> fileContents, FirstIndexMode indexMode = FirstIndexMode.Zero)
		{
			lines = fileContents;
			mode = indexMode;
			defaultSeparator = separators;
		}
		public TextFileManager(List<string> fileContents, char[] separators, FirstIndexMode indexMode = FirstIndexMode.Zero) : this(fileContents, indexMode)
		{
			this.separators = separators;
			defaultSeparator = separators;
		}


		public List<string> FileContents { get { return lines; } }
		public FirstIndexMode Mode { get { return mode; } }
		public char[] Separators { get { return separators; } }

		#region Replace
		// * * * * * * * * * * * * * * * * * * * * * * * *
		// REPLACE Lines
		// * * * * * * * * * * * * * * * * * * * * * * * *
		public void ReplaceLines(int line, int count, List<string> values, int frequency = 1)
		{
			int idx = Idx(line);
			lines.RemoveRange(idx, count);
			lines.InsertRange(idx, values);
		}
		public void ReplaceLines(string anchorText, string stopText, int skipLines, List<string> values, int frequency = 1) => ReplaceLines(new List<string>() { anchorText }, stopText, skipLines, values, frequency);
		public void ReplaceLines(List<string> anchorTexts, string stopText, int skipLines, List<string> values, int frequency = 1)
		{
			int anchorIdx = GetAnchorIndex(anchorTexts) + skipLines;
			int idx = anchorIdx;
			while (!lines[idx].Contains(stopText)) idx++;
			lines.RemoveRange(anchorIdx, idx - anchorIdx);
			lines.InsertRange(anchorIdx, values);
		}
		public void ReplaceLines(List<string> anchorTexts, List<int> relativeAnchorLines, string stopText, int skipLines, List<string> values, int frequency = 1)
		{
			int anchorIdx = GetAnchorIndex(anchorTexts, relativeAnchorLines) + skipLines;
			int idx = anchorIdx;
			while (!lines[idx].Contains(stopText)) idx++;
			lines.RemoveRange(anchorIdx, idx - anchorIdx);
			lines.InsertRange(anchorIdx, values);
		}
		#endregion

		#region Update

		// * * * * * * * * * * * * * * * * * * * * * * * *
		// UPDATE ARRAYS
		// * * * * * * * * * * * * * * * * * * * * * * * *
		// rel - rel
		public void UpdateArray(string anchorText, int skipLines, int position, List<string> values, char[] separators = null, int frequency = 1) => UpdateArray(new List<string>() { anchorText }, skipLines, position, values, separators, frequency);
		public void UpdateArray(List<string> anchorTexts, int skipLines, int position, List<string> values, char[] separators = null, int frequency = 1)
		{
			int anchorIdx = GetAnchorIndex(anchorTexts);

			int idx = anchorIdx + skipLines;
			foreach (var l in values)
			{
				UpdateWord_(idx, position, l, separators);
				idx += frequency;
			}
		}
		// fix - rel
		public void UpdateArray(int line, int position, List<string> values, char[] separators = null, int frequency = 1)
		{
			int idx = line;
			foreach (var l in values)
			{
				UpdateWord(idx, position, l);
				idx += frequency;
			}
		}
		// rel - fix
		public void UpdateArray(List<string> anchorTexts, List<int> relativeAnchorLines, int skipLines, int startColumn, int endColumn, List<string> values, int frequency = 1)
		{
			int anchorIdx = GetAnchorIndex(anchorTexts, relativeAnchorLines);
			int idx = anchorIdx + skipLines;
			foreach (var l in values)
			{
				UpdateWord_(idx, startColumn, endColumn, l);
				idx += frequency;
			}
		}
		public void UpdateArray(string anchorText, int skipLines, int startColumn, int endColumn, List<string> values, int frequency = 1) => UpdateArray(new List<string> { anchorText }, skipLines, startColumn, endColumn, values, frequency);
		public void UpdateArray(List<string> anchorTexts, int skipLines, int startColumn, int endColumn, List<string> values, int frequency = 1)
		{
			int anchorIdx = GetAnchorIndex(anchorTexts);

			int anchorLineNumber = anchorIdx;
			int idx = anchorLineNumber + skipLines;
			foreach (var l in values)
			{
				UpdateWord_(idx, startColumn, endColumn, l);
				idx += frequency;
			}
		}
		// fix - fix
		public void UpdateArray(int line, int startColumn, int endColumn, List<string> values, int frequency = 1)
		{
			int idx = Idx(line);
			foreach (var l in values)
			{
				UpdateWord_(idx, startColumn, endColumn, l);
				idx += frequency;
			}
		}
		
		// * * * * * * * * * * * * * * * * * * * * * * * *
		// UPDATE LINES
		// * * * * * * * * * * * * * * * * * * * * * * * *

		public void UpdateLines(int line, List<string> values, int frequency = 1)
		{
			int idx = Idx(line);
			foreach (var l in values)
			{
				lines[idx] = l;
				idx += frequency;
			}
		}
		public void UpdateLines(string anchorText, int skipLines, List<string> values, int frequency = 1) => UpdateLines(new List<string>() { anchorText }, skipLines, values, frequency);
		public void UpdateLines(List<string> anchorTexts, int skipLines, List<string> values, int frequency = 1)
		{
			int anchorIdx = GetAnchorIndex(anchorTexts);
			int anchorLineNumber = anchorIdx;
			int idx = anchorLineNumber + skipLines;
			foreach (var line in values)
			{
				lines[idx] = line;
				idx += frequency;
			}
		}
		public void UpdateLines(List<string> anchorTexts, List<int> relativeAnchorLines, int skipLines, List<string> values, int frequency = 1)
		 {
			int anchorIdx = GetAnchorIndex(anchorTexts, relativeAnchorLines);
			int idx = anchorIdx + skipLines;
			foreach (var line in values)
			{
				lines[idx] = line;
				idx += frequency;
			}
		}

		public void UpdateLine(int line, string value) => lines[Idx(line)] = value;
		public void UpdateLine(string anchorText, int skipLines, string value) => UpdateLine(new List<string>() { anchorText }, skipLines, value);
		public void UpdateLine(List<string> anchorTexts, int skipLines, string value) => lines[GetAnchorIndex(anchorTexts) + skipLines] = value;
		public void UpdateLine(List<string> anchorTexts, List<int> relativeAnchorLines, int skipLines, string value) => lines[GetAnchorIndex(anchorTexts, relativeAnchorLines) + skipLines] = value;


		// * * * * * * * * * * * * * * * * * * * * * * * *
		// UPDATE WORDS
		// * * * * * * * * * * * * * * * * * * * * * * * *

		// rel - rel
		public void UpdateWord(string anchorText, int skipLines, int position, string value, char[] separators = null) => UpdateWord(new List<string>() { anchorText }, skipLines, position, value, separators);
		public void UpdateWord(List<string> anchorTexts, int skipLines, int position, string value, char[] separators = null)
		{
			int idx = GetAnchorIndex(anchorTexts) + skipLines;
			UpdateWord_(idx, position, value, separators);
		}
		// fix - rel
		public void UpdateWord(int line, int position, string value, char[] separators = null) => UpdateWord_(Idx(line), position, value, separators);
		private void UpdateWord_(int line, int position, string value, char[] separators = null)
		{
			UseSeparator(separators);
			var words = lines[line].Trim().Split(this.separators, StringSplitOptions.RemoveEmptyEntries);
			var word = (Idx(position) < words.Length) ? words[Idx(position)] : "";
			int startColumn = IndexOf(line, position, word, words);
			int endColumn = startColumn + word.Length;
			UpdateWord__(line, startColumn, endColumn, value);
			GoBackToDefaultSeparator();
		}
		// rel - fix
		public void UpdateWord(string anchorText, int skipLines, int startColumn, int endColumn, string value) => UpdateWord(new List<string>() { anchorText }, skipLines, startColumn, endColumn, value);
		public void UpdateWord(List<string> anchorTexts, int skipLines, int startColumn, int endColumn, string value) => UpdateWord_(GetAnchorIndex(anchorTexts) + skipLines, startColumn, endColumn, value);
		public void UpdateWord(List<string> anchorTexts, List<int> relativeAnchorLines, int skipLines, int startColumn, int endColumn, string value) => UpdateWord_(GetAnchorIndex(anchorTexts, relativeAnchorLines) + skipLines, startColumn, endColumn, value);
		// fix - fix
		public void UpdateWord(int line, int startColumn, int endColumn, string value) => UpdateWord__(Idx(line), Idx(startColumn), Idx(endColumn), value);
		private void UpdateWord_(int line, int startColumn, int endColumn, string value) => UpdateWord__(line, Idx(startColumn), Idx(endColumn), value);
		private void UpdateWord__(int line, int startColumn, int endColumn, string value)
		{
			string l = lines[line];
			if (endColumn < l.Length)
				lines[line] = l.Substring(0, startColumn) + value + l.Substring(endColumn);
			else if (startColumn < l.Length)
				lines[line] = l.Substring(0, startColumn) + value;
		}
		#endregion

		#region Read

		// * * * * * * * * * * * * * * * * * * * * * * * *
		// READ ARRAYS
		// * * * * * * * * * * * * * * * * * * * * * * * *

		// rel - rel
		public List<string> ReadArray(string anchorText, string stopText, int skipLines, int position, int frequency = 1, char[] separators = null,  bool removeEmpty = true) => ReadArray(new List<string>() { anchorText }, stopText, skipLines, position, frequency, separators, removeEmpty);
		public List<string> ReadArray(List<string> anchorTexts, string stopText, int skipLines, int position, int frequency = 1, char[] separators = null, bool removeEmpty = true)
		{
			List<string> values = new List<string>();
			int anchorIdx = GetAnchorIndex(anchorTexts);

			int anchorLineNumber = anchorIdx;
			int idx = anchorLineNumber + skipLines;
			int idxAdd = idx;
			while (!lines[idx].Contains(stopText))
			{
				if (idx == idxAdd)
				{
					values.Add(ReadWord_(idx, position, separators));
					idxAdd += frequency;
				}
				idx++;
				if (idx == lines.Count) break;
			}
			if (removeEmpty) values.RemoveAll(s => String.IsNullOrWhiteSpace(s));
			return values;
		}
		// fix - rel
		public List<string> ReadArray(int line, int count, int position, int frequency = 1, char[] separators = null, bool removeEmpty = true)
		{
			int idxAdd = line;
			List<string> values = new List<string>();
			for (int idx = line; idx < line + count*frequency; idx += frequency)
				values.Add(ReadWord(idx, position, separators));
			if (removeEmpty) values.RemoveAll(s => String.IsNullOrWhiteSpace(s));
			return values;
		}
		public List<string> ReadArray(int line, string stopText, int position, int frequency = 1, char[] separators = null, bool removeEmpty = true)
		{
			int idx = Idx(line);
			int idxAdd = idx;
			List<string> values = new List<string>();
			while (!lines[idx].Contains(stopText))
			{
				if (idx == idxAdd)
				{
					values.Add(ReadWord_(idx, position, separators));
					idxAdd += frequency;
				}
				idx++;
				if (idx == lines.Count) break;
			}
			if (removeEmpty) values.RemoveAll(s => String.IsNullOrWhiteSpace(s));
			return values;
		}
		// rel - fix
		public List<string> ReadArray(List<string> anchorTexts, List<int> relativeAnchorLines, string stopText, int skipLines, int startColumn, int endColumn, int frequency = 1, bool removeEmpty = true)
		{
			List<string> values = new List<string>();
			int anchorIdx = GetAnchorIndex(anchorTexts, relativeAnchorLines);
			int idx = anchorIdx + skipLines;
			int idxAdd = idx;
			while (!lines[idx].Contains(stopText))
			{
				if (idx == idxAdd)
				{
					values.Add(ReadWord_(idx, startColumn, endColumn, false));
					idxAdd += frequency;
				}
				idx++;
				if (idx == lines.Count) break;
			}
			if (removeEmpty) values.RemoveAll(s => String.IsNullOrWhiteSpace(s));
			return values;
		}
		public List<string> ReadArray(string anchorText, string stopText, int skipLines, int startColumn, int endColumn, int frequency = 1, bool removeEmpty = true) => ReadArray(new List<string> { anchorText }, stopText, skipLines, startColumn, endColumn, frequency, removeEmpty);
		public List<string> ReadArray(List<string> anchorTexts, string stopText, int skipLines, int startColumn, int endColumn, int frequency = 1, bool removeEmpty = true)
		{
			List<string> values = new List<string>();
			int anchorIdx = GetAnchorIndex(anchorTexts);
			int idx = anchorIdx + skipLines;
			int idxAdd = idx;
			while (!lines[idx].Contains(stopText))
			{
				if (idx == idxAdd)
				{
					values.Add(ReadWord_(idx, startColumn, endColumn, false));
					idxAdd += frequency;
				}
				idx++;
				if (idx == lines.Count) break;
			}
			if (removeEmpty) values.RemoveAll(s => String.IsNullOrWhiteSpace(s));
			return values;
		}
		// fix - fix
		public List<string> ReadArray(int line, int count, int startColumn, int endColumn, int frequency = 1, bool removeEmpty = true)
		{
			line = Idx(line);
			List<string> values = new List<string>();
			for (int idx = line; idx < line + count*frequency; idx += frequency)
			{
				values.Add(ReadWord_(idx, startColumn, endColumn));
			}
			if (removeEmpty) values.RemoveAll(s => String.IsNullOrWhiteSpace(s));
			return values;
		}
		public List<string> ReadArray(int line, string stopText, int startColumn, int endColumn, int frequency = 1, bool removeEmpty = true)
		{
			int idx = Idx(line);
			int idxAdd = idx;
			List<string> values = new List<string>();
			while (!lines[idx].Contains(stopText))
			{
				if (idx == idxAdd)
				{
					values.Add(ReadWord_(idx, startColumn, endColumn, false));
					idxAdd += frequency;
				}
				idx++;
			}
			if (removeEmpty) values.RemoveAll(s => String.IsNullOrWhiteSpace(s));
			return values;
		}




        public List<string> ReadRowArray(int line, int startColumn, int endColumn, char[] separators)
        {
            if (endColumn == -1)
                endColumn = lines[line].Length;
            List<string> values = ReadWord_(line, startColumn, endColumn, false).Trim('"').Split(separators, StringSplitOptions.RemoveEmptyEntries).ToList();
            //if (removeEmpty) values.RemoveAll(s => String.IsNullOrWhiteSpace(s));
            return values;
        }

        // * * * * * * * * * * * * * * * * * * * * * * * *
        // READ LINES
        // * * * * * * * * * * * * * * * * * * * * * * * *

        public List<string> ReadLines(int line, int count, int frequency = 1, bool removeEmpty = true)
		{
			line = Idx(line);
			List<string> values = new List<string>();
			for (int idx = line; idx < line + count*frequency; idx += frequency)
				values.Add(lines[idx]);
			if (removeEmpty) values.RemoveAll(s => String.IsNullOrWhiteSpace(s));
			return values;
		}
		public List<string> ReadLines(int line, string stopText, int frequency = 1, bool removeEmpty = true)
		{
			int idx = Idx(line);
			int idxAdd = idx;
			List<string> values = new List<string>();
			while (!lines[idx].Contains(stopText))
			{
				if (idx == idxAdd)
				{
					values.Add(lines[idx]);
					idxAdd += frequency;
				}
				idx++;
				if (idx == lines.Count) break;
			}
			if (removeEmpty) values.RemoveAll(s => String.IsNullOrWhiteSpace(s));
			return values;
		}
		public List<string> ReadLines(string anchorText, string stopText, int skipLines, int frequency = 1, bool removeEmpty = true) => ReadLines(new List<string>() { anchorText }, stopText, skipLines, frequency, removeEmpty);
		public List<string> ReadLines(List<string> anchorTexts, string stopText, int skipLines, int frequency = 1, bool removeEmpty = true)
		{
			List<string> values = new List<string>();
			int anchorIdx = GetAnchorIndex(anchorTexts);

			int anchorLineNumber = anchorIdx;
			int idx = anchorLineNumber + skipLines;
			int idxAdd = idx;
			while (!lines[idx].Contains(stopText))
			{
				if (idx == idxAdd)
				{
					values.Add(lines[idx]);
					idxAdd += frequency;
				}
				idx++;
				if (idx == lines.Count) break;
			}
			if (removeEmpty) values.RemoveAll(s => String.IsNullOrWhiteSpace(s));
			return values;
		}
		public List<string> ReadLines(List<string> anchorTexts, List<int> relativeAnchorLines, string stopText, int skipLines, int frequency = 1, bool removeEmpty = true)
		{
			List<string> values = new List<string>();
			int anchorIdx = GetAnchorIndex(anchorTexts, relativeAnchorLines);
			int idx = anchorIdx + skipLines;
			int idxAdd = idx;
			while (!lines[idx].Contains(stopText))
			{
				if (idx == idxAdd)
				{
					values.Add(lines[idx]);
					idxAdd += frequency;
				}
				idx++;
				if (idx == lines.Count) break;
			}
			if (removeEmpty) values.RemoveAll(s => String.IsNullOrWhiteSpace(s));
			return values;
		}

		public string ReadLine(int line) => lines[Idx(line)];
		public string ReadLine(string anchorText, int skipLines) => ReadLine(new List<string>() { anchorText }, skipLines);
		public string ReadLine(List<string> anchorTexts, int skipLines) => lines[GetAnchorIndex(anchorTexts) + skipLines];
		public string ReadLine(List<string> anchorTexts, List<int> relativeAnchorLines, int skipLines) => lines[GetAnchorIndex(anchorTexts, relativeAnchorLines) + skipLines];

		// * * * * * * * * * * * * * * * * * * * * * * * *
		// READ WORDS
		// * * * * * * * * * * * * * * * * * * * * * * * *

		// rel - rel
		public string ReadWord(string anchorText, int skipLines, int position, char[] separators = null) => ReadWord(new List<string>() { anchorText }, skipLines, position, separators);
		public string ReadWord(List<string> anchorTexts, int skipLines, int position, char[] separators = null) => ReadWord_(GetAnchorIndex(anchorTexts) + skipLines, position, separators);
		// fix - rel
		public string ReadWord(int line, int position, char[] separators = null) => ReadWord_(Idx(line), position, separators);
		private string ReadWord_(int line, int position, char[] separators = null)
		{
			UseSeparator(separators);
			var lineString = lines[line];
			var words = lineString.Trim().Split(this.separators, StringSplitOptions.RemoveEmptyEntries);
			var word = (Idx(position) < words.Length) ? words[Idx(position)] : "";
			GoBackToDefaultSeparator();
			return word;
		}
		// rel - fix
		public string ReadWord(string anchorText, int skipLines, int startColumn, int endColumn) => ReadWord(new List<string>() { anchorText }, skipLines, startColumn, endColumn);
		public string ReadWord(List<string> anchorTexts, int skipLines, int startColumn, int endColumn)
		{
			int anchorIdx = GetAnchorIndex(anchorTexts);
			int idx = anchorIdx + skipLines;
			return ReadWord_(idx, startColumn, endColumn);
		}
		public string ReadWord(List<string> anchorTexts, List<int> relativeAnchorLines, int skipLines, int startColumn, int endColumn)
		{
			int anchorIdx = GetAnchorIndex(anchorTexts, relativeAnchorLines);
			int idx = anchorIdx + skipLines;
			return ReadWord_(idx, startColumn, endColumn);
		}
		// fix - fix	
		public string ReadWord(int line, int startColumn, int endColumn) => ReadWord_(Idx(line), startColumn, endColumn);
		private string ReadWord_(int line, int startColumn, int endColumn, bool throwExceptions = true)
		{
			startColumn = Idx(startColumn);
			endColumn = Idx(endColumn);
			string l = lines[line];
			if (l != null)
			{
				if (startColumn > l.Length)
				{
					if (throwExceptions)
						throw new IndexOutOfRangeException($"Error reading columns {startColumn} to {endColumn} in {l}. The initial column is greater than the line lenght ({l.Length})");
					else return "";
				}

                if (endColumn <= l.Length)
                {
                    string s = l.Substring(startColumn, endColumn - startColumn);
                    return s;
                }
                else
                    return l.Substring(startColumn);
			}
			else if (throwExceptions)
				throw new NullReferenceException($"Error reading line {line}. The line is null");
			else return "";
		}

		#endregion

		#region
		private int GetAnchorIndex(List<string> anchorTextLines, List<int> relativeLineNumbers)
		{
			List<int> anchorNumbers = new List<int>();
			string firstLineAnchorText = anchorTextLines[0];
			for (int i = 0; i < lines.Count; i++)
			{
				if (lines[i].Contains(firstLineAnchorText))
				{
					bool correct = true;
					int c = 1;
					while (c < anchorTextLines.Count)
					{
						if (!lines[i + relativeLineNumbers[c - 1]].Contains(anchorTextLines[c]))
						{
							correct = false;
							break;
						}
						c++;
					}
					if (correct)
						anchorNumbers.Add(i);
				}
			}

			if (anchorNumbers.Count == 0)
				throw new Exception($"Anchor text: '{anchorTextLines.Aggregate((s1, s2) => $"{s1} | {s2}")}' does not appear in the text file");
			else if (anchorNumbers.Count > 1)
				throw new Exception($"Anchor text {anchorTextLines.Aggregate((s1, s2) => $"{s1} | {s2}")} appears at more than 1 locations.");
			else
				return anchorNumbers.First();
		}
		private int GetAnchorIndex(List<string> anchorTextLines)
		{
			List<int> anchorNumbers = new List<int>();
			int idxAnchor = 0;
			int matches = 0;
			for (int i = 0; i < lines.Count; i++)
			{
				if (lines[i]?.Contains(anchorTextLines[idxAnchor]) == true)
				{
					matches++;
					idxAnchor++;
					if (matches == anchorTextLines.Count)
					{
						anchorNumbers.Add(i);
						matches = 0;
						idxAnchor = 0;
					}
				}
			}

			if (anchorNumbers.Count == 0)
				throw new Exception($"Anchor text: '{anchorTextLines.Aggregate((s1, s2) => $"{s1} | {s2}")}' does not appear in the text file");
			else if (anchorNumbers.Count > 1)
				throw new Exception($"Anchor text {anchorTextLines.Aggregate((s1, s2) => $"{s1} | {s2}")} appears at more than 1 locations.");
			else
				return anchorNumbers.First();
		}

		private void GoBackToDefaultSeparator() => separators = defaultSeparator;

		private void UseSeparator(char[] separators)
		{
			this.separators = (separators == null) ? defaultSeparator : separators;
		}

		private int Idx(int index) => index - (int)mode;

		private int IndexOf(int line, int position, string word, string[] words)
		{
			int count = 0;
			for (int i = 0; i < word.Length; i++)
			{
				if (words[i] == word) break;
				else count += words[i].Length;
			}
			return lines[line].IndexOf(word, Math.Max(0, count - 1));
		}

		#endregion


		#region

		
		#endregion

	}

	public enum FirstIndexMode { Zero = 0, One = 1 }

}
