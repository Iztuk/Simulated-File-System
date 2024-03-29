using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static Simulated_File_System.DataStructures;

namespace Simulated_File_System
{
    internal class FileOperations
    {
        public class FileSystem
        {
            // Defines a system-wide open file table.
            private static Dictionary<string, FileControlBlock> systemOpenFileTable = new Dictionary<string, FileControlBlock>();

            // Defines a per-process open file table.
            private Dictionary<string, FileControlBlock> perProcessFileTable = new Dictionary<string, FileControlBlock>();

            // List to store directory entries.
            public static List<DirectoryEntry> Directory = new List<DirectoryEntry>();

            // Volume control block instance.
            public static VolumeControlBlock volumeControlBlock = new VolumeControlBlock(512, 2048); // Define the amount of blocks and the block size.

            // Create a file with specified size in blocks.
            public static void CreateFile(string fileName, int fileSizeInBlocks)
            {
                // Checks if a file with the same name already exists in the directory.
                if (Directory.Exists(file => file.FileName == fileName))
                {
                    Console.WriteLine("File with the same name already exists in the directory.");
                    return;
                }

                // Create a new directory entry for the file.
                DirectoryEntry newEntry = new DirectoryEntry
                {
                    FileName = fileName,
                    FileSize = fileSizeInBlocks * volumeControlBlock.BlockSize,
                    StartBlockNumber = volumeControlBlock.NextAvailableBlock // Placeholder for the actual block number.
                };

                // Update the next available block.
                volumeControlBlock.NextAvailableBlock += fileSizeInBlocks;

                // Add the new entry to the directory.
                Directory.Add(newEntry);
                Console.WriteLine($"File '{fileName}' has been created with a size of '{fileSizeInBlocks}' blocks.");
            }

            // Open a file and update open file tables.
            public void OpenFile(string fileName)
            {
                // Locate the file in the directory.
                DirectoryEntry fileEntry = Directory.Find(file => file.FileName == fileName);
                if (fileEntry == null)
                {
                    Console.WriteLine("File not found.");
                    return;
                }

                // Create a new instance of the per process file control block.
            }

            // Close a file and update the open file tables.
            public void CloseFile(string fileName)
            {

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

        }
    }
}
