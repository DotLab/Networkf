using Networkf;

namespace NetworkfSample {
	static class MainClass {
		public static void Main() {
			NetworkService.ParseMessage = ParseMessage;  // provide ParseMessage

			System.Console.Write("start (server/client)? ");
			var option = System.Console.ReadLine();
			if (option.Length > 0 && option[0] == 's') {
				var server = new SampleServer();
				NetworkHelper.StartServer(server.OnClientConnected);
				System.Console.ReadLine();
			} else {
				var service = NetworkHelper.StartClient();
				var client = new SampleClient(service);
				client.Start();
			}
		}

		static Message ParseMessage(byte[] buf, ref int i) {
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
				throw new System.NotImplementedException();
			}
		}
	}
}
