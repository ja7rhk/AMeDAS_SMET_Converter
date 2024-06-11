using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AMeDAS_SMET_Converter
{
    public partial class Form2 : Form
    {

        public Form1.AMeDAS_Loc ReturnValue;  // returm data from Form2

        public Form2()
        {
            InitializeComponent();
        }

        //private string[] loc;
        private Form1.AMeDAS_Loc loc = new Form1.AMeDAS_Loc();

        private const MidpointRounding away = MidpointRounding.AwayFromZero;

        private void komanoyu_CheckedChanged(object sender, EventArgs e)
        {
            // <<駒ノ湯>> 北緯：  38 度 54.8 分　東経：  140 度 49.7 分　標高： 525 m
            textBox1.Text = "駒ノ湯";
            loc.station_id = "AMeDAS_34012";
            loc.station_name = "KOMANOYU";
            textBox2.Text = Convert.ToString(Math.Round((38 + (54.8 / 60)), 3, away));
            textBox3.Text = Convert.ToString(Math.Round((140 + (49.7 / 60)), 3, away));
            textBox4.Text = Convert.ToString(525);
        }

        private void sukayu_CheckedChanged(object sender, EventArgs e)
        {
            // <<酸ヶ湯>> 北緯：  40 度 38.9 分　東経：  140 度 50.9 分　標高： 890 m
            textBox1.Text = "酸ヶ湯";
            loc.station_id = "AMeDAS_31482";
            loc.station_name = "SUKAYU";
            textBox2.Text = Convert.ToString(Math.Round((40 + (38.9 / 60)), 3, away));
            textBox3.Text = Convert.ToString(Math.Round((140 + (50.9 / 60)), 3, away));
            textBox4.Text = Convert.ToString(890);
        }

        private void aniai_CheckedChanged(object sender, EventArgs e)
        {
            // <<阿仁合>> 北緯：  39 度 59.6 分　東経：  140 度 24.2 分　標高： 120 m 
            textBox1.Text = "阿仁合";
            loc.station_id = "AMeDAS_32311";
            loc.station_name = "ANIAI";
            textBox2.Text = Convert.ToString(Math.Round((39 + (59.6 / 60)), 3, away));
            textBox3.Text = Convert.ToString(Math.Round((140 + (24.2 / 60)), 3, away));
            textBox4.Text = Convert.ToString(120);
        }

        private void matsuo_CheckedChanged(object sender, EventArgs e)
        {
            // <<岩手松尾>> 北緯：  39 度 57.1 分　東経：  141 度 3.9 分　標高： 275 m 
            textBox1.Text = "岩手松尾";
            loc.station_id = "AMeDAS_33226";
            loc.station_name = "IWATEMATSUO";
            textBox2.Text = Convert.ToString(Math.Round((39 + (57.1 / 60)), 3, away));
            textBox3.Text = Convert.ToString(Math.Round((141 + (3.9 / 60)), 3, away));
            textBox4.Text = Convert.ToString(275);
        }

        private void hakuba_CheckedChanged(object sender, EventArgs e)
        {
            // <<白馬>> 北緯：  36 度 41.9 分　東経：  137 度 51.7 分　標高： 703 m 
            textBox1.Text = "白馬";
            loc.station_id = "AMeDAS_48141";
            loc.station_name = "HAKUBA";
            textBox2.Text = Convert.ToString(Math.Round((36 + (41.9 / 60)), 3, away));
            textBox3.Text = Convert.ToString(Math.Round((137 + (51.7 / 60)), 3, away));
            textBox4.Text = Convert.ToString(703);
        }

        private void hijiori_CheckedChanged(object sender, EventArgs e)
        {
            // <<肘折>> 北緯：  38 度 36.4 分　東経：  140 度 9.8 分　標高： 330 m
            textBox1.Text = "肘折";
            loc.station_id = "AMeDAS_35216";
            loc.station_name = "HIJIORI";
            textBox2.Text = Convert.ToString(Math.Round((38 + (36.4 / 60)), 3, away));
            textBox3.Text = Convert.ToString(Math.Round((140 + (9.8 / 60)), 3, away));
            textBox4.Text = Convert.ToString(330);
        }

        private void ooisawa_CheckedChanged(object sender, EventArgs e)
        {
            // <<大井沢>> 北緯：  38 度 23.4 分　東経：  139 度 59.6 分　標高： 440 m
            textBox1.Text = "大井沢";
            loc.station_id = "AMeDAS_35361";
            loc.station_name = "OOISAWA";
            textBox2.Text = Convert.ToString(Math.Round((38 + (23.4 / 60)), 3, away));
            textBox3.Text = Convert.ToString(Math.Round((139 + (59.6 / 60)), 3, away));
            textBox4.Text = Convert.ToString(440);
        }

        private void minakami_CheckedChanged(object sender, EventArgs e)
        {
            // <<水上>> 北緯：  36 度 48.0 分　東経：  138 度 59.5 分　標高： 531 m
            textBox1.Text = "水上";
            loc.station_id = "AMeDAS_42091";
            loc.station_name = "MINAKAMI";
            textBox2.Text = Convert.ToString(Math.Round((36 + (48.0 / 60)), 3, away));
            textBox3.Text = Convert.ToString(Math.Round((138 + (59.5 / 60)), 3, away));
            textBox4.Text = Convert.ToString(531);
        }

        private void sendai_CheckedChanged(object sender, EventArgs e)
        {
            // <<仙台水上>> 北緯: 38 度 15.7 分 東経: 140 度 53.8 分 標高: 39m
            textBox1.Text = "仙台";
            loc.station_id = "AMeDAS_34392";
            loc.station_name = "SENDAI";
            textBox2.Text = Convert.ToString(Math.Round((38 + (15.7 / 60)), 3, away));
            textBox3.Text = Convert.ToString(Math.Round((140 + (53.8 / 60)), 3, away));
            textBox4.Text = Convert.ToString(39);
        }

    private void button1_Click(object sender, EventArgs e)
        {
            loc.station = textBox1.Text;
            loc.latitude = Convert.ToDouble(textBox2.Text);
            loc.longitude = Convert.ToDouble(textBox3.Text);
            loc.altitude = Convert.ToDouble(textBox4.Text);

            this.ReturnValue = loc;
            this.Close();
        }

        static public Form1.AMeDAS_Loc ShowMapForm()
        {
            Form2 f = new Form2();
            f.ShowDialog();
            f.Dispose();

            return f.ReturnValue;
        }

    }
}
