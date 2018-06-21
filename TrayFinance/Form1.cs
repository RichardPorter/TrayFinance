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
using System.Text.RegularExpressions;
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
        private string retrieveData(string ticker)
        {
            var getRestApi = new WebClient();
            getRestApi.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36");
            try
            {
                Regex currencyRegex = new Regex(@"^CURRENCY", RegexOptions.IgnoreCase);
                Match currencyMatch = currencyRegex.Match(ticker);
                if (currencyMatch.Success) 
                {
                    String[] currencyParams = ticker.Split(':');
                    var response = getRestApi.DownloadString("https://www.alphavantage.co/query?function=CURRENCY_EXCHANGE_RATE&from_currency="+
                        currencyParams[1]+"&to_currency="+currencyParams[2]+ "&apikey=BLXYVLRVS6HFKM60");
                    // The following is less nasty than adding a dependency for a JSON parser
                    Regex exchangeRateRegex = new Regex("(?:\\\"5. Exchange Rate\\\": \\\")([0-9\\.]+)",RegexOptions.IgnoreCase);
                    Match rateMatches = exchangeRateRegex.Match(response);
                    String exchangeRate = currencyParams[1] + '/' + currencyParams[2] + ": " + rateMatches.Groups[1].Value;
                    return (exchangeRate);
                }
                //as of 2018-06-21, block quotes only qork for US stocks, which is why it's done one by one
                Regex stockRegex = new Regex(@"^STOCK", RegexOptions.IgnoreCase);
                Match stockMatch = stockRegex.Match(ticker);
                if (stockMatch.Success)
                {
                    String[] stockParams = ticker.Split(':');
                    var response = getRestApi.DownloadString("https://www.alphavantage.co/query?function=TIME_SERIES_INTRADAY&symbol=" + stockParams[1] + "&interval=1min" 
                        + "&apikey=BLXYVLRVS6HFKM60&datatype=csv");
                    String[] stockQuotes = response.Split(',');
                    return (stockParams[1] + ": " + stockQuotes[9]); //another hack. will break if any fields are added to ouptut
                }
            }
            catch (Exception)
            {

                return("");
            }
            return ("");
        }
        private void button1_Click(object sender, EventArgs e)
        {
            int numTickers = tickers.Length;
            Array.Resize<string>(ref tickers, numTickers+1);
            tickers[numTickers] = textBox1.Text;
            ListViewItem listItem = new ListViewItem(tickers[numTickers]);
            listView1.Items.Add(listItem);
            textBox1.Text = "";
            File.WriteAllLines(@"tickers.dat",tickers, Encoding.UTF8);
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
        private void updateData()
        {
            String newData = "";
            String newDataElement = "";
            foreach (String ticker in tickers)
            {
                newDataElement = retrieveData(ticker);
                if (newDataElement != "")
                {
                    newData = newData + newDataElement + '\n';
                }
            }
            if (newData == "")
            {
                newData = "No data retrieved. Check network connectivity.";
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
