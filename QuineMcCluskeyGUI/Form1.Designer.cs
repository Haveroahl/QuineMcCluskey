namespace QuineMcCluskeyGUI
{
    partial class MainForm
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

        private void InitializeComponent()
        {
            this.lblNumVariables = new System.Windows.Forms.Label();
            this.txtNumVariables = new System.Windows.Forms.TextBox();
            this.rbMinterm = new System.Windows.Forms.RadioButton();
            this.rbMaxterm = new System.Windows.Forms.RadioButton();
            this.lblTerms = new System.Windows.Forms.Label();
            this.txtTerms = new System.Windows.Forms.TextBox();
            this.lblDontCares = new System.Windows.Forms.Label();
            this.txtDontCares = new System.Windows.Forms.TextBox();
            this.btnRun = new System.Windows.Forms.Button();
            this.lblResultSOP = new System.Windows.Forms.Label();
            this.txtResultSOP = new System.Windows.Forms.TextBox();
            this.lblResultPOS = new System.Windows.Forms.Label();
            this.txtResultPOS = new System.Windows.Forms.TextBox();
            this.SuspendLayout();

            // lblNumVariables
            this.lblNumVariables.AutoSize = true;
            this.lblNumVariables.Location = new System.Drawing.Point(20, 20);
            this.lblNumVariables.Name = "lblNumVariables";
            this.lblNumVariables.Size = new System.Drawing.Size(112, 13);
            this.lblNumVariables.TabIndex = 0;
            this.lblNumVariables.Text = "Số lượng biến (1 - 31):";

            // txtNumVariables
            this.txtNumVariables.Location = new System.Drawing.Point(150, 17);
            this.txtNumVariables.Name = "txtNumVariables";
            this.txtNumVariables.Size = new System.Drawing.Size(100, 20);
            this.txtNumVariables.TabIndex = 1;

            // rbMinterm
            this.rbMinterm.AutoSize = true;
            this.rbMinterm.Location = new System.Drawing.Point(20, 50);
            this.rbMinterm.Name = "rbMinterm";
            this.rbMinterm.Size = new System.Drawing.Size(63, 17);
            this.rbMinterm.TabIndex = 2;
            this.rbMinterm.TabStop = true;
            this.rbMinterm.Text = "Minterm";
            this.rbMinterm.UseVisualStyleBackColor = true;

            // rbMaxterm
            this.rbMaxterm.AutoSize = true;
            this.rbMaxterm.Location = new System.Drawing.Point(100, 50);
            this.rbMaxterm.Name = "rbMaxterm";
            this.rbMaxterm.Size = new System.Drawing.Size(65, 17);
            this.rbMaxterm.TabIndex = 3;
            this.rbMaxterm.TabStop = true;
            this.rbMaxterm.Text = "Maxterm";
            this.rbMaxterm.UseVisualStyleBackColor = true;

            // lblTerms
            this.lblTerms.AutoSize = true;
            this.lblTerms.Location = new System.Drawing.Point(20, 80);
            this.lblTerms.Name = "lblTerms";
            this.lblTerms.Size = new System.Drawing.Size(115, 13);
            this.lblTerms.TabIndex = 4;
            this.lblTerms.Text = "Nhập Minterm/Maxterm:";

            // txtTerms
            this.txtTerms.Location = new System.Drawing.Point(20, 100);
            this.txtTerms.Name = "txtTerms";
            this.txtTerms.Size = new System.Drawing.Size(300, 20);
            this.txtTerms.TabIndex = 5;

            // lblDontCares
            this.lblDontCares.AutoSize = true;
            this.lblDontCares.Location = new System.Drawing.Point(20, 130);
            this.lblDontCares.Name = "lblDontCares";
            this.lblDontCares.Size = new System.Drawing.Size(104, 13);
            this.lblDontCares.TabIndex = 6;
            this.lblDontCares.Text = "Nhập Don't Cares (tùy):";

            // txtDontCares
            this.txtDontCares.Location = new System.Drawing.Point(20, 150);
            this.txtDontCares.Name = "txtDontCares";
            this.txtDontCares.Size = new System.Drawing.Size(300, 20);
            this.txtDontCares.TabIndex = 7;

            // btnRun
            this.btnRun.Location = new System.Drawing.Point(20, 185);
            this.btnRun.Name = "btnRun";
            this.btnRun.Size = new System.Drawing.Size(100, 30);
            this.btnRun.TabIndex = 8;
            this.btnRun.Text = "Tối giản";
            this.btnRun.UseVisualStyleBackColor = true;
            this.btnRun.Click += new System.EventHandler(this.btnRun_Click);

            // lblResultSOP
            this.lblResultSOP.AutoSize = true;
            this.lblResultSOP.Location = new System.Drawing.Point(20, 230);
            this.lblResultSOP.Name = "lblResultSOP";
            this.lblResultSOP.Size = new System.Drawing.Size(86, 13);
            this.lblResultSOP.TabIndex = 9;
            this.lblResultSOP.Text = "Kết quả SOP (Y):";

            // txtResultSOP
            this.txtResultSOP.Location = new System.Drawing.Point(20, 250);
            this.txtResultSOP.Multiline = true;
            this.txtResultSOP.Name = "txtResultSOP";
            this.txtResultSOP.ReadOnly = true;
            this.txtResultSOP.Size = new System.Drawing.Size(400, 50);
            this.txtResultSOP.TabIndex = 10;

            // lblResultPOS
            this.lblResultPOS.AutoSize = true;
            this.lblResultPOS.Location = new System.Drawing.Point(20, 310);
            this.lblResultPOS.Name = "lblResultPOS";
            this.lblResultPOS.Size = new System.Drawing.Size(86, 13);
            this.lblResultPOS.TabIndex = 11;
            this.lblResultPOS.Text = "Kết quả POS (Y):";

            // txtResultPOS
            this.txtResultPOS.Location = new System.Drawing.Point(20, 330);
            this.txtResultPOS.Multiline = true;
            this.txtResultPOS.Name = "txtResultPOS";
            this.txtResultPOS.ReadOnly = true;
            this.txtResultPOS.Size = new System.Drawing.Size(400, 50);
            this.txtResultPOS.TabIndex = 12;

            // MainForm
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(450, 400);
            this.Controls.Add(this.txtResultPOS);
            this.Controls.Add(this.lblResultPOS);
            this.Controls.Add(this.txtResultSOP);
            this.Controls.Add(this.lblResultSOP);
            this.Controls.Add(this.btnRun);
            this.Controls.Add(this.txtDontCares);
            this.Controls.Add(this.lblDontCares);
            this.Controls.Add(this.txtTerms);
            this.Controls.Add(this.lblTerms);
            this.Controls.Add(this.rbMaxterm);
            this.Controls.Add(this.rbMinterm);
            this.Controls.Add(this.txtNumVariables);
            this.Controls.Add(this.lblNumVariables);
            this.Name = "MainForm";
            this.Text = "Quine-McCluskey Minimizer";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblNumVariables;
        private System.Windows.Forms.TextBox txtNumVariables;
        private System.Windows.Forms.RadioButton rbMinterm;
        private System.Windows.Forms.RadioButton rbMaxterm;
        private System.Windows.Forms.Label lblTerms;
        private System.Windows.Forms.TextBox txtTerms;
        private System.Windows.Forms.Label lblDontCares;
        private System.Windows.Forms.TextBox txtDontCares;
        private System.Windows.Forms.Button btnRun;
        private System.Windows.Forms.Label lblResultSOP;
        private System.Windows.Forms.TextBox txtResultSOP;
        private System.Windows.Forms.Label lblResultPOS;
        private System.Windows.Forms.TextBox txtResultPOS;
    }
}
