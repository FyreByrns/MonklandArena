using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace MonkArena {
    public static class Network {
        public static UdpListener Server { get; private set; }
        public static UdpUser Client { get; private set; }
        public static bool Connected { get; private set; }
        public static bool IsServer { get; private set; }
        public static bool IsClient { get; private set; }

        public static List<IPEndPoint> ConnectedClients { get; private set; }

        static Network() {
        }

        public static void Disconnect() {
            if (IsServer) SendMessage("Server shutting down.");
            else if (IsClient) SendMessage("disconnect");
        }

        public static void SetupServer() {
            RWConsole.LogInfo("Starting server...");
            ConnectedClients = new List<IPEndPoint>();
            Server = new UdpListener();
            Server.MessageReceivedEvent += Server_MessageReceivedEvent;
            Server.StartReceive();
            IsServer = true;
        }

        private static void Server_MessageReceivedEvent(Received data) {
            if (!ConnectedClients.Contains(data.Sender)) ConnectedClients.Add(data.Sender);
        }

        public static void SetupClient(string address) {
            RWConsole.LogInfo("Attempting connection to " + address);
            Client = UdpUser.ConnectTo(address, 19000);
            Client.StartReceive();
            IsClient = true;
            Connected = true;

            RWConsole.LogInfo("Sending handshake...");
            Client.Send(Message.FromString("handshake"));
        }

        public static void SendMessage(string message) {
            RWConsole.LogInfo("Attempting to send message " + message);
            if (!Connected && !IsServer) {
                RWConsole.LogError("Can't send messages when disconnected.");
                return;
            }

            if (IsServer) foreach (IPEndPoint ipep in ConnectedClients) Server.Reply(Message.FromString(message), ipep);
            else Client.Send(Message.FromString(message));
        }
    }
    public struct Received {
        public IPEndPoint Sender { get; set; }
        public string Message { get; set; }
    }

    public abstract class UdpBase {
        public delegate void MessageReceived(Received data);
        public event MessageReceived MessageReceivedEvent;

        protected UdpClient Client;

        protected UdpBase() {
            Client = new UdpClient();
        }

        public struct UdpState {
            public UdpClient u;
            public IPEndPoint e;
        }

        public virtual void StartReceive() { }

        public void ReceiveCallback(IAsyncResult ar) {
            UdpClient u = ((UdpState)ar.AsyncState).u;
            IPEndPoint e = ((UdpState)(ar.AsyncState)).e;

            byte[] receivedBytes = u.EndReceive(ar, ref e);
            string receivedString = Encoding.ASCII.GetString(receivedBytes);

            RWConsole.LogInfo($"Received: {receivedString} From: {e}");
            MessageReceivedEvent?.Invoke(new Received() { Sender = e, Message = receivedString });
        }
    }

    public class UdpListener : UdpBase {
        IPEndPoint listenOn;

        public UdpListener() : this(new IPEndPoint(IPAddress.Any, 19000)) { }
        public UdpListener(IPEndPoint endpoint) {
            listenOn = endpoint;
            Client = new UdpClient(listenOn);
        }

        public override void StartReceive() {
            base.StartReceive();

            UdpState state = new UdpState() {
                e = listenOn,
                u = Client
            };
            Client.BeginReceive(new AsyncCallback(ReceiveCallback), state);
        }

        public void Reply(Message message, IPEndPoint endpoint) {
            var datagram = Encoding.ASCII.GetBytes(message.ToString());
            Client.Send(datagram, datagram.Length, endpoint);
        }
    }
    public class UdpUser : UdpBase {
        private UdpUser() { }

        public static UdpUser ConnectTo(string hostname, int port) {
            var connection = new UdpUser();
            connection.Client.Connect(hostname, port);
            return connection;
        }

        public override void StartReceive() {
            base.StartReceive();

            UdpState state = new UdpState() {
                e = new IPEndPoint(IPAddress.Any, 19001),
                u = Client
            };
            Client.BeginReceive(new AsyncCallback(ReceiveCallback), state);
        }

        public void Send(Message message) {
            var datagram = Encoding.ASCII.GetBytes(message.ToString());
            Client.Send(datagram, datagram.Length);
        }
    }
}
