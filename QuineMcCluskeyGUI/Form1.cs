using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using QuineMcCluskey;

namespace QuineMcCluskeyGUI
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void btnRun_Click(object sender, EventArgs e)
        {
            try
            {
                int numVariables = int.Parse(txtNumVariables.Text);
                string[] dontCareStr = txtDontCares.Text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                HashSet<int> dontCares = new HashSet<int>(dontCareStr.Select(int.Parse));

                IInputStrategy strategy;
                if (rbMinterm.Checked)
                    strategy = new GuiMintermInputStrategy(txtTerms.Text);
                else
                    strategy = new GuiMaxtermInputStrategy(txtTerms.Text, numVariables);

                IBooleanFunctionMinimizer minimizer = MinimizerFactory.CreateMinimizer(numVariables, strategy, dontCares);

                var sop = minimizer.MinimizeSOP();
                string pos = minimizer.MinimizePOS();

                txtResultSOP.Text = sop.Any() ? string.Join(" + ", sop) : "0";
                txtResultPOS.Text = string.IsNullOrEmpty(pos) ? "1" : pos;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message);
            }
        }
    }

    public class GuiMintermInputStrategy : IInputStrategy
    {
        private readonly string _input;

        public GuiMintermInputStrategy(string input)
        {
            _input = input;
        }

        public HashSet<int> GetMinterms(int numVariables, HashSet<int> dontCares)
        {
            var terms = _input.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var minterms = new HashSet<int>(terms.Select(int.Parse));
            Validate(numVariables, minterms);
            return minterms;
        }

        private void Validate(int numVariables, HashSet<int> terms)
        {
            int max = (1 << numVariables) - 1;
            foreach (var t in terms)
            {
                if (t < 0 || t > max)
                    throw new ArgumentException($"Giá trị {t} không hợp lệ.");
            }
        }
    }

    public class GuiMaxtermInputStrategy : IInputStrategy
    {
        private readonly string _input;
        private readonly int _numVariables;

        public GuiMaxtermInputStrategy(string input, int numVariables)
        {
            _input = input;
            _numVariables = numVariables;
        }

        public HashSet<int> GetMinterms(int numVariables, HashSet<int> dontCares)
        {
            var terms = _input.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var maxterms = new HashSet<int>(terms.Select(int.Parse));
            Validate(numVariables, maxterms);
            var minimizer = new QuineMcCluskeyMinimizer(numVariables, null, dontCares);
            return minimizer.GetMintermsFromMaxterms(maxterms);
        }

        private void Validate(int numVariables, HashSet<int> terms)
        {
            int max = (1 << numVariables) - 1;
            foreach (var t in terms)
            {
                if (t < 0 || t > max)
                    throw new ArgumentException($"Giá trị {t} không hợp lệ.");
            }
        }
    }
}
