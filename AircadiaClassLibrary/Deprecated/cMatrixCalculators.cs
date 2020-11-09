/**************************************************************************************************
AIRCADIA - Multiphysics model-based design tool                                                   *
Engineering Design Group                                                                          *
Department of Aerospace Engineering, School of Engineering, Cranfield University                  *
Cranfield, Bedfordshire, MK43 0AL, UK                                                             *
/**************************************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using MathNet.Numerics.LinearAlgebra.Double;
using System.IO;


using Aircadia.ObjectModel.DataObjects;
using Aircadia.ObjectModel.Models;
using MathNet.Numerics.LinearAlgebra;

namespace Aircadia
{
	[Serializable()]
	////This class defines a collections of static methods (mainly mathematical and matrix operations) which can be called from other files.
	public class MatrixCalculators
	{
		/// <summary>
		/// Converts an incidence matrix to design structure matrix.
		/// </summary>
		/// <param name="iMatrix"></param>
		/// <returns></returns>
		public static Matrix<double> ImmtoDsm(Matrix<double> iMatrix)
		{
			// converts imm to dsm
			var vDsm = new DenseMatrix(iMatrix.RowCount, iMatrix.RowCount);
			MatrixCalculators.Eye(vDsm);
			double[,] vSpRowCol = MatrixCalculators.FindValue(iMatrix, 3);
			for (int nsprow = 0; nsprow < vSpRowCol.GetLength(0); nsprow++)
			{
				Vector<double> vSpInp = MatrixCalculators.FindLocValueInVector(iMatrix.Column((int)vSpRowCol[nsprow, 1]), 2);
				if (vSpInp != null)
				{
					for (int nvSpInp = 0; nvSpInp < vSpInp.Count; nvSpInp++)
					{
						vDsm[(int)vSpRowCol[nsprow, 0], (int)vSpInp[nvSpInp]] = 1;
					}
				}
			}
			return vDsm;
		}
		/// <summary>
		/// find a double value in a string
		/// </summary>
		/// <param name="str"></param>
		/// <returns></returns>
		public static double FindDoubleinString(string str) => (Convert.ToDouble(str.Split(' ')[0]));

		/// <summary>
		/// Converts OutputString to ArrayList
		/// </summary>
		/// <param name="instr"></param>
		/// <param name="len"></param>
		/// <returns></returns>
		public static double[,] ConvertOutputStringtoArray(string instr, int len)
		{
			double[,] arrayoutput;
			string tempfil = Path.GetTempFileName();
			var outfile = new StreamWriter(tempfil);
			outfile.Write(instr);
			outfile.Close();
			using (var sr = new StreamReader(tempfil))
			{
				string reader = sr.ReadLine();
				int ncount = 0;
				while (reader != null)
				{
					reader = sr.ReadLine();
					ncount++;
				}
				using (var srt = new StreamReader(tempfil))
				{
					arrayoutput = new double[ncount - 1, len];
					reader = srt.ReadLine();
					for (int i = 0; i < ncount - 1; i++)
					{
						reader = srt.ReadLine();
						string[] readervalues = reader.Split();
						for (int j = 0; j < len; j++)
						{
							arrayoutput[i, j] = Convert.ToDouble(readervalues[j]);
						}
					}
					return arrayoutput;
				}
			}

		}
		/// <summary>
		/// Get the names in a list from the 'output' string of the treatment
		/// </summary>
		/// <param name="instr"></param>
		/// <returns></returns>
		public static List<string> GetNamesArray(string instr)
		{
			string[] readernames;
			string tempfil = Path.GetTempFileName();
			var outfile = new StreamWriter(tempfil);
			outfile.Write(instr);
			outfile.Close();
			using (var sr = new StreamReader(tempfil))
			{
				var readernameslist = new List<string>();
				string reader = sr.ReadLine();
				readernames = reader.Split();
				for (int i = 0; i < readernames.Length; i++)
				{
					if ((readernames[i] != " ") & (readernames[i] != ""))
					{
						readernameslist.Add(readernames[i]);
					}
				}
				return readernameslist;
			}
		}
		/// <summary>
		/// retruns the inputs and output data objects for the given modelobjects collection
		/// </summary>
		/// <param name="modelobjects"></param>
		/// <param name="dataobjects"></param>
		/// <returns></returns>
		public static object[] GetInputOutputs(List<WorkflowComponent> modelobjects, List<Data> dataobjects)
		{
			Matrix<double> iMatrix = WorkflowComponent.GetIncidenceMatrix(modelobjects, dataobjects);
			object[] vInOut = new object[2];
			var vInputs = new List<Data>();
			var vOutputs = new List<Data>();
			for (int ncount = 0; ncount < iMatrix?.ColumnCount; ncount++)
			{
				if ((MatrixCalculators.FindValueInVector(iMatrix.Column(ncount), 2) > 0) & (MatrixCalculators.FindValueInVector(iMatrix.Column(ncount), 3) > 0))
				{ vOutputs.Add(dataobjects[ncount]); }
				else if (MatrixCalculators.FindValueInVector(iMatrix.Column(ncount), 3) > 0)
				{ vOutputs.Add(dataobjects[ncount]); }
				else if (MatrixCalculators.FindValueInVector(iMatrix.Column(ncount), 2) > 0)
				{ vInputs.Add(dataobjects[ncount]); }
			}
			vInOut[0] = vInputs;
			vInOut[1] = vOutputs;
			return vInOut;
		}
		/// <summary>
		/// replaces values in vMatrix whic are > vWhat with vWith
		/// </summary>
		/// <param name="vMatrix"></param>
		/// <param name="vWhat"></param>
		/// <param name="vWith"></param>
		public static void ReplaceGreaterthanWith(Matrix<double> vMatrix, int vWhat, int vWith)
		{
			for (int nfirstList = 0; nfirstList < vMatrix.RowCount; nfirstList++)
			{
				for (int nsecondList = 0; nsecondList < vMatrix.ColumnCount; nsecondList++)
				{
					if (vMatrix[nfirstList, nsecondList] > vWhat)
					{
						vMatrix[nfirstList, nsecondList] = vWith;
					}
				}
			}
		}
		/// <summary>
		/// replaces values in vMatrix whic are %lt; vWhat with vWith
		/// </summary>
		/// <param name="vMatrix"></param>
		/// <param name="vWhat"></param>
		/// <param name="vWith"></param>
		public static void ReplaceLessthanWith(Matrix<double> vMatrix, int vWhat, int vWith)
		{
			for (int nfirstList = 0; nfirstList < vMatrix.RowCount; nfirstList++)
			{
				for (int nsecondList = 0; nsecondList < vMatrix.ColumnCount; nsecondList++)
				{
					if (vMatrix[nfirstList, nsecondList] < vWhat)
					{
						vMatrix[nfirstList, nsecondList] = vWith;
					}
				}
			}
		}
		/// <summary>
		/// Replace diagonal elements of the given martix with one.
		/// </summary>
		/// <param name="vMatrix"></param>
		public static void Eye(Matrix<double> vMatrix)
		{

			for (int ncount = 0; ncount < vMatrix.RowCount; ncount++)
			{
				vMatrix[ncount, ncount] = 1;
			}
		}
		/// <summary>
		/// Returns the unique vector
		/// </summary>
		/// <param name="vVector"></param>
		/// <returns></returns>
		public static Vector<double> Unique(Vector<double> vVector)
		{

			var vVectorU = new ArrayList();
			for (int ncount = 0; ncount < vVector.Count; ncount++)
			{
				if (!vVectorU.Contains(vVector[ncount]))
					vVectorU.Add(vVector[ncount]);
			}
			double[] vVecF = (double[])vVectorU.ToArray(typeof(double));
			Vector<double> vVecFD = Vector.Build.Dense(vVecF);
			return vVecFD;
		}
		/// <summary>
		/// Multiplies  non zero values of a vector(vVector) and returns it as double
		/// </summary>
		/// <param name="vVector"></param>
		/// <returns></returns>
		public static double MultiplyNonZeroVector(Vector<double> vVector)
		{
			double vMulNZero = 1;

			for (int ncount = 0; ncount < vVector.Count; ncount++)
			{
				if (!vVector[ncount].Equals(0))
					vMulNZero = vMulNZero * vVector[ncount];
			}
			return vMulNZero;
		}
		/// <summary>
		/// Multiplies a value(vValue) to a column(vColNum) of the Matrix(vMatrix)
		/// </summary>
		/// <param name="vMatrix"></param>
		/// <param name="vColNum"></param>
		/// <param name="vValue"></param>
		public static void MultiplyColumn(Matrix<double> vMatrix, int vColNum, int vValue)
		{
			for (int i = 0; i < vMatrix.RowCount; i++)
			{
				vMatrix[i, vColNum] = vMatrix[i, vColNum] * vValue;
			}
		}
		/// <summary>
		/// Compares two Matrixes for modified models.
		/// Returns the number of modified models
		/// </summary>
		/// <param name="firstList"></param>
		/// <param name="secondList"></param>
		/// <returns></returns>
		public static int CompareMatrix(Matrix<double> firstList, Matrix<double> secondList)
		{

			int vNumMod = 0;
			for (int nfirstList = 0; nfirstList < firstList.RowCount; nfirstList++)
			{
				if (!CompareVectors(firstList.Row(nfirstList), secondList.Row(nfirstList)))
					vNumMod++;


			}
			return vNumMod;
		}
		/// <summary>
		/// Compares two vectors only for the values greater than one.
		/// Returns true or false
		/// </summary>
		/// <param name="firstList"></param>
		/// <param name="secondList"></param>
		/// <returns></returns>
		public static bool CompareVectors(Vector<double> firstList, Vector<double> secondList)
		{

			for (int nfirstList = 0; nfirstList < firstList.Count; nfirstList++)
			{
				if ((!firstList[nfirstList].Equals(secondList[nfirstList])) & (secondList[nfirstList] != 1))
				{
					return false;
				}
			}
			return true;
		}
		/// <summary>
		/// finds the 'value' in 'vMatrix'
		/// </summary>
		/// <param name="vMatrixRow"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static int FindValueInVector(Vector<double> vMatrixRow, int value)
		{
			int vFoundValues = 0;
			for (int i = 0; i < vMatrixRow.Count; i++)
			{
				if (vMatrixRow[i] == value)
				{
					vFoundValues++;
				}
			}
			return vFoundValues;
		}
		/// <summary>
		/// finds the 'value' in 'vMatrix' and returns the location
		/// </summary>
		/// <param name="vMatrixRow"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static Vector<double> FindLocValueInVector(Vector<double> vMatrixRow, int value)
		{

			Vector<double> vFoundValues = Vector.Build.Dense(vMatrixRow.Count);
			int nvFoundValues = 0;
			for (int i = 0; i < vMatrixRow.Count; i++)
			{
				if (vMatrixRow[i] == value)
				{
					vFoundValues[nvFoundValues] = i;
					nvFoundValues++;
				}
			}
			if (nvFoundValues > 0)
			{
				Vector<double> vFoundValuesT = vFoundValues.SubVector(0, nvFoundValues);
				return vFoundValuesT as Vector<double>;
			}
			else
			{
				return null;
			}
		}
		/// <summary>
		/// finds the 'value' in 'vMatrix' and returns the total number
		/// </summary>
		/// <param name="vMatrixRow"></param>
		/// <returns></returns>
		public static int FindAnyValueInVector(Vector<double> vMatrixRow)
		{

			int vFoundValues = 0;
			for (int i = 0; i < vMatrixRow.Count; i++)
			{
				if (vMatrixRow[i] > 0)
				{
					vFoundValues++;
				}
			}
			return vFoundValues;
		}
		/// <summary>
		/// finds the 'value' in 'vMatrix' and return it in an double Array
		/// </summary>
		/// <param name="vMatrix"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static double[,] FindValue(Matrix<double> vMatrix, int value)
		{

			var viFoundValues = new DenseMatrix(vMatrix.ColumnCount * vMatrix.RowCount, 2);
			int vinviFoundValues = 0;
			double[,] vRetviFoundValuesT = new double[0, 0];
			for (int i = 0; i < vMatrix.RowCount; i++)
			{
				for (int j = 0; j < vMatrix.ColumnCount; j++)
				{
					if (vMatrix[i, j] == value)
					{
						viFoundValues[vinviFoundValues, 0] = i;
						viFoundValues[vinviFoundValues, 1] = j;
						vinviFoundValues++;
					}
				}
			}
			if (vinviFoundValues > 0)
			{
				Matrix<double> vRetviFoundValues = viFoundValues.SubMatrix(0, vinviFoundValues, 0, 2);
				return vRetviFoundValues.ToArray();
			}
			else
			{
				vRetviFoundValuesT = null;
				return vRetviFoundValuesT;
			}
		}
		/// <summary>
		/// replaces non-zero values in the vMatrix with one.
		/// </summary>
		/// <param name="vMatrix"></param>
		public static void ReplaceAlltoOne(Matrix<double> vMatrix)
		{
			int rows = vMatrix.RowCount;
			int columns = vMatrix.ColumnCount;
			for (int i = 0; i < rows; i++)
			{
				for (int j = 0; j < columns; j++)
				{
					if (vMatrix[i, j] != 0)
						vMatrix[i, j] = 1;
				}
			}
		}
	}
}
