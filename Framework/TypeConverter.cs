namespace TESVSnip.Framework
{
    using System;
    using System.Runtime.InteropServices;
    using System.Text;

    using Encoding = TESVSnip.Framework.Services.Encoding;

    [StructLayout(LayoutKind.Explicit)]
    internal struct TypeConverter
    {
        [FieldOffset(0)]
        private uint i;

        [FieldOffset(0)]
        private int si;

        [FieldOffset(0)]
        private ushort s;

        [FieldOffset(0)]
        private short ss;

        [FieldOffset(0)]
        private float f;

        [FieldOffset(0)]
        private byte b1;

        [FieldOffset(1)]
        private byte b2;

        [FieldOffset(2)]
        private byte b3;

        [FieldOffset(3)]
        private byte b4;

        [FieldOffset(0)]
        private readonly sbyte sb1;

        private static TypeConverter tc;

        private static readonly byte[] bytes = new byte[4];

        /*public static float i2f(uint i) {
            tc.i=i;
            return tc.f;
        }*/
        /*public static uint f2i(float f) {
            tc.f=f;
            return tc.i;
        }*/
        public static float h2f(byte b1, byte b2, byte b3, byte b4)
        {
          try
          {
            tc.b1 = b1;
            tc.b2 = b2;
            tc.b3 = b3;
            tc.b4 = b4;
            return tc.f;
          }
          catch (Exception)
          {
            throw;
          }

        }

        public static object GetObject<T>(byte[] data, int offset)
        {
          try
          {
            return (T)GetObject<T>(new ArraySegment<byte>(data, offset, data.Length - offset));
          }
          catch (Exception)
          {
            throw;
          }

        }

        public static object GetObject<T>(ArraySegment<byte> data)
        {
          try
          {


            T result = default(T);
            if (result is float)
            {
                return h2f(data);
            }

            if (result is int)
            {
                return h2si(data);
            }

            if (result is uint)
            {
                return h2i(data);
            }

            if (result is short)
            {
                return h2ss(data);
            }

            if (result is ushort)
            {
                return h2s(data);
            }

            if (result is sbyte)
            {
                return h2sb(data);
            }

            if (result is byte)
            {
                return h2b(data);
            }

            if (result is string)
            {
                return GetString(data);
            }

            return default(T);
          }
          catch (Exception)
          {
            throw;
          }
        }

        public static object GetValue<T>(ArraySegment<byte> data)
        {
          try
          {
            return (T) GetObject<T>(data);
          }
          catch (Exception)
          {
            throw;
          }
        }

        public static bool TryGetObject<T>(byte[] data, int offset, out object result)
        {
          try
          {
            result = GetObject<T>(new ArraySegment<byte>(data, offset, data.Length - offset));
            return true;
          }
          catch (Exception)
          {
            throw;
          }

        }

        public static ArraySegment<byte> GetData<T>(object value)
        {
          try
          {


            Type t = typeof(T);
            if (t == typeof(float))
            {
                return new ArraySegment<byte>(f2h(Extensions.CastValue<float>(value)), 0, 4);
            }

            if (t == typeof(int))
            {
                return new ArraySegment<byte>(si2h(Extensions.CastValue<int>(value)), 0, 4);
            }

            if (t == typeof(uint))
            {
                return new ArraySegment<byte>(i2h(Extensions.CastValue<uint>(value)), 0, 4);
            }

            if (t == typeof(short))
            {
                return new ArraySegment<byte>(ss2h(Extensions.CastValue<short>(value)), 0, 2);
            }

            if (t == typeof(ushort))
            {
                return new ArraySegment<byte>(s2h(Extensions.CastValue<ushort>(value)), 0, 2);
            }

            if (t == typeof(sbyte))
            {
                return new ArraySegment<byte>(sb2h(Extensions.CastValue<sbyte>(value)), 0, 1);
            }

            if (t == typeof(byte))
            {
                return new ArraySegment<byte>(si2h(Extensions.CastValue<byte>(value)), 0, 1);
            }

            if (t == typeof(string))
            {
                return new ArraySegment<byte>(str2h(Extensions.CastValue<string>(value)));
            }

            if (t == typeof(ArraySegment<byte>))
            {
                return (ArraySegment<byte>)value;
            }

            return new ArraySegment<byte>(new byte[0]);
          }
          catch (Exception)
          {
            throw;
          }
        }

        public static bool TrySetValue<T>(ArraySegment<byte> data, object value)
        {
          try
          {
            var seg = GetData<T>(value);
            if (seg.Count == data.Count)
            {
              Array.Copy(seg.Array, seg.Offset, data.Array, data.Offset, data.Count);
              return true;
            }

            return false;
          }
          catch (Exception)
          {
            throw;
          }

        }

        public static float h2f(byte[] data, int offset)
        {
          try
          {


            if (offset + sizeof(float) > data.Length)
            {
                return default(float);
            }

            tc.b1 = data[offset + 0];
            tc.b2 = data[offset + 1];
            tc.b3 = data[offset + 2];
            tc.b4 = data[offset + 3];
            return tc.f;
          }
          catch (Exception)
          {
            throw;
          }
        }

        public static float h2f(ArraySegment<byte> data)
        {
            //float f = 0;
            try
            {
                if (data.Count >= 4)
                {
                    tc.b1 = data.Array[data.Offset + 0];
                    tc.b2 = data.Array[data.Offset + 1];
                    tc.b3 = data.Array[data.Offset + 2];
                    tc.b4 = data.Array[data.Offset + 3];
                    return tc.f;
                }
                return default(float);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static uint h2i(byte b1, byte b2, byte b3, byte b4)
        {
          try
          {
            tc.b1 = b1;
            tc.b2 = b2;
            tc.b3 = b3;
            tc.b4 = b4;
            return tc.i;
          }
          catch (Exception)
          {
            throw;
          }
 
        }

        public static uint h2i(ArraySegment<byte> data)
        {
          try
          {
            if (data.Count >= 4)
            {
              tc.b1 = data.Array[data.Offset + 0];
              tc.b2 = data.Array[data.Offset + 1];
              tc.b3 = data.Array[data.Offset + 2];
              tc.b4 = data.Array[data.Offset + 3];
              return tc.i;
            }

            return 0;
          }
          catch (Exception)
          {
            throw;
          }

        }

        public static int h2si(byte b1, byte b2, byte b3, byte b4)
        {
          try
          {
            tc.b1 = b1;
            tc.b2 = b2;
            tc.b3 = b3;
            tc.b4 = b4;
            return tc.si;
          }
          catch (Exception)
          {
            throw;
          }

        }

        public static int h2si(byte[] data, int offset)
        {
          try
          {
            if (data.Length >= 4)
            {
              tc.b1 = data[offset + 0];
              tc.b2 = data[offset + 1];
              tc.b3 = data[offset + 2];
              tc.b4 = data[offset + 3];
              return tc.si;
            }

            return 0;
          }
          catch (Exception)
          {
            throw;
          }

        }

        public static int h2si(ArraySegment<byte> data)
        {
          try
          {
            if (data.Count >= 4)
            {
              tc.b1 = data.Array[data.Offset + 0];
              tc.b2 = data.Array[data.Offset + 1];
              tc.b3 = data.Array[data.Offset + 2];
              tc.b4 = data.Array[data.Offset + 3];
              return tc.si;
            }

            return 0;
          }
          catch (Exception)
          {
            throw;
          }

        }

        public static ushort h2s(byte b1, byte b2)
        {
          try
          {
            tc.b1 = b1;
            tc.b2 = b2;
            return tc.s;
          }
          catch (Exception)
          {
            throw;
          }
  
        }

        public static ushort h2s(ArraySegment<byte> data)
        {
          try
          {
            if (data.Count >= 2)
            {
              tc.b1 = data.Array[data.Offset + 0];
              tc.b2 = data.Array[data.Offset + 1];
              return tc.s;
            }

            return default(ushort);
          }
          catch (Exception)
          {
            throw;
          }

        }

        public static short h2ss(byte b1, byte b2)
        {
          try
          {
            tc.b1 = b1;
            tc.b2 = b2;
            return tc.ss;
          }
          catch (Exception)
          {
            throw;
          }
  
        }

        public static short h2ss(ArraySegment<byte> data)
        {

          try
          {
            if (data.Count >= 2)
            {
                tc.b1 = data.Array[data.Offset + 0];
                tc.b2 = data.Array[data.Offset + 1];
                return tc.ss;
            }

            return default(short);
          }
          catch (Exception)
          {
            throw;
          }
        }

        public static byte h2b(ArraySegment<byte> data)
        {
          try
          {
            if (data.Count >= 1)
            {
              return data.Array[data.Offset + 0];
            }

            return default(byte);
          }
          catch (Exception)
          {
            throw;
          }

        }

        public static sbyte h2sb(ArraySegment<byte> data)
        {
          try
          {
            if (data.Count >= 1)
            {
              tc.b1 = data.Array[data.Offset + 0];
              return tc.sb1;
            }

            return default(sbyte);
          }
          catch (Exception)
          {
            throw;
          }

        }

        private static byte[] UpdateBytes()
        {

          try
          {
            bytes[0] = tc.b1;
            bytes[1] = tc.b2;
            bytes[2] = tc.b3;
            bytes[3] = tc.b4;
            return bytes;
          }
          catch (Exception)
          {
            throw;
          }
        }

        public static byte[] f2h(float f)
        {
          try
          {
            tc.f = f;
            return UpdateBytes();
          }
          catch (Exception)
          {
            throw;
          }
   
        }

        public static byte[] i2h(uint i)
        {

          try
          {
            tc.i = i;
            return UpdateBytes();
          }
          catch (Exception)
          {
            throw;
          }
        }

        public static byte[] si2h(int si)
        {

          try
          {
            tc.si = si;
            return UpdateBytes();
          }
          catch (Exception)
          {
            throw;
          }
        }

        public static byte[] ss2h(short ss)
        {
          try
          {
            tc.ss = ss;
            return new[] { tc.b1, tc.b2 };
          }
          catch (Exception)
          {
            throw;
          }

        }

        public static byte[] s2h(ushort ss)
        {
          try
          {
            tc.s = ss;
            return new[] { tc.b1, tc.b2 };
          }
          catch (Exception)
          {
            throw;
          }

        }

        /*public static void f2h(float f, byte[] data, int offset) {
            tc.f=f;
            data[offset+0]=tc.b1;
            data[offset+1]=tc.b2;
            data[offset+2]=tc.b3;
            data[offset+3]=tc.b4;
        }*/

        public static void i2h(uint i, byte[] data, int offset)
        {
          try
          {
            tc.i = i;
            data[offset + 0] = tc.b1;
            data[offset + 1] = tc.b2;
            data[offset + 2] = tc.b3;
            data[offset + 3] = tc.b4;
          }
          catch (Exception)
          {
            throw;
          }

        }

        public static void si2h(int si, byte[] data, int offset)
        {
          try
          {

            tc.si = si;
            data[offset + 0] = tc.b1;
            data[offset + 1] = tc.b2;
            data[offset + 2] = tc.b3;
            data[offset + 3] = tc.b4;
          }
          catch (Exception)
          {
            throw;
          }
        }

        public static void ss2h(short ss, byte[] data, int offset)
        {
          try
          {
            tc.ss = ss;
            data[offset + 0] = tc.b1;
            data[offset + 1] = tc.b2;
          }
          catch (Exception)
          {
            throw;
          }

        }

        public static void s2h(ushort ss, byte[] data, int offset)
        {
          try
          {
            tc.s = ss;
            data[offset + 0] = tc.b1;
            data[offset + 1] = tc.b2;
          }
          catch (Exception)
          {
            throw;
          }
 
        }

        public static bool IsLikelyString(ArraySegment<byte> data)
        {

          try
          {
            bool isValid = true;
            for (int i = 0; i < data.Count - 1 && isValid; ++i)
            {
                var c = (char)data.Array[data.Offset + i];

                // if (c == 0) return (i > 0);
                isValid = !char.IsControl(c) || (c == 0x0D) || (c == 0x0A) || (c == 0x09) || (Properties.Settings.Default.UseUTF8 && ((c & 0x80) != 0));
                    
                    // Include CR, LF and TAB as normal characters to allow multiline strings + Allow Multibyte UTF-8
            }

            return isValid && data.Array[data.Count - 1] == 0;
          }
          catch (Exception)
          {
            throw;
          }
        }

        public static string GetZString(ArraySegment<byte> data)
        {
          try
          {
            var sb = new StringBuilder();
            for (int i = 0; i < data.Count; ++i)
            {
              var c = (char)data.Array[data.Offset + i];
              if (c == 0)
              {
                return sb.ToString();
              }

              sb.Append(c);
            }

            return sb.ToString();
          }
          catch (Exception)
          {
            throw;
          }
   
        }

        public static string GetBString(ArraySegment<byte> data)
        {
          try
          {
            ushort len = h2s(data);
            if (len > 0 && len <= data.Count + 2)
            {
              return Encoding.Instance.GetString(data.Array, data.Offset + 2, len);
            }

            return string.Empty;
          }
          catch (Exception)
          {
            throw;
          }

        }

        public static string GetIString(ArraySegment<byte> data)
        {
          try
          {
            int len = h2si(data);
            if (len > 0 && len <= data.Count + 4)
            {
              return Encoding.Instance.GetString(data.Array, data.Offset + 4, len);
            }

            return string.Empty;
          }
          catch (Exception)
          {
            throw;
          }

        }

        public static string GetString(ArraySegment<byte> data)
        {
          try
          {
            // remove the tailing null
            int len = data.Count > 0 && data.Array[data.Count - 1] == 0 ? data.Count - 1 : data.Count;
            return Encoding.Instance.GetString(data.Array, data.Offset, len);
          }
          catch (Exception)
          {
            throw;
          }

        }

        public static string GetHexData(byte[] data, int offset, int count)
        {
          try
          {
            var sb = new StringBuilder();
            for (int i = 0; i < count && (offset + i) < data.Length; ++i)
            {
              sb.Append(data[offset + i].ToString("X2")).Append(" ");
            }

            return sb.ToString();
          }
          catch (Exception)
          {
            throw;
          }

        }

        public static string GetHexData(ArraySegment<byte> data)
        {
          try
          {
            return GetHexData(data.Array, data.Offset, data.Count);
          }
          catch (Exception)
          {
            throw;
          }

        }

        /// <summary>
        /// Encode string including null termination character.
        /// </summary>
        /// <param name="str">
        /// </param>
        /// <returns>
        /// The System.Byte[].
        /// </returns>
        public static byte[] str2h(string str)
        {
          try
          {
            int len = Encoding.Instance.GetByteCount(str);
            var data = new byte[len + 1];
            Encoding.Instance.GetBytes(str).CopyTo(data, 0);
            data[len] = 0;
            return data;
          }
          catch (Exception)
          {
            throw;
          }

        }

        /// <summary>
        /// Encode short byte length prefixed string.
        /// </summary>
        /// <param name="str">
        /// </param>
        /// <returns>
        /// The System.Byte[].
        /// </returns>
        public static byte[] bstr2h(string str)
        {
          try
          {
            int len = Encoding.Instance.GetByteCount(str);
            var data = new byte[2 + len];
            Array.Copy(s2h((ushort)len), 0, data, 0, 2);
            Array.Copy(Encoding.Instance.GetBytes(str), 0, data, 2, len);
            return data;
          }
          catch (Exception)
          {
            throw;
          }

        }

        /// <summary>
        /// Encode int length prefixed string.
        /// </summary>
        /// <param name="str">
        /// </param>
        /// <returns>
        /// The System.Byte[].
        /// </returns>
        public static byte[] istr2h(string str)
        {
          try
          {
            int len = Encoding.Instance.GetByteCount(str);
            var data = new byte[4 + len];
            Array.Copy(si2h(len), 0, data, 0, 4);
            Array.Copy(Encoding.Instance.GetBytes(str), 0, data, 4, len);
            return data;
          }
          catch (Exception)
          {
            throw;
          }

        }

        public static byte[] b2h(byte i)
        {
          try
          {
            return new[] { i };
          }
          catch (Exception)
          {
            throw;
          }
   
        }

        public static byte[] sb2h(sbyte i)
        {
          try
          {
            return new[] { (byte)i };
          }
          catch (Exception)
          {
            throw;
          }
    
        }
    }
}
