using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

using MonkArena.Networking;

namespace MonkArena {
    public class Script : MonoBehaviour {
        public MessageParser Parser { get; private set; } = new MessageParser();
    }
}
