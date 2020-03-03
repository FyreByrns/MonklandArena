using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonkArena {
    public class Message {
        public string Type { get; private set; }
        public string Token { get; private set; }
        public string Contents { get; private set; }

        public static Message FromString(string s) => new Message() { Type = "string", Token = GenerateToken(), Contents = s };
        public override string ToString() => $"({Type}){Token}:{Contents}";

        static int tokenLength = 20;
        static Random rng = new Random();
        static string GenerateToken() {
            StringBuilder tokenBuilder = new StringBuilder();
            for (int i = 0; i < tokenLength; i++)
                tokenBuilder.Append((char)rng.Next(65, 122));
            return tokenBuilder.ToString();
        }
    }
}
