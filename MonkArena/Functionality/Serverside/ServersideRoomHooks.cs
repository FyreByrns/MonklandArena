using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonkArena.Functionality.Serverside {
    public static class ServersideRoomHooks {
        public static Dictionary<short, PhysicalObject> PhysicalObjects { get; } = new Dictionary<short, PhysicalObject>();

        public static void Hook() {
            On.Room.Update += Room_Update;
            On.Room.AddObject += Room_AddObject;
            On.Room.RemoveObject += Room_RemoveObject;
        }

        private static void Room_AddObject(On.Room.orig_AddObject orig, Room self, UpdatableAndDeletable obj) {
            orig(self, obj);

            if(obj is PhysicalObject pObj) { // Inform clients of added object


                PhysicalObjects[ServerMonkScript.GetNewID()] = pObj;
            }
        }

        private static void Room_RemoveObject(On.Room.orig_RemoveObject orig, Room self, UpdatableAndDeletable obj) {
            orig(self, obj);
        }

        private static void Room_Update(On.Room.orig_Update orig, Room self) {
            orig(self);
        }
    }
}
