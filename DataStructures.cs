using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulated_File_System
{
    public class DataStructures
    {
        public class VolumeControlBlock
        {
            public int TotalBlocks = 512;
            public int BlockSize = 2048;
            public int FreeBlocks { get; set; }
            public bool[] FreeBlockBitMap { get; set; }

            // Constructor
            public VolumeControlBlock()
            {
                FreeBlockBitMap = new bool[TotalBlocks];
                for (int i = 0; i < TotalBlocks; i++)
                {
                    FreeBlockBitMap[i] = true; // Sets all the initial entries as true. (Simulates free space on the volume control block)
                }

                FreeBlocks = TotalBlocks;
            }
        }

        public class DirectoryEntry
        {
            public string FileName { get; set; }
            public int StartBlockNumber { get; set; }
            public int FileSizeInBlocks { get; set; }
            public int FileSizeInBytes { get; set; }
            public byte[] FileData { get; set; }
        }

        public class FileControlBlock
        {
            public int FileSize { get; set; }
            public int FirstDataBlockPointer { get; set; }
        }

        public class SystemWideOpenFileTable
        {
            public string FileName { get; set; }
            public int FCBPointer { get; set; }
        }

        public class PerProcessOpenFileTable
        {
            public string FileName { get; set; }
            public int Handle { get; set; }
        }
    }
}
