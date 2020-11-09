using System;
using System.Collections.Generic;

using System.IO;

namespace Aircadia.ObjectModel.Treatments.Optimisers.COBYLA
{
	#region DELEGATES

	/// <summary>
	/// Signature for the objective and constraints function evaluation method used in <see cref="COBYLA"/> minimization.
	/// </summary>
	/// <param name="n">Number of variables.</param>
	/// <param name="m">Number of constraints.</param>
	/// <param name="x">Variable values to be employed in function and constraints calculation.</param>
	/// <param name="f">Calculated objective function value.</param>
	/// <param name="con">Calculated function values of the constraints.</param>
	public delegate void CalcfcDelegate(int n, int m, double[] x, out double f, double[] con);

    #endregion

    #region ENUMERATIONS

    /// <summary>
    /// Status of optimization upon return.
    /// </summary>
    public enum CobylaExitStatus
    {
        /// <summary>
        /// Optimization successfully completed.
        /// </summary>
        Normal,

        /// <summary>
        /// Maximum number of iterations (function/constraints evaluations) reached during optimization.
        /// </summary>
        MaxIterationsReached,

        /// <summary>
        /// Size of rounding error is becoming damaging, terminating prematurely.
        /// </summary>
        DivergingRoundingErrors
    }

    #endregion

    /// <summary>
    /// Constrained Optimization BY Linear Approximation for .NET
    ///
    /// COBYLA2 is an implementation of Powell’s nonlinear derivative–free constrained optimization that uses 
    /// a linear approximation approach. The algorithm is a sequential trust–region algorithm that employs linear 
    /// approximations to the objective and constraint functions, where the approximations are formed by linear 
    /// interpolation at n + 1 points in the space of the variables and tries to maintain a regular–shaped simplex 
    /// over iterations.
    ///
    /// It solves nonsmooth NLP with a moderate number of variables (about 100). Inequality constraints only.
    ///
    /// The initial point X is taken as one vertex of the initial simplex with zero being another, so, X should 
    /// not be entered as the zero vector.
    /// </summary>
    public class COBYLA
    {
        #region FIELDS

        private readonly string IterationResultFormatter = Environment.NewLine + "NFVALS = {0,5}   F = {1,13:E6}    MAXCV = {2,13:E6}";

        #endregion

        #region METHODS

        /// <summary>
        /// Minimizes the objective function F with respect to a set of inequality constraints CON, 
        /// and returns the optimal variable array. F and CON may be non-linear, and should preferably be smooth.
        /// </summary>
        /// <param name="calcfc">Method for calculating objective function and constraints.</param>
        /// <param name="n">Number of variables.</param>
        /// <param name="m">Number of constraints.</param>
        /// <param name="x">On input initial values of the variables (zero-based array). On output
        /// optimal values of the variables obtained in the COBYLA minimization.</param>
        /// <param name="rhobeg">Initial size of the simplex.</param>
        /// <param name="rhoend">Final value of the simplex.</param>
        /// <param name="iprint">Print level, 0 &lt;= iprint &lt;= 3, where 0 provides no output and
        /// 3 provides full output to the console.</param>
        /// <param name="maxfun">Maximum number of function evaluations before terminating.</param>
        /// <param name="logger">If defined, text writer where to log output from Cobyla.</param>
        /// <returns>Return status of the COBYLA2 optimization.</returns>
        public CobylaExitStatus FindMinimum(CalcfcDelegate calcfc, int n, int m, double[] x, double rhobeg = 0.5, double rhoend = 1.0e-6, int iprint = 2, int maxfun = 3500, TextWriter logger = null)
        {
			int iters = maxfun;
            return Cobyla(calcfc, n, m, x, rhobeg, rhoend, iprint, ref iters, logger);
        }

        /// <summary>
        /// Minimizes the objective function F with respect to a set of inequality constraints CON, 
        /// and returns the optimal variable array. F and CON may be non-linear, and should preferably be smooth.
        /// This overloaded method provides more detailed results of the optimization, including final
        /// objective function value, constraints function values and performed number of function evaluations.
        /// </summary>
        /// <param name="calcfc">Method for calculating objective function and constraints.</param>
        /// <param name="n">Number of variables.</param>
        /// <param name="m">Number of constraints.</param>
        /// <param name="x">On input initial values of the variables (zero-based array). On output
        /// optimal values of the variables obtained in the COBYLA minimization.</param>
        /// <param name="rhobeg">Initial size of the simplex.</param>
        /// <param name="rhoend">Final value of the simplex.</param>
        /// <param name="iprint">Print level, 0 &lt;= iprint &lt;= 3, where 0 provides no output and
        /// 3 provides full output to the console.</param>
        /// <param name="maxfun">Maximum number of function evaluations before terminating.</param>
        /// <param name="f">Objective function value at optimal variable values.</param>
        /// <param name="con">Constraints function values at optimal variable values.</param>
        /// <param name="iters">Performed number of function and constraints evaluations.</param>
        /// <param name="logger">If defined, text writer where to log output from Cobyla.</param>
        /// <returns>Return status of the COBYLA2 optimization.</returns>
        public CobylaExitStatus FindMinimum(CalcfcDelegate calcfc, int n, int m, double[] x, double rhobeg, double rhoend, int iprint, int maxfun, out double f, out double[] con, out int iters, TextWriter logger = null)
        {
            iters = maxfun;
			CobylaExitStatus status = Cobyla(calcfc, n, m, x, rhobeg, rhoend, iprint, ref iters, logger);

            con = new double[m];
            calcfc(n, m, x, out f, con);

            return status;
        }

        private CobylaExitStatus Cobyla(CalcfcDelegate calcfc, int n, int m, double[] x, double rhobeg, double rhoend, int iprint, ref int iters, TextWriter logger)
        {
			//     This subroutine minimizes an objective function F(X) subject to M
			//     inequality constraints on X, where X is a vector of variables that has
			//     N components.  The algorithm employs linear approximations to the
			//     objective and constraint functions, the approximations being formed by
			//     linear interpolation at N+1 points in the space of the variables.
			//     We regard these interpolation points as vertices of a simplex.  The
			//     parameter RHO controls the size of the simplex and it is reduced
			//     automatically from RHOBEG to RHOEND.  For each RHO the subroutine tries
			//     to achieve a good vector of variables for the current size, and then
			//     RHO is reduced until the value RHOEND is reached.  Therefore RHOBEG and
			//     RHOEND should be set to reasonable initial changes to and the required
			//     accuracy in the variables respectively, but this accuracy should be
			//     viewed as a subject for experimentation because it is not guaranteed.
			//     The subroutine has an advantage over many of its competitors, however,
			//     which is that it treats each constraint individually when calculating
			//     a change to the variables, instead of lumping the constraints together
			//     into a single penalty function.  The name of the subroutine is derived
			//     from the phrase Constrained Optimization BY Linear Approximations.

			//     The user must set the values of N, M, RHOBEG and RHOEND, and must
			//     provide an initial vector of variables in X.  Further, the value of
			//     IPRINT should be set to 0, 1, 2 or 3, which controls the amount of
			//     printing during the calculation. Specifically, there is no output if
			//     IPRINT=0 and there is output only at the end of the calculation if
			//     IPRINT=1.  Otherwise each new value of RHO and SIGMA is printed.
			//     Further, the vector of variables and some function information are
			//     given either when RHO is reduced or when each new value of F(X) is
			//     computed in the cases IPRINT=2 or IPRINT=3 respectively. Here SIGMA
			//     is a penalty parameter, it being assumed that a change to X is an
			//     improvement if it reduces the merit function
			//                F(X)+SIGMA*MAX(0.0, - C1(X), - C2(X),..., - CM(X)),
			//     where C1,C2,...,CM denote the constraint functions that should become
			//     nonnegative eventually, at least to the precision of RHOEND. In the
			//     printed output the displayed term that is multiplied by SIGMA is
			//     called MAXCV, which stands for 'MAXimum Constraint Violation'.  The
			//     argument ITERS is an integer variable that must be set by the user to a
			//     limit on the number of calls of CALCFC, the purpose of this routine being
			//     given below.  The value of ITERS will be altered to the number of calls
			//     of CALCFC that are made.

			//     In order to define the objective and constraint functions, we require
			//     a subroutine that has the name and arguments
			//                SUBROUTINE CALCFC (N,M,X,F,CON)
			//                DIMENSION X(:),CON(:)  .
			//     The values of N and M are fixed and have been defined already, while
			//     X is now the current vector of variables. The subroutine should return
			//     the objective and constraint functions at X in F and CON(1),CON(2),
			//     ...,CON(M).  Note that we are trying to adjust X so that F(X) is as
			//     small as possible subject to the constraint functions being nonnegative.

			// Local variables
			int mpp = m + 2;

			// Internal base-1 X array
			double[] xinout = new double[n + 1];
            Array.Copy(x, 0, xinout, 1, n);

            // Internal representation of the objective and constraints calculation method, 
            // accounting for that X and CON arrays in the cobylb method are base-1 arrays.
            var fcalcfc = new CalcfcDelegate(
                (int nn, int mm, double[] xx, out double f, double[] con) =>
                {
					double[] ixx = new double[nn];
                    Array.Copy(xx, 1, ixx, 0, nn);
					double[] ocon = new double[mm];
                    calcfc(nn, mm, ixx, out f, ocon);
                    Array.Copy(ocon, 0, con, 1, mm);
                });

			CobylaExitStatus status = Cobylb(fcalcfc, n, m, mpp, xinout, rhobeg, rhoend, iprint, ref iters, logger);

            Array.Copy(xinout, 1, x, 0, n);

            return status;
        }

        private CobylaExitStatus Cobylb(CalcfcDelegate calcfc, int n, int m, int mpp, double[] x, double rhobeg, double rhoend, int iprint, ref int maxfun, TextWriter logger)
        {
            // N.B. Arguments CON, SIM, SIMI, DATMAT, A, VSIG, VETA, SIGBAR, DX, W & IACT
            //      have been removed.

            //     Set the initial values of some parameters. The last column of SIM holds
            //     the optimal vertex of the current simplex, and the preceding N columns
            //     hold the displacements from the optimal vertex to the other vertices.
            //     Further, SIMI holds the inverse of the matrix that is contained in the
            //     first N columns of SIM.

            // Local variables

            const double alpha = 0.25;
            const double beta = 2.1;
            const double gamma = 0.5;
            const double delta = 1.1;

			double resmax, total;

			int np = n + 1;
			int mp = m + 1;
			double rho = rhobeg;
			double parmu = 0.0;

			bool iflag = false;
			bool ifull = false;
			double parsig = 0.0;
			double prerec = 0.0;
			double prerem = 0.0;

			double[] con = new double[1 + mpp];
			double[,] sim = new double[1 + n, 1 + np];
			double[,] simi = new double[1 + n, 1 + n];
			double[,] datmat = new double[1 + mpp, 1 + np];
			double[,] a = new double[1 + n, 1 + mp];
			double[] vsig = new double[1 + n];
			double[] veta = new double[1 + n];
			double[] sigbar = new double[1 + n];
			double[] dx = new double[1 + n];
			double[] w = new double[1 + n];

            if (iprint >= 2 && logger != null)
                logger.WriteLine(Environment.NewLine + "The initial value of RHO is {0,13:F6} and PARMU is set to zero.", rho);

			int nfvals = 0;
			double temp = 1.0 / rho;

            for (int i = 1; i <= n; ++i)
            {
                sim[i, np] = x[i];
                sim[i, i] = rho;
                simi[i, i] = temp;
            }

			int jdrop = np;
			bool ibrnch = false;

            CobylaExitStatus status;

            //     Make the next call of the user-supplied subroutine CALCFC. These
        //     instructions are also used for calling CALCFC during the iterations of
        //     the algorithm.

            L_40:
            if (nfvals >= maxfun && nfvals > 0)
            {
                if (iprint >= 1 && logger != null)
                    logger.WriteLine(Environment.NewLine + "Return from subroutine COBYLA because the MAXFUN limit has been reached.");
                status = CobylaExitStatus.MaxIterationsReached;
                goto L_600;
            }

            ++nfvals;

            calcfc(n, m, x, out double f, con);
            resmax = 0.0; for (int k = 1; k <= m; ++k) resmax = Math.Max(resmax, -con[k]);

            if ((nfvals == iprint - 1 || iprint == 3) && logger != null)
            {
                logger.WriteLine(IterationResultFormatter, nfvals, f, resmax);
                logger.WriteLine("X = {0}", x.PART(1, n).FORMAT());
            }

            con[mp] = f;
            con[mpp] = resmax;
            if (ibrnch) goto L_440;

            //     Set the recently calculated function values in a column of DATMAT. This
            //     array has a column for each vertex of the current simplex, the entries of
            //     each column being the values of the constraint functions (if any)
            //     followed by the objective function and the greatest constraint violation
            //     at the vertex.

            for (int i = 1; i <= mpp; ++i) datmat[i, jdrop] = con[i];

            if (nfvals <= np)
            {

                //     Exchange the new vertex of the initial simplex with the optimal vertex if
                //     necessary. Then, if the initial simplex is not complete, pick its next
                //     vertex and calculate the function values there.

                if (jdrop <= n)
                {
                    if (datmat[mp, np] <= f)
                    {
                        x[jdrop] = sim[jdrop, np];
                    }
                    else
                    {
                        sim[jdrop, np] = x[jdrop];
                        for (int k = 1; k <= mpp; ++k)
                        {
                            datmat[k, jdrop] = datmat[k, np];
                            datmat[k, np] = con[k];
                        }
                        for (int k = 1; k <= jdrop; ++k)
                        {
                            sim[jdrop, k] = -rho;
                            temp = 0.0; for (int i = k; i <= jdrop; ++i) temp -= simi[i, k];
                            simi[jdrop, k] = temp;
                        }
                    }
                }
                if (nfvals <= n)
                {
                    jdrop = nfvals;
                    x[jdrop] += rho;
                    goto L_40;
                }
            }

            ibrnch = true;

            //     Identify the optimal vertex of the current simplex.

            L_140:
			double phimin = datmat[mp, np] + parmu * datmat[mpp, np];
			int nbest = np;

            for (int j = 1; j <= n; ++j)
            {
                temp = datmat[mp, j] + parmu * datmat[mpp, j];
                if (temp < phimin)
                {
                    nbest = j;
                    phimin = temp;
                }
                else if (temp == phimin && parmu == 0.0 && datmat[mpp, j] < datmat[mpp, nbest])
                {
                    nbest = j;
                }
            }

            //     Switch the best vertex into pole position if it is not there already,
            //     and also update SIM, SIMI and DATMAT.

            if (nbest <= n)
            {
                for (int i = 1; i <= mpp; ++i)
                {
                    temp = datmat[i, np];
                    datmat[i, np] = datmat[i, nbest];
                    datmat[i, nbest] = temp;
                }
                for (int i = 1; i <= n; ++i)
                {
                    temp = sim[i, nbest];
                    sim[i, nbest] = 0.0;
                    sim[i, np] += temp;

					double tempa = 0.0;
                    for (int k = 1; k <= n; ++k)
                    {
                        sim[i, k] -= temp;
                        tempa -= simi[k, i];
                    }
                    simi[nbest, i] = tempa;
                }
            }

			//     Make an error return if SIGI is a poor approximation to the inverse of
			//     the leading N by N submatrix of SIG.

			double error = 0.0;
            for (int i = 1; i <= n; ++i)
            {
                for (int j = 1; j <= n; ++j)
                {
                    temp = DOT_PRODUCT(simi.ROW(i).PART(1, n), sim.COL(j).PART(1, n)) - (i == j ? 1.0 : 0.0);
                    error = Math.Max(error, Math.Abs(temp));
                }
            }
            if (error > 0.1)
            {
                if (iprint >= 1 && logger != null)
                    logger.WriteLine(Environment.NewLine + "Return from subroutine COBYLA because rounding errors are becoming damaging.");
                status = CobylaExitStatus.DivergingRoundingErrors;
                goto L_600;
            }

            //     Calculate the coefficients of the linear approximations to the objective
            //     and constraint functions, placing minus the objective function gradient
            //     after the constraint gradients in the array A. The vector W is used for
            //     working space.

            for (int k = 1; k <= mp; ++k)
            {
                con[k] = -datmat[k, np];
                for (int j = 1; j <= n; ++j) w[j] = datmat[k, j] + con[k];

                for (int i = 1; i <= n; ++i)
                {
                    a[i, k] = (k == mp ? -1.0 : 1.0) * DOT_PRODUCT(w.PART(1, n), simi.COL(i).PART(1, n));
                }
            }

            //     Calculate the values of sigma and eta, and set IFLAG = 0 if the current
            //     simplex is not acceptable.

            iflag = true;
            parsig = alpha * rho;
			double pareta = beta * rho;

            for (int j = 1; j <= n; ++j)
            {
				double wsig = 0.0; for (int k = 1; k <= n; ++k) wsig += simi[j, k] * simi[j, k];
				double weta = 0.0; for (int k = 1; k <= n; ++k) weta += sim[k, j] * sim[k, j];
                vsig[j] = 1.0 / Math.Sqrt(wsig);
                veta[j] = Math.Sqrt(weta);
                if (vsig[j] < parsig || veta[j] > pareta) iflag = false;
            }

            //     If a new vertex is needed to improve acceptability, then decide which
            //     vertex to drop from the simplex.

            if (!ibrnch && !iflag)
            {
                jdrop = 0;
                temp = pareta;
                for (int j = 1; j <= n; ++j)
                {
                    if (veta[j] > temp)
                    {
                        jdrop = j;
                        temp = veta[j];
                    }
                }
                if (jdrop == 0)
                {
                    for (int j = 1; j <= n; ++j)
                    {
                        if (vsig[j] < temp)
                        {
                            jdrop = j;
                            temp = vsig[j];
                        }
                    }
                }

                //     Calculate the step to the new vertex and its sign.

                temp = gamma * rho * vsig[jdrop];
                for (int k = 1; k <= n; ++k) dx[k] = temp * simi[jdrop, k];
				double cvmaxp = 0.0;
				double cvmaxm = 0.0;

                total = 0.0;
                for (int k = 1; k <= mp; ++k)
                {
                    total = DOT_PRODUCT(a.COL(k).PART(1, n), dx.PART(1, n));
                    if (k < mp)
                    {
                        temp = datmat[k, np];
                        cvmaxp = Math.Max(cvmaxp, -total - temp);
                        cvmaxm = Math.Max(cvmaxm, total - temp);
                    }
                }
				double dxsign = parmu * (cvmaxp - cvmaxm) > 2.0 * total ? -1.0 : 1.0;

                //     Update the elements of SIM and SIMI, and set the next X.

                temp = 0.0;
                for (int i = 1; i <= n; ++i)
                {
                    dx[i] = dxsign * dx[i];
                    sim[i, jdrop] = dx[i];
                    temp += simi[jdrop, i] * dx[i];
                }
                for (int k = 1; k <= n; ++k) simi[jdrop, k] /= temp;

                for (int j = 1; j <= n; ++j)
                {
                    if (j != jdrop)
                    {
                        temp = DOT_PRODUCT(simi.ROW(j).PART(1, n), dx.PART(1, n));
                        for (int k = 1; k <= n; ++k) simi[j, k] -= temp * simi[jdrop, k];
                    }
                    x[j] = sim[j, np] + dx[j];
                }
                goto L_40;
            }

            //     Calculate DX = x(*)-x(0).
            //     Branch if the length of DX is less than 0.5*RHO.

            Trstlp(n, m, a, con, rho, dx, out ifull);
            if (!ifull)
            {
                temp = 0.0; for (int k = 1; k <= n; ++k) temp += dx[k] * dx[k];
                if (temp < 0.25 * rho * rho)
                {
                    ibrnch = true;
                    goto L_550;
                }
            }

            //     Predict the change to F and the new maximum constraint violation if the
            //     variables are altered from x(0) to x(0) + DX.

            total = 0.0;
			double resnew = 0.0;
            con[mp] = 0.0;
            for (int k = 1; k <= mp; ++k)
            {
                total = con[k] - DOT_PRODUCT(a.COL(k).PART(1, n), dx.PART(1, n));
                if (k < mp) resnew = Math.Max(resnew, total);
            }

            //     Increase PARMU if necessary and branch back if this change alters the
            //     optimal vertex. Otherwise PREREM and PREREC will be set to the predicted
            //     reductions in the merit function and the maximum constraint violation
            //     respectively.

            prerec = datmat[mpp, np] - resnew;
			double barmu = prerec > 0.0 ? total / prerec : 0.0;
            if (parmu < 1.5 * barmu)
            {
                parmu = 2.0 * barmu;
                if (iprint >= 2 && logger != null) logger.WriteLine(Environment.NewLine + "Increase in PARMU to {0,13:F6}", parmu);
				double phi = datmat[mp, np] + parmu * datmat[mpp, np];
                for (int j = 1; j <= n; ++j)
                {
                    temp = datmat[mp, j] + parmu * datmat[mpp, j];
                    if (temp < phi || (temp == phi && parmu == 0.0 && datmat[mpp, j] < datmat[mpp, np])) goto L_140;
                }
            }
            prerem = parmu * prerec - total;

            //     Calculate the constraint and objective functions at x(*).
            //     Then find the actual reduction in the merit function.

            for (int k = 1; k <= n; ++k) x[k] = sim[k, np] + dx[k];
            ibrnch = true;
            goto L_40;

        L_440:
			double vmold = datmat[mp, np] + parmu * datmat[mpp, np];
			double vmnew = f + parmu * resmax;
			double trured = vmold - vmnew;
            if (parmu == 0.0 && f == datmat[mp, np])
            {
                prerem = prerec;
                trured = datmat[mpp, np] - resmax;
            }

			//     Begin the operations that decide whether x(*) should replace one of the
			//     vertices of the current simplex, the change being mandatory if TRURED is
			//     positive. Firstly, JDROP is set to the index of the vertex that is to be
			//     replaced.

			double ratio = trured <= 0.0 ? 1.0 : 0.0;
            jdrop = 0;
            for (int j = 1; j <= n; ++j)
            {
                temp = Math.Abs(DOT_PRODUCT(simi.ROW(j).PART(1, n), dx.PART(1, n)));
                if (temp > ratio)
                {
                    jdrop = j;
                    ratio = temp;
                }
                sigbar[j] = temp * vsig[j];
            }

			//     Calculate the value of ell.

			double edgmax = delta * rho;
			int l = 0;
            for (int j = 1; j <= n; ++j)
            {
                if (sigbar[j] >= parsig || sigbar[j] >= vsig[j])
                {
                    temp = veta[j];
                    if (trured > 0.0)
                    {
                        temp = 0.0; for (int k = 1; k <= n; ++k) temp += Math.Pow(dx[k] - sim[k, j], 2.0);
                        temp = Math.Sqrt(temp);
                    }
                    if (temp > edgmax)
                    {
                        l = j;
                        edgmax = temp;
                    }
                }
            }
            if (l > 0) jdrop = l;
            if (jdrop == 0) goto L_550;

            //     Revise the simplex by updating the elements of SIM, SIMI and DATMAT.

            temp = 0.0;
            for (int i = 1; i <= n; ++i)
            {
                sim[i, jdrop] = dx[i];
                temp += simi[jdrop, i] * dx[i];
            }
            for (int k = 1; k <= n; ++k) simi[jdrop, k] /= temp;
            for (int j = 1; j <= n; ++j)
            {
                if (j != jdrop)
                {
                    temp = DOT_PRODUCT(simi.ROW(j).PART(1, n), dx.PART(1, n));
                    for (int k = 1; k <= n; ++k) simi[j, k] -= temp * simi[jdrop, k];
                }
            }
            for (int k = 1; k <= mpp; ++k) datmat[k, jdrop] = con[k];

            //     Branch back for further iterations with the current RHO.

            if (trured > 0.0 && trured >= 0.1 * prerem) goto L_140;

        L_550:
            if (!iflag)
            {
                ibrnch = false;
                goto L_140;
            }

            //     Otherwise reduce RHO if it is not at its least value and reset PARMU.

            if (rho > rhoend)
            {
                double cmin = 0.0, cmax = 0.0;

                rho *= 0.5;
                if (rho <= 1.5 * rhoend) rho = rhoend;
                if (parmu > 0.0)
                {
					double denom = 0.0;
                    for (int k = 1; k <= mp; ++k)
                    {
                        cmin = datmat[k, np];
                        cmax = cmin;
                        for (int i = 1; i <= n; ++i)
                        {
                            cmin = Math.Min(cmin, datmat[k, i]);
                            cmax = Math.Max(cmax, datmat[k, i]);
                        }
                        if (k <= m && cmin < 0.5 * cmax)
                        {
                            temp = Math.Max(cmax, 0.0) - cmin;
                            denom = denom <= 0.0 ? temp : Math.Min(denom, temp);
                        }
                    }
                    if (denom == 0.0)
                    {
                        parmu = 0.0;
                    }
                    else if (cmax - cmin < parmu * denom)
                    {
                        parmu = (cmax - cmin) / denom;
                    }
                }
                if (logger != null)
                {
                    if (iprint >= 2)
                        logger.WriteLine(Environment.NewLine + "Reduction in RHO to {0,13:E6}  and PARMU = {1,13:E6}", rho, parmu);
                    if (iprint == 2)
                    {
                        logger.WriteLine(IterationResultFormatter, nfvals, datmat[mp, np], datmat[mpp, np]);
                        logger.WriteLine("X = {0}", sim.COL(np).PART(1, n).FORMAT());
                    }
                }
                goto L_140;
            }

            //     Return the best calculated values of the variables.

            status = CobylaExitStatus.Normal;
            if (iprint >= 1 && logger != null) logger.WriteLine(Environment.NewLine + "Normal return from subroutine COBYLA");
            if (ifull) goto L_620;

        L_600:
            for (int k = 1; k <= n; ++k) x[k] = sim[k, np];
            f = datmat[mp, np];
            resmax = datmat[mpp, np];

        L_620:
            if (iprint >= 1 && logger != null)
            {
                logger.WriteLine(IterationResultFormatter, nfvals, f, resmax);
                logger.WriteLine("X = {0}", x.PART(1, n).FORMAT());
            }

            maxfun = nfvals;

            return status;
        }

        private void Trstlp(int n, int m, double[,] a, double[] b, double rho, double[] dx, out bool ifull)
        {
            // N.B. Arguments Z, ZDOTA, VMULTC, SDIRN, DXNEW, VMULTD & IACT have been removed.

            //     This subroutine calculates an N-component vector DX by applying the
            //     following two stages. In the first stage, DX is set to the shortest
            //     vector that minimizes the greatest violation of the constraints
            //       A(1,K)*DX(1)+A(2,K)*DX(2)+...+A(N,K)*DX(N) .GE. B(K), K = 2,3,...,M,
            //     subject to the Euclidean length of DX being at most RHO. If its length is
            //     strictly less than RHO, then we use the resultant freedom in DX to
            //     minimize the objective function
            //              -A(1,M+1)*DX(1) - A(2,M+1)*DX(2) - ... - A(N,M+1)*DX(N)
            //     subject to no increase in any greatest constraint violation. This
            //     notation allows the gradient of the objective function to be regarded as
            //     the gradient of a constraint. Therefore the two stages are distinguished
            //     by MCON .EQ. M and MCON .GT. M respectively. It is possible that a
            //     degeneracy may prevent DX from attaining the target length RHO. Then the
            //     value IFULL = 0 would be set, but usually IFULL = 1 on return.

            //     In general NACT is the number of constraints in the active set and
            //     IACT(1),...,IACT(NACT) are their indices, while the remainder of IACT
            //     contains a permutation of the remaining constraint indices.  Further, Z
            //     is an orthogonal matrix whose first NACT columns can be regarded as the
            //     result of Gram-Schmidt applied to the active constraint gradients.  For
            //     J = 1,2,...,NACT, the number ZDOTA(J) is the scalar product of the J-th
            //     column of Z with the gradient of the J-th active constraint.  DX is the
            //     current vector of variables and here the residuals of the active
            //     constraints should be zero. Further, the active constraints have
            //     nonnegative Lagrange multipliers that are held at the beginning of
            //     VMULTC. The remainder of this vector holds the residuals of the inactive
            //     constraints at DX, the ordering of the components of VMULTC being in
            //     agreement with the permutation of the indices of the constraints that is
            //     in IACT. All these residuals are nonnegative, which is achieved by the
            //     shift RESMAX that makes the least residual zero.

            //     Initialize Z and some other variables. The value of RESMAX will be
            //     appropriate to DX = 0, while ICON will be the index of a most violated
            //     constraint if RESMAX is positive. Usually during the first stage the
            //     vector SDIRN gives a search direction that reduces all the active
            //     constraint violations by one simultaneously.

            // Local variables

            ifull = true;

            double temp;

			int nactx = 0;
			double resold = 0.0;

			double[,] z = new double[1 + n, 1 + n];
			double[] zdota = new double[2 + m];
			double[] vmultc = new double[2 + m];
			double[] sdirn = new double[1 + n];
			double[] dxnew = new double[1 + n];
			double[] vmultd = new double[2 + m];
			int[] iact = new int[2 + m];

			int mcon = m;
			int nact = 0;
            for (int i = 1; i <= n; ++i)
            {
                z[i, i] = 1.0;
                dx[i] = 0.0;
            }

			int icon = 0;
			double resmax = 0.0;
            if (m >= 1)
            {
                for (int k = 1; k <= m; ++k)
                {
                    if (b[k] > resmax)
                    {
                        resmax = b[k];
                        icon = k;
                    }
                }
                for (int k = 1; k <= m; ++k)
                {
                    iact[k] = k;
                    vmultc[k] = resmax - b[k];
                }
            }
            if (resmax == 0.0) goto L_480;

            //     End the current stage of the calculation if 3 consecutive iterations
        //     have either failed to reduce the best calculated value of the objective
        //     function or to increase the number of active constraints since the best
        //     value was calculated. This strategy prevents cycling, but there is a
        //     remote possibility that it will cause premature termination.

            L_60:
			double optold = 0.0;
			int icount = 0;

        L_70:
			double optnew = mcon == m ? resmax : -DOT_PRODUCT(dx.PART(1, n), a.COL(mcon).PART(1, n));

            if (icount == 0 || optnew < optold)
            {
                optold = optnew;
                nactx = nact;
                icount = 3;
            }
            else if (nact > nactx)
            {
                nactx = nact;
                icount = 3;
            }
            else
            {
                --icount;
            }
            if (icount == 0) goto L_490;

            //     If ICON exceeds NACT, then we add the constraint with index IACT(ICON) to
            //     the active set. Apply Givens rotations so that the last N-NACT-1 columns
            //     of Z are orthogonal to the gradient of the new constraint, a scalar
            //     product being set to zero if its nonzero value could be due to computer
            //     rounding errors. The array DXNEW is used for working space.

            if (icon <= nact) goto L_260;
			int kk = iact[icon];
            for (int k = 1; k <= n; ++k) dxnew[k] = a[k, kk];
			double tot = 0.0;

            {
				int k = n;
                while (k > nact)
                {
					double sp = 0.0;
					double spabs = 0.0;
                    for (int i = 1; i <= n; ++i)
                    {
                        temp = z[i, k] * dxnew[i];
                        sp += temp;
                        spabs += Math.Abs(temp);
                    }
					double acca = spabs + 0.1 * Math.Abs(sp);
					double accb = spabs + 0.2 * Math.Abs(sp);
                    if (spabs >= acca || acca >= accb) sp = 0.0;
                    if (tot == 0.0)
                    {
                        tot = sp;
                    }
                    else
                    {
						int kp = k + 1;
                        temp = Math.Sqrt(sp * sp + tot * tot);
						double alpha = sp / temp;
						double beta = tot / temp;
                        tot = temp;
                        for (int i = 1; i <= n; ++i)
                        {
                            temp = alpha * z[i, k] + beta * z[i, kp];
                            z[i, kp] = alpha * z[i, kp] - beta * z[i, k];
                            z[i, k] = temp;
                        }
                    }
                    --k;
                }
            }

            //     Add the new constraint if this can be done without a deletion from the
            //     active set.

            if (tot != 0.0)
            {
                ++nact;
                zdota[nact] = tot;
                vmultc[icon] = vmultc[nact];
                vmultc[nact] = 0.0;
                goto L_210;
            }

			//     The next instruction is reached if a deletion has to be made from the
			//     active set in order to make room for the new active constraint, because
			//     the new constraint gradient is a linear combination of the gradients of
			//     the old active constraints.  Set the elements of VMULTD to the multipliers
			//     of the linear combination.  Further, set IOUT to the index of the
			//     constraint to be deleted, but branch if no suitable index can be found.

			double ratio = -1.0;
            {
				int k = nact;
                do
                {
					double zdotv = 0.0;
					double zdvabs = 0.0;

                    for (int i = 1; i <= n; ++i)
                    {
                        temp = z[i, k] * dxnew[i];
                        zdotv = zdotv + temp;
                        zdvabs = zdvabs + Math.Abs(temp);
                    }
					double acca = zdvabs + 0.1 * Math.Abs(zdotv);
					double accb = zdvabs + 0.2 * Math.Abs(zdotv);
                    if (zdvabs < acca && acca < accb)
                    {
                        temp = zdotv / zdota[k];
                        if (temp > 0.0 && iact[k] <= m)
                        {
							double tempa = vmultc[k] / temp;
                            if (ratio < 0.0 || tempa < ratio) ratio = tempa;
                        }

                        if (k >= 2)
                        {
							int kw = iact[k];
                            for (int i = 1; i <= n; ++i) dxnew[i] -= temp * a[i, kw];
                        }
                        vmultd[k] = temp;
                    }
                    else
                    {
                        vmultd[k] = 0.0;
                    }
                } while (--k > 0);
            }
            if (ratio < 0.0) goto L_490;

            //     Revise the Lagrange multipliers and reorder the active constraints so
            //     that the one to be replaced is at the end of the list. Also calculate the
            //     new value of ZDOTA(NACT) and branch if it is not acceptable.

            for (int k = 1; k <= nact; ++k)
                vmultc[k] = Math.Max(0.0, vmultc[k] - ratio * vmultd[k]);
            if (icon < nact)
            {
				int isave = iact[icon];
				double vsave = vmultc[icon];
				int k = icon;
                do
                {
					int kp = k + 1;
					int kw = iact[kp];
					double sp = DOT_PRODUCT(z.COL(k).PART(1, n), a.COL(kw).PART(1, n));
                    temp = Math.Sqrt(sp * sp + zdota[kp] * zdota[kp]);
					double alpha = zdota[kp] / temp;
					double beta = sp / temp;
                    zdota[kp] = alpha * zdota[k];
                    zdota[k] = temp;
                    for (int i = 1; i <= n; ++i)
                    {
                        temp = alpha * z[i, kp] + beta * z[i, k];
                        z[i, kp] = alpha * z[i, k] - beta * z[i, kp];
                        z[i, k] = temp;
                    }
                    iact[k] = kw;
                    vmultc[k] = vmultc[kp];
                    k = kp;
                } while (k < nact);
                iact[k] = isave;
                vmultc[k] = vsave;
            }
            temp = DOT_PRODUCT(z.COL(nact).PART(1, n), a.COL(kk).PART(1, n));
            if (temp == 0.0) goto L_490;
            zdota[nact] = temp;
            vmultc[icon] = 0.0;
            vmultc[nact] = ratio;

            //     Update IACT and ensure that the objective function continues to be
        //     treated as the last active constraint when MCON>M.

            L_210:
            iact[icon] = iact[nact];
            iact[nact] = kk;
            if (mcon > m && kk != mcon)
            {
				int k = nact - 1;
				double sp = DOT_PRODUCT(z.COL(k).PART(1, n), a.COL(kk).PART(1, n));
                temp = Math.Sqrt(sp * sp + zdota[nact] * zdota[nact]);
				double alpha = zdota[nact] / temp;
				double beta = sp / temp;
                zdota[nact] = alpha * zdota[k];
                zdota[k] = temp;
                for (int i = 1; i <= n; ++i)
                {
                    temp = alpha * z[i, nact] + beta * z[i, k];
                    z[i, nact] = alpha * z[i, k] - beta * z[i, nact];
                    z[i, k] = temp;
                }
                iact[nact] = iact[k];
                iact[k] = kk;
                temp = vmultc[k];
                vmultc[k] = vmultc[nact];
                vmultc[nact] = temp;
            }

            //     If stage one is in progress, then set SDIRN to the direction of the next
            //     change to the current vector of variables.

            if (mcon > m) goto L_320;
            kk = iact[nact];
            temp = (DOT_PRODUCT(sdirn.PART(1, n), a.COL(kk).PART(1, n)) - 1.0) / zdota[nact];
            for (int k = 1; k <= n; ++k) sdirn[k] -= temp * z[k, nact];
            goto L_340;

            //     Delete the constraint that has the index IACT(ICON) from the active set.

            L_260:
            if (icon < nact)
            {
				int isave = iact[icon];
				double vsave = vmultc[icon];
				int k = icon;
                do
                {
					int kp = k + 1;
                    kk = iact[kp];
					double sp = DOT_PRODUCT(z.COL(k).PART(1, n), a.COL(kk).PART(1, n));
                    temp = Math.Sqrt(sp * sp + zdota[kp] * zdota[kp]);
					double alpha = zdota[kp] / temp;
					double beta = sp / temp;
                    zdota[kp] = alpha * zdota[k];
                    zdota[k] = temp;
                    for (int i = 1; i <= n; ++i)
                    {
                        temp = alpha * z[i, kp] + beta * z[i, k];
                        z[i, kp] = alpha * z[i, k] - beta * z[i, kp];
                        z[i, k] = temp;
                    }
                    iact[k] = kk;
                    vmultc[k] = vmultc[kp];
                    k = kp;
                } while (k < nact);

                iact[k] = isave;
                vmultc[k] = vsave;
            }
            --nact;

            //     If stage one is in progress, then set SDIRN to the direction of the next
            //     change to the current vector of variables.

            if (mcon > m) goto L_320;
            temp = DOT_PRODUCT(sdirn.PART(1, n), z.COL(nact + 1).PART(1, n));
            for (int k = 1; k <= n; ++k) sdirn[k] -= temp * z[k, nact + 1];
            goto L_340;

            //     Pick the next search direction of stage two.

            L_320:
            temp = 1.0 / zdota[nact];
            for (int k = 1; k <= n; ++k) sdirn[k] = temp * z[k, nact];

            //     Calculate the step to the boundary of the trust region or take the step
        //     that reduces RESMAX to zero. The two statements below that include the
        //     factor 1.0E-6 prevent some harmless underflows that occurred in a test
        //     calculation. Further, we skip the step if it could be zero within a
        //     reasonable tolerance for computer rounding errors.

            L_340:
			double dd = rho * rho;
			double sd = 0.0;
			double ss = 0.0;
            for (int i = 1; i <= n; ++i)
            {
                if (Math.Abs(dx[i]) >= 1.0E-6 * rho) dd -= dx[i] * dx[i];
                sd += dx[i] * sdirn[i];
                ss += sdirn[i] * sdirn[i];
            }
            if (dd <= 0.0) goto L_490;
            temp = Math.Sqrt(ss * dd);
            if (Math.Abs(sd) >= 1.0E-6 * temp) temp = Math.Sqrt(ss * dd + sd * sd);
			double stpful = dd / (temp + sd);
			double step = stpful;
            if (mcon == m)
            {
				double acca = step + 0.1 * resmax;
				double accb = step + 0.2 * resmax;
                if (step >= acca || acca >= accb) goto L_480;
                step = Math.Min(step, resmax);
            }

            //     Set DXNEW to the new variables if STEP is the steplength, and reduce
            //     RESMAX to the corresponding maximum residual if stage one is being done.
            //     Because DXNEW will be changed during the calculation of some Lagrange
            //     multipliers, it will be restored to the following value later.

            for (int k = 1; k <= n; ++k) dxnew[k] = dx[k] + step * sdirn[k];
            if (mcon == m)
            {
                resold = resmax;
                resmax = 0.0;
                for (int k = 1; k <= nact; ++k)
                {
                    kk = iact[k];
                    temp = b[kk] - DOT_PRODUCT(a.COL(kk).PART(1, n), dxnew.PART(1, n));
                    resmax = Math.Max(resmax, temp);
                }
            }

            //     Set VMULTD to the VMULTC vector that would occur if DX became DXNEW. A
            //     device is included to force VMULTD(K) = 0.0 if deviations from this value
            //     can be attributed to computer rounding errors. First calculate the new
            //     Lagrange multipliers.

            {
				int k = nact;
            L_390:
				double zdotw = 0.0;
				double zdwabs = 0.0;
                for (int i = 1; i <= n; ++i)
                {
                    temp = z[i, k] * dxnew[i];
                    zdotw += temp;
                    zdwabs += Math.Abs(temp);
                }
				double acca = zdwabs + 0.1 * Math.Abs(zdotw);
				double accb = zdwabs + 0.2 * Math.Abs(zdotw);
                if (zdwabs >= acca || acca >= accb) zdotw = 0.0;
                vmultd[k] = zdotw / zdota[k];
                if (k >= 2)
                {
                    kk = iact[k];
                    for (int i = 1; i <= n; ++i) dxnew[i] -= vmultd[k] * a[i, kk];
                    --k;
                    goto L_390;
                }
                if (mcon > m) vmultd[nact] = Math.Max(0.0, vmultd[nact]);
            }

            //     Complete VMULTC by finding the new constraint residuals.

            for (int k = 1; k <= n; ++k) dxnew[k] = dx[k] + step * sdirn[k];
            if (mcon > nact)
            {
				int kl = nact + 1;
                for (int k = kl; k <= mcon; ++k)
                {
                    kk = iact[k];
					double total = resmax - b[kk];
					double sumabs = resmax + Math.Abs(b[kk]);
                    for (int i = 1; i <= n; ++i)
                    {
                        temp = a[i, kk] * dxnew[i];
                        total += temp;
                        sumabs += Math.Abs(temp);
                    }
					double acca = sumabs + 0.1 * Math.Abs(total);
					double accb = sumabs + 0.2 * Math.Abs(total);
                    if (sumabs >= acca || acca >= accb) total = 0.0;
                    vmultd[k] = total;
                }
            }

            //     Calculate the fraction of the step from DX to DXNEW that will be taken.

            ratio = 1.0;
            icon = 0;
            for (int k = 1; k <= mcon; ++k)
            {
                if (vmultd[k] < 0.0)
                {
                    temp = vmultc[k] / (vmultc[k] - vmultd[k]);
                    if (temp < ratio)
                    {
                        ratio = temp;
                        icon = k;
                    }
                }
            }

            //     Update DX, VMULTC and RESMAX.

            temp = 1.0 - ratio;
            for (int k = 1; k <= n; ++k) dx[k] = temp * dx[k] + ratio * dxnew[k];
            for (int k = 1; k <= mcon; ++k)
                vmultc[k] = Math.Max(0.0, temp * vmultc[k] + ratio * vmultd[k]);
            if (mcon == m) resmax = resold + ratio * (resmax - resold);

            //     If the full step is not acceptable then begin another iteration.
            //     Otherwise switch to stage two or end the calculation.

            if (icon > 0) goto L_70;
            if (step == stpful) return;

        L_480:
            mcon = m + 1;
            icon = mcon;
            iact[mcon] = mcon;
            vmultc[mcon] = 0.0;
            goto L_60;

            //     We employ any freedom that may be available to reduce the objective
        //     function before returning a DX whose length is less than RHO.

            L_490:
            if (mcon == m) goto L_480;
            ifull = false;
        }

        private double DOT_PRODUCT(double[] lhs, double[] rhs)
        {
			double sum = 0.0; for (int i = 0; i < lhs.Length; ++i) sum += lhs[i] * rhs[i];
            return sum;
        }

        #endregion
    }


    #region Extension Methods

    static class XX
    {
        public static T[] ROW<T>(this T[,] src, int rowidx)
        {
			int cols = src.GetLength(1);
            var dest = new T[cols];
            for (int col = 0; col < cols; ++col) dest[col] = src[rowidx, col];
            return dest;
        }

        public static T[] COL<T>(this T[,] src, int colidx)
        {
			int rows = src.GetLength(0);
            var dest = new T[rows];
            for (int row = 0; row < rows; ++row) dest[row] = src[row, colidx];
            return dest;
        }

        public static T[] PART<T>(this IList<T> src, int from, int to)
        {
            var dest = new T[to - from + 1];
			int destidx = 0;
            for (int srcidx = from; srcidx <= to; ++srcidx, ++destidx) dest[destidx] = src[srcidx];
            return dest;
        }

        public static string FORMAT(this double[] x)
        {
			string[] xStr = new string[x.Length];
            for (int i = 0; i < x.Length; ++i) xStr[i] = String.Format("{0,13:F6}", x[i]);
            return String.Concat(xStr);
        }
    }

    #endregion Extionsion Methods
}
