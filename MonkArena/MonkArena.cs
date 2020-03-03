﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            Network.Me.MessageReceivedEvent += Me_MessageReceivedEvent;
            Network.Server.MessageReceivedEvent += Server_MessageReceivedEvent;

            On.Player.Update += Player_Update;
        }

        private void Server_MessageReceivedEvent(Received data) {
            Logger.LogInfo($"{data.Sender}: {data.Message}");
            Network.Server.Reply("received", data.Sender);
            Network.Server.StartReceive();
        }

        private void Me_MessageReceivedEvent(Received data) {
            Logger.LogInfo($"{data.Sender}: {data.Message}");
            Network.Me.StartReceive();
        }

        private void Player_Update(On.Player.orig_Update orig, Player self, bool eu) {
            orig(self, eu);

            if (Input.GetKeyDown(KeyCode.C))
                Network.Connect("127.0.0.1");

            if (Network.Connected) {
                if (Input.GetKeyDown(KeyCode.Space))
                    Network.SendMessage("test");
            }
        }

        public override void OnDisable() {
            base.OnDisable();
        }
    }
}
