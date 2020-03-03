using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MonkArena {
    public class MonkArenaScript : MonoBehaviour {
        public void Update() {
            if (Input.GetKeyUp(KeyCode.S)) {
                Network.SetupServer();
                Network.Server.MessageReceivedEvent += Server_MessageReceivedEvent;
            }

            if (Input.GetKeyUp(KeyCode.C)) {
                Network.SetupClient("127.0.0.1");
                Network.Me.MessageReceivedEvent += Me_MessageReceivedEvent;
            }

            if (Network.Connected) {
                if (Input.GetKeyDown(KeyCode.Space))
                    Network.SendMessage("test");
            }
        }

        private void Server_MessageReceivedEvent(Received data) {
            RWConsole.LogInfo($"{data.Sender}: {data.Message}");
            Network.Server.Reply("received", data.Sender);
            Network.Server.StartReceive();
        }

        private void Me_MessageReceivedEvent(Received data) {
            RWConsole.LogInfo($"{data.Sender}: {data.Message}");
            Network.Me.StartReceive();
        }
    }
}
