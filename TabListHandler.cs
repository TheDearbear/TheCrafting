﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

using Minecraft.Entities;
using Minecraft.Packets;

namespace Minecraft
{
    public class TabListHandler
    {
        public List<TabListPlayer> Players;
        public bool Enabled
        {
            get => _enabled;
            set
            {
                if (!Enabled && value != Enabled) Start();
                _enabled = value;
            }
        }

        private bool _enabled;
        private readonly MinecraftServer Server;

        internal TabListHandler(MinecraftServer server)
        {
            Players = new List<TabListPlayer>();
            Server = server;
        }

        private void Start()
        {
            Task.Run(() =>
            {
                while (Enabled)
                {
                    Update();

                    Task.Delay(1000).GetAwaiter().GetResult();
                }
            });
        }

        /// <summary>
        /// Updates info about players and sends it to everyone
        /// </summary>
        public void Update()
        {
            IEnumerable<TabListPlayer> toRemove = Players.Where(player =>
            {
                bool notOnServer = !Server.Players.Where(serverPlayer => serverPlayer.Nickname.Equals(player.Nickname, StringComparison.OrdinalIgnoreCase)).Any();
                bool differentDisplayName = Server.Players.Where(serverPlayer => serverPlayer.DisplayName != player.DisplayName).Any();

                return notOnServer || differentDisplayName;
            });

            foreach (Player player in Server.Players)
                foreach (TabListPlayer remove in toRemove)
                    if (player.Connection.Connected)
                        player.Connection.SendPacketAsync(new PlayerListItemPacket(remove.DisplayName, false, 0));

            // Removing all real players from list ...
            Players.RemoveAll(player => !player.Dummy);

            // and adding them again!
            foreach (Player player in Server.Players)
                Players.Add(new TabListPlayer(player.Nickname) { DisplayName = player.DisplayName, IsOnline = player.Connection.Connected, Ping = player.Ping, Dummy = false });

            foreach (Player player in Server.Players)
                foreach (TabListPlayer tabPlayer in Players)
                    if (player.Connection.Connected)
                        player.Connection.SendPacketAsync(new PlayerListItemPacket(tabPlayer.DisplayName, tabPlayer.IsOnline, tabPlayer.Ping));
        }

        public struct TabListPlayer
        {
            public readonly string Nickname;

            public string DisplayName { get; set; }
            public short Ping { get; set; }
            public bool IsOnline { get; set; }
            internal bool Dummy { get; set; } = true;

            public TabListPlayer() : this("") { }
            public TabListPlayer(string nickname) => (Nickname, DisplayName, Ping, IsOnline, Dummy) = (nickname, "", 0, true, true);
        }
    }
}
