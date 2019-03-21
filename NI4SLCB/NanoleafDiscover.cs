using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;

namespace NI4SLCB {
    class NanoleafDiscover {
        public static int timeout = 7;          // seconds
        public static int bufferSize = 64000;   // in bytes

        public static void Go(MainForm Mf) {
            string ssdp_host = "239.255.255.250";
            int ssdp_port = 1900;
            string ssdp_mx = "3";
            string ssdp_st = "nanoleaf_aurora:light";

            string seaarch_aurora = "M-SEARCH * HTTP/1.1\r\n" +
                                    "HOST: " + ssdp_host + ":" + ssdp_port + "\r\n" +
                                    "ST:" + ssdp_st + "\r\n" +
                                    "MAN:\"ssdp:discover\"\r\n" +
                                    "MX:" + ssdp_mx + "\r\n" +
                                    "\r\n";

            IPEndPoint LocalEndPoint = new IPEndPoint(IPAddress.Any, 0);
            IPEndPoint MulticastEndPoint = new IPEndPoint(IPAddress.Parse(ssdp_host), ssdp_port);

            Socket UdpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            UdpSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            UdpSocket.Bind(LocalEndPoint);
            UdpSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(MulticastEndPoint.Address, IPAddress.Any));
            UdpSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, 3);
            UdpSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastLoopback, true);

            Thread InstanceCaller = new Thread(() => DiscoverThread.Recveiver(Mf, UdpSocket));
            InstanceCaller.Start();
            UdpSocket.SendTo(Encoding.UTF8.GetBytes(seaarch_aurora), SocketFlags.None, MulticastEndPoint);
            Thread.Sleep(timeout*1000);
            InstanceCaller.Abort();
        }
    }

    class DiscoverThread {
        public static void Recveiver(MainForm Mf,Socket UdpSocket) {
            Mf.NewDisvoveredDevices();
            Stopwatch timer = new Stopwatch();
            timer.Start();
            byte[] ReceiveBuffer = new byte[NanoleafDiscover.bufferSize];
            int ReceivedBytes = 0;
            int i = 1;
            while (timer.Elapsed.TotalSeconds < NanoleafDiscover.timeout) {
                if (UdpSocket.Available > 0) {
                    ReceivedBytes = UdpSocket.Receive(ReceiveBuffer, SocketFlags.None);
                    if (ReceivedBytes > 0) {
                        string[] receive = Encoding.UTF8.GetString(ReceiveBuffer, 0, ReceivedBytes).Split(
                            new[] { "\r\n", "\r", "\n" },
                            StringSplitOptions.None
                        );
                        
                        Boolean OK = false;
                        string Location = null;
                        Boolean ST = false;
                        for( int j = 0; j<receive.Length; j++) {
                            if (receive[j].Equals("HTTP/1.1 200 OK"))
                                OK = true;
                            if (receive[j].ToUpper().StartsWith("LOCATION: "))
                                Location = receive[j].Substring("LOCATION: ".Length);
                            if (receive[j].StartsWith("ST: nanoleaf"))
                                ST = true;
                        }
                        if( OK && ST && Location!=null )
                            Mf.SetDisvoveredDevices(Location);
                    }
                }
            }
            timer.Stop();
        }
    }
}
