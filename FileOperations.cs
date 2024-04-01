using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

using static Simulated_File_System.DataStructures;
using DirectoryEntry = Simulated_File_System.DataStructures.DirectoryEntry;

namespace Simulated_File_System
{
    internal class FileOperations
    {
        public class FileSystem
        {
            private VolumeControlBlock vcb;
            private DirectoryEntry[] directory;
            private FileControlBlock[] fcbList;
            private SystemWideOpenFileTable[] systemWideOpenFileTable;
            private PerProcessOpenFileTable[] perProcessOpenFileTable;
            private int maxOpenFiles;

            // Constructor to initalize the volume control block, directory, and file control block.
            public FileSystem(int maxOpenFiles, int maxProcesses)
            {
                vcb = new VolumeControlBlock();
                directory = new DirectoryEntry[vcb.TotalBlocks];
                fcbList = new FileControlBlock[vcb.TotalBlocks];
                systemWideOpenFileTable = new SystemWideOpenFileTable[maxOpenFiles];
                perProcessOpenFileTable = new PerProcessOpenFileTable[maxProcesses * maxOpenFiles];
                this.maxOpenFiles = maxOpenFiles;
            }

            // Create a file with specified size in blocks.
            public void CreateFile(string fileName, int fileSize)
            {
                Console.WriteLine($"Running CreateFile function for file '{fileName}'");
                // Check if the file already exists.
                if (DoesFileExist(fileName))
                {
                    Console.WriteLine("File already exists. (CreateFile)");
                    return;
                }

                // Allocate a new file control block for the file.
                int fcbPointer = vcb.NextAvailableBlock;
                fcbList[fcbPointer] = new FileControlBlock
                {
                    FileSize = fileSize,
                    FirstDataBlockPointer = vcb.NextAvailableBlock
                };

                // Update the directory entry.
                directory[vcb.NextAvailableBlock] = new DirectoryEntry
                {
                    FileName = fileName,
                    StartBlockNumber = fcbList[fcbPointer].FirstDataBlockPointer,
                    FileSize = fileSize
                };

                vcb.FreeBlocks -= fileSize;
                vcb.NextAvailableBlock += fileSize;

                PrintFileSystemState();

                Console.WriteLine($"File '{fileName}' created successfully!");
            }

            // Checks if the file name exists in the directory.
            private bool DoesFileExist(string fileName)
            {
                foreach (var entry in directory)
                {
                    if (entry != null && entry.FileName == fileName)
                    {
                        return true;
                    }
                }
                return false;
            }

            // Open a file and update open file tables.
            public int OpenFile(string fileName, int processId)
            {
                Console.WriteLine($"Running OpenFile function for file '{fileName}'");
                // Search for the file in the directory.
                int fileIndex = FindFileIndex(fileName);
                if (fileIndex == -1)
                {
                    Console.WriteLine($"File '{fileName}' not found. (OpenFile)");
                    return -1;
                }

                // Update system-wide open file table.
                int systemWideIndex = AddToSystemWideOpenFileTable(fileName, fileIndex);

                // Update per-process open file table.
                int perProcessHandle = AddToPerProcessOpenFileTable(fileName, fileIndex, processId);

                // Return the per-process file handle.
                return perProcessHandle;
            }

            // Searches for the file in the directory and returns it's index.
            private int FindFileIndex(string fileName)
            {
                for (int i = 0; i < directory.Length; i++)
                {
                    if (directory[i] != null && directory[i].FileName == fileName)
                    {
                        return i;
                    }
                }

                return -1;
            }

            private int AddToSystemWideOpenFileTable(string fileName, int fileIndex)
            {
                Console.WriteLine($"Running AddToSystemWideOpenFileTable function for file '{fileName}'");
                for (int i = 0; i < systemWideOpenFileTable.Length; i++)
                {
                    if (systemWideOpenFileTable[i] == null)
                    {
                        systemWideOpenFileTable[i] = new SystemWideOpenFileTable()
                        {
                            FileName = fileName,
                            FCBPointer = directory[fileIndex].StartBlockNumber
                        };
                        return i; // Returns the index in system-wide open file table.
                    }
                }
                Console.WriteLine("System-wide open file table is full.");
                return -1;
            }

            private int AddToPerProcessOpenFileTable(string fileName, int fileIndex, int processId)
            {
                Console.WriteLine($"Running AddToPerProcessOpenFileTable function for file '{fileName}' and process {processId}");
                int perProcessTableIndex = processId * this.maxOpenFiles; // Start index of per-process table for the given process.

                for (int i = perProcessTableIndex; i < perProcessTableIndex; i++)
                {
                    if (perProcessOpenFileTable[i] == null)
                    {
                        perProcessOpenFileTable[i] = new PerProcessOpenFileTable()
                        {
                            FileName = fileName,
                            Handle = i - perProcessTableIndex // Use the index as the handle.
                        };
                        return i - perProcessTableIndex; // Return handle.
                    }
                }
                Console.WriteLine($"Per-process open file table for process {processId} is full");
                return -1;
            }

            // Close a file and update the open file tables.
            public void CloseFile(string fileName, int processId)
            {
                Console.WriteLine($"Running CloseFile function for file '{fileName}'");
                
                // Search for the file in the system-wide open file table.
                int systemWideIndex = FindSystemWideOpenFileIndex(fileName);
                Console.WriteLine($"This is the value of systemWideIndex: {systemWideIndex} of processId: {processId}");
                if (systemWideIndex == -1)
                {
                    Console.WriteLine($"File '{fileName}' not found in system-wide open file table. (CloseFile)");
                    return;
                }
                
                // Find the file in the per-process open file table.
                int perProcessIndex = FindPerProcessOpenFileIndex(fileName, processId);
                Console.WriteLine($"This is the value of perProcessIndex: {perProcessIndex} of processId: {processId}");
                if (perProcessIndex == -1)
                {
                    Console.WriteLine($"File '{fileName}' not found in per-process open file table.");
                    return;
                }
                
                // Update the system-wide open file table.
                systemWideOpenFileTable[systemWideIndex] = null;
                
                // Update per-process open file table.
                perProcessOpenFileTable[perProcessIndex] = null;
                
                Console.WriteLine($"File '{fileName}' closed successfully.");
            }

            private int FindSystemWideOpenFileIndex(string fileName)
            {
                for (int i = 0; i < systemWideOpenFileTable.Length; i++)
                {
                    if (systemWideOpenFileTable[i].FileName == fileName)
                    {
                        Console.WriteLine($"This is the index of {fileName} being returned in FindSystemWideOpenFileIndex: {i}");
                        return i;
                    }
                }
                return -1;
            }

            private int FindPerProcessOpenFileIndex(string fileName, int processId)
            {
                int perProcessTableIndex = processId * maxOpenFiles;
                for (int i = perProcessTableIndex; i < perProcessTableIndex; i++)
                {
                    if (perProcessOpenFileTable[i].FileName == fileName)
                    {
                        Console.WriteLine($"This is the index of {fileName} being returned in FindPerProcessOpenFileIndex: {i}");
                        return i;
                    }
                }

                return -1;
            }

            // Write data to a file.
            public void WriteFile(string fileName, byte[] data)
            {

            }

            // Display all files in the directory.
            public void ListFiles()
            {

            }

            // Delete a file from the directory.
            public void DeleteFile(string fileName)
            {

            }

            private void PrintFileSystemState()
            {
                Console.WriteLine("\nCurrent State of File System:");
                Console.WriteLine("Volume Control Block (VCB):");
                Console.WriteLine($"Total Blocks: {vcb.TotalBlocks}");
                Console.WriteLine($"Block Size: {vcb.BlockSize}");
                Console.WriteLine($"Free Blocks: {vcb.FreeBlocks}");
                Console.WriteLine($"Next Available Block: {vcb.NextAvailableBlock}");

                Console.WriteLine("\nDirectory:");
                for (int i = 0; i < directory.Length; i++)
                {
                    if (directory[i] != null)
                    {
                        Console.WriteLine($"Index: {i}, FileName: {directory[i].FileName}, StartBlockNumber: {directory[i].StartBlockNumber}, FileSize: {directory[i].FileSize}");
                    }
                }

                Console.WriteLine("\nFile Control Blocks (FCB):");
                for (int i = 0; i < fcbList.Length; i++)
                {
                    if (fcbList[i] != null)
                    {
                        Console.WriteLine($"Index: {i}, FileSize: {fcbList[i].FileSize}, FirstDataBlockPointer: {fcbList[i].FirstDataBlockPointer}");
                    }
                }
            }
        }
    }
}
