using System;
using System.Collections.Generic;
using System.Linq;
//using dnAnalytics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra;

namespace Aircadia.WorkflowManagement
{
	class MatrixCalculator
    {
        /// <summary>
        /// Converts an incidence matrix (IM) to design structure matrix (DSM).
        /// </summary>
        /// <param name="IM"></param>
        /// <returns></returns>
        public static DenseMatrix IMtoDSM(DenseMatrix IM)
        {
            var DSM = new DenseMatrix(IM.RowCount, IM.RowCount);
            MatrixCalculator.ToIdentity(DSM);
            double[,] LocOf3_IM = FindValue_InIM(IM, 3); //this 2 by 2 matrix stores locations of element '3' in IM
            for (int r = 0; r < LocOf3_IM.GetLength(0); r++) //loop over all these locations containing '3' in IM
            {
                DenseVector LocOf2_IMCol = FindValueLocInVector(IM.Column((int)LocOf3_IM[r, 1]), 2);
                if (LocOf2_IMCol != null)
                {
                    for (int rr = 0; rr < LocOf2_IMCol.Count; rr++) //loop over all the locations containing '2' in IM column vector
                    {
                        DSM[(int)LocOf3_IM[r, 0], (int)LocOf2_IMCol[rr]] = 1;
                    }
                }
            }
            return DSM;
        }

        /// <summary>
        /// Convert given square matrix to Identity matrix
        /// </summary>
        /// <param name="Matrix"></param>
        public static void ToIdentity(DenseMatrix Matrix)
        {

            for (int ncount = 0; ncount < Matrix.RowCount; ncount++)
            {
                Matrix[ncount, ncount] = 1;
            }
        }

        /// <summary>
        /// Finds given value in the given matrix and gives out locations of the found values
        /// </summary>
        /// <param name="Matrix"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static double[,] FindValue_InIM(DenseMatrix Matrix, int value)
        {

            var LocOf_FoundValue = new DenseMatrix(Matrix.ColumnCount * Matrix.RowCount, 2);
            int counter = 0;
            double[,] LocOf_FoundValueExactR = new double[0, 0];
            for (int i = 0; i < Matrix.RowCount; i++)
            {
                for (int j = 0; j < Matrix.ColumnCount; j++)
                {
                    if (Matrix[i, j] == value)
                    {
                        LocOf_FoundValue[counter, 0] = i;
                        LocOf_FoundValue[counter, 1] = j;
                        counter++;
                    }
                }
            }
            if (counter > 0)
            {
                Matrix<double> LocOf_FoundValueExact = LocOf_FoundValue.SubMatrix(0, counter, 0, 2);
                return LocOf_FoundValueExact.ToArray();
            }
            else
            {
                LocOf_FoundValueExactR = null;
                return LocOf_FoundValueExactR;
            }
        }

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



        public static DenseVector FindValueLocInVector(Vector<double> IM_ColVector, int value)
        {

            var LocOf_Value = new DenseVector(IM_ColVector.Count);
            int foundCounter = 0;
            for (int i = 0; i < IM_ColVector.Count; i++)
            {
                if (IM_ColVector[i] == value)
                {
                    LocOf_Value[foundCounter] = i;
                    foundCounter++;
                }
            }
            if (foundCounter > 0)
            {
                Vector<double> LocOf_ValueExact = LocOf_Value.SubVector(0, foundCounter);
                return LocOf_ValueExact as DenseVector;
            }
            else
            {
                return null;
            }
        }


        public static void mReplaceGreaterthanWith(DenseMatrix vMatrix, int vWhat, int vWith)
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

        public static void mReplaceLessthanWith(DenseMatrix vMatrix, int vWhat, int vWith)
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
        /// This method gives next E-vector
        /// </summary>
        /// <param name="currentLevelVector"></param>
        public static void NextLevelVector(DenseMatrix currentLevelVector) //YHB
        { //varachya methadala replacement aahe hi method
            for (int i = 0; i < currentLevelVector.RowCount; i++)
            {
                for (int j = 0; j < currentLevelVector.ColumnCount; j++)
                {
                    if (currentLevelVector[i, j] == 0 || currentLevelVector[i, j] == 1)
                    {
                        currentLevelVector[i, j] = 0;
                    }
                    else
                    {
                        currentLevelVector[i, j] = 1;
                    }
                }
            }

        }

        /// <summary>
        /// This method is used to collapse rows of the DSM
        /// </summary>
        /// <param name="rv1"></param>
        /// <param name="rv2"></param>
        /// <returns></returns>
        public static DenseMatrix OrOperationOnMatrixRowVector(DenseMatrix rv1, DenseMatrix rv2)
        {
            var rv = new DenseMatrix(1, rv1.ColumnCount);

            for (int i = 0; i < rv1.ColumnCount; i++)
            {
                if (rv1[0, i] == 0 && rv2[0, i] == 0)
                {
                    rv[0, i] = 0;
                }
                else
                {
                    rv[0, i] = 1;
                }

            }

            return rv;
        }


        /// <summary>
        /// This method is used to collapse columns of the DSM
        /// </summary>
        /// <param name="cv1"></param>
        /// <param name="cv2"></param>
        /// <returns></returns>
        public static DenseMatrix OrOperationOnMatrixColumnVector(DenseMatrix cv1, DenseMatrix cv2)
        {
            var cv = new DenseMatrix(cv1.RowCount, 1);

            for (int i = 0; i < cv1.RowCount; i++)
            {
                if (cv1[i, 0] == 0 && cv2[i, 0] == 0)
                {
                    cv[i, 0] = 0;
                }
                else
                {
                    cv[i, 0] = 1;
                }

            }

            return cv;
        }


        public static Dictionary<int, int> IndexMapReduToOrigDSMStatVar; //This var is made static to make it accessible to DSM Sequecing method in DSM_Sequencing class
        /// <summary>
        /// This method collapses SCCs in a given DSM and gives back collapsed DSM
        /// </summary>
        /// <param name="DSMOrig"></param>
        /// <param name="SCCs"></param>
        /// <returns></returns>
        public static DenseMatrix CollapseSCCsInDSM(DenseMatrix DSMOrig, List<List<int>> SCCs)
        {//Its working fine as per first test , need further testing
            DenseMatrix DSM = DSMOrig;

            var DeleteRCs = new List<int>(); //Columns and Rows to be deleted

            for (int i = 0; i < SCCs.Count(); i++)
            {//Collapsing Rows: scc madhalya rows chi addition karun
                if (SCCs[i].Count > 1)
                {
                    int counter = 0;
                    var v = new DenseMatrix(1, DSM.ColumnCount); //Row vector
                    while (counter < (SCCs[i] as List<int>).Count())
                    {//summing up the rows associated with SCC elements
                        if (counter != 0)
                        {
                            DeleteRCs.Add((SCCs[i] as List<int>)[counter]);
                        }
                        v = OrOperationOnMatrixRowVector(v, (DenseMatrix)DSM.SubMatrix(SCCs[i][counter++], 1, 0, DSM.ColumnCount));

                    }
                    DSM.SetRow(SCCs[i][0], v.Row(0));

                }
            }

            for (int i = 0; i < SCCs.Count(); i++)
            {//Collapsing Columns: scc madhalya columns chi addition karun
                if (SCCs[i].Count() != 1)
                {
                    int counter = 0;
                    var v = new DenseMatrix(DSM.RowCount, 1); //Column vector
                    while (counter < SCCs[i].Count())
                    {//summing up the rows associated with SCC elements
                        v = OrOperationOnMatrixColumnVector(v, (DenseMatrix)DSM.SubMatrix(0, DSM.RowCount, SCCs[i][counter++], 1));
                    }

                    DSM.SetColumn(SCCs[i][0], v.Column(0));

                }
            }


            ////////////////// To mapping of indexes in original and reduced DSM
            var OrigDSMIndex = new List<int>();
            for (int i = 0; i < DSMOrig.RowCount; i++)
            {
                OrigDSMIndex.Add(i);
            }
            ////////////////// To mapping of indexes in original and reduced DSM


            //utarta kram
            int[] DeleteRCsArray = DeleteRCs.ToArray();
            Array.Sort(DeleteRCsArray);
            Array.Reverse(DeleteRCsArray);
            DeleteRCs = DeleteRCsArray.ToList();

            foreach (int item in DeleteRCs)
            {//row kadhane
                DSM = (DenseMatrix)DSM.RemoveRow(item);
                OrigDSMIndex.Remove(item);
            }

            foreach (int item in DeleteRCs)
            {//column kadhane
                DSM = (DenseMatrix)DSM.RemoveColumn(item);
            }


            ///////// To mapping of indexes in original and reduced DSM

            var IndexMapReduToOrigDSM = new Dictionary<int, int>();
            var ReduDSMIndex = new List<int>();
            for (int i = 0; i < DSM.RowCount; i++)
            {
                ReduDSMIndex.Add(i);
            }

            if (OrigDSMIndex.Count() == ReduDSMIndex.Count())
            {
                for (int i = 0; i < OrigDSMIndex.Count(); i++)
                {
                    IndexMapReduToOrigDSM.Add(ReduDSMIndex[i], OrigDSMIndex[i]);
                }

            }

            IndexMapReduToOrigDSMStatVar = IndexMapReduToOrigDSM;

            ////////// To mapping of indexes in original and reduced DSM



            return DSM;

        }

        public static Vector<double> OrOperationOnMatrixRowVector1(Vector<double> rV1, Vector<double> rV2)
        {//Performs Or operation on given input vectors
            var rV = Vector<double>.Build.Dense(rV1.Count); //Vector.CreateSparse<double>(1000000)

            for (int i = 0; i < rV1.Count; i++)
            {
                if (rV1[i] == 1 | rV2[i] == 1)
                {
                    rV[i] = 1;
                }
            }

            return rV;
        }

        public static SparseMatrix TransitiveClosureCalculator(SparseMatrix rM)
        {//Calculates transitive closure of the given matrix
            var tM = new SparseMatrix(rM.RowCount, rM.ColumnCount);
            rM.CopyTo(tM);

            for (int j = 0; j < rM.ColumnCount; j++)
            {
                for (int i = 0; i < rM.RowCount; i++)
                {
                    if (tM[i, j] == 1)
                    {
                        var rV = OrOperationOnMatrixRowVector1(tM.Row(i), tM.Row(j));
                        tM.SetRow(i, rV);
                    }
                }

            }



            return tM;

        }














        //public static DenseMatrix CopyVectorToMatrixRow(DenseMatrix M, DenseVector v, int idx)
        //{
        //    for (int j = 0; j < v.Count(); j++)
        //    {
        //        M[idx, j] = v[j];
        //    }

        //    return M;
        //}

        //public static DenseMatrix CopyVectorToMatrixColumn(DenseMatrix M, DenseVector v, int idx)
        //{
        //    for (int j = 0; j < v.Count(); j++)
        //    {
        //        M[j, idx] = v[j];
        //    }

        //    return M;
        //}








    }
}

