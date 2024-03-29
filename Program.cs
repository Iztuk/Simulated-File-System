namespace Simulated_File_System
{
    using static FileOperations;
    internal class Program
    {
        static void Main(string[] args)
        {
            // Test case 1: Create a file
            FileSystem.CreateFile("file1", 2);

            // Test case 2: Create another file
            FileSystem.CreateFile("file2", 3);

            // Display directory entries to verify
            Console.WriteLine("Directory Entries:");
            foreach (var entry in FileSystem.Directory)
            {
                Console.WriteLine($"File Name: {entry.FileName}, Start Block: {entry.StartBlockNumber}, File Size: {entry.FileSize} bytes");
            }
        }
    }
}
