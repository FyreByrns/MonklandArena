using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MonkArena {
    public class MonkArena : Partiality.Modloader.PartialityMod {
        #region Versioning
        public string MajorVersion { get; } = "0";
        public string MinorVersion { get; } = "0";
        public string Revision { get; } = "1";
        #endregion

        public MonkArena() {
            author = "Little Tiny Big";
            ModID = "MonkArena";
            Version = $"{MajorVersion}.{MinorVersion}.{Revision}";
        }

        public override void OnEnable() {
            base.OnEnable();

            Debug.Log("------------------------------------------------------------INITIALIZING LOGGER");
            Logger.Initialize();

            On.Player.Update += Player_Update;
        }

        private void Player_Update(On.Player.orig_Update orig, Player self, bool eu) {
            orig(self, eu);

            if (Input.GetKeyDown(KeyCode.Space))
                Logger.LogInfo("ASFJHLAKSJDHLKAJFHSLKJFSHLKJFSH");

            if (Input.GetKeyDown(KeyCode.C))
                Network.Connect("127.0.0.1");

            if (Network.Connected) {
                Received result = Network.Server.Receive().Result;
                Logger.LogInfo($"{result.Sender}: {result.Message}");

                if (Input.GetKeyDown(KeyCode.Space))
                    Network.SendMessage("test");
            }
        }

        public override void OnDisable() {
            base.OnDisable();
        }
    }
}
