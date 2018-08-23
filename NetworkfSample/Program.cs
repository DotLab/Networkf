using System;
using Networkf;

namespace NetworkfSample {
	static class MainClass {
		public static void Main() {
			Console.InputEncoding = System.Text.Encoding.Unicode;
			Console.OutputEncoding = System.Text.Encoding.Unicode;
			Console.Write("start ([s]erver/client/[l]ocal): ");
			var option = Console.ReadLine();
			if (option.Length > 0 && option[0] == 's') {
				var server = new SampleServer();
				NetworkHelper.StartServer(server.OnClientConnected);
				Console.ReadLine();
			} else if (option.Length > 0 && option[0] == 'l') {
				var server = new SampleServer();
				NetworkHelper.StartServer(server.OnClientConnected);
				var service = NetworkHelper.StartClient("127.0.0.1");
				var client = new SampleClient(service);
				client.Start();
			} else {
				var service = NetworkHelper.StartClient("s1.dotlab.cc");
				service.parseMessage = ParseMessage;  // provide ParseMessage
				var client = new SampleClient(service);
				client.Start();
			}
		}

		public static Message ParseMessage(byte[] buf, ref int i) {
			var type = Message.ReadMessageType(buf, ref i);
			switch (type) {
			case CltHelloMessage.KType:
				return new CltHelloMessage();
			case SerHelloMessage.KType:
				return new SerHelloMessage();

			case CltSetNameMessage.KType:
				return new CltSetNameMessage(buf, ref i);
			case CltChatMessage.KType:
				return new CltChatMessage(buf, ref i);
			case SerChatMessage.KType:
				return new SerChatMessage(buf, ref i);
			default:
				throw new NotImplementedException();
			}
		}
	}
}
