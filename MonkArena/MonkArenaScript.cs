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
                Network.SetupClient("127.0.0.1");
                Network.Client.MessageReceivedEvent += Client_MessageReceivedEvent;
            }

            if (Network.Connected || Network.IsServer) {
                if (Input.GetKeyDown(KeyCode.Space))
                    Network.SendString("test");
            }
        }

        private void Server_MessageReceivedEvent(Received data) {
            Server.StartReceive(); // Start listening again immediately

            RWConsole.LogInfo($"{data.Sender}: {data.Message}");
            Message m = new Message(data.Message);

            Server.Reply(new Message("received", "", m.Token), data.Sender);

            if (!ConnectedClients.ContainsKey(data.Sender)) {
                RWConsole.LogInfo("Creating PlayerInfo...");
                ConnectedClients[data.Sender] = new PlayerInfo();

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
                    ConnectedClients[data.Sender].Player = abstractPlayer?.realizedCreature as Player;
                    if (ConnectedClients[data.Sender].Player == null) RWConsole.LogError("Player is null!");
                }
                catch (Exception e) { RWConsole.LogError(e); }
                RWConsole.LogInfo("Realized player.");

                PlayerShell playerShell = new PlayerShell(ConnectedClients[data.Sender]);
                Game.Players[0].Room.realizedRoom.AddObject(playerShell);
                RWConsole.LogInfo("Created PlayerInfo");
            }

            Message receivedMessage = new Message(data.Message);

            switch (receivedMessage.Type) {
                case "player_animation":
                    if (int.TryParse(receivedMessage.Contents, out int result)) {
                        ConnectedClients[data.Sender].Animation = (Player.AnimationIndex)result;
                    }
                    else RWConsole.LogError($"Bad animation string: {receivedMessage.Contents}");
                    break;
                case "player_position":
                    string[] pos = receivedMessage.Contents.Split(',');
                    if (float.TryParse(pos[0], out float x) && float.TryParse(pos[1], out float y)) {
                        ConnectedClients[data.Sender].Creature.bodyChunks[0].pos = new UnityEngine.Vector2(x, y);
                    }
                    else RWConsole.LogError($"Bad position string: {receivedMessage.Contents}");
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
