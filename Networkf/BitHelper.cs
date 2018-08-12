namespace Networkf {
	public static class BitHelper {
		public static byte ReadUInt8(byte[] buf, ref int i) {
			return buf[i++];
		}

		public static short ReadInt16(byte[] buf, ref int i) {
			return (short)(buf[i++] | (sbyte)buf[i++] << 8);
		}

		public static ushort ReadUInt16(byte[] buf, ref int i) {
			return (ushort)(buf[i++] | buf[i++] << 8);
		}

		public static int ReadInt32(byte[] buf, ref int i) {
			return buf[i++] | buf[i++] << 8 | buf[i++] << 16 | (sbyte)buf[i++] << 24;
		}

		public static uint ReadUInt32(byte[] buf, ref int i) {
			return (uint)buf[i++] | (uint)buf[i++] << 8 | (uint)buf[i++] << 16 | (uint)buf[i++] << 24;
		}

		public static string ReadString(byte[] buf, ref int i, int len) {
			string str = System.Text.Encoding.UTF8.GetString(buf, i, len);
			i += len;
			return str;
		}

		public static void WriteUInt8(byte[] buf, ref int i, byte val) {
			buf[i++] = val;
		}

		public static void WriteInt16(byte[] buf, ref int i, short val) {
			buf[i++] = (byte)(val & 0xFF);
			buf[i++] = (byte)((val >> 8) & 0xFF);
		}

		public static void WriteUInt16(byte[] buf, ref int i, ushort val) {
			buf[i++] = (byte)(val & 0xFF);
			buf[i++] = (byte)((val >> 8) & 0xFF);
		}

		public static void WriteInt32(byte[] buf, ref int i, int val) {
			buf[i++] = (byte)(val & 0xFF);
			buf[i++] = (byte)((val >> 8) & 0xFF);
			buf[i++] = (byte)((val >> 16) & 0xFF);
			buf[i++] = (byte)((val >> 24) & 0xFF);
		}

		public static void WriteUInt32(byte[] buf, ref int i, uint val) {
			buf[i++] = (byte)(val & 0xFF);
			buf[i++] = (byte)((val >> 8) & 0xFF);
			buf[i++] = (byte)((val >> 16) & 0xFF);
			buf[i++] = (byte)((val >> 24) & 0xFF);
		}

		public static int GetStringByteCount (string val) {
			return System.Text.Encoding.UTF8.GetByteCount(val);
		}

		public static void WriteString (byte[] buf, ref int i, string val) {
			int len = System.Text.Encoding.UTF8.GetBytes(val, 0, val.Length, buf, i);
			i += len;
		}
	}
}

