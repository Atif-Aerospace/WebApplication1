namespace Aircadia
{
    partial class ROP_ConstraintSetup
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.comboBox_ROP_constraint_Metric = new System.Windows.Forms.ComboBox();
            this.comboBox_ROP_constraint_DstrbnAssmptn = new System.Windows.Forms.ComboBox();
            this.textBox_ROP_constraint_Prb = new System.Windows.Forms.TextBox();
            this.button_ROP_constraint_set = new System.Windows.Forms.Button();
            this.label_ROP_constraint_Name = new System.Windows.Forms.Label();
            this.label_ROP_constraint_Metric = new System.Windows.Forms.Label();
            this.label_ROP_constraint_DstrbnAssmptn = new System.Windows.Forms.Label();
            this.label_ROP_constraint_Prb = new System.Windows.Forms.Label();
            this.label_ROP_FormulationOption3_constraint_Limit = new System.Windows.Forms.Label();
            this.label_ROP_constraint_Prb_subscript = new System.Windows.Forms.Label();
            this.label_ROP_TitleCnstrFormulation = new System.Windows.Forms.Label();
            this.radioButton_ROP_FormulationOption1 = new System.Windows.Forms.RadioButton();
            this.comboBox_ROP_FormulationOption1 = new System.Windows.Forms.ComboBox();
            this.label_ROP_FormulationOption1 = new System.Windows.Forms.Label();
            this.textBox_ROP_FormulationOption1_LimitValue = new System.Windows.Forms.TextBox();
            this.comboBox_ROP_FormulationOption1_InequalitySign = new System.Windows.Forms.ComboBox();
            this.radioButton_ROP_FormulationOption2 = new System.Windows.Forms.RadioButton();
            this.textBox_ROP_FormulationOption2K = new System.Windows.Forms.TextBox();
            this.label_ROP_FormulationOption2Variance = new System.Windows.Forms.Label();
            this.comboBox_ROP_FormulationOption2Sign = new System.Windows.Forms.ComboBox();
            this.label_ROP_FormulationOption2Mean = new System.Windows.Forms.Label();
            this.label_ROP_FormulationOption2 = new System.Windows.Forms.Label();
            this.textBox_ROP_FormulationOption2_LimitValue = new System.Windows.Forms.TextBox();
            this.comboBox_ROP_FormulationOption2_InequalitySign = new System.Windows.Forms.ComboBox();
            this.radioButton_ROP_FormulationOption3 = new System.Windows.Forms.RadioButton();
            this.label_ROP_FormulationOption3K = new System.Windows.Forms.Label();
            this.label_ROP_FormulationOption3Variance = new System.Windows.Forms.Label();
            this.comboBox_ROP_FormulationOption3Sign = new System.Windows.Forms.ComboBox();
            this.label_ROP_FormulationOption3Mean = new System.Windows.Forms.Label();
            this.label_ROP_FormulationOption3 = new System.Windows.Forms.Label();
            this.textBox_ROP_FormulationOption3_LimitValue = new System.Windows.Forms.TextBox();
            this.comboBox_ROP_FormulationOption3_InequalitySign = new System.Windows.Forms.ComboBox();
            this.label_ROP_FormulationOption3LossFunc_with = new System.Windows.Forms.Label();
            this.label_ROP_FormulationOption2_constraint_Limit = new System.Windows.Forms.Label();
            this.label_ROP_FormulationOption1_constraint_Limit = new System.Windows.Forms.Label();
            this.button_ROP_objective_cancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // comboBox_ROP_constraint_Metric
            // 
            this.comboBox_ROP_constraint_Metric.Enabled = false;
            this.comboBox_ROP_constraint_Metric.FormattingEnabled = true;
            this.comboBox_ROP_constraint_Metric.Items.AddRange(new object[] {
            "Quantile",
            "TCE"});
            this.comboBox_ROP_constraint_Metric.Location = new System.Drawing.Point(352, 399);
            this.comboBox_ROP_constraint_Metric.Name = "comboBox_ROP_constraint_Metric";
            this.comboBox_ROP_constraint_Metric.Size = new System.Drawing.Size(121, 21);
            this.comboBox_ROP_constraint_Metric.TabIndex = 18;
            this.comboBox_ROP_constraint_Metric.Text = "Quantile";
            // 
            // comboBox_ROP_constraint_DstrbnAssmptn
            // 
            this.comboBox_ROP_constraint_DstrbnAssmptn.Enabled = false;
            this.comboBox_ROP_constraint_DstrbnAssmptn.FormattingEnabled = true;
            this.comboBox_ROP_constraint_DstrbnAssmptn.Items.AddRange(new object[] {
            "None",
            "Normality",
            "Symmetry",
            "Unimodality",
            "Symmetry + Unimodality"});
            this.comboBox_ROP_constraint_DstrbnAssmptn.Location = new System.Drawing.Point(213, 399);
            this.comboBox_ROP_constraint_DstrbnAssmptn.Name = "comboBox_ROP_constraint_DstrbnAssmptn";
            this.comboBox_ROP_constraint_DstrbnAssmptn.Size = new System.Drawing.Size(121, 21);
            this.comboBox_ROP_constraint_DstrbnAssmptn.TabIndex = 17;
            this.comboBox_ROP_constraint_DstrbnAssmptn.Text = "None";
            // 
            // textBox_ROP_constraint_Prb
            // 
            this.textBox_ROP_constraint_Prb.Enabled = false;
            this.textBox_ROP_constraint_Prb.Location = new System.Drawing.Point(125, 399);
            this.textBox_ROP_constraint_Prb.Name = "textBox_ROP_constraint_Prb";
            this.textBox_ROP_constraint_Prb.Size = new System.Drawing.Size(68, 20);
            this.textBox_ROP_constraint_Prb.TabIndex = 15;
            this.textBox_ROP_constraint_Prb.Text = "0.9";
            // 
            // button_ROP_constraint_set
            // 
            this.button_ROP_constraint_set.Location = new System.Drawing.Point(193, 467);
            this.button_ROP_constraint_set.Name = "button_ROP_constraint_set";
            this.button_ROP_constraint_set.Size = new System.Drawing.Size(75, 23);
            this.button_ROP_constraint_set.TabIndex = 14;
            this.button_ROP_constraint_set.Text = "SET";
            this.button_ROP_constraint_set.UseVisualStyleBackColor = true;
            this.button_ROP_constraint_set.Click += new System.EventHandler(this.button_ROP_constraint_set_Click);
            // 
            // label_ROP_constraint_Name
            // 
            this.label_ROP_constraint_Name.AutoSize = true;
            this.label_ROP_constraint_Name.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_ROP_constraint_Name.Location = new System.Drawing.Point(335, 4);
            this.label_ROP_constraint_Name.Name = "label_ROP_constraint_Name";
            this.label_ROP_constraint_Name.Size = new System.Drawing.Size(57, 20);
            this.label_ROP_constraint_Name.TabIndex = 12;
            this.label_ROP_constraint_Name.Text = "label1";
            // 
            // label_ROP_constraint_Metric
            // 
            this.label_ROP_constraint_Metric.AutoSize = true;
            this.label_ROP_constraint_Metric.Enabled = false;
            this.label_ROP_constraint_Metric.Location = new System.Drawing.Point(392, 383);
            this.label_ROP_constraint_Metric.Name = "label_ROP_constraint_Metric";
            this.label_ROP_constraint_Metric.Size = new System.Drawing.Size(36, 13);
            this.label_ROP_constraint_Metric.TabIndex = 20;
            this.label_ROP_constraint_Metric.Text = "Metric";
            // 
            // label_ROP_constraint_DstrbnAssmptn
            // 
            this.label_ROP_constraint_DstrbnAssmptn.AutoSize = true;
            this.label_ROP_constraint_DstrbnAssmptn.Enabled = false;
            this.label_ROP_constraint_DstrbnAssmptn.Location = new System.Drawing.Point(210, 383);
            this.label_ROP_constraint_DstrbnAssmptn.Name = "label_ROP_constraint_DstrbnAssmptn";
            this.label_ROP_constraint_DstrbnAssmptn.Size = new System.Drawing.Size(116, 13);
            this.label_ROP_constraint_DstrbnAssmptn.TabIndex = 19;
            this.label_ROP_constraint_DstrbnAssmptn.Text = "Distribution Assumption";
            // 
            // label_ROP_constraint_Prb
            // 
            this.label_ROP_constraint_Prb.AutoSize = true;
            this.label_ROP_constraint_Prb.Enabled = false;
            this.label_ROP_constraint_Prb.Location = new System.Drawing.Point(145, 383);
            this.label_ROP_constraint_Prb.Name = "label_ROP_constraint_Prb";
            this.label_ROP_constraint_Prb.Size = new System.Drawing.Size(20, 13);
            this.label_ROP_constraint_Prb.TabIndex = 16;
            this.label_ROP_constraint_Prb.Text = "Po";
            // 
            // label_ROP_FormulationOption3_constraint_Limit
            // 
            this.label_ROP_FormulationOption3_constraint_Limit.AutoSize = true;
            this.label_ROP_FormulationOption3_constraint_Limit.Enabled = false;
            this.label_ROP_FormulationOption3_constraint_Limit.Location = new System.Drawing.Point(414, 320);
            this.label_ROP_FormulationOption3_constraint_Limit.Name = "label_ROP_FormulationOption3_constraint_Limit";
            this.label_ROP_FormulationOption3_constraint_Limit.Size = new System.Drawing.Size(28, 13);
            this.label_ROP_FormulationOption3_constraint_Limit.TabIndex = 22;
            this.label_ROP_FormulationOption3_constraint_Limit.Text = "Limit";
            // 
            // label_ROP_constraint_Prb_subscript
            // 
            this.label_ROP_constraint_Prb_subscript.AutoSize = true;
            this.label_ROP_constraint_Prb_subscript.Enabled = false;
            this.label_ROP_constraint_Prb_subscript.Font = new System.Drawing.Font("Microsoft Sans Serif", 6F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_ROP_constraint_Prb_subscript.Location = new System.Drawing.Point(160, 389);
            this.label_ROP_constraint_Prb_subscript.Name = "label_ROP_constraint_Prb_subscript";
            this.label_ROP_constraint_Prb_subscript.Size = new System.Drawing.Size(7, 9);
            this.label_ROP_constraint_Prb_subscript.TabIndex = 23;
            this.label_ROP_constraint_Prb_subscript.Text = "i";
            // 
            // label_ROP_TitleCnstrFormulation
            // 
            this.label_ROP_TitleCnstrFormulation.AutoSize = true;
            this.label_ROP_TitleCnstrFormulation.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_ROP_TitleCnstrFormulation.Location = new System.Drawing.Point(146, 4);
            this.label_ROP_TitleCnstrFormulation.Name = "label_ROP_TitleCnstrFormulation";
            this.label_ROP_TitleCnstrFormulation.Size = new System.Drawing.Size(188, 20);
            this.label_ROP_TitleCnstrFormulation.TabIndex = 29;
            this.label_ROP_TitleCnstrFormulation.Text = "Robust formulation of:";
            // 
            // radioButton_ROP_FormulationOption1
            // 
            this.radioButton_ROP_FormulationOption1.AutoSize = true;
            this.radioButton_ROP_FormulationOption1.Checked = true;
            this.radioButton_ROP_FormulationOption1.Location = new System.Drawing.Point(21, 51);
            this.radioButton_ROP_FormulationOption1.Name = "radioButton_ROP_FormulationOption1";
            this.radioButton_ROP_FormulationOption1.Size = new System.Drawing.Size(181, 17);
            this.radioButton_ROP_FormulationOption1.TabIndex = 30;
            this.radioButton_ROP_FormulationOption1.TabStop = true;
            this.radioButton_ROP_FormulationOption1.Text = "Optimisation of Mean or Variance";
            this.radioButton_ROP_FormulationOption1.UseVisualStyleBackColor = true;
            this.radioButton_ROP_FormulationOption1.CheckedChanged += new System.EventHandler(this.radioButton_ROP_FormulationOption1_CheckedChanged);
            // 
            // comboBox_ROP_FormulationOption1
            // 
            this.comboBox_ROP_FormulationOption1.Font = new System.Drawing.Font("Symbol", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(2)));
            this.comboBox_ROP_FormulationOption1.FormattingEnabled = true;
            this.comboBox_ROP_FormulationOption1.Items.AddRange(new object[] {
            "m"});
            this.comboBox_ROP_FormulationOption1.Location = new System.Drawing.Point(174, 83);
            this.comboBox_ROP_FormulationOption1.Name = "comboBox_ROP_FormulationOption1";
            this.comboBox_ROP_FormulationOption1.Size = new System.Drawing.Size(121, 28);
            this.comboBox_ROP_FormulationOption1.TabIndex = 37;
            this.comboBox_ROP_FormulationOption1.Text = "m";
            // 
            // label_ROP_FormulationOption1
            // 
            this.label_ROP_FormulationOption1.AutoSize = true;
            this.label_ROP_FormulationOption1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_ROP_FormulationOption1.Location = new System.Drawing.Point(114, 88);
            this.label_ROP_FormulationOption1.Name = "label_ROP_FormulationOption1";
            this.label_ROP_FormulationOption1.Size = new System.Drawing.Size(53, 20);
            this.label_ROP_FormulationOption1.TabIndex = 36;
            this.label_ROP_FormulationOption1.Text = "G   = ";
            // 
            // textBox_ROP_FormulationOption1_LimitValue
            // 
            this.textBox_ROP_FormulationOption1_LimitValue.Location = new System.Drawing.Point(374, 88);
            this.textBox_ROP_FormulationOption1_LimitValue.Name = "textBox_ROP_FormulationOption1_LimitValue";
            this.textBox_ROP_FormulationOption1_LimitValue.Size = new System.Drawing.Size(68, 20);
            this.textBox_ROP_FormulationOption1_LimitValue.TabIndex = 39;
            // 
            // comboBox_ROP_FormulationOption1_InequalitySign
            // 
            this.comboBox_ROP_FormulationOption1_InequalitySign.FormattingEnabled = true;
            this.comboBox_ROP_FormulationOption1_InequalitySign.Items.AddRange(new object[] {
            "<=",
            ">="});
            this.comboBox_ROP_FormulationOption1_InequalitySign.Location = new System.Drawing.Point(312, 88);
            this.comboBox_ROP_FormulationOption1_InequalitySign.Name = "comboBox_ROP_FormulationOption1_InequalitySign";
            this.comboBox_ROP_FormulationOption1_InequalitySign.Size = new System.Drawing.Size(44, 21);
            this.comboBox_ROP_FormulationOption1_InequalitySign.TabIndex = 38;
            this.comboBox_ROP_FormulationOption1_InequalitySign.Text = "<=";
            // 
            // radioButton_ROP_FormulationOption2
            // 
            this.radioButton_ROP_FormulationOption2.AutoSize = true;
            this.radioButton_ROP_FormulationOption2.Location = new System.Drawing.Point(21, 163);
            this.radioButton_ROP_FormulationOption2.Name = "radioButton_ROP_FormulationOption2";
            this.radioButton_ROP_FormulationOption2.Size = new System.Drawing.Size(299, 17);
            this.radioButton_ROP_FormulationOption2.TabIndex = 40;
            this.radioButton_ROP_FormulationOption2.TabStop = true;
            this.radioButton_ROP_FormulationOption2.Text = "Optimisation of a Loss Function with an established weight";
            this.radioButton_ROP_FormulationOption2.UseVisualStyleBackColor = true;
            this.radioButton_ROP_FormulationOption2.CheckedChanged += new System.EventHandler(this.radioButton_ROP_FormulationOption2_CheckedChanged);
            // 
            // textBox_ROP_FormulationOption2K
            // 
            this.textBox_ROP_FormulationOption2K.Enabled = false;
            this.textBox_ROP_FormulationOption2K.Location = new System.Drawing.Point(262, 219);
            this.textBox_ROP_FormulationOption2K.Name = "textBox_ROP_FormulationOption2K";
            this.textBox_ROP_FormulationOption2K.Size = new System.Drawing.Size(35, 20);
            this.textBox_ROP_FormulationOption2K.TabIndex = 45;
            this.textBox_ROP_FormulationOption2K.Text = "1";
            // 
            // label_ROP_FormulationOption2Variance
            // 
            this.label_ROP_FormulationOption2Variance.AutoSize = true;
            this.label_ROP_FormulationOption2Variance.Enabled = false;
            this.label_ROP_FormulationOption2Variance.Font = new System.Drawing.Font("Symbol", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(2)));
            this.label_ROP_FormulationOption2Variance.Location = new System.Drawing.Point(302, 217);
            this.label_ROP_FormulationOption2Variance.Name = "label_ROP_FormulationOption2Variance";
            this.label_ROP_FormulationOption2Variance.Size = new System.Drawing.Size(19, 20);
            this.label_ROP_FormulationOption2Variance.TabIndex = 44;
            this.label_ROP_FormulationOption2Variance.Text = "u";
            // 
            // comboBox_ROP_FormulationOption2Sign
            // 
            this.comboBox_ROP_FormulationOption2Sign.Enabled = false;
            this.comboBox_ROP_FormulationOption2Sign.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboBox_ROP_FormulationOption2Sign.FormattingEnabled = true;
            this.comboBox_ROP_FormulationOption2Sign.Items.AddRange(new object[] {
            "+",
            "-"});
            this.comboBox_ROP_FormulationOption2Sign.Location = new System.Drawing.Point(209, 214);
            this.comboBox_ROP_FormulationOption2Sign.Name = "comboBox_ROP_FormulationOption2Sign";
            this.comboBox_ROP_FormulationOption2Sign.Size = new System.Drawing.Size(35, 28);
            this.comboBox_ROP_FormulationOption2Sign.TabIndex = 43;
            this.comboBox_ROP_FormulationOption2Sign.Text = "+";
            // 
            // label_ROP_FormulationOption2Mean
            // 
            this.label_ROP_FormulationOption2Mean.AutoSize = true;
            this.label_ROP_FormulationOption2Mean.Enabled = false;
            this.label_ROP_FormulationOption2Mean.Font = new System.Drawing.Font("Symbol", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(2)));
            this.label_ROP_FormulationOption2Mean.Location = new System.Drawing.Point(170, 217);
            this.label_ROP_FormulationOption2Mean.Name = "label_ROP_FormulationOption2Mean";
            this.label_ROP_FormulationOption2Mean.Size = new System.Drawing.Size(19, 20);
            this.label_ROP_FormulationOption2Mean.TabIndex = 42;
            this.label_ROP_FormulationOption2Mean.Text = "m";
            // 
            // label_ROP_FormulationOption2
            // 
            this.label_ROP_FormulationOption2.AutoSize = true;
            this.label_ROP_FormulationOption2.Enabled = false;
            this.label_ROP_FormulationOption2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_ROP_FormulationOption2.Location = new System.Drawing.Point(114, 219);
            this.label_ROP_FormulationOption2.Name = "label_ROP_FormulationOption2";
            this.label_ROP_FormulationOption2.Size = new System.Drawing.Size(53, 20);
            this.label_ROP_FormulationOption2.TabIndex = 41;
            this.label_ROP_FormulationOption2.Text = "G   = ";
            // 
            // textBox_ROP_FormulationOption2_LimitValue
            // 
            this.textBox_ROP_FormulationOption2_LimitValue.Enabled = false;
            this.textBox_ROP_FormulationOption2_LimitValue.Location = new System.Drawing.Point(398, 219);
            this.textBox_ROP_FormulationOption2_LimitValue.Name = "textBox_ROP_FormulationOption2_LimitValue";
            this.textBox_ROP_FormulationOption2_LimitValue.Size = new System.Drawing.Size(68, 20);
            this.textBox_ROP_FormulationOption2_LimitValue.TabIndex = 47;
            // 
            // comboBox_ROP_FormulationOption2_InequalitySign
            // 
            this.comboBox_ROP_FormulationOption2_InequalitySign.Enabled = false;
            this.comboBox_ROP_FormulationOption2_InequalitySign.FormattingEnabled = true;
            this.comboBox_ROP_FormulationOption2_InequalitySign.Items.AddRange(new object[] {
            "<=",
            ">="});
            this.comboBox_ROP_FormulationOption2_InequalitySign.Location = new System.Drawing.Point(336, 219);
            this.comboBox_ROP_FormulationOption2_InequalitySign.Name = "comboBox_ROP_FormulationOption2_InequalitySign";
            this.comboBox_ROP_FormulationOption2_InequalitySign.Size = new System.Drawing.Size(44, 21);
            this.comboBox_ROP_FormulationOption2_InequalitySign.TabIndex = 46;
            this.comboBox_ROP_FormulationOption2_InequalitySign.Text = "<=";
            // 
            // radioButton_ROP_FormulationOption3
            // 
            this.radioButton_ROP_FormulationOption3.AutoSize = true;
            this.radioButton_ROP_FormulationOption3.Location = new System.Drawing.Point(21, 294);
            this.radioButton_ROP_FormulationOption3.Name = "radioButton_ROP_FormulationOption3";
            this.radioButton_ROP_FormulationOption3.Size = new System.Drawing.Size(474, 17);
            this.radioButton_ROP_FormulationOption3.TabIndex = 48;
            this.radioButton_ROP_FormulationOption3.TabStop = true;
            this.radioButton_ROP_FormulationOption3.Text = "Optimisation of a Loss Function for a given satisfaction probability and distribu" +
                "tional assumptions";
            this.radioButton_ROP_FormulationOption3.UseVisualStyleBackColor = true;
            this.radioButton_ROP_FormulationOption3.CheckedChanged += new System.EventHandler(this.radioButton_ROP_FormulationOption3_CheckedChanged);
            // 
            // label_ROP_FormulationOption3K
            // 
            this.label_ROP_FormulationOption3K.AutoSize = true;
            this.label_ROP_FormulationOption3K.Enabled = false;
            this.label_ROP_FormulationOption3K.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_ROP_FormulationOption3K.Location = new System.Drawing.Point(268, 334);
            this.label_ROP_FormulationOption3K.Name = "label_ROP_FormulationOption3K";
            this.label_ROP_FormulationOption3K.Size = new System.Drawing.Size(18, 20);
            this.label_ROP_FormulationOption3K.TabIndex = 53;
            this.label_ROP_FormulationOption3K.Text = "k";
            // 
            // label_ROP_FormulationOption3Variance
            // 
            this.label_ROP_FormulationOption3Variance.AutoSize = true;
            this.label_ROP_FormulationOption3Variance.Enabled = false;
            this.label_ROP_FormulationOption3Variance.Font = new System.Drawing.Font("Symbol", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(2)));
            this.label_ROP_FormulationOption3Variance.Location = new System.Drawing.Point(302, 334);
            this.label_ROP_FormulationOption3Variance.Name = "label_ROP_FormulationOption3Variance";
            this.label_ROP_FormulationOption3Variance.Size = new System.Drawing.Size(19, 20);
            this.label_ROP_FormulationOption3Variance.TabIndex = 52;
            this.label_ROP_FormulationOption3Variance.Text = "u";
            // 
            // comboBox_ROP_FormulationOption3Sign
            // 
            this.comboBox_ROP_FormulationOption3Sign.Enabled = false;
            this.comboBox_ROP_FormulationOption3Sign.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboBox_ROP_FormulationOption3Sign.FormattingEnabled = true;
            this.comboBox_ROP_FormulationOption3Sign.Items.AddRange(new object[] {
            "+",
            "-"});
            this.comboBox_ROP_FormulationOption3Sign.Location = new System.Drawing.Point(209, 331);
            this.comboBox_ROP_FormulationOption3Sign.Name = "comboBox_ROP_FormulationOption3Sign";
            this.comboBox_ROP_FormulationOption3Sign.Size = new System.Drawing.Size(35, 28);
            this.comboBox_ROP_FormulationOption3Sign.TabIndex = 51;
            this.comboBox_ROP_FormulationOption3Sign.Text = "+";
            // 
            // label_ROP_FormulationOption3Mean
            // 
            this.label_ROP_FormulationOption3Mean.AutoSize = true;
            this.label_ROP_FormulationOption3Mean.Enabled = false;
            this.label_ROP_FormulationOption3Mean.Font = new System.Drawing.Font("Symbol", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(2)));
            this.label_ROP_FormulationOption3Mean.Location = new System.Drawing.Point(170, 334);
            this.label_ROP_FormulationOption3Mean.Name = "label_ROP_FormulationOption3Mean";
            this.label_ROP_FormulationOption3Mean.Size = new System.Drawing.Size(19, 20);
            this.label_ROP_FormulationOption3Mean.TabIndex = 50;
            this.label_ROP_FormulationOption3Mean.Text = "m";
            // 
            // label_ROP_FormulationOption3
            // 
            this.label_ROP_FormulationOption3.AutoSize = true;
            this.label_ROP_FormulationOption3.Enabled = false;
            this.label_ROP_FormulationOption3.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_ROP_FormulationOption3.Location = new System.Drawing.Point(103, 334);
            this.label_ROP_FormulationOption3.Name = "label_ROP_FormulationOption3";
            this.label_ROP_FormulationOption3.Size = new System.Drawing.Size(53, 20);
            this.label_ROP_FormulationOption3.TabIndex = 49;
            this.label_ROP_FormulationOption3.Text = "G   = ";
            // 
            // textBox_ROP_FormulationOption3_LimitValue
            // 
            this.textBox_ROP_FormulationOption3_LimitValue.Enabled = false;
            this.textBox_ROP_FormulationOption3_LimitValue.Location = new System.Drawing.Point(398, 336);
            this.textBox_ROP_FormulationOption3_LimitValue.Name = "textBox_ROP_FormulationOption3_LimitValue";
            this.textBox_ROP_FormulationOption3_LimitValue.Size = new System.Drawing.Size(68, 20);
            this.textBox_ROP_FormulationOption3_LimitValue.TabIndex = 55;
            // 
            // comboBox_ROP_FormulationOption3_InequalitySign
            // 
            this.comboBox_ROP_FormulationOption3_InequalitySign.Enabled = false;
            this.comboBox_ROP_FormulationOption3_InequalitySign.FormattingEnabled = true;
            this.comboBox_ROP_FormulationOption3_InequalitySign.Items.AddRange(new object[] {
            "<=",
            ">="});
            this.comboBox_ROP_FormulationOption3_InequalitySign.Location = new System.Drawing.Point(336, 336);
            this.comboBox_ROP_FormulationOption3_InequalitySign.Name = "comboBox_ROP_FormulationOption3_InequalitySign";
            this.comboBox_ROP_FormulationOption3_InequalitySign.Size = new System.Drawing.Size(44, 21);
            this.comboBox_ROP_FormulationOption3_InequalitySign.TabIndex = 54;
            this.comboBox_ROP_FormulationOption3_InequalitySign.Text = "<=";
            // 
            // label_ROP_FormulationOption3LossFunc_with
            // 
            this.label_ROP_FormulationOption3LossFunc_with.AutoSize = true;
            this.label_ROP_FormulationOption3LossFunc_with.Enabled = false;
            this.label_ROP_FormulationOption3LossFunc_with.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_ROP_FormulationOption3LossFunc_with.Location = new System.Drawing.Point(81, 402);
            this.label_ROP_FormulationOption3LossFunc_with.Name = "label_ROP_FormulationOption3LossFunc_with";
            this.label_ROP_FormulationOption3LossFunc_with.Size = new System.Drawing.Size(29, 13);
            this.label_ROP_FormulationOption3LossFunc_with.TabIndex = 56;
            this.label_ROP_FormulationOption3LossFunc_with.Text = "with:";
            // 
            // label_ROP_FormulationOption2_constraint_Limit
            // 
            this.label_ROP_FormulationOption2_constraint_Limit.AutoSize = true;
            this.label_ROP_FormulationOption2_constraint_Limit.Enabled = false;
            this.label_ROP_FormulationOption2_constraint_Limit.Location = new System.Drawing.Point(414, 203);
            this.label_ROP_FormulationOption2_constraint_Limit.Name = "label_ROP_FormulationOption2_constraint_Limit";
            this.label_ROP_FormulationOption2_constraint_Limit.Size = new System.Drawing.Size(28, 13);
            this.label_ROP_FormulationOption2_constraint_Limit.TabIndex = 57;
            this.label_ROP_FormulationOption2_constraint_Limit.Text = "Limit";
            // 
            // label_ROP_FormulationOption1_constraint_Limit
            // 
            this.label_ROP_FormulationOption1_constraint_Limit.AutoSize = true;
            this.label_ROP_FormulationOption1_constraint_Limit.Location = new System.Drawing.Point(392, 72);
            this.label_ROP_FormulationOption1_constraint_Limit.Name = "label_ROP_FormulationOption1_constraint_Limit";
            this.label_ROP_FormulationOption1_constraint_Limit.Size = new System.Drawing.Size(28, 13);
            this.label_ROP_FormulationOption1_constraint_Limit.TabIndex = 58;
            this.label_ROP_FormulationOption1_constraint_Limit.Text = "Limit";
            // 
            // button_ROP_objective_cancel
            // 
            this.button_ROP_objective_cancel.Location = new System.Drawing.Point(305, 467);
            this.button_ROP_objective_cancel.Name = "button_ROP_objective_cancel";
            this.button_ROP_objective_cancel.Size = new System.Drawing.Size(75, 23);
            this.button_ROP_objective_cancel.TabIndex = 60;
            this.button_ROP_objective_cancel.Text = "Cancel";
            this.button_ROP_objective_cancel.UseVisualStyleBackColor = true;
            this.button_ROP_objective_cancel.Click += new System.EventHandler(this.button_ROP_objective_cancel_Click);
            // 
            // ROP_ConstraintSetup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(583, 502);
            this.Controls.Add(this.textBox_ROP_FormulationOption2_LimitValue);
            this.Controls.Add(this.button_ROP_objective_cancel);
            this.Controls.Add(this.label_ROP_FormulationOption2_constraint_Limit);
            this.Controls.Add(this.label_ROP_FormulationOption3LossFunc_with);
            this.Controls.Add(this.textBox_ROP_FormulationOption3_LimitValue);
            this.Controls.Add(this.comboBox_ROP_FormulationOption3_InequalitySign);
            this.Controls.Add(this.label_ROP_FormulationOption3K);
            this.Controls.Add(this.label_ROP_FormulationOption3Variance);
            this.Controls.Add(this.comboBox_ROP_FormulationOption3Sign);
            this.Controls.Add(this.label_ROP_FormulationOption3Mean);
            this.Controls.Add(this.label_ROP_FormulationOption3);
            this.Controls.Add(this.radioButton_ROP_FormulationOption3);
            this.Controls.Add(this.comboBox_ROP_FormulationOption2_InequalitySign);
            this.Controls.Add(this.textBox_ROP_FormulationOption2K);
            this.Controls.Add(this.label_ROP_FormulationOption2Variance);
            this.Controls.Add(this.comboBox_ROP_FormulationOption2Sign);
            this.Controls.Add(this.label_ROP_FormulationOption2Mean);
            this.Controls.Add(this.label_ROP_FormulationOption2);
            this.Controls.Add(this.radioButton_ROP_FormulationOption2);
            this.Controls.Add(this.textBox_ROP_FormulationOption1_LimitValue);
            this.Controls.Add(this.comboBox_ROP_FormulationOption1_InequalitySign);
            this.Controls.Add(this.comboBox_ROP_FormulationOption1);
            this.Controls.Add(this.label_ROP_FormulationOption1);
            this.Controls.Add(this.radioButton_ROP_FormulationOption1);
            this.Controls.Add(this.label_ROP_TitleCnstrFormulation);
            this.Controls.Add(this.textBox_ROP_constraint_Prb);
            this.Controls.Add(this.label_ROP_constraint_Prb_subscript);
            this.Controls.Add(this.label_ROP_FormulationOption3_constraint_Limit);
            this.Controls.Add(this.comboBox_ROP_constraint_Metric);
            this.Controls.Add(this.comboBox_ROP_constraint_DstrbnAssmptn);
            this.Controls.Add(this.button_ROP_constraint_set);
            this.Controls.Add(this.label_ROP_constraint_Name);
            this.Controls.Add(this.label_ROP_constraint_Metric);
            this.Controls.Add(this.label_ROP_constraint_DstrbnAssmptn);
            this.Controls.Add(this.label_ROP_constraint_Prb);
            this.Controls.Add(this.label_ROP_FormulationOption1_constraint_Limit);
            this.Name = "ROP_ConstraintSetup";
            this.Text = "ROP_ConstraintSetup";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox comboBox_ROP_constraint_Metric;
        private System.Windows.Forms.ComboBox comboBox_ROP_constraint_DstrbnAssmptn;
        private System.Windows.Forms.TextBox textBox_ROP_constraint_Prb;
        private System.Windows.Forms.Button button_ROP_constraint_set;
        private System.Windows.Forms.Label label_ROP_constraint_Name;
        private System.Windows.Forms.Label label_ROP_constraint_Metric;
        private System.Windows.Forms.Label label_ROP_constraint_DstrbnAssmptn;
        private System.Windows.Forms.Label label_ROP_constraint_Prb;
        private System.Windows.Forms.Label label_ROP_FormulationOption3_constraint_Limit;
        private System.Windows.Forms.Label label_ROP_constraint_Prb_subscript;
        private System.Windows.Forms.Label label_ROP_TitleCnstrFormulation;
        private System.Windows.Forms.RadioButton radioButton_ROP_FormulationOption1;
        private System.Windows.Forms.ComboBox comboBox_ROP_FormulationOption1;
        private System.Windows.Forms.Label label_ROP_FormulationOption1;
        private System.Windows.Forms.TextBox textBox_ROP_FormulationOption1_LimitValue;
        private System.Windows.Forms.ComboBox comboBox_ROP_FormulationOption1_InequalitySign;
        private System.Windows.Forms.RadioButton radioButton_ROP_FormulationOption2;
        private System.Windows.Forms.TextBox textBox_ROP_FormulationOption2K;
        private System.Windows.Forms.Label label_ROP_FormulationOption2Variance;
        private System.Windows.Forms.ComboBox comboBox_ROP_FormulationOption2Sign;
        private System.Windows.Forms.Label label_ROP_FormulationOption2Mean;
        private System.Windows.Forms.Label label_ROP_FormulationOption2;
        private System.Windows.Forms.TextBox textBox_ROP_FormulationOption2_LimitValue;
        private System.Windows.Forms.ComboBox comboBox_ROP_FormulationOption2_InequalitySign;
        private System.Windows.Forms.RadioButton radioButton_ROP_FormulationOption3;
        private System.Windows.Forms.Label label_ROP_FormulationOption3K;
        private System.Windows.Forms.Label label_ROP_FormulationOption3Variance;
        private System.Windows.Forms.ComboBox comboBox_ROP_FormulationOption3Sign;
        private System.Windows.Forms.Label label_ROP_FormulationOption3Mean;
        private System.Windows.Forms.Label label_ROP_FormulationOption3;
        private System.Windows.Forms.TextBox textBox_ROP_FormulationOption3_LimitValue;
        private System.Windows.Forms.ComboBox comboBox_ROP_FormulationOption3_InequalitySign;
        private System.Windows.Forms.Label label_ROP_FormulationOption3LossFunc_with;
        private System.Windows.Forms.Label label_ROP_FormulationOption2_constraint_Limit;
        private System.Windows.Forms.Label label_ROP_FormulationOption1_constraint_Limit;
        private System.Windows.Forms.Button button_ROP_objective_cancel;
    }
}