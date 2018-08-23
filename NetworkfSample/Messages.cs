using Networkf;
using Bit = Networkf.BitHelper;

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
		 * len      4 int32  // NOT text.Length
		 * text       <len> string
		 */
		public CltSetNameMessage(byte[] buf, ref int i) : base(KType) {
			int len = Bit.ReadInt32(buf, ref i);
			name = Bit.ReadString(buf, ref i, len);
		}

		public override void WriteTo(byte[] buf, ref int i) {
			Bit.WriteInt32(buf, ref i, Bit.GetStringByteCount(name));
			Bit.WriteString(buf, ref i, name);
		}
	}

	public sealed class CltChatMessage : Message {
		public const int KType = 2;
		public readonly string text;

		public CltChatMessage(string text) : base(KType) {
			this.text = text;
		}

		/**
		 * len      4 int32  // NOT text.Length
		 * text       <len> string
		 */
		public CltChatMessage(byte[] buf, ref int i) : base(KType) {
			int len = Bit.ReadInt32(buf, ref i);
			text = Bit.ReadString(buf, ref i, len);
		}

		public override void WriteTo(byte[] buf, ref int i) {
			Bit.WriteInt32(buf, ref i, Bit.GetStringByteCount(text));
			Bit.WriteString(buf, ref i, text);
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
		 * len      4 int32  // NOT text.Length
		 * name       <len> string
		 * len      4 int32  // NOT text.Length
		 * text       <len> string
		 */
		public SerChatMessage(byte[] buf, ref int i) : base(KType) {
			int len = Bit.ReadInt32(buf, ref i);
			name = Bit.ReadString(buf, ref i, len);
			len = Bit.ReadInt32(buf, ref i);
			text = Bit.ReadString(buf, ref i, len);
		}

		public override void WriteTo(byte[] buf, ref int i) {
			Bit.WriteInt32(buf, ref i, Bit.GetStringByteCount(name));
			Bit.WriteString(buf, ref i, name);
			Bit.WriteInt32(buf, ref i, Bit.GetStringByteCount(text));
			Bit.WriteString(buf, ref i, text);
		}
	}
}

