using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            public void CreateFile(string fileName, int fileSizeInBytes, int processId)
            {
                Console.WriteLine($"Process {processId} is running CreateFile function for file '{fileName}'");
                // Check if the file already exists.
                if (DoesFileExist(fileName))
                {
                    Console.WriteLine("File already exists. (CreateFile)");
                    return;
                }

                // Calculate the number of blocks the file will require.
                int fileSizeInBlocks = (fileSizeInBytes / vcb.BlockSize) + 1;

                // Allocate a new file control block for the file.
                int fcbPointer = vcb.NextAvailableBlock;
                fcbList[fcbPointer] = new FileControlBlock
                {
                    FileSize = fileSizeInBlocks,
                    FirstDataBlockPointer = vcb.NextAvailableBlock
                };

                // Update the directory entry.
                directory[vcb.NextAvailableBlock] = new DirectoryEntry
                {
                    FileName = fileName,
                    StartBlockNumber = fcbList[fcbPointer].FirstDataBlockPointer,
                    FileSizeInBlocks = fileSizeInBlocks,
                    FileSizeInBytes = fileSizeInBytes,
                    FileData = new byte[fileSizeInBytes * vcb.BlockSize]
                };

                vcb.FreeBlocks -= fileSizeInBlocks;
                vcb.NextAvailableBlock += fileSizeInBlocks;

                Console.WriteLine($"File '{fileName}' created successfully!\n");

                OpenFile(fileName, processId);
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
                Console.WriteLine($"Process {processId} is running OpenFile function for file '{fileName}'.");
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

                Console.WriteLine($"{fileName} has been opened by process {processId}.\n");
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

            // Adds the file to the system wide open file table.
            private int AddToSystemWideOpenFileTable(string fileName, int fileIndex)
            {
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
                Console.WriteLine("System-wide open file table is full.\n");
                return -1;
            }

            // Adds the file to the per process open file table.
            private int AddToPerProcessOpenFileTable(string fileName, int fileIndex, int processId)
            {
                int perProcessTableIndex = (processId - 1) * this.maxOpenFiles; // Start index of per-process table for the given process.

                for (int i = perProcessTableIndex; i < perProcessOpenFileTable.Length; i++)
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
                Console.WriteLine($"Per-process open file table for process {processId} is full.\n");
                return -1;
            }

            // Close a file and update the open file tables.
            public void CloseFile(string fileName, int processId)
            {
                Console.WriteLine($"Process {processId} is running CloseFile function for file '{fileName}'");
                
                // Search for the file in the system-wide open file table.
                int systemWideIndex = FindSystemWideOpenFileIndex(fileName);
                if (systemWideIndex == -1)
                {
                    Console.WriteLine($"File '{fileName}' not found in system-wide open file table. (CloseFile)");
                    return;
                }
                
                // Find the file in the per-process open file table.
                int perProcessIndex = FindPerProcessOpenFileIndex(fileName, processId);
                if (perProcessIndex == -1)
                {
                    Console.WriteLine($"File '{fileName}' not found in per-process open file table.");
                    return;
                }
                
                // Update the system-wide open file table.
                systemWideOpenFileTable[systemWideIndex] = null;
                
                // Update per-process open file table.
                perProcessOpenFileTable[perProcessIndex] = null;
                
                Console.WriteLine($"File '{fileName}' closed successfully.\n");
            }

            // Searches the index of the file in the system wide open file table.
            private int FindSystemWideOpenFileIndex(string fileName)
            {
                for (int i = 0; i < systemWideOpenFileTable.Length; i++)
                {
                    if (systemWideOpenFileTable[i] != null && systemWideOpenFileTable[i].FileName == fileName)
                    {
                        return i;
                    }
                }
                return -1;
            }

            // Searches the index of the file in the per process open file table.
            private int FindPerProcessOpenFileIndex(string fileName, int processId)
            {
                int perProcessTableIndex = (processId - 1) * maxOpenFiles;

                for (int i = perProcessTableIndex; i < perProcessOpenFileTable.Length; i++)
                {
                    if (perProcessOpenFileTable[i] != null && perProcessOpenFileTable[i].FileName == fileName)
                    {
                        return i;
                    }
                }

                return -1;
            }

            // Read the data from a file.
            public void ReadFile(string fileName, int processId)
            {
                Console.WriteLine($"Process {processId} is running ReadFile function for file '{fileName}'");

                // Searches for the file in the directory.
                int fileIndex = FindFileIndex(fileName);
                if (fileIndex == -1)
                {
                    Console.WriteLine($"File '{fileName}' not found.");
                    return;
                }

                // Checks if the file is open in the system wide open file table.
                int systemWideIndex = FindSystemWideOpenFileIndex(fileName);
                if (systemWideIndex == -1)
                {
                    Console.WriteLine($"File '{fileName} cannot be read. (Not open)'");
                    return;
                }

                // Access the directory entry for the file.
                DirectoryEntry fileEntry = directory[fileIndex];

                // Check if the file has data.
                if (fileEntry.FileData == null || fileEntry.FileData.Length == 0)
                {
                    Console.WriteLine($"File '{fileName}' is empty.");
                    return;
                }

                // Display the data from the file.
                Console.WriteLine($"File data for '{fileName}':");
                Console.WriteLine(Encoding.UTF8.GetString(fileEntry.FileData));
                Console.WriteLine();
;           }

            // Write data to a file.
            public void WriteFile(string fileName, int processId, byte[] data)
            {
                Console.WriteLine($"Process {processId} is running WriteFile function for file '{fileName}'");

                // Find the file in the directory.
                int fileIndex = FindFileIndex(fileName);
                if (fileIndex == -1)
                {
                    Console.WriteLine($"File '{fileName}' not found.");
                    return;
                }

                // Check if the file is open by the specified process.
                int perProcessIndex = FindPerProcessOpenFileIndex(fileName, processId);
                if (perProcessIndex == -1)
                {
                    Console.WriteLine($"File '{fileName}' not found in per-process open file table.");
                    return;
                }

                // Access the directory entry for the file.
                DirectoryEntry fileEntry = directory[fileIndex];

                // Append the data to the file's content.
                byte[] currentData = fileEntry.FileData;
                int currentDataLength = currentData != null ? currentData.Length : 0;

                byte[] newData = new byte[currentDataLength + data.Length];
                if (currentData != null )
                {
                    for (int i = 0; i < currentDataLength; i++)
                    {
                        newData[i] = currentData[i];
                    }
                }

                for (int i = 0; i < data.Length; i++)
                {
                    newData[currentDataLength + i] = data[i];
                }

                // Update the file's size and data in the directory entry.
                fileEntry.FileSizeInBytes += data.Length;
                fileEntry.FileData = newData;

                Console.WriteLine($"Data written to file '{fileName}' successfully.\n");
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
                        // Console.WriteLine($"Index: {i}, FileName: {directory[i].FileName}, StartBlockNumber: {directory[i].StartBlockNumber}, FileSize: {directory[i].FileSize}");
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

                Console.WriteLine("\nSystem-Wide Open File Table:");
                for (int i = 0; i < systemWideOpenFileTable.Length; i++)
                {
                    if (systemWideOpenFileTable[i] != null)
                    {
                        Console.WriteLine($"Index: {i}, FileName: {systemWideOpenFileTable[i].FileName}, FCBPointer: {systemWideOpenFileTable[i].FCBPointer}");
                    }
                }

                Console.WriteLine("\nPer-Process Open File Table:");
                for (int i = 0; i < perProcessOpenFileTable.Length; i++)
                {
                    if (perProcessOpenFileTable[i] != null)
                    {
                        Console.WriteLine($"Index: {i}, FileName: {perProcessOpenFileTable[i].FileName}, Handle: {perProcessOpenFileTable[i].Handle}");
                    }
                }
            }
        }
    }
}
