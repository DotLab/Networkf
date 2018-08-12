using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Networkf {
	public static class NetworkHelper {
		static void Log(string str) {
			#if DEBUG
			System.Console.WriteLine("net: " + str);
			#endif
		}

		public static uint GetTimestamp() {
			return (uint)(System.DateTime.UtcNow.Subtract(new System.DateTime(1970, 1, 1))).TotalSeconds;
		}

		public const int KServerPort = 11000;

		public static void StartServer(System.Action<NetworkService> onClientConnected = null, int port = KServerPort) {
			var ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
			var ipAddress = ipHostInfo.AddressList[0];
			var localEndPoint = new IPEndPoint(ipAddress, port);

			var server = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
			server.Bind(localEndPoint);
			server.Listen(10);  // backlog is 10

			int clientIndex = 0;

			Task.Run(() => {
				while (true) {
					Log("waiting for connection...");
					var client = server.Accept();
					
					Log("new client " + clientIndex);
					Task.Run(() => {
						var service = new NetworkService(clientIndex, client);
						/**
						 * => serHeader              6 "NfSer1"
						 * => serUdpPort             2 uint16
						 * <= cliHeader              6 "NfCli1"
						 * <= byteCount              1 uint8
						 * <= cliLocalAddr <byteCount> string
						 * <= cliTcpPort             2 uint16
						 * <= cliUdpPort             2 uint16
						 */
						Task.Run((System.Action)service.MessageListener);
						if (onClientConnected != null) {
							onClientConnected(service);
						}
					});
					
					clientIndex += 1;
				}
			});
		}

		public static NetworkService StartClient(string hostNameOrAddress = "127.0.0.1", int port = KServerPort) {
			var ipHostInfo = Dns.GetHostEntry(hostNameOrAddress);
			var ipAddress = ipHostInfo.AddressList[0];
			var remoteEndPoint = new IPEndPoint(ipAddress, port);

			var socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
			socket.Connect(remoteEndPoint);
			var service = new NetworkService(-1, socket);
			/**
			 * => cliHeader              6 "NfCli1"
			 * => byteCount              1 uint8
			 * => cliLocalAddr <byteCount> string
			 * => cliTcpPort             2 uint16
			 * => cliUdpPort             2 uint16
			 * <= serHeader              6 "NfSer1"
			 * <= serUdpPort             2 uint16
			 */
			Task.Run((System.Action)service.MessageListener);
			return service;
		}
	}
}

