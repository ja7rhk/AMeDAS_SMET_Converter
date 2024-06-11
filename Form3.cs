using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;

namespace AMeDAS_SMET_Converter
{
    public partial class Form3 : Form
    {
        private string[] argumentValues;        // recieved parameter strings

        public Form3(params string[] argumentValues)
        {
            this.argumentValues = argumentValues;

            InitializeComponent();

            // ProductName
            label1.Text = Application.ProductName;
            // ProductVersion
            label3.Text = Application.ProductVersion;

            // Copyright
            AssemblyCopyrightAttribute asmcpy =
                (AssemblyCopyrightAttribute)
                Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(AssemblyCopyrightAttribute));
            label4.Text = asmcpy.Copyright.ToString();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

    }
}
