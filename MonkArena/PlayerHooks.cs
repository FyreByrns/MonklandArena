using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

using InputPackage = Player.InputPackage;

namespace MonkArena {
    public static class PlayerManager {
        static Player.AnimationIndex oldAnimation;


        public static void Hook() {
            On.Player.checkInput += Player_checkInput;
            On.Player.UpdateAnimation += Player_UpdateAnimation;
            On.Player.Update += Player_Update;

        }

        private static void Player_Update(On.Player.orig_Update orig, Player self, bool eu) {
            orig(self, eu);

            if (oldAnimation != self.animation) { // If the animation has changed, notify the server.
                Network.SendMessage(new Message("player_animation", Message.GenerateToken(), $"{(int)self.animation}"));
                oldAnimation = self.animation;
            }
        }

        private static void Player_UpdateAnimation(On.Player.orig_UpdateAnimation orig, Player self) {
            throw new NotImplementedException();
        }

        private static void Player_checkInput(On.Player.orig_checkInput orig, Player self) {
            orig(self);
        }
    }
}
