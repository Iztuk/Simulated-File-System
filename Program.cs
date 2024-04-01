namespace Simulated_File_System
{
    using static Simulated_File_System.FileOperations;

    internal class Program
    {
        public static void Main(string[] args)
        {
            // Create an instance of the file system
            FileOperations.FileSystem fileSystem = new FileOperations.FileSystem(maxOpenFiles: 2, maxProcesses: 3);

            // Start threads to simulate processes
            Thread p1 = new Thread(() => SimulateProcess(fileSystem, processId: 1));
            Thread p2 = new Thread(() => SimulateProcess(fileSystem, processId: 2));
            Thread p3 = new Thread(() => SimulateProcess(fileSystem, processId: 3));

            // Start the threads
            p1.Start();
            p2.Start();
            p3.Start();

            // Wait for threads to complete
            p1.Join();
            p2.Join();
            p3.Join();

            // Display files in the directory after all operations
            fileSystem.ListFiles();
        }

        // Simulate a process (thread) interacting with the file system
        public static void SimulateProcess(FileOperations.FileSystem fileSystem, int processId)
        {
            // Create files
            fileSystem.CreateFile($"file_process{processId}_1.txt", fileSize: 2);
            fileSystem.CreateFile($"file_process{processId}_2.txt", fileSize: 3);

            // Open files
            int handle1 = fileSystem.OpenFile($"file_process{processId}_1.txt", processId);
            int handle2 = fileSystem.OpenFile($"file_process{processId}_2.txt", processId);

            // Perform other file operations...
        }
    }
}
