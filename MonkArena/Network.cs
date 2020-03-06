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

        public static string ServerIP { get; set; } = "127.0.0.1";
        public static int ServerPort { get; set; } = 19000;

        /// <summary>
        /// [Clientside] Set of server-controlled <see cref="PlayerInfo"/>s by username
        /// </summary>
        public static Dictionary<string, PlayerInfo> RemotePlayers { get; private set; }
        /// <summary>
        /// [Serverside] All <see cref="PlayerInfo"/>s for all connected clients by IP
        /// </summary>
        public static Dictionary<IPEndPoint, PlayerInfo> ConnectedClients { get; private set; }

        static Network() {
        }

        /// <summary>
        /// Notifies of disconnection
        /// </summary>
        public static void Disconnect() {
            if (IsServer) SendMessage(new Message(MessageType.Disconnect, ""));
            else if (IsClient) SendMessage(new Message(MessageType.Disconnect, ""));
        }

        #region Server
        /// <summary>
        /// Makes the current instance of RainWorld a server.
        /// </summary>
        public static void SetupServer() {
            RWConsole.LogInfo("Starting server...");

            ConnectedClients = new Dictionary<IPEndPoint, PlayerInfo>();

            Server = new UdpListener();
            MonkArena.Instance.AddServerScript();
            Server.StartReceive();
            IsServer = true;

        }
        #endregion

        #region Client
        /// <summary>
        /// [Clientside] Connects to the server.
        /// </summary>
        /// <param name="address">Server address</param>
        /// <param name="port">Server port</param>
        public static void SetupClient(string address, int port) {
            RWConsole.LogInfo("Attempting connection to " + address);

            RemotePlayers = new Dictionary<string, PlayerInfo>();

            Client = UdpUser.ConnectTo(address, port);
            MonkArena.Instance.AddClientScript();
            Client.StartReceive();
            IsClient = true;
            Connected = true;

            RWConsole.LogInfo("Sending handshake...");
            Client.Send(new Message(MessageType.Handshake, ""));

        }
        #endregion

        /// <summary>
        /// <para>[Clientside] Sends a message to the server.</para>
        /// <para>[Serverside] Sends a message to all clients.</para>
        /// </summary>
        /// <param name="message"></param>
        public static void SendMessage(Message message) {
            //RWConsole.LogInfo("Attempting to send message " + message.ToString());
            if (!Connected && !IsServer) {
                RWConsole.LogError("Can't send when disconnected.");
                return;
            }

            if (IsServer)
                foreach (IPEndPoint ipep in ConnectedClients.Keys) Server.Reply(message, ipep);
            else {
                Client.Send(message);
            }
        }
        /// <summary>
        /// [Serverside] Sends a message to a specific client.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="to"></param>
        public static void SendMessageTo(Message message, IPEndPoint to) {
            if (!IsServer) return;
            Server.Reply(message, to);
        }
        /// <summary>
        /// [Serverside] Sends a message to all clients except one
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exclusion"></param>
        public static void SendMessageExcluding(Message message, IPEndPoint exclusion) {
            if (IsServer)
                foreach (IPEndPoint ipep in ConnectedClients.Keys)
                    if (!ipep.Equals(exclusion))
                        Server.Reply(message, ipep);
        }

        /// <summary>
        /// Information relevant to networked players
        /// </summary>
        public class PlayerInfo {
            public string Username { get; set; }

            public bool Alive => !Creature.dead;
            public bool Dead => !Alive;

            public Player Player { get; set; }
            public PlayerShell Shell { get; set; }
            public Creature Creature => Player;

            public Player.AnimationIndex Animation { get => Player.animation; set { Player.animation = value; } }
            public Player.BodyModeIndex BodyMode { get => Player.bodyMode; set { Player.bodyMode = value; } }
        }
    }

    #region Networking Code
    public struct Received {
        public IPEndPoint Sender { get; set; }
        public Message Message { get; set; }
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
            Message receivedMessage = new Message(receivedBytes);

            //RWConsole.LogInfo($"Received: {receivedString} From: {e}");
            MessageReceivedEvent?.Invoke(new Received() { Sender = e, Message = receivedMessage });
        }
    }

    public class UdpListener : UdpBase {
        IPEndPoint listenOn;

        public UdpListener() : this(new IPEndPoint(IPAddress.Any, Network.ServerPort)) { }
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
            var datagram = message.GetData();
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
            var datagram = message.GetData();
            Client.Send(datagram, datagram.Length);
        }
    }
    #endregion
}
