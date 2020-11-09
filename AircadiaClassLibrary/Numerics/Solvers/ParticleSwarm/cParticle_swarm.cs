/**************************************************************************************************
AIRCADIA - Multiphysics model-based design tool                                                   *
Engineering Design Group                                                                          *
Department of Aerospace Engineering, School of Engineering, Cranfield University                  *
Cranfield, Bedfordshire, MK43 0AL, UK                                                             *
/**************************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using Aircadia.ObjectModel.DataObjects;
using Aircadia.ObjectModel.Models;

namespace Aircadia.Numerics.Solvers
{
	[Serializable()]
    public class Particle_swarm
    {
        public string name;
        public List<Data> vModi;
        public List<Data> vModo;
        public List<WorkflowComponent> models;
        double[] requiredipv;
        Random random = new Random();
        //Constructor for the class
        public Particle_swarm(string n, List<Data> vModVarsin, List<Data> vModVarsout, List<WorkflowComponent> vm)
        {
            name=n;
            vModi = vModVarsin;
            vModo = vModVarsout;
            models = vm;
        }
        //Method to evaluate particle swarm optimization giving the best particle position after predefined steps
        public bool Particle_eval()
        {
            int ninpv=vModo.Count;
            double[] lb= new double[ninpv];
            double[] ub = new double[ninpv];
            int sz = 50; int iter = 5000;
            double[] tryi = new double[ninpv];
            //cognitive velocity vector
            double[,] vcog = new double[sz, ninpv];
            //global best velocity
            double[] vglob = new double[ninpv];
            
            for (int i = 0; i < ninpv; i++)
            {
                if (vModo[i] is DoubleData)
                {
                    lb[i] = (int)vModo[i].Min;
                    ub[i] = (int)vModo[i].Max;
                }
                else if (vModo[i] is DoubleData)
                {
                    lb[i] = (double)vModo[i].Min;
                    ub[i] = (double)vModo[i].Max;
                }
            }
            double[,] pop = InitializeSwarm(sz, lb, ub);

			//This is for storing the history of evaluations for debugginf purposes +++++++++++++++++++++++++++++++++++++++++++++++++++
			string execpath = Directory.GetCurrentDirectory();
			if (File.Exists(execpath + "\\PSOhistory.txt"))
                File.Delete(execpath + "\\PSOhistory.txt");
            var filer = new FileStream(execpath + "\\PSOhistory.txt", FileMode.OpenOrCreate, FileAccess.Write);
            var sw = new StreamWriter(filer);
            sw.Write(@"===========================================================================" + "\r\n");
            sw.Write(@"Particle Swarm Optimisation - History of Evaluations" + "\r\n");
            sw.Write(@"===========================================================================" + "\r\n");
            sw.Write(@"" + "\r\n");
            sw.Write(@"Initial Population:" + "\r\n");
            for (int ii = 0; ii < ninpv; ii++) 
            {
                for (int i = 0; i < sz; i++)
                {
                    string PopElement_of_ii_ninpv = Convert.ToString(pop[i, ii]);
                    sw.Write(PopElement_of_ii_ninpv + "   ");
                }
                sw.Write(@"" + "\r\n");
            }
            sw.Write(@"" + "\r\n");
            sw.Close();
            filer.Close();
            // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

            double[,] popp = new double[sz,ninpv];
            popp = Array2Copy(popp,pop);
            double[] r = new double[ninpv];
            double[] s = new double[sz];
            double[] pb = new double[sz];
            double gb = 100; int j = 0; int flag = 1;
            double eps = 1e-4;
            while(flag==1)
            {
                for (int i = 0; i < sz; i++)
                {
                    for (int ii = 0; ii < ninpv; ii++)
                    {
                        tryi[ii] = pop[i, ii];
                    }
                    r = Testfun(tryi);
                    if (r == null)
                    {
                        Console.WriteLine(">>> Warning - The execution of the following point returned a null value for the '" + (models[0] as WorkflowComponent).Name + "' model:");
                        foreach (Data IthInput in (models[0] as WorkflowComponent).ModelDataInputs)
                        {
                            string IthInputName = IthInput.Name;
                            string IthInputValue = Convert.ToString(IthInput.Value);
                            Console.WriteLine(IthInputName + " = " + IthInputValue);
                        }
                    }
                    s[i] = Ssd(r);
                }
                    if (j == 0)
                    {
                        vcog = Array2Copy(vcog, pop);
                        pb = Array1Copy(pb, s);
                        gb = pb.Min();
                        int minl = Minloc(pb);
                        for (int ii = 0; ii < ninpv; ii++)
                        {
                            vglob[ii] = vcog[minl, ii];
                        } 
                    }
                    else
                    {
                        for (int i = 0; i < sz; i++)
                        {
                            if (s[i] < pb[i])
                            {
                                for (int ii = 0; ii < ninpv; ii++)
                                {
                                    vcog[i, ii] = pop[i, ii];
                                }
                                pb[i] = s[i];
                            }
                        }
                        if (pb.Min() < gb)
                        {
                            gb = pb.Min();
                            int minl = Minloc(pb);
                            for (int ii = 0; ii < ninpv; ii++)
                            {
                                vglob[ii] = vcog[minl, ii];
                            }
                        }
                    }  
                pop= UpdatePop(j,popp,pop,sz, vglob, vcog);

                //This is for storing the history of evaluations for debugginf purposes +++++++++++++++++++++++++++++++++++++++++++++++++++
                filer = new FileStream(execpath + "\\PSOhistory.txt", FileMode.Append, FileAccess.Write);
                sw = new StreamWriter(filer);
                sw.Write(@"Population No. " + Convert.ToString(j) + ":" + "\r\n");
                for (int ii = 0; ii < ninpv; ii++)
                {
                    for (int i = 0; i < sz; i++)
                    {
                        string PopElement_of_ii_ninpv = Convert.ToString(pop[i, ii]);
                        sw.Write(PopElement_of_ii_ninpv + "   ");
                    }
                    sw.Write(@"" + "\r\n");
                }
                sw.Write(@"" + "\r\n");
                sw.Close();
                filer.Close();
                // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

                popp = Array2Copy(popp, pop);
                j=j+1;
                if ((j > iter) || gb < eps)
                    flag = 0;
            }
			vModo.SetValuesDouble(vglob);
            bool status = (models[0] as WorkflowComponent).Execute();
            return status;
        }
        //defines the objective function
        private double[] Testfun(double[] tryinp)
        {
            bool status = true;
			vModo.SetValuesDouble(tryinp);
            status = (models[0] as WorkflowComponent).Execute();
            if (status == false)
                return null;
            double[] ret = new double[vModi.Count];
            for (int i = 0; i < vModi.Count; i++)
            {
                ret[i] = (vModi[i] as Data).GetValueAsDouble() - requiredipv[i];
            }
            return ret;
        }
        //Initialize the swarm positions
        private double[,] InitializeSwarm(int size, double[] lowerb, double[] upperb)
        {
            int nv = lowerb.Count();
            requiredipv = vModi.GetValuesDouble();
            double[,] popi = new double[size, nv];
            for (int ii = 0; ii < size;ii++)
            { 
                for (int jj = 0; jj < nv; jj++)
                {
                    popi[ii, jj] = Rndgen(lowerb[jj], upperb[jj]);  
                }
            }
            return popi;
        }
        //Random number generator
        private double Rndgen(double min, double max)
        {
            double r1 = random.NextDouble();
            double r2 = min + r1 * (max - min);
            return r2;
        }
        private double Ssd(double[] testfo)
        {
            double sum = 0;
            for (int i = 0; i < testfo.Length;i++)
            {
                sum = sum + (testfo[i] * testfo[i]);
                sum = Math.Pow(sum, 0.5);
            }
            return sum;
        }
        private int Minloc(double [] p)
        {
            int loc = 0;
            for (int ii = 1; ii < p.Length; ii++)
            {if(p[ii]<p[loc])
                loc=ii;
            }
            return loc;
        }
        private double[,] UpdatePop(int itr, double[,] pp,double[,] p,int sz1,double[] vg, double[,] vc)
        {
            
            double phip = 2;//Personal best weight
            double phig = 2;//Global best weight
            double phi = 0;
            phi = phip + phig;
            double Chi = 2 / (phi - 2 + Math.Sqrt(phi * phi - 4 * phi));
            phip = phip * Chi;
            phig = phig * Chi;
            double omega = 0;
            if (itr == 0)
            { omega=0;}
            else if (itr < 5 & itr > 0)//Intertial weight reduction
            {omega=Chi;}
            else
            {omega=(Chi*itr)/(itr+5);}
            
            
            for (int i=0;i<sz1;i++)
            {
                for (int j = 0; j < vg.Length; j++)
                {
                    p[i, j] = (p[i, j] * omega) + (random.NextDouble()*phip * (vc[i, j]-p[i, j])) + (random.NextDouble()*phig * (vg[j]-p[i, j]));
                    p[i, j] = pp[i, j] + p[i, j];   
                }
                }
            return p;
        }
        private double[,] Array2Copy(double[,] a, double[,] b)
        {
            int m = a.GetLength(0);
            int n = a.GetLength(1);
            for(int i=0;i<m;i++)
            {
                for(int j=0;j<n;j++)
                {
                    a[i,j]=b[i,j];
                }
            }
            return a;
        }
        private double[] Array1Copy(double[] a, double[] b)
        {
            int m = a.GetLength(0);
            for (int i = 0; i < m; i++)
            {
                a[i] = b[i];
            }
            return a;
        }
    }
}
