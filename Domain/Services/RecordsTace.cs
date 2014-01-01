namespace TESVSnip.Domain.Services
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// RecordsTace : Trace records plugin for maintenance
    /// </summary>
    public static class RecordsTace
    {
        public static List<string> AllRecords { get; private set; } // list of plugin records

        public static List<string> CompressedRecords { get; private set; } // list of compressed records

        /// <summary>
        /// Add a new record in list of compressed records. Can trace all compressed record after reading a plugin.
        /// </summary>
        /// <param name="recordName">The record Name.</param>
        public static void AddRecordToCompressedRecordsList(string recordName)
        {
            if (CompressedRecords.IndexOf(recordName, 0) == -1)
            {
                CompressedRecords.Add(recordName);
                CompressedRecords.Add(Environment.NewLine);
            }
        }

        /// <summary>
        /// Add a new record in list of records.
        /// Can trace all used records after reading a plugin.
        /// </summary>
        /// <param name="recordName">
        /// The record Name.
        /// </param>
        public static void AddRecordToRecordsList(string recordName)
        {
            if (AllRecords.IndexOf(recordName, 0) == -1)
            {
                AllRecords.Add(recordName);
                AllRecords.Add(Environment.NewLine);
            }
        }

        /// <summary>
        /// Clear all list
        /// </summary>
        public static void ClearList()
        {
            if (CompressedRecords != null) CompressedRecords.Clear();
            if (AllRecords != null) AllRecords.Clear();
        }

        /// <summary>
        /// Init all list
        /// </summary>
        public static void InitListOfRecords()
        {
            if (CompressedRecords == null) CompressedRecords = new List<string>();
            if (AllRecords == null) AllRecords = new List<string>();
            ClearList();
        }
    }
}