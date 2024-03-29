﻿using System;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Minecraft.Packets;

namespace Minecraft
{
    public class PlayerConnection
    {
        private readonly Socket Client;

        public MinecraftServer Server { get; private set; }
        public bool Connected { get => Client.Connected; }
        public string Nickname { get; internal set; }
        public bool IsPlayer { get; internal set; }

        internal PlayerConnection(MinecraftServer server, Socket client)
        {
            Server = server;
            Client = client;
            Nickname = string.Empty;
        }

        public int SendPacket(IPacket packet)
        {
            byte[] packet_raw = packet.Raw.ToArray();

            if (Server.Settings.ShowOutgoing) Console.WriteLine("\nBINARY(HEX): " + BitConverter.ToString(packet_raw).Replace('-', ' ') + "\nUTF8: " + Encoding.UTF8.GetString(packet_raw));

            return Client.Send(packet_raw);
        }

        public Task<int> SendPacketAsync(IPacket packet)
        {
            byte[] packetRaw = packet.Raw.ToArray();

            if (Server.Settings.ShowOutgoing) Console.WriteLine("\nBINARY(HEX): " + BitConverter.ToString(packetRaw).Replace('-', ' ') + "\nUTF8: " + Encoding.UTF8.GetString(packetRaw));

            return Client.SendAsync(packetRaw, SocketFlags.None);
        }

        public void Close()
        {
            Console.WriteLine("Disconnected: " + Client.RemoteEndPoint);
            if (IsPlayer)
            {
                Console.WriteLine("[WORLD] " + Nickname + " left the game");
                Server.BroadcastMessage(ChatColor.YELLOW + Nickname + " left the game");
            }

            Server._connections.RemoveAll(connection => connection.Player.Connection == this);
            Client.Close();
        }

        internal byte[] Receive()
        {
            if (Client.Available < 1)
                return Array.Empty<byte>();

            byte[] bytes = new byte[Client.Available];
            int received = 0;

            for (int i = 0; Client.Available > 0; i++)
            {
                received += Client.Receive(bytes, received, Math.Min(Client.Available, Client.ReceiveBufferSize), SocketFlags.None);
            }

            return bytes;
        }
    }
}
