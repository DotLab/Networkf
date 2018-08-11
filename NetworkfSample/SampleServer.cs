using System.Threading;
using System.Collections.Generic;

using Networkf;

namespace NetworkfSample {
	public sealed class SampleServer {
		static void Log(string str) {
			#if DEBUG
			System.Console.WriteLine("server: " + str);
			#endif
		}

		readonly Dictionary<int, ClientHandler> _clientDict = new Dictionary<int, ClientHandler>();

		public void OnClientConnected(NetworkService service) {
			var clientHandler = new ClientHandler(this, service);
			_clientDict.Add(service.id, clientHandler);
			clientHandler.Start();
		}

		void OnChatReceived(string name, string text) {
			var message = new SerChatMessage(name, text);
			foreach (var client in _clientDict.Values) {
				client.service.SendMessage(message);
			}
		}

		void OnClientLeave(int id) {
			_clientDict.Remove(id);
		}

		class ClientHandler {
			public readonly SampleServer server;
			public readonly NetworkService service;
			public readonly object mtx = new object();

			public bool hasReceivedClientHello;
			public string name;

			void Log(string str) {
				System.Console.WriteLine("server {0,3}: " + str, service.id);
			}

			public ClientHandler(SampleServer server, NetworkService service) {
				this.server = server;
				this.service = service;
				service.OnMessageReceived += OnMessageReceived;
				service.OnServiceTeardown += OnServiceTeardown;
			}

			public void OnMessageReceived(int id, Message message) {
				switch (message.type) {
				case CltHelloMessage.KType:
					hasReceivedClientHello = true;
					break;
				case CltSetNameMessage.KType:
					name = (message as CltSetNameMessage).name;
					break;
				case CltChatMessage.KType:
					server.OnChatReceived(name, (message as CltChatMessage).text);
					break;
				default:
					throw new System.NotImplementedException();
				}

				lock (mtx) {
					Monitor.Pulse(mtx);
				}
			}

			public void OnServiceTeardown() {
				service.OnMessageReceived -= OnMessageReceived;
				service.OnServiceTeardown -= OnServiceTeardown;
				server.OnClientLeave(service.id);
			}

			public void Start() {
				service.SendMessage(new SerHelloMessage());
				Log("waiting client hello...");
				lock (mtx) {
					while (!hasReceivedClientHello) Monitor.Wait(mtx);
				}
				Log("client hello received");
				lock (mtx) {
					while (string.IsNullOrEmpty(name)) Monitor.Wait(mtx);
				}
				Log("client name received");
			}
		}
	}
}

