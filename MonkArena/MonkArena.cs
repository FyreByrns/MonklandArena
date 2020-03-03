using System;
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

        MonkArenaScript script;

        public MonkArena() {
            author = "Little Tiny Big";
            ModID = "MonkArena";
            Version = $"{MajorVersion}.{MinorVersion}.{Revision}";
        }

        public override void OnEnable() {
            base.OnEnable();

            Debug.Log("------------------------------------------------------------INITIALIZING LOGGER");
            RWConsole.Initialize();

            GameObject scriptObject = new GameObject();
            script = scriptObject.AddComponent<MonkArenaScript>();
            script = new MonkArenaScript();

            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
            On.RainWorldGame.ExitGame += RainWorldGame_ExitGame;

            On.Player.Update += Player_Update;
        }

        private void RainWorldGame_ExitGame(On.RainWorldGame.orig_ExitGame orig, RainWorldGame self, bool asDeath, bool asQuit) {
            Network.Disconnect();
        }

        private void CurrentDomain_ProcessExit(object sender, EventArgs e) {
            Network.Disconnect();
        }

        private void Player_Update(On.Player.orig_Update orig, Player self, bool eu) {
            orig(self, eu);
        }

        public override void OnDisable() {
            base.OnDisable();
        }
    }
}
