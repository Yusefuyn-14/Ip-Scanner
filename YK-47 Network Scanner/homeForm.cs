using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace YK_47_Network_Scanner
{
    public partial class homeForm : Form
    {
        public homeForm()
        {
            InitializeComponent();
        }
        List<IPAddress> addressList;
        Thread[] threads;
        int delay = 0;
        private void button1_Click(object sender, EventArgs e)
        {
            dataGridView1.Rows.Clear();
            addressList = new List<IPAddress>();
            for (int one = Convert.ToInt32(numeric1_1.Value); one <= Convert.ToInt32(numeric1_2.Value); one++)
                for (int two = Convert.ToInt32(numeric2_1.Value); two <= Convert.ToInt32(numeric2_2.Value); two++)
                    for (int three = Convert.ToInt32(numeric3_1.Value); three <= Convert.ToInt32(numeric3_2.Value); three++)
                        for (int four = Convert.ToInt32(numeric4_1.Value); four <= Convert.ToInt32(numeric4_2.Value); four++)
                            addressList.Add(IPAddress.Parse(one.ToString() + "." + two.ToString() + "." + three.ToString() + "." + four.ToString()));
            progressBar1.Minimum = 0;
            progressBar1.Maximum = addressList.Count;
            progressBar1.Value = 0;
            delay = Convert.ToInt32(numericUpDown1.Value);
            threads = new Thread[addressList.Count];
            if (radioButton1.Checked == true)
                PingScanStart();
            else
                PortScanStart();
        }


        private void PortScanStart()
        {
            foreach (IPAddress item in addressList)
            {
                Thread thread = new Thread(new ParameterizedThreadStart(PortScan));
                thread.Start(item);
                threads[addressList.IndexOf(item)] = thread;
                progressBar1.Value += 1;
            }
        }

        private void PortScan(object obj) {
            IPAddress ipAdr = (IPAddress)obj;
            IPEndPoint endPoint = new IPEndPoint(ipAdr, Convert.ToInt32(numericPort.Value));
            TcpClient tClient = new TcpClient();
            try
            {
                tClient.Connect(endPoint);
                Add(ipAdr);
            }
            catch (Exception)
            {
                return;
            }
        }

        private void PingScanStart()
        {
            foreach (IPAddress item in addressList)
            {
                Thread thread = new Thread(new ParameterizedThreadStart(PingScan));
                thread.Start(item);
                threads[addressList.IndexOf(item)] = thread;
                progressBar1.Value += 1;
            }
        }
        private void PingScan(object obj)
        {
            IPAddress _address = (IPAddress)obj;
            Ping ping = new Ping();
            PingReply durum = null;
            try
            {
                if (_address.ToString() == "192.168.1.34")
                {
                }
                else if (_address.ToString() == "192.168.1.35")
                {
                }
                durum = ping.Send(_address, delay);
            }
            catch (Exception)
            {
            }
            if (durum.Status == IPStatus.Success)
            {
                Add(_address);
            }
        }


        private void Add(IPAddress ip)
        {
            string hostName = "";
            try
            {
                hostName = Dns.GetHostEntry(ip).HostName;
            }
            catch { }
            string[] strArray = new string[4] {
                             ip.ToString(),
                              findMacAddress(ip.ToString()).ToUpper() ,
                               findDevicesForMacAddress(findMacAddress(ip.ToString()).ToUpper()),
                             hostName
                          };
            if (strArray[0] == Environment.MachineName)
            { strArray[1] = "Bu bilgisayar"; strArray[2] = ""; strArray[3] = ""; }
            dataGridView1.Rows.Add(strArray);
        }

        private string findMacAddress(string ipAddress)
        {
            string macAddress = string.Empty;
            System.Diagnostics.Process pProcess = new System.Diagnostics.Process();
            pProcess.StartInfo.FileName = "arp";
            pProcess.StartInfo.Arguments = "-a " + ipAddress;
            pProcess.StartInfo.UseShellExecute = false;
            pProcess.StartInfo.RedirectStandardOutput = true;
            pProcess.StartInfo.CreateNoWindow = true;
            pProcess.Start();
            string strOutput = pProcess.StandardOutput.ReadToEnd();
            string[] substrings = strOutput.Split('-');
            if (substrings.Length >= 8)
            {
                macAddress = substrings[3].Substring(Math.Max(0, substrings[3].Length - 2))
                         + "-" + substrings[4] + "-" + substrings[5] + "-" + substrings[6]
                         + "-" + substrings[7] + "-"
                         + substrings[8].Substring(0, 2);
                return macAddress;
            }

            else
            {
                return "Bulamadım ;(";
            }
        }

        public string findDevicesForMacAddress(string MaccAddress)
        {
            string[] lines = File.ReadAllLines(@"Data/macList.txt");
            foreach (string line in lines)
            {
                if (MaccAddress.Substring(0, 7) == line.Substring(0, 7))
                    return line.Substring(9, line.Length - 9);
            }
            return "Bulunamadı";
        }

        private void homeForm_Load(object sender, EventArgs e)
        {
            Control.CheckForIllegalCrossThreadCalls = false;
        }
    }
}
