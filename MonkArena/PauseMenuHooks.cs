using System;
using Menu;
using UnityEngine;

namespace MonkArena {
    public static class PauseMenuHooks {
        public static SimpleButton disconnectButton;
        public static SimpleButton startClient;

        public static void Hook() {
            On.Menu.PauseMenu.ctor += PauseMenu_ctor;
            On.Menu.PauseMenu.Singal += PauseMenu_Singal;
        }
        
        public static void PauseMenu_ctor(On.Menu.PauseMenu.orig_ctor orig, PauseMenu self, ProcessManager manager, RainWorldGame game) {
            orig(self, manager, game);
            disconnectButton = new SimpleButton(self, self.pages[0], "DISCONNECT", "DISCONNECT", new Vector2(self.exitButton.pos.x - 140, 15f), new Vector2(110f, 30f));
            startClient = new SimpleButton(self, self.pages[0], "START CLIENT", "START_CLIENT", new Vector2(disconnectButton.pos.x - 150, 15f), new Vector2(110f, 30f));
            self.pages[0].subObjects.Add(disconnectButton);
            self.pages[0].subObjects.Add(startClient);
        }
        
        public static void PauseMenu_Singal(On.Menu.PauseMenu.orig_Singal orig, PauseMenu self, MenuObject sender, string message) {
            orig(self, sender, message);
            if (message == "DISCONNECT") {
                Network.Disconnect();
                self.Singal(sender, "EXIT");
            }

            if (message == "START_CLIENT") Network.SetupClient(Network.ServerIP, Network.ServerPort);

        }
    }
}