using Networkf;

namespace NetworkfSample {
	public sealed class SerHelloMessage : Message {
		public const int KType = -1;
		public SerHelloMessage() : base(KType) {}
	}

	public sealed class CltHelloMessage : Message {
		public const int KType = -2;
		public CltHelloMessage() : base(KType) {}
	}

	public sealed class CltSetNameMessage : Message {
		public const int KType = 1;
		public readonly string name;

		public CltSetNameMessage(string name) : base(KType) {
			this.name = name;
		}

		/**
		 * byteCount      4 int32  // NOT text.Length
		 * text       <len> string
		 */
		public CltSetNameMessage(byte[] buf, ref int i) : base(KType) {
			int byteCount = BitHelper.ReadInt32(buf, ref i);
			name = BitHelper.ReadString(buf, ref i, byteCount);
		}

		public override void WriteTo(byte[] buf, ref int i) {
			BitHelper.WriteInt32(buf, ref i, BitHelper.GetStringByteCount(name));
			BitHelper.WriteString(buf, ref i, name);
		}
	}

	public sealed class CltChatMessage : Message {
		public const int KType = 2;
		public readonly string text;

		public CltChatMessage(string text) : base(KType) {
			this.text = text;
		}

		/**
		 * byteCount      4 int32  // NOT text.Length
		 * text       <len> string
		 */
		public CltChatMessage(byte[] buf, ref int i) : base(KType) {
			int byteCount = BitHelper.ReadInt32(buf, ref i);
			text = BitHelper.ReadString(buf, ref i, byteCount);
		}

		public override void WriteTo(byte[] buf, ref int i) {
			BitHelper.WriteInt32(buf, ref i, BitHelper.GetStringByteCount(text));
			BitHelper.WriteString(buf, ref i, text);
		}
	}

	public sealed class SerChatMessage : Message {
		public const int KType = 3;
		public readonly string name, text;

		public SerChatMessage(string name, string text) : base(KType) {
			this.name = name;
			this.text = text;
		}

		/**
		 * byteCount      4 int32  // NOT text.Length
		 * name       <len> string
		 * byteCount      4 int32  // NOT text.Length
		 * text       <len> string
		 */
		public SerChatMessage(byte[] buf, ref int i) : base(KType) {
			int byteCount = BitHelper.ReadInt32(buf, ref i);
			name = BitHelper.ReadString(buf, ref i, byteCount);
			byteCount = BitHelper.ReadInt32(buf, ref i);
			text = BitHelper.ReadString(buf, ref i, byteCount);
		}

		public override void WriteTo(byte[] buf, ref int i) {
			BitHelper.WriteInt32(buf, ref i, BitHelper.GetStringByteCount(name));
			BitHelper.WriteString(buf, ref i, name);
			BitHelper.WriteInt32(buf, ref i, BitHelper.GetStringByteCount(text));
			BitHelper.WriteString(buf, ref i, text);
		}
	}
}

