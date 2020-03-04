using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MonkArena {
    public class PlayerShell : UpdatableAndDeletable, IDrawable {
        public PlayerGraphics Graphics { get; set; }

        public PlayerShell(Network.PlayerInfo info) {
            RWConsole.LogInfo("Playershell being created.");

            Graphics = new PlayerGraphics(info.Player);
            info.Shell = this;
        }

        public override void Update(bool eu) {
            base.Update(eu);

            RWConsole.LogInfo("PlayerShell updating");
            Graphics.Update();
        }

        #region IDrawable
        public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner) {
            Graphics.AddToContainer(sLeaser, rCam, newContatiner);
        }
        public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette) {
            Graphics.ApplyPalette(sLeaser, rCam, palette);
        }
        public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos) {
            Graphics.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        }
        public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam) {
            Graphics.InitiateSprites(sLeaser, rCam);
        }
        #endregion
    }
}
