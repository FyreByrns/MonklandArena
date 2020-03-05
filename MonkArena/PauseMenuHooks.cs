using System;
using Menu;
using UnityEngine;

namespace MonkArena {
    public static class PauseMenuHooks {
        public static SimpleButton disconnectButton;
        
        public static void Hook() {
            On.Menu.PauseMenu.ctor += PauseMenu_ctor;
        }
        
        public static void PauseMenu_ctor(On.Menu.PauseMenu.orig_ctor orig, PauseMenu self, ProcessManager manager, RainWorldGame game) {
            orig(self, manager, game);
            disconnectButton = new SimpleButton(self, self.pages[0], "DISCONNECT", "DISCONNECT", new Vector2(self.exitButton.pos.x - 140, 15f), new Vector2(110f, 30f));
            self.pages[0].subObjects.Add(disconnectButton);
        }
    }
}