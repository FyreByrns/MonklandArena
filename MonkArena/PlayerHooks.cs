using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

using InputPackage = Player.InputPackage;

namespace MonkArena {
    public static class PlayerManager {
        static Player.AnimationIndex oldAnimation;
        static Vector2 lastPosition;

        public static void Hook() {
            On.Player.checkInput += Player_checkInput;
            On.Player.UpdateAnimation += Player_UpdateAnimation;
            On.Player.Update += Player_Update;

        }

        private static void Player_Update(On.Player.orig_Update orig, Player self, bool eu) {
            orig(self, eu);

            Creature playerCreature = self;
            PhysicalObject playerObject = self;

            if (oldAnimation != self.animation) { // If the animation has changed, notify the server.
                oldAnimation = self.animation;
                Network.SendMessage(new Message("player_animation", Message.GenerateToken(), $"{(int)oldAnimation}"));
            }

            if (playerObject.bodyChunks[0].pos != lastPosition) { // If the position has changed, notify the server.
                lastPosition = playerObject.bodyChunks[0].pos;
                Network.SendMessage(new Message("player_position", Message.GenerateToken(), $"{lastPosition.x},{lastPosition.y}"));
            }
        }

        private static void Player_UpdateAnimation(On.Player.orig_UpdateAnimation orig, Player self) {
        }

        private static void Player_checkInput(On.Player.orig_checkInput orig, Player self) {
            orig(self);
        }
    }
}
