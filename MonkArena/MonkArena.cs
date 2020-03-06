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

        public static MonkArena Instance { get; private set; }
        public ClientMonkScript clientScript;
        public ServerMonkScript serverScript;

        public MonkArena() {
            Instance = this;

            author = "Little Tiny Big";
            ModID = "MonkArena";
            Version = $"{MajorVersion}.{MinorVersion}.{Revision}";
        }

        public void AddServerScript() {
            if (Network.IsServer || Network.IsClient) return;
            Network.SetupServer();

            GameObject serverScriptObject = new GameObject();
            serverScript = serverScriptObject.AddComponent<ServerMonkScript>();
        }

        public void AddClientScript() {
            if (Network.IsServer || Network.IsClient) return;
            Network.SetupClient(Network.ServerIP, Network.ServerPort);

            GameObject clientScriptObject = new GameObject();
            clientScript = clientScriptObject.AddComponent<ClientMonkScript>();
        }

        public override void OnEnable() {
            base.OnEnable();
            Debug.Log("Using monkland.");

            RWConsole.Initialize();

            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
            On.RainWorldGame.ExitGame += RainWorldGame_ExitGame;

            PlayerManager.Hook();
            PauseMenuHooks.Hook();
            MainMenuHooks.Hook();
        }

        private void RainWorldGame_ExitGame(On.RainWorldGame.orig_ExitGame orig, RainWorldGame self, bool asDeath, bool asQuit) {
            Network.Disconnect();
        }

        private void CurrentDomain_ProcessExit(object sender, EventArgs e) {
            Network.Disconnect();
        }

        public override void OnDisable() {
            base.OnDisable();
        }
    }
}
