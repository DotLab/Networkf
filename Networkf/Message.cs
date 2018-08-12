namespace Networkf {
	public delegate void OnMessageReceivedDelegate(int id, Message message);
	public delegate Message ParseMessageDelegate(byte[] buf, ref int i);

	public abstract class Message {  // all (de)serialization logic should be in Message
		public static int ReadMessageType(byte[] buf, ref int i) {
			return BitHelper.ReadInt32(buf, ref i);
		}
		public static void WriteMessageType(byte[] buf, ref int i, int type) {
			BitHelper.WriteInt32(buf, ref i, type);
		}

		public int type;

		protected Message(int type) {
			this.type = type;
		}

		public virtual void WriteTo(byte[] buf, ref int i) {}
	}
}

