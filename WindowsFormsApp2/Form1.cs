using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CenterTaskbar
{
    public partial class Form1 : Form
    {

        private const int defaultThreshold = 1920;

        public Form1(int initialThreshold)
        {
            InitializeComponent();
            this.numericUpDown1.Value = this.numericUpDown1.Value = new decimal(new int[] {
            initialThreshold,
            0,
            0,
            0});
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void OK_Clicked(object sender, EventArgs e)
        {
            Properties.Settings.Default.bigIconThreshold = Decimal.ToInt32(this.numericUpDown1.Value);
            this.Close();
        }

        private void Cancel_Clicked(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Default_Clicked(object sender, EventArgs e)
        {
            this.numericUpDown1.Value = this.numericUpDown1.Value = new decimal(new int[] {
            defaultThreshold,
            0,
            0,
            0});
        }
    }
}
