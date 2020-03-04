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

        public static Dictionary<IPEndPoint, PlayerInfo> ConnectedClients { get; private set; }
        public static Dictionary<string, Message> UnreceivedMessages { get; private set; }

        static Network() {
        }

        public static void Disconnect() {
            if (IsServer) SendString("Server shutting down.");
            else if (IsClient) SendString("disconnect");
        }

        #region Server
        public static void SetupServer() {
            RWConsole.LogInfo("Starting server...");
            ConnectedClients = new Dictionary<IPEndPoint, PlayerInfo>();

            Server = new UdpListener();
            Server.StartReceive();
            IsServer = true;
        }
        #endregion

        #region Client
        public static void SetupClient(string address) {
            RWConsole.LogInfo("Attempting connection to " + address);
            UnreceivedMessages = new Dictionary<string, Message>();

            Client = UdpUser.ConnectTo(address, 19000);
            Client.StartReceive();
            IsClient = true;
            Connected = true;

            RWConsole.LogInfo("Sending handshake...");
            Client.Send(new Message("handshake", Message.GenerateToken(), ""));
        }
        #endregion

        public static void SendString(string message) {
            RWConsole.LogInfo("Attempting to send string " + message);
            if (!Connected && !IsServer) {
                RWConsole.LogError("Can't send when disconnected.");
                return;
            }

            if (IsServer)
                foreach (IPEndPoint ipep in ConnectedClients.Keys) Server.Reply(Message.FromString(message), ipep);
            else {
                Message m = Message.FromString(message);
                UnreceivedMessages[m.Token] = m;
                Client.Send(m);
            }
        }

        public static void SendMessage(Message message) {
            RWConsole.LogInfo("Attempting to send message " + message.ToString());
            if (!Connected && !IsServer) {
                RWConsole.LogError("Can't send when disconnected.");
                return;
            }

            if (IsServer)
                foreach (IPEndPoint ipep in ConnectedClients.Keys) Server.Reply(message, ipep);
            else {
                UnreceivedMessages[message.Token] = message;
                Client.Send(message);
            }
        }
        public static void SendMessageTo(Message message, IPEndPoint to) {
            if (!IsServer) return;
            Server.Reply(message, to);
        }

        public class PlayerInfo {
            public string Username { get; set; }

            public bool Alive => !Creature.dead;
            public bool Dead => !Alive;

            public Player Player { get; set; }
            public Creature Creature => Player;

            public Player.AnimationIndex Animation { get => Player.animation; set { Player.animation = value; } }
            public Player.BodyModeIndex BodyMode { get => Player.bodyMode; set { Player.bodyMode = value; } }
        }
    }

    #region Networking Code
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
    #endregion
}
