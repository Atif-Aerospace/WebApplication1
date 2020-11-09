using Aircadia.ObjectModel.Treatments.Optimisers.Formulation;
using Aircadia.ObjectModel.Treatments.Optimisers.GA;
using Aircadia.ObjectModel.Treatments.Optimisers.GA.Crossover;
using Aircadia.ObjectModel.Treatments.Optimisers.GA.Mutation;
using Aircadia.ObjectModel.Treatments.Optimisers.GA.Selection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Aircadia.ObjectModel.Studies
{
    public partial class GASetup : Form
    {
        private int noOfGenerations;
        public int NoOfGenerations
        {
            get { return this.noOfGenerations; }
            // Read-only
        }

        private int populationSize;
        public int PopulationSize
        {
            get { return this.populationSize; }
            // Read-only
        }

        private SelectionOprMethods selectionOprMethods;
        public SelectionOprMethods SelectionOprMethods
        {
            get { return this.selectionOprMethods; }
            // Read-only
        }

        private CrossoverOprMethods crossoverOprMethods;
        public CrossoverOprMethods CrossoverOprMethods
        {
            get { return this.crossoverOprMethods; }
            // Read-only
        }

        private MutationOprMethods mutationOprMethods;
        public MutationOprMethods MutationOprMethods
        {
            get { return this.mutationOprMethods; }
            // Read-only
        }


        private GAParameters gaParameters;
        public GAParameters GAParameters
        {
            get { return this.gaParameters; }
            // Read-only
        }

        public GASetup(OptimisationTemplate template)
        {
            InitializeComponent();
            gaParameters = new GAParameters(template);
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            this.noOfGenerations = Convert.ToInt32(this.textBoxTotalGenerations.Text);
            this.gaParameters.NoOfGenerations = this.noOfGenerations;

            this.populationSize = Convert.ToInt32(this.textBox1.Text);
            this.gaParameters.PopulationSize = this.populationSize;

            //if ((this.comboBoxSelectionOpr.SelectedItem as RadComboBoxItem).Content.ToString() == "Roulette Wheel")
            //    this.gaParameters.SelectionOprMethod = SelectionOprMethods.RouletteWheelSelection;
            //else if ((this.comboBoxSelectionOpr.SelectedItem as RadComboBoxItem).Content.ToString() == "Tournament Without Replacement")
            //    this.gaParameters.SelectionOprMethod = SelectionOprMethods.TournamentSelectionWithoutReplacement;
            //else if ((this.comboBoxSelectionOpr.SelectedItem as RadComboBoxItem).Content.ToString() == "Tournament With Replacement")
            //    this.gaParameters.SelectionOprMethod = SelectionOprMethods.TournamentSelectionWithReplacement;

            //if ((this.comboBoxCrossoverOpr.SelectedItem as RadComboBoxItem).Content.ToString() == "One-Point")
            //    this.gaParameters.CrossoverOprMethod = CrossoverOprMethods.OnePointCrossover;
            //else if ((this.comboBoxCrossoverOpr.SelectedItem as RadComboBoxItem).Content.ToString() == "Two-Point")
            //    this.gaParameters.CrossoverOprMethod = CrossoverOprMethods.TwoPointCrossover;
            //else if ((this.comboBoxCrossoverOpr.SelectedItem as RadComboBoxItem).Content.ToString() == "Uniform")
            //    this.gaParameters.CrossoverOprMethod = CrossoverOprMethods.UniformCrossover;
            //else if ((this.comboBoxCrossoverOpr.SelectedItem as RadComboBoxItem).Content.ToString() == "Simulated Binary")
            //    this.gaParameters.CrossoverOprMethod = CrossoverOprMethods.SimulatedBinaryCrossover;

            //if ((this.comboBoxMutationOpr.SelectedItem as RadComboBoxItem).Content.ToString() == "Genewise")
            //    this.gaParameters.MutationOprMethod = MutationOprMethods.GenewiseMutation;
            //else if ((this.comboBoxMutationOpr.SelectedItem as RadComboBoxItem).Content.ToString() == "Selective")
            //    this.gaParameters.MutationOprMethod = MutationOprMethods.SelectiveMutation;
            //else if ((this.comboBoxMutationOpr.SelectedItem as RadComboBoxItem).Content.ToString() == "Polynomial")
            //    this.gaParameters.MutationOprMethod = MutationOprMethods.PolynomialMutation;

        }
    }
}
