using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using Snarl;

using NativeWifi;

 

namespace WiFiSnarler
{


    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {

        static List<string> connectedWiFis = new List<string>();
        static List<WlanClient.WlanInterface> listOfConnectWiFis = new List<WlanClient.WlanInterface>();
        static private string iconPath = "";
        static private IntPtr SnarlMsgWndHandle;
        private NativeWindowApplication.SnarlMsgWnd snarlComWindow;

        static string executablePath = "";
        static string wirelessFull = "wireless-connected-full.png";
        static string wirelessDisconnect = "wireless-disconnected.png";

        static private WlanClient client; 

        public Window1()
        {
            
            client = new WlanClient();
            InitializeComponent();

            this.snarlComWindow = new NativeWindowApplication.SnarlMsgWnd();
            SnarlMsgWndHandle = snarlComWindow.Handle;
            executablePath = System.IO.Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath) + "\\";
            iconPath = executablePath + wirelessFull;
            this.snarlComWindow.pathToIcon = iconPath;
            SnarlConnector.RegisterConfig(SnarlMsgWndHandle, "WiFiSnarler", Snarl.WindowsMessage.WM_USER + 55, iconPath);
            SnarlConnector.RegisterAlert("WiFiSnarler", "WiFi connected");
            SnarlConnector.RegisterAlert("WiFiSnarler", "WiFi connected signal strength");
            SnarlConnector.RegisterAlert("WiFiSnarler", "WiFi disconnected");


            //WlanClient client = new WlanClient();
            
            foreach (WlanClient.WlanInterface wlanIface in client.Interfaces)
            {
                //Add the event to track when the wireless connection changes
                wlanIface.WlanConnectionNotification += new WlanClient.WlanInterface.WlanConnectionNotificationEventHandler(wlanIface_WlanConnectionNotification);
                wlanIface.WlanNotification += new WlanClient.WlanInterface.WlanNotificationEventHandler(wlanIface_eventHandler);
            }





           // initializeAvailableWiFis();

//            NetworkChange.NetworkAddressChanged += new System.Net.NetworkInformation.NetworkAddressChangedEventHandler(NetworkIPChanged);
            //WlanClient test = new WlanClient();
            
        }

        ~Window1()
        {
            SnarlConnector.RevokeConfig(SnarlMsgWndHandle);
        }

        static void wlanIface_WlanConnectionNotification(Wlan.WlanNotificationData notifyData, Wlan.WlanConnectionNotificationData connNotifyData)
        {
            
            Snarl.SnarlConnector.ShowMessageEx("WiFi disconnected", "Disconnect from WiFi", connNotifyData.profileName, 10, executablePath + wirelessDisconnect, SnarlMsgWndHandle, Snarl.WindowsMessage.WM_USER + 34, "");
            Console.WriteLine(notifyData.NotificationCode);
            //Console.WriteLine("{0} to {1} with quality level {2}",connNotifyData.wlanConnectionMode, connNotifyData.profileName, "-");
        }

        static void wlanIface_eventHandler(Wlan.WlanNotificationData notifyData)
        {
            Snarl.SnarlConnector.ShowMessageEx("WiFi connected", "Event from WiFi", notifyData.notificationCode.ToString(), 10, executablePath + wirelessDisconnect, SnarlMsgWndHandle, Snarl.WindowsMessage.WM_USER + 34, "");

        }

        private void initializeAvailableWiFis()
        {
            try
            {
                WlanClient client = new WlanClient();

                foreach (WlanClient.WlanInterface wlanInterface in client.Interfaces)
                {
                    try
                    {
                        string WlanName = "";
                        Wlan.Dot11Ssid ssid = wlanInterface.CurrentConnection.wlanAssociationAttributes.dot11Ssid;
                        WlanName = new string(Encoding.ASCII.GetChars(ssid.SSID, 0, (int)ssid.SSIDLength));
                        Snarl.SnarlConnector.ShowMessageEx("WiFi connected", "Connected to WiFi", WlanName + "\n" + wlanInterface.InterfaceDescription + "\n" + "Signal strenght: " + wlanInterface.CurrentConnection.wlanAssociationAttributes.wlanSignalQuality.ToString() + "%\nChannel " + wlanInterface.Channel.ToString(), 10, iconPath, SnarlMsgWndHandle, Snarl.WindowsMessage.WM_USER + 34, "");
                        Snarl.SnarlConnector.ShowMessageEx("WiFi connected signal strength", "Connected to " + WlanName, wlanInterface.CurrentConnection.wlanAssociationAttributes.wlanSignalQuality.ToString(), 10, iconPath, SnarlMsgWndHandle, Snarl.WindowsMessage.WM_USER + 34, "");
                        connectedWiFis.Add(WlanName);
                        listOfConnectWiFis.Add(wlanInterface);
                    }
                    catch
                    {

                    }
                }
            }
            catch
            {

            }

        }


        private static void NetworkAvailabilityChanged(object sender, NetworkAvailabilityEventArgs e)
        {
            if (e.IsAvailable)
            {
                Snarl.SnarlConnector.ShowMessage("Network now available", "", 10, "", IntPtr.Zero, Snarl.WindowsMessage.WM_USER + 34);
            }
            else
            {
                Snarl.SnarlConnector.ShowMessage("Network now unavailable", "", 10, "", IntPtr.Zero, Snarl.WindowsMessage.WM_USER + 34);
            }
        }

        private static void NetworkIPChanged(object sender, EventArgs e)
        {
            List<string> newConnectedWiFis = new List<string>();
            List<WlanClient.WlanInterface> listOfNewConnectedWiFis = new List<WlanClient.WlanInterface>();
            WlanClient client = new WlanClient();
            
            

            foreach (WlanClient.WlanInterface wlanIface in client.Interfaces)
            {
                try
                {
                    string WlanName = "";
                    Wlan.Dot11Ssid ssid = wlanIface.CurrentConnection.wlanAssociationAttributes.dot11Ssid;
                    WlanName = new string(Encoding.ASCII.GetChars(ssid.SSID, 0, (int)ssid.SSIDLength));

                    //Snarl.SnarlConnector.ShowMessage("Connected to WLAN", WlanName, 10, "", IntPtr.Zero, Snarl.WindowsMessage.WM_USER + 34);
                    newConnectedWiFis.Add(WlanName);
                    listOfNewConnectedWiFis.Add(wlanIface);
                } catch {

                }
            }

            foreach (WlanClient.WlanInterface wlanInterface in listOfNewConnectedWiFis)
            {
                if(!listOfConnectWiFis.Contains(wlanInterface)) {
                    string WlanName = "";
                    Wlan.Dot11Ssid ssid = wlanInterface.CurrentConnection.wlanAssociationAttributes.dot11Ssid;
                    WlanName = new string(Encoding.ASCII.GetChars(ssid.SSID, 0, (int)ssid.SSIDLength));
                    Snarl.SnarlConnector.ShowMessageEx("WiFi connected", "Connected to WiFi", WlanName + "\n" + wlanInterface.InterfaceDescription + "\n" + "Signal strenght: " + wlanInterface.CurrentConnection.wlanAssociationAttributes.wlanSignalQuality.ToString() + "%\nChannel " + wlanInterface.Channel.ToString(), 10, iconPath, SnarlMsgWndHandle , Snarl.WindowsMessage.WM_USER + 34, "");
                    Snarl.SnarlConnector.ShowMessageEx("WiFi connected signal strength", "Connected to " + WlanName, wlanInterface.CurrentConnection.wlanAssociationAttributes.wlanSignalQuality.ToString(), 10, iconPath, SnarlMsgWndHandle, Snarl.WindowsMessage.WM_USER + 34, "");
                }
            }


            foreach (string netWorkName in connectedWiFis)
            {
                if (!newConnectedWiFis.Contains(netWorkName))
                {

                    Snarl.SnarlConnector.ShowMessageEx("WiFi disconnected", "Disconnect from WiFi", netWorkName, 10, executablePath + wirelessDisconnect, SnarlMsgWndHandle, Snarl.WindowsMessage.WM_USER + 34, "");
                }
            }

            connectedWiFis = newConnectedWiFis;
            listOfConnectWiFis = listOfNewConnectedWiFis;

            /*
                Snarl.SnarlConnector.ShowMessage("New IP", "", 10, "", IntPtr.Zero, Snarl.WindowsMessage.WM_USER + 34);
                int doof = wiFiCatcher.GetSignalStrengthAsInt();
                if (doof != 0)
                {
                    Snarl.SnarlConnector.ShowMessage("Strength", doof.ToString(), 10, "", IntPtr.Zero, Snarl.WindowsMessage.WM_USER + 34);
                }
              */  
        }

        private void scanNow_Click(object sender, RoutedEventArgs e)
        {
            Snarl.SnarlConnector.ShowMessageEx("Scan started", "Scanning for WiFis", "scan in progress...", 10, executablePath + wirelessDisconnect, SnarlMsgWndHandle, Snarl.WindowsMessage.WM_USER + 34, "");
            foreach (WlanClient.WlanInterface wlanIface in client.Interfaces)
            {
                Wlan.WlanAvailableNetwork[] visibleWlans = wlanIface.GetAvailableNetworkList(Wlan.WlanGetAvailableNetworkFlags.IncludeAllAdhocProfiles);
                foreach(Wlan.WlanAvailableNetwork network in visibleWlans) {
                    string WlanName = "";
                    Wlan.Dot11Ssid ssid = network.dot11Ssid;
                    WlanName = new string(Encoding.ASCII.GetChars(ssid.SSID, 0, (int)ssid.SSIDLength));

                    Snarl.SnarlConnector.ShowMessageEx("WiFi connected", "Found WifI WiFi", network.profileName + " with strength " + network.wlanSignalQuality + " % with name " + WlanName, 10, executablePath + wirelessDisconnect, SnarlMsgWndHandle, Snarl.WindowsMessage.WM_USER + 34, "");
                }
            }
        }


     }

    class wiFiCatcher
    {
        public static int GetSignalStrengthAsInt()
        {
            Int32 returnStrength = 0;
            ManagementObjectSearcher searcher = null;
            try
            {
                // Query the management object with the valid scope and the
                // correct query statment
                searcher = new ManagementObjectSearcher(@"root\WMI", "select Ndis80211ReceivedSignalStrength from MSNdis_80211_ReceivedSignalStrength where active=true");

                // Call the get in order to populate the collection
                ManagementObjectCollection adapterObjects = searcher.Get();
                // Loop though the management object and pull out the signal
                // strength
                foreach (ManagementObject mo in adapterObjects)
                {
                    returnStrength = Convert.ToInt32(mo["Ndis80211ReceivedSignalStrength"].ToString());
                    break;
                }





            }
            catch (Exception e)
            {
                Console.WriteLine("Excepetion : " + e.Message);
            }
            finally
            {
                if (searcher != null)
                {
                    searcher.Dispose();
                }
            }

            return returnStrength;
        }
    }
}
