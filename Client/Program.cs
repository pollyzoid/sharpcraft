using System;
using System.Net;
using System.Text;
using Core.Networking;
using Core.Networking.Packets;
using MCLib.Enums;

namespace Client
{
    class Program
    {
        private const int Version = 12;

        private readonly PacketHandler _handler;

        private readonly string _username;
        private readonly string _password;

        public Program()
        {
            Console.Write("Username: ");
            _username = Console.ReadLine();

            Console.Write("Password: ");

            var passwd = new StringBuilder();
            ConsoleKeyInfo cki;
            while ((cki = Console.ReadKey(true)).Key != ConsoleKey.Enter)
            {
                Console.Write("*");
                passwd.Append(cki.KeyChar);
            }

            Console.WriteLine();

            _password = passwd.ToString();

            _handler = new PacketHandler(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 25565), HandlerMode.Client);

            Handshake();
            Login();

            Console.WriteLine("Logged in");

            _handler.UnsubscribedPacket += p => { };
            _handler.Subscribe(Packet.Disconnect, p => Console.Write("You were kicked: " + ((Disconnect)p).Reason));

            _handler.Subscribe(Packet.ChatMessage, p => Console.WriteLine(((ChatMessage) p).Message));
            _handler.EventMode = true;

            while (true)
            {
                _handler.SendPacket(new ChatMessage {Message = Console.ReadLine()});
            }
        }

        void Handshake()
        {
            var wc = new WebClient();
            var getVerResp = wc.DownloadString("http://www.minecraft.net/game/getversion.jsp?user=" + _username + "&password=" + _password + "&version=" + Version).Split(':');

            _handler.SendPacket(new Handshake {Username = _username});
            Console.Write("Handshaking... ");

            var p = _handler.ReceivePacket();
            Console.WriteLine(p.Id);

            var packet = p as HandshakeResponse;

            if (packet == null)
                throw new ProtocolViolationException("Expected handshake");

            var connectionHash = packet.ConnectionHash;
            Console.WriteLine("ok: " + connectionHash);

            Console.Write("Authorizing... ");
            wc.DownloadString("http://www.minecraft.net/game/joinserver.jsp?user=" + getVerResp[2] + "&sessionId=" + getVerResp[3] + "&serverId=" + connectionHash);
            Console.WriteLine("ok");
        }

        void Login()
        {
            _handler.SendPacket(new LoginRequest
                                    {Protocol = 7, Username = _username, Password = "", Seed = 0, Dimension = 0});

            var packet = _handler.ReceivePacket();

            if (packet.Id == Packet.Disconnect)
            {
                Console.WriteLine("You were kicked.");
                Console.ReadKey();
                Environment.Exit(1);
            }
            if (packet.Id != Packet.Login)
                throw new ProtocolViolationException("Expected login response");
        }

        static void Main()
        {
            new Program();
        }
    }
}
