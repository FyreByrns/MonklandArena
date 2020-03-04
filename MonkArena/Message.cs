using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonkArena {
    public class Message {
        public string Type { get; private set; }
        public string Token { get; private set; }
        public string Contents { get; private set; }

        public static string GetToken(string s) => s.Split(')')[1].Split(':')[0];

        private Message() { }
        public Message(string from) {
            string[] data = from.Split(':');
            Type = data[0];
            Token = data[1];
            Contents = data[2];
        }
        public Message(string type, string token, string contents) {
            Type = type;
            Token = token;
            Contents = contents;
        }

        public static Message FromString(string s) => new Message() { Type = "string", Token = GenerateToken(), Contents = s };
        public static Message FromInt(int i) => new Message() { Type = "int", Token = GenerateToken(), Contents = $"{i}" };
        public static Message FromFloat(float f) => new Message() { Type = "float", Token = GenerateToken(), Contents = $"{f}" };

        public override string ToString() => $"{Type}:{Token}:{Contents}";

        static int tokenLength = 20;
        static Random rng = new Random();
        public static string GenerateToken() {
            StringBuilder tokenBuilder = new StringBuilder();
            for (int i = 0; i < tokenLength; i++)
                tokenBuilder.Append((char)rng.Next('A', 'Z'));
            return tokenBuilder.ToString();
        }
    }
}
