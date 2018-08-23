using System.Net;
using System.Net.Sockets;
using System.Threading;
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

		public static IPAddress GetLocalIpAddress() {
			var ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
			foreach (var address in ipHostInfo.AddressList) {
				if (address.AddressFamily == AddressFamily.InterNetwork) {
					return address;
				}
			}
			throw new System.NotSupportedException();
		}

		public static IPAddress GetRemoteIpAddress(string name) {
			var ipHostInfo = Dns.GetHostEntry(name);
			foreach (var address in ipHostInfo.AddressList) {
				if (address.AddressFamily == AddressFamily.InterNetwork) {
					return address;
				}
			}
			throw new System.NotSupportedException();
		}

		public static void StartServer(System.Action<NetworkService> onClientConnected = null, int port = KServerPort) {
			var localIpAddress = GetLocalIpAddress();
			var localTcpEp = new IPEndPoint(localIpAddress, port);

			var server = new Socket(localIpAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
			server.Bind(localTcpEp);
			server.Listen(10);  // backlog is 10

			int clientIndex = 0;

			Task.Run(() => {
				while (true) {
					Log("waiting for connection...");
					var client = server.Accept();
					
					Log("new client " + clientIndex);
					new Thread(() => {
						var udpClient = new Socket(localIpAddress.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
						udpClient.Bind(new IPEndPoint(localIpAddress, 0));

						var service = new NetworkService(clientIndex, client, udpClient, localTcpEp, client.RemoteEndPoint as IPEndPoint, udpClient.LocalEndPoint as IPEndPoint);

						/**
						 * => serHeader              6 "NfSer1"
						 * => serUdpPort             2 uint16
						 * <= cliHeader              6 "NfCli1"
						 * <= cliTcpPort             2 uint16
						 * <= cliUdpPort             2 uint16
						 * <= byteCount              1 uint8
						 * <= cliLocalAddr <byteCount> string
						 */
						int i = 0;
						BitHelper.WriteString(service.bufferSnd, ref i, "NfSer1");
						BitHelper.WriteUInt16(service.bufferSnd, ref i, (ushort)(udpClient.LocalEndPoint as IPEndPoint).Port);
						service.Send(0, i);
						Log(i.ToString());
						service.Receive(0, 11);
						i = 0;
						string cliHeader = BitHelper.ReadString(service.bufferRev, ref i, 6);
						Log(cliHeader);
						int cliTcpPort = BitHelper.ReadUInt16(service.bufferRev, ref i);
						int cliUdpPort = BitHelper.ReadUInt16(service.bufferRev, ref i);
						int byteCount = BitHelper.ReadUInt8(service.bufferRev, ref i);
						service.Receive(i, byteCount);
						string cliLocalAddrStr = BitHelper.ReadString(service.bufferRev, ref i, byteCount);
						Log(cliLocalAddrStr);
						var cliLocalAddr = IPAddress.Parse(cliLocalAddrStr);
						var remoteUdpEp = new IPEndPoint((client.RemoteEndPoint as IPEndPoint).Address, cliUdpPort);
						udpClient.Connect(remoteUdpEp);
						service.SetRemoteUdpEp(remoteUdpEp);

						Task.Run((System.Action)service.MessageListener);

						if (onClientConnected != null) {
							onClientConnected(service);
						}
					}).Start();
					
					clientIndex += 1;
				}
			});
		}

		public static NetworkService StartClient(string hostNameOrAddress, int port = KServerPort) {
			var remoteIpAddress = GetRemoteIpAddress(hostNameOrAddress);
			var remoteEndPoint = new IPEndPoint(remoteIpAddress, port);

			var socket = new Socket(remoteIpAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
			socket.Connect(remoteEndPoint);

			var localIpAddress = GetLocalIpAddress();
			var udpSocket = new Socket(localIpAddress.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
			udpSocket.Bind(new IPEndPoint(localIpAddress, 0));

			var service = new NetworkService(-1, socket, udpSocket, socket.LocalEndPoint as IPEndPoint, socket.RemoteEndPoint as IPEndPoint, udpSocket.LocalEndPoint as IPEndPoint);
			/**
			 * => cliHeader              6 "NfCli1"
			 * => cliTcpPort             2 uint16
			 * => cliUdpPort             2 uint16
			 * => byteCount              1 uint8
			 * => cliLocalAddr <byteCount> string
			 * <= serHeader              6 "NfSer1"
			 * <= serUdpPort             2 uint16
			 */
			string cliLocalAddrStr = remoteIpAddress.ToString();
			int byteCount = BitHelper.GetStringByteCount(cliLocalAddrStr);
			int i = 0;
			BitHelper.WriteString(service.bufferSnd, ref i, "NfCli1");
			BitHelper.WriteUInt16(service.bufferSnd, ref i, (ushort)(socket.LocalEndPoint as IPEndPoint).Port);
			BitHelper.WriteUInt16(service.bufferSnd, ref i, (ushort)(udpSocket.LocalEndPoint as IPEndPoint).Port);
			BitHelper.WriteUInt8(service.bufferSnd, ref i, (byte)byteCount);
			BitHelper.WriteString(service.bufferSnd, ref i, cliLocalAddrStr);
			service.Send(0, i);
			service.Receive(0, 8);
			i = 0;
			string serHeader = BitHelper.ReadString(service.bufferRev, ref i, 6);
			Log(serHeader);
			int serUdpPort = BitHelper.ReadUInt16(service.bufferRev, ref i);
			var remoteUdpEp = new IPEndPoint(remoteIpAddress, serUdpPort);
			udpSocket.Connect(remoteUdpEp);
			service.SetRemoteUdpEp(remoteUdpEp);

			Task.Run((System.Action)service.MessageListener);
			return service;
		}
	}
}

