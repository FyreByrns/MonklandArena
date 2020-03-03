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
        public static UdpUser Me { get; private set; }
        public static bool Connected { get; private set; }

        static Network() {
            Server = new UdpListener();
        }

        public static void Connect(string address) {
            Logger.LogInfo("Attempting connection to " + address);
            Me = UdpUser.ConnectTo(address, 19000);
            Server.StartReceive();
            Me.StartReceive();
            Connected = true;
        }

        public static void SendMessage(string message) {
            Logger.LogInfo("Attempting to send message " + message);
            if (!Connected) {
                Logger.LogError("Can't send messages when disconnected.");
                return;
            }

            Me.Send(message);
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

            Logger.LogInfo($"Received: {receivedString} From: {e}");
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

        public void Reply(string message, IPEndPoint endpoint) {
            var datagram = Encoding.ASCII.GetBytes(message);
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

        public void Send(string message) {
            var datagram = Encoding.ASCII.GetBytes(message);
            Client.Send(datagram, datagram.Length);
        }
    }
}
