using System;

using Aircadia.ObjectModel.Treatments.Optimisers.Formulation;
using Aircadia.ObjectModel.Treatments.Optimisers.GA.Selection;
using Aircadia.ObjectModel.Treatments.Optimisers.GA.Crossover;
using Aircadia.ObjectModel.Treatments.Optimisers.GA.Mutation;
using Aircadia.ObjectModel.Treatments.Optimisers.GA.Elitism;

using System.IO;
using MathNet.Numerics;
using Aircadia.Numerics;

namespace Aircadia.ObjectModel.Treatments.Optimisers.GA
{
	[Serializable()]
    public class GAParameters
    {
		//****************************
		//*****general parameters*****
		//****************************

		//genetic algorithm type
		public GATypes GAType { get; set; } = GATypes.NSGA_II;

		//total number of genetic algorithm generations
		public int NoOfGenerations { get; set; } = 20;

		//size of the population
		public int PopulationSize { get; set; } = 50;

		//file writer
		public string EvaluatedSolutionsFile { get; set; } = "evaluatedSolutions.txt";

		//**********************
		//*****GA operators*****
		//**********************

		//selection/reproduction operator method
		public SelectionOprMethods SelectionOprMethod { get; set; } = SelectionOprMethods.TournamentSelectionWithoutReplacement;

		//tournament size for tournament selection operator
		[Option("TournamentSize")]
		public int TournamentSize { get; set; } = 2;

		//crossover probability for crossover operator
		[Option("CrossoverProbability")]
		public double CrossoverProbability { get; set; } = 0.9;

		//crossover operator method
		public CrossoverOprMethods CrossoverOprMethod { get; set; } = CrossoverOprMethods.SimulatedBinaryCrossover;

		//genewise swap probability for uniform crossover operator and simulated binary crossover operator
		public double GeneWiseSwapProbability { get; set; } = 0.5;

		//polynomial order for simulated binary crossover operator
		public int CrossoverPolynomialOrder { get; set; } = 10;

		//mutation probability for mutation operator
		public double MutationProbability { get; set; } = 0.1;

		//mutation operator method
		public MutationOprMethods MutationOprMethod { get; set; } = MutationOprMethods.PolynomialMutation;

		//polynomial order for polynomial mutation operator
		public int MutationPolynomialOrder { get; set; } = 20;

		//elitism operator method
		public ElitismOprMethods ElitismOprMethod { get; set; } = ElitismOprMethods.ProportionalElitism;

		//elitism proportion for proportional elitism operator
		public double ElitismProportion { get; set; } = 0.9;

		//*********************
		//*****constraints*****
		//*********************

		//penalty weights of the constraints
		public double[] PenaltyWeights { get; set; }

		//constraint handling method
		public ConstraintHandlingMethods ConstraintHandlingMethod { get; set; } = ConstraintHandlingMethods.Tournament;

		public GAParameters(OptimisationTemplate template) : this(template.Constraints.Count)
		{
		}

		public GAParameters(int NConstraints)
        {
			PenaltyWeights = Generate.Repeat(NConstraints, 1.0);
		}

        public void LoadParametersFromFile(string inputFilePath)
        {

			EvaluatedSolutionsFile = Directory.GetCurrentDirectory() + "\\evaluatedSolutions.txt";

            try
            {
                var fileStreamReader = new StreamReader(inputFilePath);
				string[] lineToken = null;


				//*****reading line (GA type) from input file*****
				string line = ReadNonCommentedLine(fileStreamReader);
                if (line == null)
                {
                    System.Console.WriteLine("Invalid input file format, refer to documentation");
                    System.Environment.Exit(1);
                }
                else if (line.Equals("NSGA-II"))
					GAType = GATypes.NSGA_II;
                else if (line.Equals("SGA"))
					GAType = GATypes.SGA;
                //System.Console.WriteLine("gaType: " + this.gaType);



                //*****reading constraint handling methods type*****
                line = ReadNonCommentedLine(fileStreamReader);
                if (line == null)
                {
                    System.Console.WriteLine("Invalid input file format (cannot read the constraint handling method type for GA), refer to documentation");
                    System.Environment.Exit(1);
                }
                lineToken = line.Split(' ');
                if (lineToken[0].Equals("LinearPenalty"))
                {
					ConstraintHandlingMethod = ConstraintHandlingMethods.LinearPenalty;
                }
                else if (lineToken[0].Equals("QuadraticPenalty"))
                {
					ConstraintHandlingMethod = ConstraintHandlingMethods.QuadraticPenalty;
                }
                else if (lineToken[0].Equals("Tournament"))
                {
					ConstraintHandlingMethod = ConstraintHandlingMethods.Tournament;
                    if (lineToken.Length == 1)
                    {
						TournamentSize = 2;
                    }
                    else if (lineToken.Length == 2)
                    {
						TournamentSize = Int32.Parse(lineToken[1]);
                    }
                    else
                    {
                        System.Console.WriteLine("Invalid input file format (required: Tournament), refer to documentation");
                        System.Environment.Exit(1);
                    }
                }
                else
                {
                    System.Console.WriteLine("Invalid input file format (constraint handling method type must be either LinearPenalty, QuadraticPenalty or Tournament), refer to documentation");
                    System.Environment.Exit(1);
                }
                //System.Console.WriteLine("constraint handling method: " + this.constraintHandlingMethod);

                if (ConstraintHandlingMethod == ConstraintHandlingMethods.LinearPenalty || ConstraintHandlingMethod == ConstraintHandlingMethods.QuadraticPenalty)
                {
                    /*
                    //*****reading penalty weight for each constraint*****
                    this.penaltyWeights = new double[Template.Constraints.Length];
                    for (int i = 0; i < Template.Constraints.Length; i++)
                    {
                        line = readNonCommentedLine(fileStreamReader);
                        if (line == null)
                        {
                            System.Console.WriteLine("Invalid input file format (cannot read the penalty weight of constraints), refer to documentation");
                            System.Environment.Exit(1);
                        }
                        if ((this.penaltyWeights[i] = Double.Parse(line)) < 0)
                        {
                            System.Console.WriteLine("Penalty weight must be greater than or equal to 0");
                            System.Environment.Exit(1);
                        }
                        System.Console.WriteLine("penalty weight: " + this.penaltyWeights[i]);
                    }
                    */
                    line = ReadNonCommentedLine(fileStreamReader);
                    string[] weights = line.Split(' ');
					PenaltyWeights = new double[weights.Length];
                    for (int i = 0; i < weights.Length; i++)
						PenaltyWeights[i] = Int32.Parse(weights[i]);
                }

                //*****reading the population size from input file*****
                line = ReadNonCommentedLine(fileStreamReader);
                if (line == null)
                {
                    System.Console.WriteLine("Invalid input file format (cannot read the size of population), refer to documentation");
                    System.Environment.Exit(1);
                }
                if (line.Equals("Default"))
                {
					/*
                    //using default value for the size of population 30*ell*log(ell)
                    System.Console.WriteLine("Using default size of population, thumbrule: n = 30*ell*log(ell)");
                    //taking care of small problem sizes where log(ell) < 1
                    if (Template.DesignVariables.Length > 2)
                        this.populationSize = (int)(30 * Template.DesignVariables.Length * Math.Log((double)Template.DesignVariables.Length));
                    else
                        this.populationSize = (int)(30 * Template.DesignVariables.Length);
                    //System.Console.WriteLine("population size: " + this.populationSize);
                    //rounding it to next nearest tenth number
                    if ((this.populationSize % 10) != 0)
                        this.populationSize += (10 - (this.populationSize) % 10);
                    */
					PopulationSize = 50;
                }
                else
                {
					PopulationSize = Int32.Parse(line);
                    if (PopulationSize <= 0)
                    {
                        System.Console.WriteLine("Population size must be greater than 0");
                        System.Environment.Exit(1);
                    }
                    if (PopulationSize % 2 != 0)
                    {
                        System.Console.WriteLine("Population size must be an even number");
                        System.Environment.Exit(1);
                    }
                }
                //System.Console.WriteLine("The population size used is: " + this.populationSize);

                //*****reading the number of generations for GA from input file*****
                line = ReadNonCommentedLine(fileStreamReader);
                if (line == null)
                {
                    System.Console.WriteLine("Invalid input file format (cannot read the number of generations for GA), refer to documentation");
                    System.Environment.Exit(1);
                }
                if (line.Equals("Default"))
                {
					/*
                    //using default value for the number of generations for GA
                    System.Console.WriteLine("Using default convergence-time thumbrule: tc = 6*ell");
                    //taking care of small problem sizes where log(ell) < 1
                    this.noOfGenerations = 6 * Template.DesignVariables.Length;
                    //System.Console.WriteLine("no of generations: " + this.noOfGenerations);
                    //rounding it to next nearest tenth number
                    if ((this.noOfGenerations % 10) != 0)
                        this.noOfGenerations += (10 - (this.noOfGenerations) % 10);
                    */
					NoOfGenerations = 20;
                }
                else if ((NoOfGenerations = Int32.Parse(line)) == 0)
                {
                    System.Console.WriteLine("Invalid input file format (number of generations for GA must be greater than 0), refer to documentation");
                    System.Environment.Exit(1);
                }
                //System.Console.WriteLine("The number of generations used is: " + this.noOfGenerations);

                //*****reading the replacement proportion*****
                line = ReadNonCommentedLine(fileStreamReader);
                if (line == null)
                {
                    System.Console.WriteLine("Invalid input file format (cannot read the replacement proportion), refer to documentation");
                    System.Environment.Exit(1);
                }
                if (line.Equals("Default"))
                {
					//using default value for the replacement proportion
					ElitismProportion = 0.9;
                }
                else
                {
					ElitismProportion = Double.Parse(line);
                    if (ElitismProportion < 0.0 || ElitismProportion > 1.0)
                    {
                        System.Console.WriteLine("Invalid input file format (replacement proportion must be greater than or equal to 0.0 and less than or equal to 1.0), refer to documentation");
                        System.Environment.Exit(1);
                    }
                }
                //System.Console.WriteLine("The replacement proportion is: " + this.elitismProportion);



                //*****reading the selection/reproduction operator type for GA*****
                line = ReadNonCommentedLine(fileStreamReader);
                if (line == null)
                {
                    System.Console.WriteLine("Invalid input file format (cannot read the selection/reproduction operator type for GA), refer to documentation");
                    System.Environment.Exit(1);
                }
                lineToken = line.Split(' ');
                if (lineToken[0].Equals("Default") && lineToken.Length == 1)
                {
					SelectionOprMethod = SelectionOprMethods.TournamentSelectionWithoutReplacement;
                }
                else if (lineToken[0].Equals("TournamentSelectionWithoutReplacement"))
                {
					SelectionOprMethod = SelectionOprMethods.TournamentSelectionWithoutReplacement;
                    //                if (lineToken.length == 1)
                    //                    this.selectionOpr = new TournamentSelectionWithoutReplacement(2);
                    //                else if (lineToken.length == 2)
                    //                    this.selectionOpr = new TournamentSelectionWithoutReplacement(Integer.parseInt(lineToken[1]));
                    //                else {
                    //                    System.out.println("Invalid input file format (required: TOURNAMENT_WITHOUT_REPLACEMENT <TournamentConstraintHandling Size>), refer to documentation");
                    //                    System.exit(1);
                    //                }
                }
                else if (lineToken[0].Equals("TournamentSelectionWithReplacement"))
                {
					SelectionOprMethod = SelectionOprMethods.TournamentSelectionWithReplacement;
                    if (lineToken.Length == 1)
						TournamentSize = 2;
                    else if (lineToken.Length == 2)
						TournamentSize = Int32.Parse(lineToken[1]);
                    else
                    {
                        System.Console.WriteLine("Invalid input file format (required: TOURNAMENT_WITH_REPLACEMENT <TournamentConstraintHandling Size>), refer to documentation");
                        System.Environment.Exit(1);
                    }
                }
                else if (lineToken[0].Equals("TruncationSelection"))
                {
                    //                    this.selectionOprMethod = SelectionOprMethods.Truncation;
                    //                    if (lineToken.Length == 1)
                    //                        this.selectionOprMethod = new TruncationSelectionOpr(2);
                    //                    else if (lineToken.Length == 2)
                    //                        this.selectionOprMethod = new TruncationSelectionOpr(Integer.parseInt(lineToken[1]));
                    //                    else
                    //                    {
                    //                        System.Console.WriteLine("Invalid input file format (required: TRUNCATION <No of copies>), refer to documentation");
                    //                        System.Environment.Exit(1);
                    //                    }
                }
                else
                {
                    System.Console.WriteLine("Invalid input file format (selection/reproduction operator type must be either TOURNAMENT_WITHOUT_REPLACEMENT, TOURNAMENT_WITH_REPLACEMENT or TRUNCATION), refer to documentation");
                    System.Environment.Exit(1);
                }
                //System.Console.WriteLine("Selection/reproduction method is: " + this.selectionOprMethod);

                //*****reading the crossover probability for GA*****
                line = ReadNonCommentedLine(fileStreamReader);
                if (line == null)
                {
                    System.Console.WriteLine("Invalid input file format (cannot read the crossover probability for GA), refer to documentation");
                    System.Environment.Exit(1);
                }
                if (line.Equals("Default"))
					CrossoverProbability = 0.9;
                else
                {
					CrossoverProbability = Double.Parse(line);
                    if (CrossoverProbability < 0 || CrossoverProbability > 1)
                    {
                        System.Console.WriteLine("Crossover probability must be greater than or equal to 0 and less than or equal to 1");
                        System.Environment.Exit(1);
                    }
                }
                //System.Console.WriteLine("Crossover probability is: " + this.crossoverProbability);

                //*****reading the crossover operator type for GA*****
                line = ReadNonCommentedLine(fileStreamReader);
                if (line == null)
                {
                    System.Console.WriteLine("Invalid input file format (cannot read the crossover operator type for GA), refer to documentation");
                    System.Environment.Exit(1);
                }
                lineToken = line.Split(' ');
                if (lineToken[0].Equals("Default") && lineToken.Length == 1)
                {
					CrossoverOprMethod = CrossoverOprMethods.SimulatedBinaryCrossover;
					GeneWiseSwapProbability = 0.5;
					CrossoverPolynomialOrder = 10;
                }
                else if (lineToken[0].Equals("OnePoint"))
                {
                    if (lineToken.Length == 1)
                    {
						CrossoverOprMethod = CrossoverOprMethods.OnePointCrossover;
                    }
                    else
                    {
                        System.Console.WriteLine("Invalid input file format (required: ONE_POINT), refer to documentation");
                        System.Environment.Exit(1);
                    }
                }
                else if (lineToken[0].Equals("TwoPoint"))
                {
                    if (lineToken.Length == 1)
                    {
						CrossoverOprMethod = CrossoverOprMethods.TwoPointCrossover;
                    }
                    else
                    {
                        System.Console.WriteLine("Invalid input file format (required: TWO_POINT), refer to documentation");
                        System.Environment.Exit(1);
                    }
                }
                else if (lineToken[0].Equals("Uniform"))
                {
                    if (lineToken.Length == 1)
                    {
						CrossoverOprMethod = CrossoverOprMethods.UniformCrossover;
						CrossoverPolynomialOrder = 10;
                    }
                    else if (lineToken.Length == 2)
                    {
						CrossoverOprMethod = CrossoverOprMethods.UniformCrossover;
						CrossoverPolynomialOrder = Int32.Parse(lineToken[1]);
                    }
                    else
                    {
                        System.Console.WriteLine("Invalid input file format (required: UNIFORM <Genewise Swap Probability>), refer to documentation");
                        System.Environment.Exit(1);
                    }
                }
                else if (line.Equals("SimulatedBinaryCrossover"))
                {
                    if (lineToken.Length == 1)
                    {
						CrossoverOprMethod = CrossoverOprMethods.SimulatedBinaryCrossover;
						GeneWiseSwapProbability = 0.5;
						CrossoverPolynomialOrder = 10;
                    }
                    else if (lineToken.Length == 3)
                    {
						CrossoverOprMethod = CrossoverOprMethods.SimulatedBinaryCrossover;
						GeneWiseSwapProbability = Double.Parse(lineToken[1]);
						CrossoverPolynomialOrder = Int32.Parse(lineToken[2]);
                    }
                    else
                    {
                        System.Console.WriteLine("Invalid input file format (required: SIMULATED_BINARY <Genewise Swap Probability> <Polynomial Order>), refer to documentation");
                        System.Environment.Exit(1);
                    }
                }
                else
                {
                    System.Console.WriteLine("Invalid input file format (crossover operator type must be either ONE_POINT, TWO_POINT, UNIFORM or SIMULATED_BINARY), refer to documentation");
                    System.Environment.Exit(1);
                }
                //System.Console.WriteLine("Crossover method is: " + this.crossoverOprMethod);

                //*****reading the mutation probability for GA*****
                line = ReadNonCommentedLine(fileStreamReader);
                if (line == null)
                {
                    System.Console.WriteLine("Invalid input file format (cannot read the mutation probability for GA), refer to documentation");
                    System.Environment.Exit(1);
                }
                if (line.Equals("Default"))
					MutationProbability = 0.1;
                else
                {
					MutationProbability = Double.Parse(line);
                    if (MutationProbability < 0 || MutationProbability > 1)
                    {
                        System.Console.WriteLine("Crossover probability must be greater than or equal to 0 and less than or equal to 1");
                        System.Environment.Exit(1);
                    }
                }
                //System.Console.WriteLine("Mutation probability is: " + this.mutationProbability);

                //*****reading the mutation operator type for GA*****
                line = ReadNonCommentedLine(fileStreamReader);
                if (line == null)
                {
                    System.Console.WriteLine("Invalid input file format (cannot read the mutation operator type for GA), refer to documentation");
                    System.Environment.Exit(1);
                }
                lineToken = line.Split(' ');
                if (lineToken[0].Equals("Default"))
                {
					MutationOprMethod = MutationOprMethods.PolynomialMutation;
                }
                else if (lineToken[0].Equals("SelectiveMutation"))
                {
                    if (lineToken.Length == 1)
                    {
						MutationOprMethod = MutationOprMethods.SelectiveMutation;
                    }
                    else
                    {
                        System.Console.WriteLine("Invalid input file format (required: SELECTIVE), refer to documentation");
                        System.Environment.Exit(1);
                    }
                }
                else if (line.Equals("GenewiseMutation"))
                {
                    //                    if (lineToken.length == 1) {
                    //                        this.mutationType = MutationOprMethod.GENEWISE;
                    //                        this.mutationOpr = new GenewiseMutation(0.5, 9);
                    //                    }
                    //                    else if (lineToken.length == 3) {
                    //                        this.mutationType = MutationOprMethod.GENEWISE;
                    //                        this.mutationOpr = new GenewiseMutation(Double.parseDouble(lineToken[1]), Integer.parseInt(lineToken[2]));
                    //                    }
                    //                    else {
                    //                        System.out.println("Invalid input file format (required: SIMULATED_BINARY <Genewise Swap Probability> <Polynomial Order>), refer to documentation");
                    //                        System.exit(1);
                    //                    }
                }
                else if (lineToken[0].Equals("PolynomialMutation"))
                {
                    if (lineToken.Length == 1)
                    {
						MutationOprMethod = MutationOprMethods.PolynomialMutation;
						MutationPolynomialOrder = 20;
                    }
                    else if (lineToken.Length == 2)
                    {
						MutationOprMethod = MutationOprMethods.PolynomialMutation;
						MutationPolynomialOrder = Int32.Parse(lineToken[1]);
                    }
                    else
                    {
                        System.Console.WriteLine("Invalid input file format (required: POLYNOMIAL <Polynomial Order>), refer to documentation");
                        System.Environment.Exit(1);
                    }
                }
                else
                {
                    System.Console.WriteLine("Invalid input file format (mutation operator type must be either SELECTIVE, GENEWISE or POLYNOMIAL), refer to documentation");
                    System.Environment.Exit(1);
                }
                //System.Console.WriteLine("Mutation method is: " + this.mutationOprMethod);

            }
            catch (Exception e)
            {
                if (e is FileNotFoundException)
                    System.Console.WriteLine("file not found");
                if (e is IOException)
                    System.Console.WriteLine("Error reading file");
                System.Console.WriteLine(e.StackTrace);
            }


        }

        private string ReadNonCommentedLine(StreamReader fileStreamReader)
        {
			string tempLine = fileStreamReader.ReadLine();
            while (tempLine.StartsWith("//") || tempLine.Equals(""))
            {
                tempLine = fileStreamReader.ReadLine();
            }
            return tempLine;
        }


		

	}
}
