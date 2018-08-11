using System;
using System.Threading;
using Networkf;

namespace NetworkfSample {
	public class SampleClient {
		static void Log(string str) {
			#if DEBUG
			Console.WriteLine("client: " + str);
			#endif
		}

		public readonly NetworkService service;
		public readonly object mtx = new object();

		public bool hasReceivedServerHello;

		public SampleClient(NetworkService service) {
			this.service = service;
			service.OnMessageReceived += OnMessageReceived;
			service.OnServiceTeardown += OnServiceTeardown;
		}

		public void OnMessageReceived(int id, Message message) {
			switch (message.type) {
			case SerHelloMessage.KType:
				hasReceivedServerHello = true;
				break;
			case SerChatMessage.KType:
				var chatMessage = message as SerChatMessage;
				Console.WriteLine("{0}: {1}", chatMessage.name, chatMessage.text);
				break;
			default:
				throw new NotImplementedException();
			}

			lock (mtx) {
				Monitor.Pulse(mtx);
			}
		}

		public void OnServiceTeardown() {
			service.OnMessageReceived -= OnMessageReceived;
			service.OnServiceTeardown -= OnServiceTeardown;
		}

		public void Start() {
			service.SendMessage(new CltHelloMessage());
			Log("waiting server hello...");
			lock (mtx) {
				while (!hasReceivedServerHello) Monitor.Wait(mtx);
			}
			Log("server hello received");

			Console.Write("name: ");
			string name = Console.ReadLine();
			service.SendMessage(new CltSetNameMessage(name));

			Console.WriteLine("Happy chating, " + name + "!");
			string text = Console.ReadLine();
			while (true) {
				service.SendMessage(new CltChatMessage(text));
			}
		}
	}
}

