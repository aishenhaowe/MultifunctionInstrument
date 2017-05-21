/***************************************************************************************
 * Opens a USB device by vendor and product id. 
 *
 * Gets and displays the UsbDeviceDescriptor and the UsbConfigDescriptor. 
 *
 * Gets and displays the ManufacturerString, if one exists. 
 *
 * Gets and displays the ProductString, if one exists. 
 *
 * Gets and displays the SerialString, if one exists. 
 ***************************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using LibUsbDotNet;
using LibUsbDotNet.Info;
using LibUsbDotNet.Main;
using System.Collections.ObjectModel;


namespace _01.ShowInfo
{
    public partial class MainForm : Form
    {
        public static UsbDevice MyUsbDevice;

        public MainForm()
        {
            InitializeComponent();
        }

        private void openDeviceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBox1.Text = "=======================================================\n";

            // Dump all devices and descriptor information to console output.
            UsbRegDeviceList allDevices = UsbDevice.AllDevices;
            foreach (UsbRegistry usbRegistry in allDevices)
            {
                if (usbRegistry.Open(out MyUsbDevice))
                {
                    richTextBox1.Text += "=======================================================\n";
                    richTextBox1.Text += MyUsbDevice.Info.ToString();
                    Console.WriteLine(MyUsbDevice.Info.ToString());
                    for (int iConfig = 0; iConfig < MyUsbDevice.Configs.Count; iConfig++)
                    {
                        UsbConfigInfo configInfo = MyUsbDevice.Configs[iConfig];
                        richTextBox1.Text += "=======================================================\n";
                        richTextBox1.Text += configInfo.ToString();
                        Console.WriteLine(configInfo.ToString());

                        ReadOnlyCollection<UsbInterfaceInfo> interfaceList = configInfo.InterfaceInfoList;
                        for (int iInterface = 0; iInterface < interfaceList.Count; iInterface++)
                        {
                            UsbInterfaceInfo interfaceInfo = interfaceList[iInterface];
                            richTextBox1.Text += "=======================================================\n";
                            richTextBox1.Text += interfaceInfo.ToString();
                            Console.WriteLine(interfaceInfo.ToString());

                            ReadOnlyCollection<UsbEndpointInfo> endpointList = interfaceInfo.EndpointInfoList;
                            for (int iEndpoint = 0; iEndpoint < endpointList.Count; iEndpoint++)
                            {
                                richTextBox1.Text += "=======================================================\n";
                                richTextBox1.Text += endpointList[iEndpoint].ToString();
                                Console.WriteLine(endpointList[iEndpoint].ToString());
                            }
                        }
                    }
                }
            }
        }


    }
}
