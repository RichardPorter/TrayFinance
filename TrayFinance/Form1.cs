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
        private System.Windows.Forms.MenuItem menuItemExit;
        private string[] tickers=new string[0];
        private System.Windows.Forms.ContextMenuStrip rightClickTicker;
        private string selectedTicker="";
        public TrayFinance()
        {
            if (File.Exists(@"tickers.dat")) {
                tickers = File.ReadAllLines(@"tickers.dat", Encoding.UTF8);
            }
            InitializeComponent();

        }
        
        private void Form1_Load(object sender, EventArgs e)
        {
            this.components = new System.ComponentModel.Container();
            this.trayIcon = new System.Windows.Forms.NotifyIcon(this.components);
            trayIcon.DoubleClick += new EventHandler(this.trayIcon_MouseDoubleClick);
            trayIcon.ContextMenu = new System.Windows.Forms.ContextMenu();
            this.menuItemExit = new System.Windows.Forms.MenuItem();
            rightClickTicker = new System.Windows.Forms.ContextMenuStrip();
            rightClickTicker.Items.Add("Delete Ticker", null, this.deleteItem);
            this.menuItemExit.Index = 0;
            this.menuItemExit.Text = "Exit";
            this.menuItemExit.Click += new System.EventHandler(this.menuItemExit_Click);
            trayIcon.ContextMenu.MenuItems.Add(this.menuItemExit);
            listView1.View = View.List;
            for (var i=0; i<tickers.Length;i++){
                ListViewItem listItem = new ListViewItem(tickers[i]);
                listView1.Items.Add(listItem); }
            trayIcon.Icon = Properties.Resources.TF;
            
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
        private void deleteItem(object sender, EventArgs e)
        {
            if (selectedTicker != "")
            {
                foreach (ListViewItem item in listView1.SelectedItems)
                {
                    if (item.Text == selectedTicker)
                    {
                        listView1.Items.Remove(item);
                    }
                }
                int itemIndex = Array.IndexOf(tickers, selectedTicker);
                int numTickers = tickers.Length;
                for (int i=itemIndex;i<numTickers-1; i++)
                {
                    tickers[i] = tickers[i + 1];
                }
                Array.Resize<string>(ref tickers, numTickers - 1);
                File.WriteAllLines(@"tickers.dat", tickers, Encoding.UTF8);
                selectedTicker = "";

            }
            
        }

        private void trayIcon_MouseDoubleClick(object sender, EventArgs e)
        {
            //trayIcon.ShowBalloonTip(1000);
            updateData();
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
            ListViewItem listItem = new ListViewItem(tickers[numTickers]);
            listView1.Items.Add(listItem);
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
               client.DownloadFile("http://finance.yahoo.com/d/quotes.csv?s=" + tickersymbols + "&f=nl1", "data.csv");
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
            if (trayIcon.BalloonTipText != "")
            {
                trayIcon.ShowBalloonTip(1000);
            }
        }
        private void OnScheduledUpdate(object source, ElapsedEventArgs e)
        {
            this.updateData();
        }

       
        private void TrayFinance_Resize(object sender, EventArgs e)
        {
            if (FormWindowState.Minimized == this.WindowState)
            {
                trayIcon.Visible = true;
                this.Hide();
            }
        }
        private void menuItemExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void listView1_MouseClick(object sender, MouseEventArgs e)
        {

            if (e.Button == MouseButtons.Right)
            {
                this.selectedTicker = "";
                rightClickTicker.Show(Cursor.Position);
                foreach (ListViewItem item in listView1.Items)
                {
                    if (item.Bounds.Contains(new Point(e.X, e.Y)))
                    {
                        this.selectedTicker=item.Text;
                    }
                }
            }
        }
    }
}
