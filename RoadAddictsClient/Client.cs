using System;
using Lidgren.Network;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace RoadAddictsClient
{
    class Client
    {
        // A lidgren variable holding an object that represents the local player in a game session. Used to perform all the major Lidgren tasks.
        private NetPeer session;
        // Lidgren object used to hold all the data that needs to be sent throught the network.
        public static NetOutgoingMessage packetWriter;
        private bool pingSent = false;
        private Stopwatch watch = Stopwatch.StartNew();

        public Client()
        {
            NetPeerConfiguration config = new NetPeerConfiguration("RA");
            config.EnableMessageType(NetIncomingMessageType.DiscoveryResponse);
            config.EnableMessageType(NetIncomingMessageType.DiscoveryRequest);
            config.EnableMessageType(NetIncomingMessageType.UnconnectedData);
            config.EnableMessageType(NetIncomingMessageType.StatusChanged);
            config.ConnectionTimeout = 300F;
            config.PingInterval = 10F;
            config.Port = 50001;
            //config.EnableUPnP = true;
            session = new NetPeer(config);
            try
            {
                session.Start();
            }
            catch (SocketException)
            {
                System.Threading.Thread.Sleep(100);
                try
                {
                    session.Start();
                }
                catch (SocketException)
                {
                    System.Threading.Thread.Sleep(400);
                    try
                    {
                        session.Start();
                    }
                    catch (SocketException)
                    {
                        abortSession();
                    }
                }
            }
        }

        public void Start()
        {
            session.Connect("localhost", 50000);
            //session.UPnP.ForwardPort(55500, "");
            /*string whatIsMyIp = "http://roadaddicts.site11.com/";
            Regex ipRegex = new Regex(@"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b");
            WebClient wc = new WebClient();
            UTF8Encoding utf8 = new UTF8Encoding();
            string requestHtml = "";
            try
            {
                requestHtml = utf8.GetString(wc.DownloadData(whatIsMyIp));
            }
            catch (WebException we)
            {
                // do something with exception
                Console.Write(we.ToString());
            }
            Console.WriteLine(requestHtml);
            string ip = "216.239.66.84";
            Console.Write("Enter port ");
            int port = Int32.Parse(Console.ReadLine());
            Stopwatch watch = Stopwatch.StartNew();
            while(watch.ElapsedMilliseconds < 15000)
            {
                packetWriter = session.CreateMessage();
                packetWriter.Write("hello");
                session.SendUnconnectedMessage(packetWriter, ip, port);
                System.Threading.Thread.Sleep(1000);
                session.DiscoverKnownPeer(ip, port);
                System.Threading.Thread.Sleep(1000);
                receiveMessage();
            }*/
            while (true)
            {
                receiveMessage();
                if (pingSent == false)
                {
                    pingSent = true;
                    System.Threading.Thread.Sleep(100);
                    watch = Stopwatch.StartNew();
                    sendPing();
                }
            }
        }

        private void receiveMessage()
        {
            NetIncomingMessage im;
            while ((im = session.ReadMessage()) != null)
            {
                switch (im.MessageType)
                {
                    case NetIncomingMessageType.DebugMessage:
                    case NetIncomingMessageType.ErrorMessage:
                    case NetIncomingMessageType.WarningMessage:
                    case NetIncomingMessageType.VerboseDebugMessage:
                        Console.WriteLine(im.ReadString() + "\n");
                        break;
                    case NetIncomingMessageType.UnconnectedData:
                        Console.WriteLine(im.ReadString());
                        break;
                    case NetIncomingMessageType.StatusChanged:
                        NetConnectionStatus status = (NetConnectionStatus)im.ReadByte();

                        // When a new player connects, the host is charged to find a suitable default name between player1 and player4
                        // depending on what names are available. The host then sends the new list of player names to the other players
                        if (status == NetConnectionStatus.Connected)
                        {
                            /*packetWriter = session.CreateMessage();
                            packetWriter.Write((Byte)ConnectedMessageType.Chat);
                            packetWriter.Write("Hello");
                            session.SendMessage(packetWriter, im.SenderConnection, NetDeliveryMethod.ReliableOrdered);*/
                        }

                        if (status == NetConnectionStatus.Disconnected)
                        {

                        }
                        // Don't need this for now
                        string reason = im.ReadString();
                        Console.WriteLine(status.ToString() + ": " + reason);
                        break;

                    case NetIncomingMessageType.Data:
                        ConnectedMessageType connectedMessageType = (ConnectedMessageType)im.ReadByte();
                        switch (connectedMessageType)
                        {
                            case ConnectedMessageType.EndRoundInfo:
                                packetWriter = session.CreateMessage();
                                packetWriter.Write((Byte)ConnectedMessageType.EndRoundInfo);
                                packetWriter.Write(im.ReadString());
                                session.SendMessage(packetWriter, im.SenderConnection, NetDeliveryMethod.ReliableOrdered);
                                break;
                            case ConnectedMessageType.Ping:
                                packetWriter = session.CreateMessage();
                                packetWriter.Write((Byte)ConnectedMessageType.PingReply);
                                session.SendMessage(packetWriter, session.Connections, NetDeliveryMethod.ReliableOrdered, 0);
                                break;
                            case ConnectedMessageType.PingReply:
                                pingSent = false;
                                Console.WriteLine(watch.ElapsedMilliseconds);
                                break;
                        }
                        break;
                    default:
                        Console.WriteLine("Unhandled type: " + im.MessageType + " " + im.LengthBytes + " bytes \n");
                        break;
                }
            } 
        }

        private void sendPing()
        {
            packetWriter = session.CreateMessage();
            packetWriter.Write((Byte)ConnectedMessageType.Ping);
            session.SendMessage(packetWriter, session.Connections, NetDeliveryMethod.ReliableOrdered, 0);
        }

        // Called when a player leaves. Cleans up the lidgren peer service. Should be done before starting the session again.
        private void abortSession()
        {
            if (session != null)
            {
                session.Shutdown("");
            }
            session = null;
        }
    }
}
