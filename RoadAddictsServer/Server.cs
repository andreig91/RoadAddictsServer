using System;
using Lidgren.Network;
using System.Net.Sockets;

namespace RoadAddictsServer
{
    class Server
    {
        // A lidgren variable holding an object that represents the local player in a game session. Used to perform all the major Lidgren tasks.
        private NetPeer session;
        // Lidgren object used to hold all the data that needs to be sent throught the network.
        public static NetOutgoingMessage packetWriter;

        public Server()
        {
            NetPeerConfiguration config = new NetPeerConfiguration("RA");
            config.EnableMessageType(NetIncomingMessageType.DiscoveryResponse);
            config.EnableMessageType(NetIncomingMessageType.DiscoveryRequest);
            config.EnableMessageType(NetIncomingMessageType.UnconnectedData);
            config.EnableMessageType(NetIncomingMessageType.StatusChanged);
            config.ConnectionTimeout = 300F;
            config.PingInterval = 10F;
            config.Port = 14242;
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
            while (true)
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
                        case NetIncomingMessageType.StatusChanged:
                            NetConnectionStatus status = (NetConnectionStatus)im.ReadByte();

                            // When a new player connects, the host is charged to find a suitable default name between player1 and player4
                            // depending on what names are available. The host then sends the new list of player names to the other players
                            if (status == NetConnectionStatus.Connected)
                            {
                                packetWriter = session.CreateMessage();
                                packetWriter.Write((Byte)ConnectedMessageType.Chat);
                                packetWriter.Write("Hello");
                                session.SendMessage(packetWriter, im.SenderConnection, NetDeliveryMethod.ReliableOrdered);
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
                            }
                            break;
                        default:
                            Console.WriteLine("Unhandled type: " + im.MessageType + " " + im.LengthBytes + " bytes \n");
                            break;
                    }
                } 
            }
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