using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using DataCenterShared;

namespace data_center_irc_connector
{
    internal static class Program
    {
        private static void Main()
        {
            List<IrcConnection> connections = new List<IrcConnection>();
            connections.Add(new IrcConnection("irc.esper.net", 6667, "TrangarBot2"));
            Func<string, int, IrcConnection> getConnection = (host, port) => connections.First(c => c.Host == host && c.Port == port);

            Connector.On("tcp.status", message =>
            {
                string host = message.GetString("host");
                int port = message.GetInt("port");
                string status = message.GetString("status");

                Console.WriteLine("{0}:{1} is now {2}", host, port, status);

                if (status == "connected")
                {
                    getConnection(host, port).OnConnect();
                }
            });

            Connector.On("tcp.data", message =>
            {
                string host = message.GetString("host");
                int port = message.GetInt("port");
                string data = message.GetString("data");

                getConnection(host, port).AddBufer(Encoding.UTF8.GetString(Convert.FromBase64String(data)));
            });


            Thread.Sleep(1000);
            Connector.Emit("tcp.connect", new Dictionary<string, object>
            {
                { "host", "irc.esper.net" },
                { "port", 6667 }
            });

            Connector.Start();
        }
    }

	internal class IrcConnection
    {
        public IrcConnection(string host, int port, string name)
        {
            Host = host;
            Port = port;
            Name = name;
        }

        public void OnConnect()
        {
            Send(string.Format("NICK {0}", Name));
            Send(string.Format("USER {0} {0} {1} :{0}", Name, Host));
        }

	    private string Name { get; }
        public string Host { get; }
        public int Port { get; }

	    private string Buffer { get; set; }

	    private void Send(string command)
        {
            Console.WriteLine("< {0}", command);
            command += "\r\n";
            Connector.Emit("tcp.send", new Dictionary<string, object>
            {
                { "host", Host },
                { "port", Port },
                { "message", Convert.ToBase64String(Encoding.UTF8.GetBytes(command)) }
            });
        }

        public void AddBufer(string buffer)
        {
            Buffer += buffer;
            int index = Buffer.IndexOf("\r\n", StringComparison.Ordinal);
            while (index > -1)
            {
                string line = Buffer.Substring(0, index);
                Buffer = Buffer.Substring(index + 2);
                ParseLine(line);
                index = Buffer.IndexOf("\r\n", StringComparison.Ordinal);
            }
        }

		private void ParseLine(string line)
        {
            Console.WriteLine("> {0}", line);

            string[] split = line.Split(' ');
            if (split[0] == "PING")
            {
                Send("PONG " + split[1]);
                return;
            }

            if (split.Length > 3 && split[1] == "PRIVMSG" && split[0].StartsWith(":Trangar!~Trangar@") && split[3] == ":!join")
            {
                Send("JOIN #factorio");
            }
        }
    }
}
