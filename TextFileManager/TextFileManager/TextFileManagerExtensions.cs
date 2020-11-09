using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExeModelTextFileManager
{
	public static class TextFileManagerExtensions
	{
		public static double ReadDouble(this TextFileManager tfm, string anchorText, int skipLines, int position, char[] separators) => Convert.ToDouble(tfm.ReadWord(anchorText, skipLines, position, separators));

		// Read single item
		// rel - rel
		public static T ReadWord<T>(this TextFileManager tfm, int line, int startColumn, int endColumn) => (T)Convert.ChangeType(tfm.ReadWord(line, startColumn, endColumn), typeof(T));
		// fix - rel
		public static T ReadWord<T>(this TextFileManager tfm, List<string> anchorTexts, int skipLines, int startColumn, int endColumn) => (T)Convert.ChangeType(tfm.ReadWord(anchorTexts, skipLines, startColumn, endColumn), typeof(T));
		// rel - fix
		public static T ReadWord<T>(this TextFileManager tfm, int line, int position, char[] separators) => (T)Convert.ChangeType(tfm.ReadWord(line, position, separators), typeof(T));
		// fix - fix
		public static T ReadWord<T>(this TextFileManager tfm, List<string> anchorTexts, int skipLines, int position, char[] separators) => (T)Convert.ChangeType(tfm.ReadWord(anchorTexts, skipLines, position, separators), typeof(T));







		// Read Array
		// rel - rel
		public static T[] ReadArray<T>(this TextFileManager tfm, List<string> anchorTexts, string stopText, int skipLines, int position, int frequency = 1, char[] separators = null, bool removeEmpty = true) => tfm.ReadArray(anchorTexts, stopText, skipLines, position, frequency, separators, removeEmpty).ConvertAll(s => (T)Convert.ChangeType(s, typeof(T))).ToArray();
		// fix - rel
		public static T[] ReadArray<T>(this TextFileManager tfm, int line, int count, int position, int frequency = 1, char[] separators = null, bool removeEmpty = true) => tfm.ReadArray(line, count, position, frequency, separators, removeEmpty).ConvertAll(s => (T)Convert.ChangeType(s, typeof(T))).ToArray();
		public static T[] ReadArray<T>(this TextFileManager tfm, int line, string stopText, int position, int frequency = 1, char[] separators = null, bool removeEmpty = true) => tfm.ReadArray(line, stopText, position, frequency, separators, removeEmpty).ConvertAll(s => (T)Convert.ChangeType(s, typeof(T))).ToArray();
		// rel - fix
		public static T[] ReadArray<T>(this TextFileManager tfm, List<string> anchorTexts, string stopText, int skipLines, int startColumn, int endColumn, int frequency = 1, bool removeEmpty = true)
		{
			return tfm.ReadArray(anchorTexts, stopText, skipLines, startColumn, endColumn, frequency, removeEmpty).ConvertAll(s => (T)Convert.ChangeType(s, typeof(T))).ToArray();
		}

		// fix - fix
		public static T[] ReadArray<T>(this TextFileManager tfm, int line, int count, int startColumn, int endColumn, int frequency = 1, bool removeEmpty = true) => tfm.ReadArray(line, count, startColumn, endColumn, frequency, removeEmpty).ConvertAll(s => (T)Convert.ChangeType(s, typeof(T))).ToArray();
		public static T[] ReadArray<T>(this TextFileManager tfm, int line, string stopText, int startColumn, int endColumn, int frequency = 1, bool removeEmpty = true) => tfm.ReadArray(line, stopText, startColumn, endColumn, frequency, removeEmpty).ConvertAll(s => (T)Convert.ChangeType(s, typeof(T))).ToArray();

        // Row arrays
        public static T[] ReadRowArray<T>(this TextFileManager tfm, int line, int startColumn, int endColumn, char[] separators = null) => tfm.ReadRowArray(line, startColumn, endColumn, separators).ConvertAll(s => (T)Convert.ChangeType(s, typeof(T))).ToArray();





        // Update Single item
        // rel - rel
        public static void UpdateWord<T>(this TextFileManager tfm, int line, int startColumn, int endColumn, T value, string format = "") => tfm.UpdateWord(line, startColumn, endColumn, Format(value, format));

		private static string Format<T>(T value, string format)
		{
			string formatted = string.Format($"{{0:{format}}}", value);
			if ((value is double || value is int) && formatted.Length > format.Length + (format.StartsWith("-") ? 1 : 0))
				formatted = formatted.Substring(0, format.Length);
			return formatted;
		}

		// fix - rel
		public static void UpdateWord<T>(this TextFileManager tfm, List<string> anchorTexts, int skipLines, int startColumn, int endColumn, T value, string format = "") => tfm.UpdateWord(anchorTexts, skipLines, startColumn, endColumn, string.Format($"{{0:{format}}}", value));
		// rel - fix
		public static void UpdateWord<T>(this TextFileManager tfm, int line, int position, char[] separators, T value, string format = "") => tfm.UpdateWord(line, position, separators, string.Format($"{{0:{format}}}", value));
		// fix - fix
		public static void UpdateWord<T>(this TextFileManager tfm, List<string> anchorTexts, int skipLines, int position, char[] separators, T value, string format = "") => tfm.UpdateWord(anchorTexts, skipLines, position, separators, string.Format($"{{0:{format}}}", value));

		// Update Array
		// rel - rel
		public static void UpdateArray<T>(this TextFileManager tfm, List<string> anchorTexts, int skipLines, int position, T[] values, char[] separators, int frequency = 1, string format = "") => tfm.UpdateArray(anchorTexts, skipLines, position, values.ToList().ConvertAll(o => string.Format($"{{0:{format}}}", o)), separators, frequency);
		// fix - rel
		public static void UpdateArray<T>(this TextFileManager tfm, int line, int position, T[] values, char[] separators, int frequency = 1, string format = "") => tfm.UpdateArray(line, position, values.ToList().ConvertAll(o => string.Format($"{{0:{format}}}", o)), separators, frequency);
		// rel - fix
		public static void UpdateArray<T>(this TextFileManager tfm, List<string> anchorTexts, int skipLines, int startColumn, int endColumn, T[] values, int frequency = 1, string format = "") => tfm.UpdateArray(anchorTexts, skipLines, startColumn, endColumn, values.ToList().ConvertAll(o => string.Format($"{{0:{format}}}", o)), frequency);
		// fix - fix
		public static void UpdateArray<T>(this TextFileManager tfm, int line, int startColumn, int endColumn, T[] values, int frequency = 1, string format = "") => tfm.UpdateArray(line, startColumn, endColumn, values.ToList().ConvertAll(o => string.Format($"{{0:{format}}}", o)), frequency);


		public static double ReadDouble(this TextFileManager tfm, int line, int startColumn, int endColumn) => Convert.ToDouble(tfm.ReadWord(line, startColumn, endColumn));
		public static int ReadInt(this TextFileManager tfm, int line, int startColumn, int endColumn) => Convert.ToInt32(tfm.ReadWord(line, startColumn, endColumn));
		public static bool ReadBool(this TextFileManager tfm, int line, int startColumn, int endColumn) => Convert.ToBoolean(tfm.ReadWord(line, startColumn, endColumn));

		public static double ReadDouble(this TextFileManager tfm, List<string> anchorTexts, int skipLines, int startColumn, int endColumn) => Convert.ToDouble(tfm.ReadWord(anchorTexts, skipLines, startColumn, endColumn));
		public static int ReadInt(this TextFileManager tfm, List<string> anchorTexts, int skipLines, int startColumn, int endColumn) => Convert.ToInt32(tfm.ReadWord(anchorTexts, skipLines, startColumn, endColumn));
		public static bool ReadBool(this TextFileManager tfm, List<string> anchorTexts, int skipLines, int startColumn, int endColumn) => Convert.ToBoolean(tfm.ReadWord(anchorTexts, skipLines, startColumn, endColumn));

		public static double ReadDouble(this TextFileManager tfm, int line, int position, char[] separators) => Convert.ToDouble(tfm.ReadWord(line, position, separators));
		public static int ReadInt(this TextFileManager tfm, int line, int position, char[] separators) => Convert.ToInt32(tfm.ReadWord(line, position, separators));
		public static bool ReadBool(this TextFileManager tfm, int line, int position, char[] separators) => Convert.ToBoolean(tfm.ReadWord(line, position, separators));

		public static double ReadDouble(this TextFileManager tfm, List<string> anchorTexts, int skipLines, int position, char[] separators) => Convert.ToDouble(tfm.ReadWord(anchorTexts, skipLines, position, separators));
		public static int ReadInt(this TextFileManager tfm, List<string> anchorTexts, int skipLines, int position, char[] separators) => Convert.ToInt32(tfm.ReadWord(anchorTexts, skipLines, position, separators));
		public static bool ReadBool(this TextFileManager tfm, List<string> anchorTexts, int skipLines, int position, char[] separators) => Convert.ToBoolean(tfm.ReadWord(anchorTexts, skipLines, position, separators));

		//public static void UpdateWord(this TextFileManager tfm, List<string> anchorTexts, int skipLines, int startColumn, int endColumn, double value, string format = "") => tfm.UpdateWord(anchorTexts, skipLines, startColumn, endColumn, string.Format($"{{0:{format}}}", value));
		//public static void UpdateWord(this TextFileManager tfm, List<string> anchorTexts, int skipLines, int startColumn, int endColumn, int value, string format = "") => tfm.UpdateWord(anchorTexts, skipLines, startColumn, endColumn, string.Format($"{{0:{format}}}", value));
		//public static void UpdateWord(this TextFileManager tfm, List<string> anchorTexts, int skipLines, int startColumn, int endColumn, bool value) => tfm.UpdateWord(anchorTexts, skipLines, startColumn, endColumn, value);
		//public static void UpdateWord(this TextFileManager tfm, List<string> anchorTexts, int skipLines, int startColumn, int endColumn, string value) => tfm.UpdateWord(anchorTexts, skipLines, startColumn, endColumn, value);

		//public static void UpdateWord(this TextFileManager tfm, int line, int position, char[] separators, double value, string format = "") => tfm.UpdateWord(line, position, separators, string.Format($"{{0:{format}}}", value));
		//public static void UpdateWord(this TextFileManager tfm, int line, int position, char[] separators, int value, string format = "") => tfm.UpdateWord(line, position, separators, string.Format($"{{0:{format}}}", value));
		//public static void UpdateWord(this TextFileManager tfm, int line, int position, char[] separators, bool value) => tfm.UpdateWord(line, position, separators, value);
		//public static void UpdateWord(this TextFileManager tfm, int line, int position, char[] separators, string value) => tfm.UpdateWord(line, position, separators, value);

		//public static void UpdateWord(this TextFileManager tfm, List<string> anchorTexts, int skipLines, int position, char[] separators, double value, string format = "") => tfm.UpdateWord(anchorTexts, skipLines, position, separators, string.Format($"{{0:{format}}}", value));
		//public static void UpdateWord(this TextFileManager tfm, List<string> anchorTexts, int skipLines, int position, char[] separators, int value, string format = "") => tfm.UpdateWord(anchorTexts, skipLines, position, separators, string.Format($"{{0:{format}}}", value));
		//public static void UpdateWord(this TextFileManager tfm, List<string> anchorTexts, int skipLines, int position, char[] separators, bool value) => tfm.UpdateWord(anchorTexts, skipLines, position, separators, value);
		//public static void UpdateWord(this TextFileManager tfm, List<string> anchorTexts, int skipLines, int position, char[] separators, string value) => tfm.UpdateWord(anchorTexts, skipLines, position, separators, value);

		//public void UpdateWord(int line, int startColumn, int endColumn, int value, string format = "{0}") => UpdateWord(line, startColumn, endColumn, string.Format(format, value));
		//public void UpdateWord(int line, int startColumn, int endColumn, bool value, string format = "{0}") => UpdateWord(line, startColumn, endColumn, string.Format(format, value));


		//public List<double> ReadArrayDouble(List<string> anchorTexts, List<int> relativeAnchorLines, string stopText, int skipLines, int startColumn, int endColumn, int frequency = 1) => ReadArray(anchorTexts, relativeAnchorLines, stopText, skipLines, startColumn, endColumn, frequency).ConvertAll((s) => Convert.ToDouble(s));
		//public List<int> ReadArrayInt(List<string> anchorTexts, List<int> relativeAnchorLines, string stopText, int skipLines, int startColumn, int endColumn, int frequency = 1) => ReadArray(anchorTexts, relativeAnchorLines, stopText, skipLines, startColumn, endColumn, frequency).ConvertAll((s) => Convert.ToInt32(s));
		//public List<bool> ReadArrayBool(List<string> anchorTexts, List<int> relativeAnchorLines, string stopText, int skipLines, int startColumn, int endColumn, int frequency = 1) => ReadArray(anchorTexts, relativeAnchorLines, stopText, skipLines, startColumn, endColumn, frequency).ConvertAll((s) => Convert.ToBoolean(s));

		//public List<double> ReadArrayDouble(string anchorText, string stopText, int skipLines, int startColumn, int endColumn, int frequency = 1) => ReadArray(new List<string> { anchorText }, stopText, skipLines, startColumn, endColumn, frequency).ConvertAll((s) => Convert.ToDouble(s));
		//public List<int> ReadArrayInt(string anchorText, string stopText, int skipLines, int startColumn, int endColumn, int frequency = 1) => ReadArray(new List<string> { anchorText }, stopText, skipLines, startColumn, endColumn, frequency).ConvertAll((s) => Convert.ToInt32(s));
		//public List<bool> ReadArrayBool(string anchorText, string stopText, int skipLines, int startColumn, int endColumn, int frequency = 1) => ReadArray(new List<string> { anchorText }, stopText, skipLines, startColumn, endColumn, frequency).ConvertAll((s) => Convert.ToBoolean(s));

		//public List<double> ReadArrayDouble(List<string> anchorTexts, string stopText, int skipLines, int startColumn, int endColumn, int frequency = 1) => ReadArray(anchorTexts, stopText, skipLines, startColumn, endColumn, frequency).ConvertAll((s) => Convert.ToDouble(s));
		//public List<int> ReadArrayInt(List<string> anchorTexts, string stopText, int skipLines, int startColumn, int endColumn, int frequency = 1) => ReadArray(anchorTexts, stopText, skipLines, startColumn, endColumn, frequency).ConvertAll((s) => Convert.ToInt32(s));
		//public List<bool> ReadArrayBool(List<string> anchorTexts, string stopText, int skipLines, int startColumn, int endColumn, int frequency = 1) => ReadArray(anchorTexts, stopText, skipLines, startColumn, endColumn, frequency).ConvertAll((s) => Convert.ToBoolean(s));


		//public double ReadWordDouble(string anchorText, int skipLines, int position) => Convert.ToDouble(ReadWord(anchorText, skipLines, position));
		//public int ReadWordInt(string anchorText, int skipLines, int position) => Convert.ToInt32(ReadWord(anchorText, skipLines, position));
		//public bool ReadWordBoolean(string anchorText, int skipLines, int position) => Convert.ToBoolean(ReadWord(anchorText, skipLines, position));

		//public double ReadWordDouble(List<string> anchorTexts, int skipLines, int position) => Convert.ToDouble(ReadWord(anchorTexts, skipLines, position));
		//public int ReadWordInt(List<string> anchorTexts, int skipLines, int position) => Convert.ToInt32(ReadWord(anchorTexts, skipLines, position));
		//public bool ReadWordBool(List<string> anchorTexts, int skipLines, int position) => Convert.ToBoolean(ReadWord(anchorTexts, skipLines, position));


		//public double ReadWordDouble(int line, int position) => Convert.ToDouble(ReadWord(line, position));
		//public int ReadWordInt(int line, int position) => Convert.ToInt32(ReadWord(line, position));
		//public bool ReadWordBoolean(int line, int position) => Convert.ToBoolean(ReadWord(line, position));

		//public double ReadWordDouble(string anchorText, int skipLines, int startColumn, int endColumn) => Convert.ToDouble(ReadWord(anchorText, skipLines, startColumn, endColumn));
		//public int ReadWordInt(string anchorText, int skipLines, int startColumn, int endColumn) => Convert.ToInt32(ReadWord(anchorText, skipLines, startColumn, endColumn));
		//public bool ReadWordBoolean(string anchorText, int skipLines, int startColumn, int endColumn) => Convert.ToBoolean(ReadWord(anchorText, skipLines, startColumn, endColumn));

		//public double ReadWordDouble(List<string> anchorTexts, int skipLines, int startColumn, int endColumn) => Convert.ToDouble(ReadWord(anchorTexts, skipLines, startColumn, endColumn));
		//public int ReadWordInt(List<string> anchorTexts, int skipLines, int startColumn, int endColumn) => Convert.ToInt32(ReadWord(anchorTexts, skipLines, startColumn, endColumn));
		//public bool ReadWordBool(List<string> anchorTexts, int skipLines, int startColumn, int endColumn) => Convert.ToBoolean(ReadWord(anchorTexts, skipLines, startColumn, endColumn));

		//public double ReadWordDouble(List<string> anchorTexts, List<int> relativeAnchorLines, int skipLines, int startColumn, int endColumn) => Convert.ToDouble(ReadWord(anchorTexts, relativeAnchorLines, skipLines, startColumn, endColumn));
		//public int ReadWordInt(List<string> anchorTexts, List<int> relativeAnchorLines, int skipLines, int startColumn, int endColumn) => Convert.ToInt32(ReadWord(anchorTexts, relativeAnchorLines, skipLines, startColumn, endColumn));
		//public bool ReadWordBool(List<string> anchorTexts, List<int> relativeAnchorLines, int skipLines, int startColumn, int endColumn) => Convert.ToBoolean(ReadWord(anchorTexts, relativeAnchorLines, skipLines, startColumn, endColumn));

		//public double ReadWordDouble(int line, int startColumn, int endColumn) => Convert.ToDouble(ReadWord(line, startColumn, endColumn));
		//public int ReadWordInt(int line, int startColumn, int endColumn) => Convert.ToInt32(ReadWord(line, startColumn, endColumn));
		//public bool ReadWordBool(int line, int startColumn, int endColumn) => Convert.ToBoolean(ReadWord(line, startColumn, endColumn));

	}
}