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
        }

        private void CreateShellServerside(System.Net.IPEndPoint sender) {
            if (!ConnectedClients.ContainsKey(sender)) {
                RWConsole.LogInfo("Creating PlayerInfo...");
                ConnectedClients[sender] = new PlayerInfo();

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

        private void Server_MessageReceivedEvent(Received data) {
            Server.StartReceive(); // Start listening again immediately

            RWConsole.LogInfo($"{data.Sender}: {data.Message}");
            Message m = new Message(data.Message);

            CreateShellServerside(data.Sender);

            Server.Reply(new Message("received", "", m.Token), data.Sender);

            Message receivedMessage = new Message(data.Message);

            switch (receivedMessage.Type) {
                case "player_animation":
                    if (int.TryParse(receivedMessage.Contents, out int result)) {
                        ConnectedClients[data.Sender].Animation = (Player.AnimationIndex)result;
                    }
                    else RWConsole.LogError($"Bad animation string: {receivedMessage.Contents}");
                    break;
                case "player_chunkposition":
                    string[] pos = receivedMessage.Contents.Split('|', ',');
                    int chunkIndex = int.Parse(pos[0]);
                    Vector2 chunkPosition = new Vector2(float.Parse(pos[1]), float.Parse(pos[2]));

                    ConnectedClients[data.Sender].Creature.bodyChunks[chunkIndex].pos = chunkPosition;
                    break;

                case "received":
                    UnreceivedMessages.Remove(receivedMessage.Contents);
                    break;
                case "handshake":
                    SendMessageTo(new Message("handshake_approved", "", ""), data.Sender);
                    break;

                default:
                    RWConsole.LogError($"Unable to handle message of type: {receivedMessage.Type} with contents: {receivedMessage.Contents}");
                    break;
            }
        }

        private void Client_MessageReceivedEvent(Received data) {
            Client.StartReceive(); // Start listening again immediately

            if (!IsServer) RWConsole.LogInfo($"{data.Sender}: {data.Message}");

            RWConsole.LogInfo($"{Network.UnreceivedMessages.Count} messages not received by server.");
            Message m = new Message(data.Message);
            if (m.Type == "received")
                UnreceivedMessages.Remove(m.Contents);
        }
    }
}
