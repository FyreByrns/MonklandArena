namespace MonkArena {
    public enum MessageType : byte {
        /// <summary>
        /// Something went wrong parsing type
        /// </summary>
        ERROR,

        /// <summary>
        /// First message to server
        /// </summary>
        Handshake,
        /// <summary>
        /// Acknowledge handshake
        /// </summary>
        HandshakeAck,

        /// <summary>
        /// Inform of disconnect
        /// </summary>
        Disconnect,
        /// <summary>
        /// Chat message
        /// <para><see cref="short"/> sender id, <see cref="string"/> message</para>
        /// </summary>
        Chat,

        PlayerAnimation,
        RemotePlayerAnimation,
        PlayerChunkPosition,
        RemotePlayerChunkPosition,

        // PhysicalObject stuff
        /// <summary>
        /// [Server -> Client] Tell client to create a network controlled <see cref="PhysicalObject"/>
        /// <para>
        /// <see cref="short"/> owner ID,
        /// <see cref="short"/> length of <see cref="BodyChunk"/> array,
        /// <see cref="float"/> air friction,
        /// <see cref="float"/> water friction,
        /// <see cref="float"/> surface friction,
        /// <see cref="float"/> buoyancy,
        /// <see cref="float"/> bounce,
        /// <see cref="bool"/> canBeHitByWeapons
        /// </para>
        /// </summary>
        PhysicalObjectInitialize,
        /// <summary>
        /// [Server -> Client] Populate <see cref="BodyChunk"/> in network controlled <see cref="PhysicalObject"/>
        /// <para>
        /// <see cref="short"/> owner ID,
        /// <see cref="short"/> index,
        /// <see cref="float"/> posX,
        /// <see cref="float"/> posY,
        /// <see cref="float"/> radius,
        /// <see cref="float"/> mass,
        /// </para>
        /// </summary>
        BodyChunkInitialize,
        /// <summary>
        /// [Server -> Client] BodyChunk update
        /// <para>
        /// <see cref="short"/> owner ID, 
        /// <see cref="short"/> chunk index, 
        /// <see cref="float"/> posX, <see cref="float"/> posY, 
        /// <see cref="float"/> rotX, <see cref="float"/> rotY,
        /// <see cref="float"/> velX, <see cref="float"/> velY
        /// </para>
        /// </summary>
        BodyChunkUpdate,
        GraphicsModule,
    }
}
