using System.Net;
using System.Net.Sockets;

namespace Networkf {
	public class NetworkService {
		static void Log(string msg) {
			#if DEBUG
			System.Console.WriteLine(msg);
			#endif
		}

		static void Log(string format, params object[] args) {
			Log(string.Format(format, args));
		}

		public const int KBufferSize = 2048, KLengthFieldSize = 2, KCrc32FieldSize = 4, KContentMaxSize = KBufferSize - KLengthFieldSize - KCrc32FieldSize;
		public ParseMessageDelegate parseMessage;

		public readonly int id;
		public readonly Socket socket;
		public event OnMessageReceivedDelegate OnMessageReceived;
		public event System.Action OnServiceTeardown;

		public readonly byte[] bufferRev = new byte[KBufferSize];
		public readonly byte[] bufferSnd = new byte[KBufferSize];

		public NetworkService(int id, Socket socket, Socket udpSocket, IPEndPoint localTcpEp, IPEndPoint remoteTcpEp, IPEndPoint localUdpEp) {
			this.id = id;
			this.socket = socket;
			this.udpSocket = udpSocket;
			this.localTcpEp = localTcpEp;
			this.remoteTcpEp = remoteTcpEp;
			this.localUdpEp = localUdpEp;
		}

		public readonly Socket udpSocket;
		public readonly IPEndPoint localTcpEp, remoteTcpEp, localUdpEp;
		public IPEndPoint remoteUdpEp;
		readonly byte[] _bufferRevUdp = new byte[KBufferSize];

		public void SetRemoteUdpEp(IPEndPoint remoteUdpEp) {
			this.remoteUdpEp = remoteUdpEp;
		}

		public void MessageListener() {
			SocketError error;
			int received;

			/**
			 * length         2 uint16
			 * content <length>
			 * crc32          4 uint32  // crc32 of content only
			 */
			while (true) {
				Log("\tnet {0,3}: waiting for message...", id);
				received = socket.Receive(bufferRev, 0, KLengthFieldSize, SocketFlags.None, out error);
				if (error != SocketError.Success) {
					Log("\t\terror: {0}", error);
					break;
				} else if (received != KLengthFieldSize) {
					Log("\t\terror: received {0}, expecting {1}", received, KLengthFieldSize);
					break;
				}

				int i = 0;
				int contentSize = BitHelper.ReadUInt16(bufferRev, ref i);
				Log("\t\tlength: {0}", contentSize);
				if (contentSize <= 0 || contentSize > KContentMaxSize) {
					Log("\t\terror: invalid length");
					break;
				}

				received = socket.Receive(bufferRev, i, contentSize + KCrc32FieldSize, SocketFlags.None, out error);
				if (error != SocketError.Success) {
					Log("\t\terror: {0}", error);
					break;
				} else if (received != contentSize + KCrc32FieldSize) {
					Log("\t\terror: received {0}, expecting {1}", received, contentSize + KCrc32FieldSize);
					break;
				}

				i += contentSize;
				uint crc32 = Crc32.Hash(bufferRev, KLengthFieldSize, contentSize), revCrc32 = BitHelper.ReadUInt32(bufferRev, ref i);
				if (crc32 != revCrc32) {
					Log("\t\terror: invalid crc32");
					break;
				}

				try {
					if (parseMessage != null) {
						int messageI = KLengthFieldSize;
						var message = parseMessage(bufferRev, ref messageI);
						if (messageI != i - KCrc32FieldSize) {
							Log("\t\terror: message used {0}, expecting {1}", messageI, i - KCrc32FieldSize);
							break;
						}
						Log("\t\treceived message type {0}", message.type);

						if (OnMessageReceived != null) {
							OnMessageReceived(id, message);
						}
					}
				} catch (System.Exception e) {
					Log(e.Message);
					break;
				}

			}

			TeardownService();
		}

		public void TeardownService() {
			socket.Shutdown(SocketShutdown.Both);
			socket.Close();

			if (OnServiceTeardown != null) {
				OnServiceTeardown();
			}
		}

		public int SendMessage(Message message) {
			int i = KLengthFieldSize;
			Message.WriteMessageType(bufferSnd, ref i, message.type);
			message.WriteTo(bufferSnd, ref i);

			int contentSize = i - KLengthFieldSize;
			int lengthI = 0;
			BitHelper.WriteUInt16(bufferSnd, ref lengthI, (ushort)contentSize);
			BitHelper.WriteUInt32(bufferSnd, ref i, Crc32.Hash(bufferSnd, KLengthFieldSize, contentSize));

			Log("\tnet {0,3}: send message type {1} length {2}", id, message.type, contentSize);

			SocketError error;
			int sent = socket.Send(bufferSnd, 0, i, SocketFlags.None, out error);
			if (error != SocketError.Success) {
				Log("\t\terror: {0}", error);
				return -1;
			} else if (sent != i) {
				Log("\t\terror: sent {0}, expecting {1}", sent, i);
				return -1;
			}

			return sent;
		}

		public int Send(int offset, int size) {
			SocketError error;
			int sent = socket.Send(bufferSnd, offset, size, SocketFlags.None, out error);
			if (error != SocketError.Success) {
				Log("\t\terror: {0}", error);
				return -1;
			} else if (sent != size) {
				Log("\t\terror: sent {0}, expecting {1}", sent, size);
				return -1;
			}
			return sent;
		}

		public int Receive(int offset, int size) {
			SocketError error;
			int received = socket.Receive(bufferRev, offset, size, SocketFlags.None, out error);
			if (error != SocketError.Success) {
				Log("\t\terror: {0}", error);
				return -1;
			} else if (received != size) {
				Log("\t\terror: received {0}, expecting {1}", received, size);
				return -1;
			}
			return received;
		}

		public int SendUdp(int offset, int size) {
			SocketError error;
			int sent = udpSocket.Send(bufferSnd, offset, size, SocketFlags.None, out error);
			if (error != SocketError.Success) {
				Log("\t\terror: {0}", error);
				return -1;
			} else if (sent != size) {
				Log("\t\terror: sent {0}, expecting {1}", sent, size);
				return -1;
			}
			return sent;
		}

		public int ReceiveUdp(int offset, int size) {
			SocketError error;
			int received = udpSocket.Receive(bufferRev, offset, size, SocketFlags.None, out error);
			if (error != SocketError.Success) {
				Log("\t\terror: {0}", error);
				return -1;
			} else if (received != size) {
				Log("\t\terror: received {0}, expecting {1}", received, size);
				return -1;
			}
			return received;
		}
	}
}

