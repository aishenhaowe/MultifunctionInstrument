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

        private UsbEndpointReader _reader;
        private bool _isOpened = false;

        #region SET YOUR USB Vendor and Product ID!

        public static UsbDeviceFinder MyUsbFinder = new UsbDeviceFinder(0x0403, 0x6014);

        #endregion

        public MainForm()
        {
            InitializeComponent();
        }

        private void findDeviceToolStripMenuItem_Click(object sender, EventArgs e)
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

            this.richTextBox1.Select(this.richTextBox1.TextLength, 0);//设置光标的位置到文本尾  
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (!_isOpened)
            {
                // Find and open the usb device.
                MyUsbDevice = UsbDevice.OpenUsbDevice(MyUsbFinder);

                // If the device is open and ready
                if (MyUsbDevice == null) throw new Exception("Device Not Found.");

                // If this is a "whole" usb device (libusb-win32, linux libusb-1.0)
                // it exposes an IUsbDevice interface. If not (WinUSB) the 
                // 'wholeUsbDevice' variable will be null indicating this is 
                // an interface of a device; it does not require or support 
                // configuration and interface selection.
                IUsbDevice wholeUsbDevice = MyUsbDevice as IUsbDevice;
                if (!ReferenceEquals(wholeUsbDevice, null))
                {
                    // This is a "whole" USB device. Before it can be used, 
                    // the desired configuration and interface must be selected.

                    // Select config #1
                    wholeUsbDevice.SetConfiguration(1);

                    // Claim interface #0.
                    wholeUsbDevice.ClaimInterface(0);
                }

                // open read endpoint 1.
                _reader = MyUsbDevice.OpenEndpointReader(ReadEndpointID.Ep01);

                richTextBox1.Text += "The device has been opened\n";
                toolStripMenuItem1.Text = "Close";
                _isOpened = true;
            }
            else
            {
                if (MyUsbDevice != null)
                {
                    if (MyUsbDevice.IsOpen)
                    {
                        // If this is a "whole" usb device (libusb-win32, linux libusb-1.0)
                        // it exposes an IUsbDevice interface. If not (WinUSB) the 
                        // 'wholeUsbDevice' variable will be null indicating this is 
                        // an interface of a device; it does not require or support 
                        // configuration and interface selection.
                        IUsbDevice wholeUsbDevice = MyUsbDevice as IUsbDevice;
                        if (!ReferenceEquals(wholeUsbDevice, null))
                        {
                            // Release interface #0.
                            wholeUsbDevice.ReleaseInterface(0);
                        }

                        MyUsbDevice.Close();
                    }
                    MyUsbDevice = null;

                    // Free usb resources
                    UsbDevice.Exit();

                    richTextBox1.Text += "The device has been closed\n";
                    toolStripMenuItem1.Text = "Open";
                    _isOpened = false;
                }
            }

            this.richTextBox1.Select(this.richTextBox1.TextLength, 0);//设置光标的位置到文本尾  
            
        }

        private void readOnlyPollingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ErrorCode ec = ErrorCode.None;

            try
            {
                byte[] readBuffer = new byte[1024];
                //while (ec == ErrorCode.None)
                {
                    int bytesRead;

                    // If the device hasn't sent data in the last 5 seconds,
                    // a timeout error (ec = IoTimedOut) will occur. 
                    ec = _reader.Read(readBuffer, 5000, out bytesRead);

                    if (bytesRead == 0) throw new Exception(string.Format("{0}:No more bytes!", ec));
                    richTextBox1.Text += bytesRead.ToString() + "bytes read\n";
                    Console.WriteLine("{0} bytes read", bytesRead);

                    // Write that output to the console.
                    richTextBox1.Text += Encoding.Default.GetString(readBuffer, 0, bytesRead);
                    Console.Write(Encoding.Default.GetString(readBuffer, 0, bytesRead));
                }

                richTextBox1.Text += "\r\nDone!\r\n";
                Console.WriteLine("\r\nDone!\r\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                richTextBox1.Text += (ec != ErrorCode.None ? ec + ":" : String.Empty) + ex.Message;
                Console.WriteLine((ec != ErrorCode.None ? ec + ":" : String.Empty) + ex.Message);
            }
            finally
            {
               

            }

            this.richTextBox1.Select(this.richTextBox1.TextLength, 0);//设置光标的位置到文本尾  

        }

        private void cleanToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBox1.Text = "";
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(richTextBox1.SelectedText))
            {
                Clipboard.SetDataObject(richTextBox1.SelectedText);
            }
        }
    }
}
