namespace MonkArena {
    public enum MessageType : byte {
        ERROR,

        Handshake,
        HandshakeAck,

        Disconnect,
        Chat,

        PlayerAnimation,
        RemotePlayerAnimation,
        PlayerChunkPosition,
        RemotePlayerChunkPosition,

        PhysicalObject,
    }
}
