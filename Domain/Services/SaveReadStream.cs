namespace TESVSnip.Domain.Services
{
    using System;
    using System.IO;

    public static class SaveReadStream
    {

        /// <summary>
        /// Save a memory stream to disk
        /// </summary>
        /// <param name="destFolder">Destination folder</param>
        /// <param name="ms">Stream to write to disk</param>
        public static void SaveStreamToDisk(string destFolder, MemoryStream ms)
        {
            string filename = DateTime.Now.ToString("yyyyMMddhhmmss");
            var file = new FileStream(Path.Combine(destFolder, filename + ".bin"), FileMode.Create, FileAccess.Write);
            var bytes = new byte[ms.Length];
            ms.Read(bytes, 0, (int) ms.Length);
            file.Write(bytes, 0, bytes.Length);
            file.Close();
        }
    }
}