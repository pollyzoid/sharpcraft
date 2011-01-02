using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using MCLib.Enums;
using MCLib.Networking;
using MCLib.Networking.Packets;

namespace Client
{
    class Program
    {
        private const int Version = 99;
        private const int Protocol = 8;

        private readonly PacketHandler _handler;

        private string _username;
        private string _password;

        private readonly string[] _loginResponse;

        private bool _loggedIn;

        public Program()
        {
            try
            {
                _handler = new PacketHandler("127.0.0.1", 25565, HandlerMode.Client);
            }
            catch (SocketException e)
            {
                Console.WriteLine(e.Message);
                Console.ReadKey();
                Environment.Exit(1);
            }

            _loginResponse = Login();

            // Subscribe packets
            _handler.Subscribe(Packet.Handshake, ShakeHands);

            _handler.Subscribe(Packet.Login, p =>
                                                 {
                                                     var packet = (LoginResponse) p;
                                                     Console.WriteLine("Entity ID: " + packet.EntityId);
                                                     Console.WriteLine("Map seed: " + packet.MapSeed);
                                                 });

            _handler.UnsubscribedPacket += p => { };
            _handler.Subscribe(Packet.Disconnect, p =>
                                                      {
                                                          Console.Write("You were kicked: " + ((Disconnect) p).Reason);
                                                          _handler.Disconnect();
                                                      });

            _handler.Subscribe(Packet.ChatMessage, p => Console.WriteLine(((ChatMessage)p).Message));
            _handler.EventMode = true;

            // START ACTION

            // This starts the server joining chain of packets
            _handler.SendPacket(new Handshake { Username = _username });

            while (_handler.IsActive)
            {
                var line = Console.ReadLine();
                if (line == "exit")
                {
                    _handler.SendPacket(new Disconnect {Reason = "This is ignored anyways, so eh"});

                    _handler.Disconnect();
                    break;
                }

                _handler.SendPacket(new ChatMessage { Message = line });
            }
        }

        string[] Login()
        {
            if (_loggedIn) return _loginResponse;

            string resp = null;

            using (var wc = new WebClient())
            {
                do
                {
                    if (!string.IsNullOrEmpty(resp))
                        Console.WriteLine(resp);

                    Console.WriteLine();
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

                    _password = passwd.ToString();
                } while (
                    !(resp =
                      wc.DownloadString("http://www.minecraft.net/game/getversion.jsp?user=" + _username + "&password=" +
                                        _password + "&version=" + Version)).Contains(":"));
            }

            return resp.Split(':');
        }

        void ShakeHands(PacketBase packet)
        {
            if (_loggedIn) return;

            var hsResp = (HandshakeResponse) packet;

            var connectionHash = hsResp.ConnectionHash;
            Console.WriteLine("Got hash: " + connectionHash);

            // No authentication
            if (connectionHash == "-")
            {
                Console.WriteLine("No name authentication needed");
                return;
            }

            Console.Write("Authorizing... ");

            using(var wc = new WebClient())
            {
                wc.DownloadString("http://www.minecraft.net/game/joinserver.jsp?user=" + _loginResponse[2] +
                                  "&sessionId=" + _loginResponse[3] + "&serverId=" + connectionHash);
            }
            
            Console.WriteLine("OK");

            Console.WriteLine("Joining...");

            _handler.SendPacket(new LoginRequest
                                    {
                                        Protocol = Protocol,
                                        Username = _username,
                                        Password = "",
                                        MapSeed = 0,
                                        Dimension = 0
                                    });

            _loggedIn = true;
        }

        static void Main()
        {
            new Program();
        }
    }
}