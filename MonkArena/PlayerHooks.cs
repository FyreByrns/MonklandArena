using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RWCustom;

using InputPackage = Player.InputPackage;

namespace MonkArena {
    public static class PlayerHooks {
        public static void Hook() {
            On.Player.checkInput += Player_checkInput;
        }

        private static void Player_checkInput(On.Player.orig_checkInput orig, Player self) {
            orig(self);
        }
    }
}
