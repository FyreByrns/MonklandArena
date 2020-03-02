using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            ServerReceive();
        }

        static void ServerReceive() {
            Thread t = new Thread(new ThreadStart(async () => {
                while (true) {
                    Received received = await Server.Receive();
                    Logger.LogInfo($"{received.Sender}: {received.Message}");
                };
            }));
        }

        public static void Connect(string address) {
            Me = UdpUser.ConnectTo(address, 19000);
            Connected = true;
        }

        public static void SendMessage(string message) {
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
        protected UdpClient Client;

        protected UdpBase() {
            Client = new UdpClient();
        }

        public async Task<Received> Receive() {
            var result = await Client.ReceiveAsync();
            return new Received() {
                Message = Encoding.ASCII.GetString(result.Buffer, 0, result.Buffer.Length),
                Sender = result.RemoteEndPoint,
            };
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
