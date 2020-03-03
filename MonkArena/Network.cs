using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace MonkArena {
    public static class Network {
        public static UdpListener Server { get; private set; }
        public static UdpUser Me { get; private set; }
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

        public void Send(string message) {
            var datagram = Encoding.ASCII.GetBytes(message);
            Client.Send(datagram, datagram.Length);
        }
    }
}
