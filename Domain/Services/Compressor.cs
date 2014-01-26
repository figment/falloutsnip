using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using TESVSnip.Framework.Services;

namespace TESVSnip.Domain.Services
{
    #region class Compressor / Decompressor

    static class CompressHelper
    {
        public static MethodInfo InitializeMethod;
        public static MethodInfo CloseMethod;
        public static MethodInfo VersionMethod;
        public static MethodInfo CompressMethod;
        public static MethodInfo DecompressMethod;

        static CompressHelper()
        {
            Platform.RegisterLibrary("ZLibMC.dll");
            try
            {
                var asm = Platform.LoadAssembly("ZLibMC.dll");
                var type = asm.GetType("DotZLib.ZLib", false, true);
                InitializeMethod = type.GetMethod("Initialize",
                                                  BindingFlags.Public | BindingFlags.Static,
                                                  null, new Type[0], new ParameterModifier[0]);
                CloseMethod = type.GetMethod("Close",
                                                  BindingFlags.Public | BindingFlags.Static,
                                                  null, new Type[0], new ParameterModifier[0]);
                CompressMethod = type.GetMethod("Compress",
                                                  BindingFlags.Public | BindingFlags.Static,
                                                  null, new Type[] { typeof(byte[]), typeof(int), typeof(int), typeof(int) }, new ParameterModifier[0]);
                DecompressMethod = type.GetMethod("Decompress",
                                                  BindingFlags.Public | BindingFlags.Static,
                                                  null, new Type[] { typeof(byte[]), typeof(int), typeof(int), typeof(int), typeof(int).MakeByRefType() }, new ParameterModifier[0]);
                VersionMethod = type.GetMethod("Version",
                                                  BindingFlags.Public | BindingFlags.Static,
                                                  null, new Type[0], new ParameterModifier[0]);

                Initialize();
            }
            catch
            {
            }            
        }

        public static void Initialize()
        {
            Platform.Initialize();
            InitializeMethod.Invoke(null, new object[0]);
            //string version = (string)VersionMethod.Invoke(null, new object[0]);
        }

        public static void Close()
        {
            CloseMethod.Invoke(null, new object[0]);
        }

        public static byte[] Compress(byte[] data, int offset, int length, int level)
        {
            return CompressMethod.Invoke(null, new object[] {data, offset, length, level}) as byte[];
        }

        public static byte[] Decompress(byte[] data, int offset, int length, int compSize, out int level)
        {
            level = -1;
            var args = new object[] {data, offset, length, compSize, level};
            var retval = DecompressMethod.Invoke(null, args) as byte[];
            level = (int)args[4];
            return retval;
        }
    }


    internal static class Compressor
    {
        private static HashSet<string> autoCompRecList;

        static Compressor()
        {
            CompressHelper.Initialize();
            // bit of a hack to avoid rebuilding this look up index
            autoCompRecList = new HashSet<string>(Properties.Settings.Default.AutoCompressRecords != null
                                  ? Properties.Settings.Default.AutoCompressRecords.Trim().Split(new[] { ';', ',' },
                                                                                                 StringSplitOptions.
                                                                                                     RemoveEmptyEntries)
                                  : new string[0], StringComparer.InvariantCultureIgnoreCase);
        }

        public static void Init()
        {
            //ms = new MemoryStream();
            //buffer = new byte[0x4000];       
            // bit of a hack to avoid rebuilding this look up index
            autoCompRecList = new HashSet<string>(Properties.Settings.Default.AutoCompressRecords != null
                                  ? Properties.Settings.Default.AutoCompressRecords.Trim().Split(new[] { ';', ',' },
                                                                                                 StringSplitOptions.
                                                                                                     RemoveEmptyEntries)
                                  : new string[0], StringComparer.InvariantCultureIgnoreCase);
        }

        public static bool CompressRecord(string name)
        {
            return autoCompRecList.Contains(name);
            //return Array.BinarySearch(autoCompRecList, name) >= 0;
        }

        public static byte[] Compress(byte[] data, int compressLevel)
        {
            return CompressHelper.Compress(data, 0, data.Length, compressLevel);
        }

        public static void Close()
        {
        }

#if false
        private static byte[] buffer;
        private static MemoryStream ms;

        public static Stream GetSharedStream()
        {
            ms.SetLength(0);
            ms.Position = 0;
            return ms;
        }

        public static BinaryWriter AllocWriter(Stream s)
        {
            return new BinaryWriter(new Ionic.Zlib.DeflateStream(s, Ionic.Zlib.CompressionMode.Decompress, Ionic.Zlib.CompressionLevel.Default, true));
        }

        public static void CopyTo(BinaryWriter output, Stream input)
        {
            long left = input.Length;
            while (left > 0)
            {
                int nread = input.Read(buffer, 0, buffer.Length);
                if (nread == 0) break;
                output.Write(buffer, 0, nread);
            }
        }

        public static byte[] Compress(byte[] data, int compressLevel)
        {
            var level = (Ionic.Zlib.CompressionLevel)compressLevel;
            if (ms == null) Init();
            ms.Position = 0;
            ms.SetLength(0);
            using (var gstream = new Ionic.Zlib.ZlibStream(ms, Ionic.Zlib.CompressionMode.Compress, level, true))
            {
                gstream.Write(data, 0, data.Length);
                gstream.Flush();
            }
            return ms.ToArray();
        }


        public static void Close()
        {
            buffer = null;
            if (ms != null) ms.Dispose();
            ms = null;
        }
#endif
    }

    internal static class Decompressor
    {
        private static byte[] input;
        private static byte[] output;
        private static MemoryStream ms;
        private static BinaryReader compReader;

        static Decompressor()
        {
            CompressHelper.Initialize();
        }

        public static BinaryReader Decompress(BinaryReader br, int size, int outsize, out int level)
        {
            if (input.Length < size)
            {
                input = new byte[size];
            }
            if (output.Length < outsize)
            {
                output = new byte[outsize];
            }
            int n = br.Read(input, 0, size);
            ms.Position = 0;
            ms.Write(input, 0, n);
            ms.SetLength(n);
            ms.Position = 0;
            var data = CompressHelper.Decompress(ms.GetBuffer(), 0, n, outsize, out level);
            ms.Position = 0;
            ms.Write(data, 0, data.Length);
            ms.SetLength(data.Length);
            ms.Position = 0;
            return compReader;
        }


#if false
        public static BinaryReader Decompress(BinaryReader br, int size, int outsize, out int compressLevel)
        {
            if (input.Length < size)
            {
                input = new byte[size];
            }
            if (output.Length < outsize)
            {
                output = new byte[outsize];
            }
            int n = br.Read(input, 0, size);
            ms.Position = 0;
            ms.Write(input, 0, n);
            ms.SetLength(n);
            ms.Position = 0;
            using (var inf = new Ionic.Zlib.ZlibStream(ms, Ionic.Zlib.CompressionMode.Decompress, Ionic.Zlib.CompressionLevel.Default, true))
            {
                n = inf.Read(output, 0, output.Length);
                if (n != outsize)
                    Trace.Write("Compression Output does not match expected size");
                compressLevel = (int)Ionic.Zlib.CompressionLevel.Default;
            }
            ms.Position = 0;
            ms.Write(output, 0, outsize);
            ms.SetLength(outsize);
            ms.Position = 0;
            return compReader;
        }
#endif

        public static void Init()
        {
            ms = new MemoryStream();
            compReader = new BinaryReader(ms);
            input = new byte[0x1000];
            output = new byte[0x4000];
        }

        public static void Close()
        {
            compReader.Close();
            compReader = null;
            input = null;
            output = null;
            ms = null;
        }
    }

    #endregion
}