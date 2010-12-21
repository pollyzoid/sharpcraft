using System;
using MCLib;
using MCLib.Enums;

/*
 * TODO:
 *  Implement packets
 */

namespace Core.Networking.Packets
{
    [Packet(Id = Packet.KeepAlive)]
    public class KeepAlive : PacketBase
    {
        public override void Read(NetworkStreamMC stream)
        {
        }

        protected override void Write(PacketWriter writer)
        {
        }
    }

    [Packet(Id = Packet.Login, Side = PacketSide.Client)]
    public class LoginRequest : PacketBase
    {
        /// <summary>
        /// Network protocol version, latest is 7
        /// </summary>
        public int Protocol;
        public string Username;
        /// <summary>
        /// Password for protected servers, not related to user's password
        /// </summary>
        public string Password;

        public long Seed;
        public byte Dimension;


        public override void Read(NetworkStreamMC stream)
        {
            Protocol = stream.ReadInt32();
            Username = stream.ReadString();
            Password = stream.ReadString();
            Seed = stream.ReadInt64();
            Dimension = stream.ReadByte();
        }

        protected override void Write(PacketWriter writer)
        {
            writer.Add(Protocol);
            writer.Add(Username);
            writer.Add(Password);
            writer.Add(Seed);
            writer.Add(Dimension);
        }
    }

    [Packet(Id = Packet.Login, Side = PacketSide.Server)]
    public class LoginResponse : PacketBase
    {
        /// <summary>
        /// Player's entity
        /// </summary>
        public int EntityId;

        public string ServerNameMaybe;
        public string MotdMaybe;

        public long MapSeed;
        public byte Dimension;

        public override void Read(NetworkStreamMC stream)
        {
            EntityId = stream.ReadInt32();
            ServerNameMaybe = stream.ReadString();
            MotdMaybe = stream.ReadString();
            MapSeed = stream.ReadInt64();
            Dimension = stream.ReadByte();
        }

        protected override void Write(PacketWriter writer)
        {
            throw new NotImplementedException();
        }
    }

    [Packet(Id = Packet.Handshake, Side = PacketSide.Client)]
    public class Handshake : PacketBase
    {
        public string Username;

        public override void Read(NetworkStreamMC stream)
        {
            Username = stream.ReadString();
        }

        protected override void Write(PacketWriter writer)
        {
            writer.Add(Username);
        }
    }

    [Packet(Id = Packet.Handshake, Side = PacketSide.Server)]
    public class HandshakeResponse : PacketBase
    {
        public string ConnectionHash;

        public override void Read(NetworkStreamMC stream)
        {
            ConnectionHash = stream.ReadString();
        }

        protected override void Write(PacketWriter writer)
        {
            writer.Add(ConnectionHash);
        }
    }

    [Packet(Id = Packet.ChatMessage)]
    public class ChatMessage : PacketBase
    {
        public string Message;

        public override void Read(NetworkStreamMC stream)
        {
            Message = stream.ReadString();
        }

        protected override void Write(PacketWriter writer)
        {
            writer.Add(Message);
        }
    }

    [Packet(Id = Packet.Disconnect)]
    public class Disconnect : PacketBase
    {
        public string Reason;


        public override void Read(NetworkStreamMC stream)
        {
            Reason = stream.ReadString();
        }

        protected override void Write(PacketWriter writer)
        {
            writer.Add(Reason);
        }
    }
}
