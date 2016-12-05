using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.IO;
using Microsoft.VisualBasic.FileIO;
using System.Timers;
namespace TrayFinance
{
    public partial class TrayFinance : Form
    {
        private NotifyIcon trayIcon;
        private string[] tickers;
        public TrayFinance()
        {
            tickers = File.ReadAllLines(@"tickers.dat", Encoding.UTF8);
            
            InitializeComponent();

        }
        
        private void Form1_Load(object sender, EventArgs e)
        {
            this.components = new System.ComponentModel.Container();
            this.trayIcon = new System.Windows.Forms.NotifyIcon(this.components);
            trayIcon.DoubleClick += new EventHandler(this.trayIcon_MouseDoubleClick);
            listView1.View = View.List;
            for (var i=0; i<tickers.Length;i++){
                ListViewItem listItem = new ListViewItem(tickers[i]);
                listView1.Items.Add(listItem); }
            trayIcon.Icon = new Icon("RSIT.ico");
            
            trayIcon.Text = "TrayFinance";
            trayIcon.BalloonTipText = "";
            trayIcon.Visible = true;
                   updateData();
           this.WindowState = FormWindowState.Minimized;
            System.Timers.Timer aTimer;
            aTimer = new System.Timers.Timer(600000);

           aTimer.Elapsed += new ElapsedEventHandler(OnScheduledUpdate);

            
            aTimer.Interval = 600000;
            aTimer.Enabled = true;
            this.ShowInTaskbar = false;
              }

        private void trayIcon_MouseDoubleClick(object sender, EventArgs e)
        {
            trayIcon.ShowBalloonTip(1000);
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.ShowInTaskbar = true;
           // trayIcon.Visible = false;
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            int numTickers = tickers.Length;
            Array.Resize<string>(ref tickers, numTickers+1);
            tickers[numTickers] = textBox1.Text;
             File.WriteAllLines(@"tickers.dat",tickers, Encoding.UTF8);
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
        private void updateData()
        {

            string tickersymbols = String.Join("+", tickers);
            using (var client = new WebClient())
            {
               client.DownloadFile("http://finance.yahoo.com/d/quotes.csv?s=" + tickersymbols + "&f=nb", "data.csv");
            }

            string newData = "";
           using (TextFieldParser csvparser = new TextFieldParser(@"data.csv"))
            {
                csvparser.TextFieldType = FieldType.Delimited;
                csvparser.SetDelimiters(",");
                while (!csvparser.EndOfData)
                {
                    //Processing row
                    string[] fields = csvparser.ReadFields();
                    newData = newData + fields[0]+ "    " +fields[1]+ Environment.NewLine;
                }
            }
            trayIcon.BalloonTipText = newData;
            trayIcon.Visible = true;
            trayIcon.ShowBalloonTip(1000);
        }
        private void OnScheduledUpdate(object source, ElapsedEventArgs e)
        {
            this.updateData();
        }
    }
}
