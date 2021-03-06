﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

using static MonkArena.Network;

namespace MonkArena {
    public class ClientMonkScript : Script {
        public static ClientMonkScript Instance { get; private set; }
        static RainWorldGame Game => FindObjectOfType<RainWorld>()?.processManager?.currentMainLoop as RainWorldGame;

        public ClientMonkScript() {
            Instance = this;
            Client.MessageReceivedEvent += Client_MessageReceivedEvent;

            Parser.AddRule(MessageType.HandshakeAck, (Message receivedMessage, Received data) => {
                RWConsole.LogInfo("Handshake acknowledged!");
            });
            Parser.AddRule(MessageType.RemotePlayerAnimation, (Message receivedMessage, Received data) => {
                string[] anim = receivedMessage.Contents.Split('|');
                string username = anim[0];
                int animation = int.Parse(anim[1]);
                int frame = int.Parse(anim[2]);

                Player.AnimationIndex animationIndex = (Player.AnimationIndex)animation;
                typeof(Player).GetProperty("animationFrame").SetValue(RemotePlayers[username].Player, frame, null);
                RemotePlayers[username].Animation = animationIndex;
            });
            Parser.AddRule(MessageType.RemotePlayerChunkPosition, (Message receivedMessage, Received data) => {
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
            });
        }

        public void Update() {
            foreach (string name in RemotePlayers.Keys) {
                int oldFrame = RemotePlayers[name].Player.animationFrame;
                RemotePlayers[name].Player.UpdateAnimation();
                RemotePlayers[name].Player.SetPrivatePropertyValue("animationFrame", oldFrame);
            }
        }

        public void OnEnable() {
            RWConsole.LogInfo("Sending handshake...");
            Client.Send(new Message(MessageType.Handshake, ""));
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

        private void Client_MessageReceivedEvent(Received data) {
            Client.StartReceive(); // Start listening again immediately
            Message receivedMessage = data.Message;

            if (Parser.TryParse(receivedMessage, data)) ; // Parse message
            else RWConsole.LogError($"Couldn't parse message with type {receivedMessage.Type} and contents {receivedMessage.Contents}");
        }
    }
}
