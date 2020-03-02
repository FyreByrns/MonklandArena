using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MonkArena {
    public class MonkArenaScript : MonoBehaviour {
        public MonkArenaScript() {
        }

        public void Update() {
            if (Input.GetKeyDown(KeyCode.C))
                Network.Connect("127.0.0.1");

            if (Network.Connected) {
                Received result = Network.Server.Receive().Result;
                Logger.LogInfo($"{result.Sender}: {result.Message}");

                if (Input.GetKeyDown(KeyCode.Space))
                    Network.SendMessage("test");
            }
        }
    }
}
