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
            public int NumberOfBlocks { get; set; }
            public int BlockSize { get; set; }
            public int FreeBlockCount { get; set; }
            public bool[] FreeBlockBitMap { get; set; }
            public int NextAvailableBlock { get; set; }

            public VolumeControlBlock(int numberOfBlocks, int blockSize)
            {
                NumberOfBlocks = numberOfBlocks;
                BlockSize = blockSize;
                FreeBlockCount = numberOfBlocks - 1;
                FreeBlockBitMap = new bool[NumberOfBlocks];
                NextAvailableBlock = 1;

                for (int i = 0; i < numberOfBlocks; i++)
                {
                    FreeBlockBitMap[i] = (i != 0); // This marks the free blocks.
                }
            }
        }

        public class DirectoryEntry
        {
            public string FileName { get; set; }
            public int StartBlockNumber { get; set; }
            public int FileSize { get; set; }
        }

        public class SystemWideFileControlBlock
        {
            public string FileName { get; set; }
            public int FileSize { get; set; }
            public int FirstDataBlockPointer { get; set; }
        }

        public class PerProcessFileControlBlock
        {
            public int FileSize { get; set; }
            public int FirstDataBlockPointer { get; set; }
        }

    }
}
