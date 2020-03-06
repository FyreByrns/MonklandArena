using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static MonkArena.Network;

namespace MonkArena {
    public class ServerMonkScript : MonoBehaviour {
        public static ServerMonkScript Instance { get; private set; }
        static RainWorldGame Game => FindObjectOfType<RainWorld>()?.processManager?.currentMainLoop as RainWorldGame;

        public ServerMonkScript() {
            Instance = this;
            Server.MessageReceivedEvent += Server_MessageReceivedEvent;
        }

        /// <summary>
        /// [Serverside] Creates a <see cref="PlayerShell"/> with a random username
        /// </summary>
        /// <param name="sender"></param>
        private void CreateShellServerside(System.Net.IPEndPoint sender) {
            if (ConnectedClients.Keys.Where(x => x.Equals(sender)).Count() == 0) {
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

        public void Update() {
            foreach (System.Net.IPEndPoint ipep in ConnectedClients.Keys)
                ConnectedClients[ipep].Player.UpdateAnimation();
        }

        private void Server_MessageReceivedEvent(Received data) {
            Server.StartReceive(); // Start listening again immediately
            Message receivedMessage = data.Message;
            CreateShellServerside(data.Sender);

            switch (receivedMessage.Type) {
                case MessageType.PlayerAnimation:
                    int animation = int.Parse(receivedMessage.Contents);
                    ConnectedClients[data.Sender].Animation = (Player.AnimationIndex)animation;
                    SendMessageExcluding(new Message(MessageType.RemotePlayerAnimation, $"{ConnectedClients[data.Sender].Username}|{animation}"), data.Sender);
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
                    RWConsole.LogError($"[SERVER] Unable to handle message of type: {receivedMessage.Type} with contents: {receivedMessage.Contents} from: {data.Sender}");
                    break;
            }
        }
    }
}
