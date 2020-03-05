using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

using InputPackage = Player.InputPackage;

namespace MonkArena {
    /// <summary>
    /// Player management for the current RainWorld instance
    /// </summary>
    public static class PlayerManager {
        static Player.AnimationIndex oldAnimation;
        static Dictionary<BodyChunk, Vector2> previousPositions = new Dictionary<BodyChunk, Vector2>();

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

            foreach (BodyChunk chunk in self.bodyChunks) {
                if (Vector2.Distance(chunk.pos, previousPositions[chunk]) > 0) { // If the position has changed enough, notify the server.
                    previousPositions[chunk] = chunk.pos;
                    int chunkIndex = self.bodyChunks.IndexOf(chunk);

                    if (Network.IsClient) {
                        Network.SendMessage(new Message("player_chunkposition", Message.GenerateToken(), $"{chunkIndex}|{chunk.pos.x},{chunk.pos.y}"));
                    }
                    else if (Network.IsServer) {
                        Network.SendMessage(new Message("remoteplayer_chunkposition", "", $"server|{chunkIndex}|{chunk.pos.x},{chunk.pos.y}"));
                     }
                }
            }
        }

        private static void Player_UpdateAnimation(On.Player.orig_UpdateAnimation orig, Player self) {
            orig(self);
        }

        private static void Player_checkInput(On.Player.orig_checkInput orig, Player self) {
            orig(self);
        }
    }
}
