namespace TESVSnip.Domain.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Ionic.Zlib;

    //internal static class Compressor
    //{
    //    private static readonly string[] autoCompRecList = new string[0];

    //    public static bool CompressRecord(string name)
    //    {
    //        return Array.BinarySearch(autoCompRecList, name) >= 0;
    //    }
    //}

    #region class Compressor / Decompressor

    internal static class Compressor
    {
        private static byte[] buffer;
        private static MemoryStream ms;
        //private static ICSharpCode.SharpZipLib.Zip.Compression.Deflater def;
        //private static ICSharpCode.SharpZipLib.Zip.Compression.Streams.DeflaterOutputStream defstr;
        private static string[] autoCompRecList = new string[0];


        public static Stream GetSharedStream()
        {
            ms.SetLength(0);
            ms.Position = 0;
            return ms;
        }

        public static BinaryWriter AllocWriter(Stream s)
        {
            //int compressLevel = 9;
            //def = new ICSharpCode.SharpZipLib.Zip.Compression.Deflater(compressLevel, false);
            //defstr = new ICSharpCode.SharpZipLib.Zip.Compression.Streams.DeflaterOutputStream(ms, def);
            //defstr.IsStreamOwner = false;
            return new BinaryWriter(new Ionic.Zlib.DeflateStream(s, CompressionMode.Decompress, CompressionLevel.Default, true));
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

        public static byte[] Compress(byte[] data)
        {
            var instream = new MemoryStream(data,false);
            var outstream = new MemoryStream(data.Length);
            using (var gstream = new DeflateStream(outstream, CompressionMode.Compress, CompressionLevel.Default,true))
            {
                instream.WriteTo(gstream);
                gstream.Flush();                
            }
            return outstream.ToArray();
        }

        public static void Init()
        {
            ms = new MemoryStream();
            buffer = new byte[0x4000];

            // bit of a hack to avoid rebuilding this look up index
            autoCompRecList = Properties.Settings.Default.AutoCompressRecords != null
                                  ? Properties.Settings.Default.AutoCompressRecords.Trim().Split(new[] { ';', ',' },
                                                                                                 StringSplitOptions.
                                                                                                     RemoveEmptyEntries)
                                  : new string[0];
            Array.Sort(autoCompRecList);
        }

        public static bool CompressRecord(string name)
        {
            return Array.BinarySearch(autoCompRecList, name) >= 0;
        }

        public static void Close()
        {
            buffer = null;
            if (ms != null) ms.Dispose();
            ms = null;
        }
    }

    internal static class Decompressor
    {
        private static byte[] input;
        private static byte[] output;
        private static MemoryStream ms;
        private static BinaryReader compReader;

        public static BinaryReader Decompress(BinaryReader br, int size, int outsize)
        {
            if (input.Length < size)
            {
                input = new byte[size];
            }
            if (output.Length < outsize)
            {
                output = new byte[outsize];
            }
            br.Read(input, 0, size);

            ms.Position = 0;
            var inf = new Ionic.Zlib.ZlibStream(ms, CompressionMode.Compress, CompressionLevel.Default, true);
            inf.Read(output, 0, outsize);
            ms.Position = 0;

            return compReader;
        }

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