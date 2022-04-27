using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SentimentModel_ConsoleApp2
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            //int n = Int32.Parse(textBox1.Text);
            if (Int32.Parse(textBox1.Text) == -0)
            {
                textBox1.Text = "0";
            }
            if (Int32.Parse(textBox1.Text) > 100)
            {
                textBox1.Text = "100";
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            String[] sa = new string[2];
            sa.SetValue(textBox2.Text, 0);
            sa.SetValue(textBox1.Text, 1);
            //Program.Main(sa, Int32.Parse(textBox1.Text));
        }

        private void label3_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() != DialogResult.Cancel)
            {
                textBox2.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            textBox2.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
        }
    }
}
