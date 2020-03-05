using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonkArena {
    public enum MessageType : byte {

    }

    public class Message {
        public MessageType Type { get; private set; }
        public short MessageLength { get; private set; }
        public string Token { get; private set; }
        public string Contents { get; private set; }

        private const int TYPE_LENGTH = 1;
        private const int LENGTH_LENGTH = 2;

        private Message() { }
        public Message(byte[] from) {
            int currentPosition = 0;

            byte[] type = new byte[1];
            Buffer.BlockCopy(from, currentPosition, type, 0, TYPE_LENGTH);
            currentPosition += TYPE_LENGTH;
            Type = (MessageType)type[0];

            short[] length = new short[1];
            Buffer.BlockCopy(from, currentPosition, length, 0, LENGTH_LENGTH);
            currentPosition += LENGTH_LENGTH;
            MessageLength = length[0];

            byte[] contents = new byte[MessageLength];
            Buffer.BlockCopy(from, currentPosition, contents, 0, MessageLength);

            string[] stringContents = Encoding.ASCII.GetString(contents).Split(':');
            Token = stringContents[0];
            Contents = stringContents[1];
        }
        public Message(MessageType type, string token, string contents) {
            Type = type;
            Token = token;
            Contents = contents;

            MessageLength = (short)Encoding.ASCII.GetByteCount(token + contents);
        }

        public override string ToString() => $"{Type}:{Token}:{Contents}";
        public byte[] GetSaveData() {
            byte[] data = new byte[TYPE_LENGTH + LENGTH_LENGTH + MessageLength];
            int currentPosition = 0;

            data[currentPosition] = (byte)Type;
            currentPosition += TYPE_LENGTH;

            short[] length = new[] { MessageLength };
            Buffer.BlockCopy(length, 0, data, currentPosition, LENGTH_LENGTH);
            currentPosition += LENGTH_LENGTH;

            byte[] contents = Encoding.ASCII.GetBytes(Token + ":" + Contents);
            Buffer.BlockCopy(contents, 0, data, currentPosition, MessageLength);
            return data;
        }

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
