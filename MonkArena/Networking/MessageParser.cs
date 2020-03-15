using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonkArena.Networking {
    public class MessageParser {
        public Dictionary<MessageType, List<Action<Message, Received>>> Rules { get; private set; }
            = new Dictionary<MessageType, List<Action<Message, Received>>>();

        public void AddRule(MessageType reactTo, Action<Message, Received> result) {
            if (!Rules.ContainsKey(reactTo)) Rules[reactTo] = new List<Action<Message, Received>>();
            Rules[reactTo].Add(result);
        }

        public bool TryParse(Message message, Received data) {
            if (!Rules.ContainsKey(message.Type)) return false;

            foreach (Action<Message, Received> result in Rules[message.Type])
                result(message, data);

            return true;
        }
    }
}
