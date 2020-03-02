using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MonkArena {
    public class MonkArenaScript : MonoBehaviour {
        public MonkArenaScript() {
            Network.Connect("127.0.0.1");
        }

        public void Update() {
            Received result = Network.Server.Receive().Result;
            Logger.LogInfo($"{result.Sender}: {result.Message}");

            if (Input.GetKeyDown(KeyCode.Space))
                Network.SendMessage("test");
        }
    }
}
