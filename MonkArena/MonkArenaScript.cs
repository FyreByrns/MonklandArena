using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

using static MonkArena.Network;

namespace MonkArena {
    public class MonkArenaScript : MonoBehaviour {
        public static MonkArenaScript Instance { get; private set; }
        static RainWorldGame Game => FindObjectOfType<RainWorld>()?.processManager?.currentMainLoop as RainWorldGame;

        public MonkArenaScript() {
            Instance = this;
        }

        public void Update() {
            if (Input.GetKeyUp(KeyCode.S) && !IsServer && !IsClient) {
                Network.SetupServer();
                Network.Server.MessageReceivedEvent += Server_MessageReceivedEvent;
            }

            if (Input.GetKeyUp(KeyCode.C) && !IsServer && !IsClient) {
                Network.SetupClient(ServerIP, ServerPort);
                Network.Client.MessageReceivedEvent += Client_MessageReceivedEvent;
            }

            if (IsServer)
                foreach (System.Net.IPEndPoint ipep in ConnectedClients.Keys)
                    ConnectedClients[ipep].Player.UpdateAnimation();
        }

        /// <summary>
        /// [Serverside] Creates a <see cref="PlayerShell"/> with a random username
        /// </summary>
        /// <param name="sender"></param>
        private void CreateShellServerside(System.Net.IPEndPoint sender) {
            if (!ConnectedClients.ContainsKey(sender)) {
                RWConsole.LogInfo("Creating PlayerInfo...");
                ConnectedClients[sender] = new PlayerInfo() {
                    Username = Message.GenerateToken()
                };

                RWConsole.LogInfo("Creating abstract player...");
                AbstractCreature abstractPlayer = null;
                try {
                    abstractPlayer = new AbstractCreature
                        (Game.world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Slugcat), null, new WorldCoordinate(), new EntityID());
                }
                catch (Exception e) {
                    if (Game == null) RWConsole.LogError("Game is null!");
                    RWConsole.LogError(e);
                }
                RWConsole.LogInfo("Created abstract player.");

                RWConsole.LogInfo("Creating PlayerState...");
                PlayerState playerState = new PlayerState(abstractPlayer, 0, 1, false);
                abstractPlayer.state = playerState;
                RWConsole.LogInfo("Created PlayerState.");

                RWConsole.LogInfo("Realizing player...");
                try {
                    abstractPlayer?.Realize();
                    ConnectedClients[sender].Player = abstractPlayer?.realizedCreature as Player;
                    if (ConnectedClients[sender].Player == null) RWConsole.LogError("Player is null!");
                }
                catch (Exception e) { RWConsole.LogError(e); }
                RWConsole.LogInfo("Realized player.");

                PlayerShell playerShell = new PlayerShell(ConnectedClients[sender]);
                Game.Players[0].Room.realizedRoom.AddObject(playerShell);
                RWConsole.LogInfo("Created PlayerInfo");
            }
        }
        /// <summary>
        /// [Clientside] Creates a <see cref="PlayerShell"/>
        /// </summary>
        /// <param name="username"></param>
        private void CreateShellClientside(string username) {
            if (!RemotePlayers.ContainsKey(username)) {
                RWConsole.LogInfo($"Creating PlayerInfo for {username}...");
                RemotePlayers[username] = new PlayerInfo();

                RWConsole.LogInfo("Creating abstract player...");
                AbstractCreature abstractPlayer = null;
                try {
                    abstractPlayer = new AbstractCreature
                        (Game.world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Slugcat), null, new WorldCoordinate(), new EntityID());
                }
                catch (Exception e) {
                    if (Game == null) RWConsole.LogError("Game is null!");
                    RWConsole.LogError(e);
                }
                RWConsole.LogInfo("Created abstract player.");

                RWConsole.LogInfo("Creating PlayerState...");
                PlayerState playerState = new PlayerState(abstractPlayer, 0, 1, false);
                abstractPlayer.state = playerState;
                RWConsole.LogInfo("Created PlayerState.");

                RWConsole.LogInfo("Realizing player...");
                try {
                    abstractPlayer?.Realize();
                    RemotePlayers[username].Player = abstractPlayer?.realizedCreature as Player;
                    if (RemotePlayers[username].Player == null) RWConsole.LogError("Player is null!");
                }
                catch (Exception e) { RWConsole.LogError(e); }
                RWConsole.LogInfo("Realized player.");

                PlayerShell playerShell = new PlayerShell(RemotePlayers[username]);
                Game.Players[0].Room.realizedRoom.AddObject(playerShell);
                RWConsole.LogInfo($"Created PlayerInfo for {username}");
            }
        }

        private void Server_MessageReceivedEvent(Received data) {
            Server.StartReceive(); // Start listening again immediately

            Message receivedMessage = data.Message;
            //RWConsole.LogInfo($"{data.Sender}: {data.Message}");
            CreateShellServerside(data.Sender);

            switch (receivedMessage.Type) {
                case MessageType.PlayerAnimation:
                    if (int.TryParse(receivedMessage.Contents, out int result)) {
                        ConnectedClients[data.Sender].Animation = (Player.AnimationIndex)result;
                    }
                    else RWConsole.LogError($"Bad animation string: {receivedMessage.Contents}");
                    break;
                case MessageType.PlayerChunkPosition:
                    string[] pos = receivedMessage.Contents.Split('|', ',');

                    if (!int.TryParse(pos[0], out int chunkIndex)) RWConsole.LogError("Bad chunkindex");
                    if (!float.TryParse(pos[1], out float x)) RWConsole.LogError("Bad chunkposition x");
                    if (!float.TryParse(pos[2], out float y)) RWConsole.LogError("Bad chunkposition y");

                    if (!float.TryParse(pos[3], out float rx)) RWConsole.LogError("Bad chunkrotation x");
                    if (!float.TryParse(pos[4], out float ry)) RWConsole.LogError("Bad chunkrotation y");
                    if (!float.TryParse(pos[5], out float velx)) RWConsole.LogError("Bad velocity x");
                    if (!float.TryParse(pos[6], out float vely)) RWConsole.LogError("Bad velocity y");

                    chunkIndex = int.Parse(pos[0]);
                    Vector2 chunkPosition = new Vector2(x, y);
                    Vector2 chunkRotation = new Vector2(rx, ry);
                    Vector2 chunkVelocity = new Vector2(velx, vely);

                    ConnectedClients[data.Sender].Creature.bodyChunks[chunkIndex].pos = chunkPosition;
                    ConnectedClients[data.Sender].Creature.bodyChunks[chunkIndex].Rotation.Set(rx, ry);
                    ConnectedClients[data.Sender].Creature.bodyChunks[chunkIndex].vel = chunkVelocity;

                    SendMessageExcluding(new Message(
                        MessageType.RemotePlayerChunkPosition, $"{ConnectedClients[data.Sender].Username}|{chunkIndex}|" +
                        $"{chunkPosition.x},{chunkPosition.y},{chunkRotation.x},{chunkRotation.y}"
                        ),
                        data.Sender);
                    break;

                case MessageType.Handshake:
                    SendMessageTo(new Message(MessageType.HandshakeAck, ""), data.Sender);
                    break;

                default:
                    RWConsole.LogError($"Unable to handle message of type: {receivedMessage.Type} with contents: {receivedMessage.Contents}");
                    break;
            }
        }

        private void Client_MessageReceivedEvent(Received data) {
            Client.StartReceive(); // Start listening again immediately

            //if (!IsServer) RWConsole.LogInfo($"{data.Sender}: {data.Message}");
            //RWConsole.LogInfo($"{Network.UnreceivedMessages.Count} messages not received by server.");

            Message receivedMessage = data.Message;

            switch (receivedMessage.Type) {
                case MessageType.RemotePlayerChunkPosition:
                    string[] pos = receivedMessage.Contents.Split('|', ',');

                    string username = pos[0];
                    int chunkIndex = int.Parse(pos[1]);

                    if (!float.TryParse(pos[4], out float rx)) RWConsole.LogError("Bad chunkrotation x");
                    if (!float.TryParse(pos[5], out float ry)) RWConsole.LogError("Bad chunkrotation y");

                    if (!float.TryParse(pos[6], out float velx)) RWConsole.LogError("Bad velocity x");
                    if (!float.TryParse(pos[7], out float vely)) RWConsole.LogError("Bad velocity y");

                    Vector2 chunkPosition = new Vector2(float.Parse(pos[2]), float.Parse(pos[3]));
                    Vector2 chunkRotation = new Vector2(rx, ry);
                    Vector2 chunkVelocity = new Vector2(velx, vely);

                    CreateShellClientside(username);
                    RemotePlayers[username].Creature.bodyChunks[chunkIndex].pos = chunkPosition;
                    RemotePlayers[username].Creature.bodyChunks[chunkIndex].Rotation.Set(rx, ry);
                    RemotePlayers[username].Creature.bodyChunks[chunkIndex].vel = chunkVelocity;
                    break;

                default:
                    RWConsole.LogError($"Unable to handle message of type: {receivedMessage.Type} with contents: {receivedMessage.Contents}");
                    break;
            }

        }
    }
}
