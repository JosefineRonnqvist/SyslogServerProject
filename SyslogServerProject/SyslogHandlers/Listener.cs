using ConnectToClavisterBlacklisting;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SyslogServerProject.SyslogHandlers
{
    //https://www.codeproject.com/Tips/441233/Multithreaded-Customizable-SysLog-Server-Csharp <= code found here
    internal class Listener
    {
        public static int PORT_NUMBER = Convert.ToInt32(ConfigurationManager.AppSettings.Get("port_number"));
        public DateTime CheckTime = DateTime.Now;

        /// <summary>
        /// Recives messages from syslog and change format
        /// </summary>
        public void SyslogReader()
        {           
            Console.WriteLine($"Syslog server started. Listening on port {PORT_NUMBER}...");

            //UdpClient reads incoming data
            UdpClient receivingUdpClient = new UdpClient(PORT_NUMBER);

            //The IPEndPoint makes it possible to read data sent from any source
            //and to record IP Address of sender
            IPEndPoint remoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);

            while (true)
            {
                try
                {
                    var sourceIP = remoteIpEndPoint.Address.ToString();

                    // Blocks until a message returns on this socket from a remote host.
                    Byte[] receivedBytes = receivingUdpClient.Receive(ref remoteIpEndPoint);

                    //Convert incoming data from bytes to ASCII
                    string receivedAscii = Encoding.ASCII.GetString(receivedBytes);

                    Console.WriteLine($"Message from {sourceIP}:{remoteIpEndPoint.Port} received: ");

                    // Start a new thread to handle received syslog event 
                    new Thread(new LogHandler(sourceIP, receivedAscii).GetInterestingValues).Start();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
                CheckIfTimeForCheckIfRenew();
            }
        }

        public void CheckIfTimeForCheckIfRenew()
        {
            if (CheckTime.AddMinutes(10) < DateTime.Now) 
            {
                SendBlacklist send = new();
                send.CheckBlacklistToRenew();
                CheckTime = DateTime.Now;
            }
        }
    }
}
