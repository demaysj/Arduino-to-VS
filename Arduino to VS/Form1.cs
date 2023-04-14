using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace Arduino_to_VS
{
    public partial class Form1 : Form
    {
        SerialPort myPort;
        string currDat;
        int k = 0;

        private delegate void ReadItDel(string text);
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            btnClose.Enabled = false;

            string[] ports = SerialPort.GetPortNames();
            for (int i = 0; i < (ports.Length); i++)
            {
                cboPorts.Items.Add(ports[i]); //populate combobox with available ports
            }
            chart1.Series.Clear();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            while(chart1.Series.Count > 0) chart1.Series.RemoveAt(0); //get rid of everything in the chart
            k = 0;
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            cboPorts.Enabled = false;
            btnOpen.Enabled = false;
            btnClose.Enabled = true;

            try
            { 
                myPort = new SerialPort(cboPorts.Text, 9600); //set baud to 9600 with odd parity and a stop bit
                myPort.Parity = Parity.Odd;
                myPort.StopBits = StopBits.One;
                myPort.DataBits = 7;
                myPort.Open();
                myPort.DataReceived += new SerialDataReceivedEventHandler(myPort_DataReceived); //subscribe method to event handler
                myPort.DiscardInBuffer(); //get rid of random numbers in buffer
            }
            catch (IOException ex) { MessageBox.Show(ex.Message); }
        }

        private void myPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if(myPort.IsOpen == true)
            {
                try
                {
                    Thread.Sleep(100); //give time to receive all data
                    if(myPort.IsOpen == true) currDat = myPort.ReadLine(); //read current data line if the port is still open
                    this.BeginInvoke(new ReadItDel(DisplayAcc), new object[] { currDat }); //call display function using delegate
                }
                catch (IOException ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
        private void DisplayAcc(string accdat)
        {
            txtCurrVal.Text = accdat;
            if (chart1.Series.Count == 0) //if the chart doesnt have a series make one
            {
                chart1.Series.Add("Acc. Data");
                chart1.Series["Acc. Data"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastPoint;
            }
            if (k >= 50) chart1.Series["Acc. Data"].Points.RemoveAt(0); //if there are more than 50 points remove the first one
            chart1.Series["Acc. Data"].Points.AddXY(k, accdat); //add current data
            chart1.ChartAreas[0].RecalculateAxesScale(); //rescale axes
            k++; //increment k
        }
        private void btnClose_Click(object sender, EventArgs e)
        {
            btnClose.Enabled = false;
            try
            {
                myPort.Close();
                btnOpen.Enabled = true; //enable open button combobox for port selection
                cboPorts.Enabled = true;
            }
            catch { }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                myPort.Close();
                if (myPort != null) myPort.Dispose(); //dispose of the port upon closing
            }
            catch { }
        }
    }
}