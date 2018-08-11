namespace Networkf {
	public static class BitHelper {
		public static byte ReadByte(byte[] buf, ref int i) {
			return buf[i++];
		}

		public static int ReadInt32(byte[] buf, ref int i) {
			return (int)buf[i++] | buf[i++] << 8 | buf[i++] << 16 | (sbyte)buf[i++] << 24;
		}

		public static uint ReadUInt32(byte[] buf, ref int i) {
			return (uint)(buf[i++] | buf[i++] << 8 | buf[i++] << 16 | buf[i++] << 24);
		}

		public static string ReadString(byte[] buf, ref int i, int len) {
			string str = System.Text.Encoding.UTF8.GetString(buf, i, len);
			i += len;
			return str;
		}

		public static void WriteInt32 (byte[] buf, ref int i, int val) {
			buf[i++] = (byte)(val & 0xFF);
			buf[i++] = (byte)((val >> 8) & 0xFF);
			buf[i++] = (byte)((val >> 16) & 0xFF);
			buf[i++] = (byte)((val >> 24) & 0xFF);
		}

		public static void WriteUInt32 (byte[] buf, ref int i, uint val) {
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

