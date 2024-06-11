using System;
using System.Windows.Forms;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualBasic.FileIO;     // to use "TextFieldParser"
using System.Text.RegularExpressions;
using System.Linq;

namespace AMeDAS_SMET_Converter
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public class AMeDASData
        {
            // fields = timestamp TA PSUM SnowFall HS Daylight VW DW RH
            // Auto-implemented properties.
            public DateTime Timestamp { get; set; }
            public double Temperature { get; set; }     // (C-deg)
            public double Precipitation { get; set; }
            public double Precipitation_Snow { get; set; }
            public double Height_Snow { get; set; }
            public double Daylight { get; set; }
            //public double Pressure { get; set; }        // (hPa) if it is availlble.
            public double Ext_Solar { get; set; }       // 大気外日射量(extraterrestrial solar radiation)(W/m^2).
            public double AirMass { get; set; }         // Air Mass = 1 / sin(α).
            public double ISWR { get; set; }            // calculated ISWR(W/m^2) using Daylight.
            public double Wind_Velocity { get; set; }
            public string Wind_Direction { get; set; }  // it shoud be converted to angle(deg).
            public double Humidity { get; set; }
            //public double Cos_ISWR { get; set; }      // hourly ISWR curve.
            public double Weight_ISWR { get; set; }     // 太陽高度hからsin(h)
            public double Pressure_0 { get; set; }      // sea level atmospheric pressure(hPa).
            public double Temperature_0 { get; set; }   // sea level temperature(C-deg)).
        }

        public class AMeDAS_Loc
        {
            // Auto-implemented properties.
            public string station { get; set; }
            public string station_id { get; set; }
            public string station_name { get; set; }
            public double latitude { get; set; }
            public double longitude { get; set; }
            public double altitude { get; set; }
        }

        private List<AMeDASData> data = new List<AMeDASData>();

        private AMeDAS_Loc location = new AMeDAS_Loc();

        private string default_filename = "";               // default input file name

        //---------------------------------------------
        //  観測点の選択ボタン
        //---------------------------------------------
        private void button_loc_Click(object sender, EventArgs e)
        {
            AMeDAS_Loc receiveText = Form2.ShowMapForm();

            label1.Text = receiveText.station;
            label4.Text = "北緯:" + Convert.ToString(receiveText.latitude)
                            + " 東経:" + Convert.ToString(receiveText.longitude)
                            + " 標高:" + Convert.ToString(receiveText.altitude) + "(m)";

            location = receiveText;
        }

        //---------------------------------------------
        //  相対湿度の欠測値に70%を挿入する
        //---------------------------------------------
        private bool RHeq70 = true;       // checkBox1 propaty : Checked = true

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                RHeq70 = true;
            }
            else
            {
                RHeq70 = false;
            }
        }

        //---------------------------------------------
        //  標高の換算をする
        //---------------------------------------------
        private bool altitude_val = false;       // checkBox1 propaty : Checked = falsee

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox3.Checked)
            {
                altitude_val = true;
            }
            else
            {
                altitude_val = false;
            }
        }


        //---------------------------------------------
        //  地表温度を換算をする
        //---------------------------------------------
        private bool TSGeq0 = true;       // checkBox1 propaty : Checked = true

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox4.Checked)
            {
                TSGeq0 = true;
            }
            else
            {
                TSGeq0 = false;
            }
        }


        //---------------------------------------------
        //  ファイル ⇒ 開く
        //---------------------------------------------
        private void ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            string openFileName;
            //string line;
            string[] ReadFields;
            string[][] header = new string[3][];
            // clear the previous data
            data.Clear();
            //sun.Clear();

            //DateTime day_start = dateTimePicker1.Value;
            //DateTime day_stop = dateTimePicker2.Value;
            //day_start = DateTime.Parse(day_start.ToString("yyyy'/'MM'/'dd' '00':'00"));
            //day_stop = DateTime.Parse(day_stop.ToString("yyyy'/'MM'/'dd' '00':'00"));

            //string year_1 = Convert.ToString(day_start.Year);
            //string year_2 = Convert.ToString(day_stop.Year);

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                openFileName = openFileDialog1.FileName;
            }
            else
            {
                return;
            }

            // save default file name
            Regex reg = new Regex(@".+\\(?<1>.+).csv");
            Match mf = reg.Match(openFileName);
            default_filename = mf.Groups[("1")].Value;

            //********************************
            // Read the selected CSV file
            //********************************
            // TimeStamp, TA(℃), PSUM(mm), PSUM(cm), HS(cm), Daylight(hr), VW(m/s), DW(deg), RH(%);

            const Int16 c_date = 0;
            Int16 c_temp = 0, c_psum = 0, c_psum_snow = 0, c_hs = 0, c_daylight = 0, c_vw = 0, c_dw = 0, c_rh = 0;
            Int16 c_pressure_0 = 0, c_temp_0 = 0;

            TextFieldParser parser =
                new TextFieldParser(openFileName, System.Text.Encoding.GetEncoding("Shift_JIS"));
            parser.TextFieldType = FieldType.Delimited;
            parser.SetDelimiters(",");

            while (!parser.EndOfData)
            {

                // read one line
                ReadFields = parser.ReadFields();

                //---------------------------------
                // Column Analysis
                //---------------------------------
                if (ReadFields[c_date] == "年月日時")
                {
                    header[0] = ReadFields;
                    header[1] = parser.ReadFields();    // read next line
                    header[2] = parser.ReadFields();    // read the one after the next line

                    for (Int16 col = 0; col < header[0].Length; col++)
                    {
                        switch (header[0][col])
                        {
                            case "気温(℃)":
                                if ((header[1][col] == "") && (header[2][col] == ""))
                                    c_temp = col;
                                break;
                            case "降水量(mm)":
                                if ((header[1][col] == "") && (header[2][col] == ""))
                                    c_psum = col;
                                break;
                            case "降雪(cm)":
                                if ((header[1][col] == "") && (header[2][col] == ""))
                                    c_psum_snow = col;
                                break;
                            case "積雪(cm)":
                                if ((header[1][col] == "") && (header[2][col] == ""))
                                    c_hs = col;
                                break;
                            case "日照時間(時間)":
                                if ((header[1][col] == "") && (header[2][col] == ""))
                                    c_daylight = col;
                                break;
                            case "風速(m/s)":
                                if ((header[1][col] == "") && (header[2][col] == ""))
                                    c_vw = col;
                                else if ((header[1][col] == "風向") && (header[2][col] == ""))
                                    c_dw = col;
                                break;
                            case "相対湿度(％)":
                                if ((header[1][col] == "") && (header[2][col] == ""))
                                    c_rh = col;
                                break;
                            case "海面気温(℃)":
                                if ((header[1][col] == "") && (header[2][col] == ""))
                                    c_temp_0 = col;
                                break;
                            case "海面気圧(hPa)":
                                if ((header[1][col] == "") && (header[2][col] == ""))
                                    c_pressure_0 = col;
                                break;
                            default:
                                // (nothing to do)
                                break;
                        }
                    }
                    // next line
                    continue;
                }

                //---------------------------------
                // Data Reading
                //---------------------------------
                // Does this line start with valid year ?
                //line = ReadFields[c_date];
                //if (line.IndexOf(year_1, 0) != 0 && line.IndexOf(year_2, 0) != 0)
                //{
                //    continue;
                //}

                // Does this line start with valid date ?
                DateTime dt;
                if (DateTime.TryParse(ReadFields[c_date], out dt) == false)
                {
                    continue;
                }

                AMeDASData s = new AMeDASData();
                s.Timestamp = DateTime.Parse(ReadFields[c_date]);
                //if ((s.Timestamp < day_start) || (s.Timestamp > day_stop))
                //{
                //    MessageBox.Show("開始日と最終日を確認して下さい。", "エラー");
                //    parser.Close();
                //    return;
                //}

                if ((c_temp != 0) && (ReadFields[c_temp] != ""))
                    s.Temperature = Convert.ToDouble(ReadFields[c_temp]);
                else
                    s.Temperature = -999;

                if ((c_psum != 0) && (ReadFields[c_psum] != ""))
                    s.Precipitation = Convert.ToDouble(ReadFields[c_psum]);
                else
                    s.Precipitation = -999;

                if ((c_psum_snow != 0) && (ReadFields[c_psum_snow] != ""))
                    s.Precipitation_Snow = Convert.ToDouble(ReadFields[c_psum_snow]);
                else
                    s.Precipitation_Snow = -999;

                if ((c_hs != 0) && (ReadFields[c_hs] != ""))
                    s.Height_Snow = Convert.ToDouble(ReadFields[c_hs]);
                else
                    s.Height_Snow = -999;

                if ((c_daylight != 0) && (ReadFields[c_daylight] != ""))
                    s.Daylight = Convert.ToDouble(ReadFields[c_daylight]);
                else
                    s.Daylight = -999;

                if ((c_vw != 0) && (ReadFields[c_vw] != ""))
                    s.Wind_Velocity = Convert.ToDouble(ReadFields[c_vw]);
                else
                    s.Wind_Velocity = -999;

                if ((c_dw != 0) && (ReadFields[c_dw] != ""))
                    s.Wind_Direction = ReadFields[c_dw];
                else
                    s.Wind_Direction = "///";

                if ((c_rh != 0) && (ReadFields[c_rh] != ""))
                    s.Humidity = Convert.ToDouble(ReadFields[c_rh]);
                else
                    s.Humidity = -999;

                if ((c_temp_0 != 0) && (ReadFields[c_temp_0] != ""))
                    s.Temperature_0 = Convert.ToDouble(ReadFields[c_temp_0]);
                else
                    s.Temperature_0 = -999;

                if ((c_pressure_0 != 0) && (ReadFields[c_pressure_0] != ""))
                    s.Pressure_0 = Convert.ToDouble(ReadFields[c_pressure_0]);
                else
                    s.Pressure_0 = -999;

                s.ISWR = -999;
                data.Add(s);

            }
            parser.Close();

            AMeDASData start_time = data.First();
            AMeDASData end_time = data.Last();

            start_day.Text = start_time.Timestamp.ToString();
            end_day.Text = end_time.Timestamp.ToString();

            //*************************************
            // Show the listed data to textBox1
            //*************************************
            textBox1.Clear();
            // make StringWriter instance.
            StringWriter ws = new StringWriter();
            // header
            //ws.WriteLine("年月日時, 気温(℃), 降水量(mm), 降雪(cm), 積雪(cm), 日照時間(hr), 風速(m/s), 風向, 相対湿度(％)");
            ws.WriteLine("年月日時, 気温(℃), 降水量(mm), 降雪(cm), 積雪(cm), 日照時間(hr), 日射量(W/m^2), 風速(m/s), 風向, 相対湿度(％)");
            // data
            for (Int16 i = 0; i < data.Count; i++)
            {
                ws.WriteLine(data[i].Timestamp.ToString() + ", "
                    + data[i].Temperature + ", "
                    + data[i].Precipitation + ", "
                    + data[i].Precipitation_Snow + ", "
                    + data[i].Height_Snow + ", "
                    + data[i].Daylight + ", "
                    + data[i].ISWR + ", "               // ISWR = -999
                    + data[i].Wind_Velocity + ", "
                    + data[i].Wind_Direction + ", "
                    + data[i].Humidity);
            }
            // write the current stream to the TextBox
            textBox1.Text = ws.ToString();

            ws.Close();
        }

        //---------------------------------------------
        //  ファイル ⇒ 保存
        //---------------------------------------------
        private void ToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            string saveFileName;

            saveFileDialog1.FileName = default_filename;
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                saveFileName = saveFileDialog1.FileName;
            }
            else
            {
                return;
            }

            Encoding encode;
            if (saveFileName.IndexOf(".txt", 0) >= 0)
                encode = Encoding.ASCII;
            else if (saveFileName.IndexOf(".csv", 0) >= 0)
                encode = Encoding.GetEncoding("Shift_JIS");
            else if (saveFileName.IndexOf(".smet", 0) >= 0)
                encode = Encoding.ASCII;
            else
                return;

            StreamWriter textFile =
                new StreamWriter(
                    new FileStream(saveFileName, FileMode.Create), encode
                );
            textFile.Write(textBox1.Text);
            textFile.Close();

            textBox1.AppendText("\r\n------------- saved -------------\r\n");
        }

        //---------------------------------------------
        //  ファイル ⇒ 終了
        //---------------------------------------------
        private void ToolStripMenuItem3_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }


        //---------------------------------------------
        //  データ変換 ⇒ 日照・日射量換算
        //---------------------------------------------
        private void EditToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            const MidpointRounding away = MidpointRounding.AwayFromZero;

            //********************************************
            // (1) 大気外日射量の推定
            //********************************************
            // Latitude (rad)
            if (location.station == null)
            {
                MessageBox.Show("観測点を設定して下さい。", "エラー");
                return;
            }
            double gamma = location.longitude * (Math.PI / 180);  // 東経γ(rad)
            double phi = location.latitude * (Math.PI / 180);     // 北緯φ(rad)

            for (Int16 i = 0; i < data.Count; i++)
            {
                //     <<太陽方位、高度、大気外日射量の計算>>
                // http://www.es.ris.ac.jp/~nakagawa/met_cal/solar.html
                //太陽から供給される日射は、地球大気系に有効な唯一のエネルギー源です。
                //任意の緯度φ、経度λの地点における任意の日時の太陽方位ψ、高度α、
                //大気外全天日射量Qは、計算で求めることができます。
                //先ず、次式
                //　　　θo=2π(dn-1)/365
                Int32 dn = data[i].Timestamp.DayOfYear;
                double theta0 = 2 * Math.PI * (dn - 1) / 365;

                //により元旦からの通し日数dnに基いて定めたθoを用いて、当該日の
                //太陽赤緯δ
                //　　　δ=0.006918-0.399912cos(θo)+0.070257sin(θo)-0.006758cos(2θo)+0.000907sin(2θo)-0.002697cos(3θo)+0.001480sin(3θo)
                double delta = 0.006918 - 0.399912 * Math.Cos(theta0) + 0.070257 * Math.Sin(theta0);
                delta += -0.006758 * Math.Cos(2 * theta0) + 0.000907 * Math.Sin(2 * theta0);
                delta += -0.002697 * Math.Cos(3 * theta0) + 0.001480 * Math.Sin(3 * theta0);
                //地心太陽距離r/r*、
                //　　　r/r*=1/{1.000110+0.034221cos(θo)+0.001280sin(θo)+0.000719cos(2θo)+0.000077sin(2θo)}^0.5
                double rstar_r = 1.000110 + 0.034221 * Math.Cos(theta0) + 0.001280 * Math.Sin(theta0);
                rstar_r += 0.000719 * Math.Cos(2 * theta0) + 0.000077 * Math.Sin(2 * theta0);
                rstar_r = Math.Sqrt(rstar_r);   // r*/r
                //均時差Eq
                //　　　Eq=0.000075+0.001868cos(θo)-0.032077sin(θo)-0.014615cos(2θo)-0.040849sin(2θo)
                double Eq = 0.000075 + 0.001868 * Math.Cos(theta0) - 0.032077 * Math.Sin(theta0);
                Eq += -0.014615 * Math.Cos(2 * theta0) - 0.040849 * Math.Sin(2 * theta0);
                //
                //日本標準時間JSTから、太陽の時角hを求めます。
                //　　　h=(JST-12)π/12+標準子午線からの経度差+均時差(Eq)
                double h = (data[i].Timestamp.Hour - 12) * Math.PI / 12;
                h += gamma - (135 * Math.PI / 180);
                h += Eq;
                //太陽赤緯δ、緯度φ、時角ｈの値が既知となったので、太陽方位ψ、高度αは、それぞれ、
                //　　　α=arcsin{sin(φ)sin(δ)+cos(φ)cos(δ)cos(h)}
                double alpha = Math.Asin(Math.Sin(phi) * Math.Sin(delta) + Math.Cos(phi) * Math.Cos(delta) * Math.Cos(h));
                //　　　ψ=arctan[cos(φ)cos(δ)sin(h)/{sin(φ)sin(α)-sin(δ)}]
                double psi = Math.Atan(Math.Cos(phi) * Math.Cos(delta) * Math.Sin(h) / (Math.Sin(phi) * Math.Sin(alpha) - Math.Sin(delta)));
                //として求めることができます。
                //最後に、大気外全天日射量Qを、
                //　　　Q=1367(r*/r)^(2)sin(α)
                double Q = 1367 * Math.Pow(rstar_r, 2) * Math.Sin(alpha);
                //により求めることができます。1367W/m^2は太陽定数です。
                if (Q < 0)
                    data[i].Ext_Solar = 0;
                else
                    data[i].Ext_Solar = Q;

                data[i].AirMass = 1 / Math.Sin(alpha);
            }

            //***************************
            // (2)ISWR(W/m^2)の計算
            //***************************

            double n = 0;      // 日照時間(0～1.0)
            double iswr_0 = 0;          // 大気外日射量(W/m^2)
            double iswr = 0;            // 時間毎の日射量(W/m^2)
            double hs = 0;              // 積雪深(cm)
            double m = 0;               // Air Mass
            double ps = 0;

            // genarate hourly weight and amount of it
            for (Int16 i = 0; i < data.Count; i++)
            {
                // clear ISWR 
                data[i].ISWR = 0;
                iswr_0 = data[i].Ext_Solar;
                n = data[i].Daylight;
                hs = data[i].Height_Snow;
                m = data[i].AirMass;
                ps = data[i].Precipitation;
#if false
                //簡易モデル(Mabuchi)

                if (clear_rate > 0 && clear_rate <= 1)
                {
                    iswr = iswr_0 * (0.241 + 0.428 * clear_rate);
                }
                else if (clear_rate == 0)
                {
                    iswr = iswr_0 * 0.041;
                }
                else
                {
                    //日照時間の範囲外、エラー
                    continue;
                }
#else
                //改良モデル(METPV-3)

                if (hs < 5)        //積雪時が5cm未満(積雪時のモデルを使用)またはデータ無(-999)
                {
                    if (n > 0)         //日照時
                    {
                        if (m < 4)
                        {
                            iswr = iswr_0 * (0.353 - 0.0189 * m + (0.441 - 0.0447 * m) * (n - 0.1));
                        }
                        else
                        {
                            iswr = iswr_0 * (0.277 + 0.263 * (n - 0.1));
                        }
                    }
                    else if (n == 0)      //不日照時
                    {
                        if (ps <= 0)    //無降水時またはデータ無(-999)
                        {
                            if (m < 3.5)
                            {
                                iswr = iswr_0 * (0.223 - 0.0155 * m);
                            }
                            else //(m >= 3.5)
                            {
                                iswr = iswr_0 * 0.169;
                            }
                        }
                        else
                            iswr = iswr_0 * (0.100 - 0.006 * m);
                    }
                    else
                    {
                        //日照時間の範囲外、エラー
                        continue;
                    }
                }
                else        //積雪時が5cm以上(積雪時のモデルを使用)
                {
                    if (n > 0)         //日照時
                    {
                        if (m < 4)
                        {
                            iswr = iswr_0 * (0.369 + (0.501 - 0.063 * m) * (n - 0.1));
                        }
                        else //(m >= 4)
                        {
                            iswr = iswr_0 * (0.369 + 0.25 * (n - 0.1));
                        }
                    }
                    else if (n == 0)      //不日照時
                    {
                        if (ps <= 0)    //無降水時またはデータ無(-999)
                        {
                            if (m < 4)
                            {
                                iswr = iswr_0 * (0.306 - 0.0132 * m);
                            }
                            else //(m >= 4)
                            {
                                iswr = iswr_0 * 0.253;
                            }
                        }
                        else if(ps > 0 && ps <= 1)
                        {
                            if (m < 2.5)
                            {
                                iswr = iswr_0 * (0.372 - 0.0772 * m);
                            }
                            else
                            {
                                iswr = iswr_0 * 0.179;
                            }
                        }
                        else
                        {
                            iswr = iswr_0 * 0.111;
                        }
                    }
                    else
                    {
                        //日照時間の範囲外、エラー
                        continue;
                    }

                }
                //9～15時に不日照の場合の補正
                if (n == 0)
                {
                    int hr_now = data[i].Timestamp.Hour;
                    if (hr_now >= 9 && hr_now <= 15)
                    {
                        //前後の1時間が共に日照ありの場合
                        if (data[i - 1].Daylight > 0 && data[i + 1].Daylight > 0)
                            iswr = 1.47 * iswr;
                    }
                }
#endif
                data[i].ISWR = Math.Round(iswr, 0, away);
            }

            //*************************************
            // (3) Show the listed data to textBox1
            //*************************************
            textBox1.Clear();

            // make StringWriter instance.
            StringWriter ws = new StringWriter();
            // header
            ws.WriteLine("年月日時, 気温(℃), 降水量(mm), 降雪(cm), 積雪(cm), 日照時間(hr), 大気外日射量(W/m^2), 日射量(W/m^2), 風速(m/s), 風向, 相対湿度(％)");
            // data
            for (Int16 i = 0; i < data.Count; i++)
            {
                ws.WriteLine(data[i].Timestamp.ToString() + ", "
                    + data[i].Temperature + ", "
                    + data[i].Precipitation + ", "
                    + data[i].Precipitation_Snow + ", "
                    + data[i].Height_Snow + ", "
                    + data[i].Daylight + ", "
                    + Math.Round(data[i].Ext_Solar, 0, away) + ", "
                    + data[i].ISWR + ", "
                    + data[i].Wind_Velocity + ", "
                    + data[i].Wind_Direction + ", "
                    + data[i].Humidity);
            }
            // write the current stream to the TextBox
            textBox1.Text = ws.ToString();

            ws.Close();
        }

        //---------------------------------------------
        //  データ変換 ⇒ CSV変換
        //---------------------------------------------
        private void EditToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            // make StringWriter instance.
            StringWriter ws = new StringWriter();
            // header
            ws.WriteLine("年月日時, 気温(℃), 降水量(mm), 降雪(cm), 積雪(cm), 日照時間(時間), 風速(m/s), 風向, 相対湿度(％)");
            // data
            for (Int16 i = 0; i < data.Count; i++)
            {
                ws.WriteLine(data[i].Timestamp.ToString() + ", "
                    + data[i].Temperature + ", "
                    + data[i].Precipitation + ", "
                    + data[i].Precipitation_Snow + ", "
                    + data[i].Height_Snow + ", "
                    + data[i].Daylight + ", "
                    + data[i].Wind_Velocity + ", "
                    + data[i].Wind_Direction + ", "
                    + data[i].Humidity);
            }

            // write the current stream to the TextBox
            textBox1.Text = ws.ToString();

            ws.Close();
        }

        //---------------------------------------------
        //  データ変換 ⇒ SMET変換
        //---------------------------------------------

        /// <summary>
        /// This function converts the string to the angle in degree, start with North(= 0 deg)
        /// </summary>
        /// <param name="sWD">srtring of wind direction.</param>
        /// <returns> angle(deg) of wind direvion start with North(= 0 deg)</returns>
        private double isWindDirection(string sWD)
        {
            double WD;

            if (String.Compare(sWD, "北") == 0)
                WD = 0;
            else if (String.Compare(sWD, "北北東") == 0)
                WD = 22.5 * 1;
            else if (String.Compare(sWD, "北東") == 0)
                WD = 22.5 * 2;
            else if (String.Compare(sWD, "東北東") == 0)
                WD = 22.5 * 3;
            else if (String.Compare(sWD, "東") == 0)
                WD = 22.5 * 4;
            else if (String.Compare(sWD, "東南東") == 0)
                WD = 22.5 * 5;
            else if (String.Compare(sWD, "南東") == 0)
                WD = 22.5 * 6;
            else if (String.Compare(sWD, "南南東") == 0)
                WD = 22.5 * 7;
            else if (String.Compare(sWD, "南") == 0)
                WD = 22.5 * 8;
            else if (String.Compare(sWD, "南南西") == 0)
                WD = 22.5 * 9;
            else if (String.Compare(sWD, "南西") == 0)
                WD = 22.5 * 10;
            else if (String.Compare(sWD, "西南西") == 0)
                WD = 22.5 * 11;
            else if (String.Compare(sWD, "西") == 0)
                WD = 22.5 * 12;
            else if (String.Compare(sWD, "西北西") == 0)
                WD = 22.5 * 13;
            else if (String.Compare(sWD, "北西") == 0)
                WD = 22.5 * 14;
            else if (String.Compare(sWD, "北北西") == 0)
                WD = 22.5 * 15;
            else
                WD = -999;

            return WD;
        }

        /// <summary>
        /// This function genarates Wet adiabatic lapse rate(K/100m)
        /// </summary>
        /// <param name="temp">temperature (K)</param>
        /// <param name="pressure">atmospheric pressure (hPa)</param>
        /// <returns>Wet adiabatic lapse rate(K/100m)</returns>
        private double isLapseRate(double temp, double pressure)
        {
            double T;       // temperature (K)
            double L;       // latent meat of evaporation of water (J/kg)
            double Es;      // saturated vapor pressure (hPa)
            double Pb;      // atmospheric pressure (hPa)
            double lapse_rate = 0.6;    // Wet adiabatic lapse rate(K/100m)

            T = temp;
            L = (596.73 - 0.601 * T) * 4.184 * 1000;
            Es = 6.1078 * Math.Pow(10, (7.5 * (T - 273.15)) / T);
            Pb = pressure;
            // temp value
            double c = (0.622 * L * Es) / (Pb * 287 * T);

            // Wet adiabatic lapse rate(K / 100m)
            lapse_rate = 0.976 * (1 + c) / (1 + (c * L * 0.622) / (1004 * T));

            return lapse_rate;
        }

        /// <summary>
        /// this function genarates the atmospheric pressure of the station
        /// based on sea level temperature and there atmospheric pressure.
        /// if the sea level data are not measured (values should be '-999'),
        /// this function returns default value.
        /// </summary>
        /// <param name="alt">altitude of the station (m)</param>
        /// <param name="temp_0">sea level temperature (K)</param>
        /// <param name="pressure_0">sea level atmospheric pressure (hPa)</param>
        /// <returns>atmospheric pressure of the station (hPa)</returns>
        private double isPressure(double alt, double temp_0, double pressure_0)
        {
            double T0 = 273.15;         // sea level temperature (K)
            double P0 = 1013.25;        // sea level pressure (hPa)
            double H;                   // altitude of the station (m)
            double pressure;

            if (temp_0 != -999)
                T0 = temp_0;
            if (pressure_0 != -999)
                P0 = pressure_0;
            H = alt;

            pressure = P0 * Math.Pow((1 - 0.0065 * H / T0), 5.257);

            return pressure;
        }


        private void EditToolStripMenuItem3_Click(object sender, EventArgs e)
        {
            // make StringWriter instance.
            StringWriter ws = new StringWriter();
            double temp, tsg, rh, vw, dw = 0, iswr, hs, psum;
            double pre_dw = 0;  // previous valid direction of wind
            double Pb;    // atmospheric pressure (hPa)
            double walr = 0.6;  // Wet adiabatic lapse rate(K/100m)
            const MidpointRounding away = MidpointRounding.AwayFromZero;
            double alt_estimated = location.altitude;
            // if altitude is changed, add it to the station ID.
            if (altitude_val == true)
            {
                try
                {
                    alt_estimated = Convert.ToDouble(textBox2.Text);
                }
                catch (Exception)
                {
                    MessageBox.Show("高度の換算をする標高を入力して下さい。", "エラー");
                    return;
                }
                location.station_id = location.station_id + "_c" + textBox2.Text;
                location.station_name = location.station_name + 
                                            " (+" + (alt_estimated - location.altitude) + "m)";
            }

            ////// header //////
            ws.WriteLine("SMET 1.1 ASCII");
            ws.WriteLine("[HEADER]");
            ws.WriteLine("station_id       = " + location.station_id);
            ws.WriteLine("station_name     = " + location.station_name);
            ws.WriteLine("latitude         = " + location.latitude);
            ws.WriteLine("longitude        = " + location.longitude);

            if (altitude_val == false)
                ws.WriteLine("altitude         = " + location.altitude);
            else
                ws.WriteLine("altitude         = " + alt_estimated);

            ws.WriteLine("nodata           = -999");
            ws.WriteLine("tz               = +9");
            ws.WriteLine("fields           = timestamp TA TSG RH VW DW ISWR HS PSUM");

            ////// data //////
            ws.WriteLine("[DATA]");
            // calculate ISWR and Cloud_cover

            for (Int16 i = 0; i < data.Count; i++)
            {
                if (data[i].Temperature != -999)
                {
                    temp = data[i].Temperature + 273.15;        // [K]

                    if (altitude_val == true)
                    {
                        // atmospheric pressure (hPa) at the estimated point
                        // isPrassure(double alt, double temp_0, double pressure_0)
                        Pb = isPressure
                                (alt_estimated                          // altitude to be estimated.
                                , (data[i].Temperature_0 + 273.15)      // sea level temperature.
                                , data[i].Pressure_0);                  // sea level pressure.

                        // Wet adiabatic lapse rate(K / 100m)
                        // isLapseRate(double temp, double pressure)
                        walr = isLapseRate(temp, Pb);
                        temp += (location.altitude - alt_estimated) * walr / 100;
                    }
                }
                else
                {
                    temp = -999;
                }

                if (TSGeq0 == true)
                    tsg = 273.15;                       // [K]
                else
                    tsg = -999;                         // AMeDAS has no TSG data.

                if (data[i].Humidity != -999)
                    rh = data[i].Humidity / 100;
                else
                {
                    if (RHeq70 == true)
                        rh = 0.7;
                    else
                        rh = -999;
                }

                if (data[i].Wind_Velocity != -999)
                    vw = data[i].Wind_Velocity;                     // [m/sec]
                else
                    vw = 0;

                if (data[i].Wind_Direction != "-999")
                {
                    dw = isWindDirection(data[i].Wind_Direction);   // [deg]
                    // save a valid direction of wind
                    if (dw != -999)
                        pre_dw = dw;
                }
                // if direction is invalid, then use previous one.
                if (dw == -999)
                    dw = pre_dw;

                iswr = data[i].ISWR;                                // [W/m^2]

                if (data[i].Height_Snow != -999)
                    hs = data[i].Height_Snow / 100;                 // [m]
                else
                    hs = -999;

                psum = data[i].Precipitation;                       // [mm]

                ws.WriteLine(data[i].Timestamp.ToString("yyyy'-'MM'-'dd'T'HH':'mm") + "\t"
                            + Math.Round(temp, 1, away) + "\t"
                            + tsg + "\t"
                            + rh + "\t"
                            + vw + "\t"
                            + dw + "\t"
                            + iswr + "\t"
                            + hs + "\t"
                            + psum);
            }

            // write the current stream to the TextBox
            textBox1.Text = ws.ToString();

            ws.Close();
        }

        //---------------------------------------------
        // オプション ⇒ バージョン情報
        //---------------------------------------------
        private void OptionStripMenuItem2_Click(object sender, EventArgs e)
        {
            Form f = new Form3();
            f.ShowDialog(this);

            f.Dispose();
        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }


        private void Form1_Load(object sender, EventArgs e)
        {

        }

    }
}
