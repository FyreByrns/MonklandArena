using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MonkArena {
    public class MonkArena : Partiality.Modloader.PartialityMod {
        #region Versioning
        public string MajorVersion { get; } = "0";
        public string MinorVersion { get; } = "0";
        public string Revision { get; } = "1";
        #endregion

        public MonkArena() {
            author = "Little Tiny Big";
            ModID = "MonkArena";
            Version = $"{MajorVersion}.{MinorVersion}.{Revision}";

            GameObject scriptObject = new GameObject();
            scriptObject.AddComponent<MonkArenaScript>();
        }

        public override void OnEnable() {
            base.OnEnable();
            Logger.LogInfo("Enabled");
        }

        public override void OnDisable() {
            base.OnDisable();
        }
    }
}
