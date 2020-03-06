using Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vector2 = UnityEngine.Vector2;

namespace MonkArena {
    public static class MainMenuHooks {
        public static SimpleButton startServer;
        public static SimpleButton startClient;

        public static void Hook() {
            On.Menu.MainMenu.ctor += MainMenu_ctor;
            On.Menu.MainMenu.Singal += MainMenu_Singal;
        }

        private static void MainMenu_ctor(On.Menu.MainMenu.orig_ctor orig, Menu.MainMenu self, ProcessManager manager, bool showRegionSpecificBkg) {
            orig(self, manager, showRegionSpecificBkg);

            startServer = new SimpleButton(self, self.pages[0], "START SERVER", "START_SERVER", new Vector2(140f, 400f), new Vector2(110f, 30f));
            startClient = new SimpleButton(self, self.pages[0], "START CLIENT", "START_CLIENT", new Vector2(startServer.pos.x + 120, startServer.pos.y), new Vector2(110f, 30f));

            self.pages[0].subObjects.Add(startServer);
            self.pages[0].subObjects.Add(startClient);
        }

        private static void MainMenu_Singal(On.Menu.MainMenu.orig_Singal orig, MainMenu self, MenuObject sender, string message) {
            orig(self, sender, message);

            if (message == "START_SERVER") Network.SetupServer();
            if (message == "START_CLIENT") Network.SetupClient(Network.ServerIP, Network.ServerPort);
        }
    }
}
