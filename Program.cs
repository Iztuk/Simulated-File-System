namespace Simulated_File_System
{
    using static Simulated_File_System.FileOperations;
    using System.Threading;

    internal class Program
    {
        // Shared ManualResetEvent to synchronize threads
        private static ManualResetEvent thread1Completed = new ManualResetEvent(false);

        public static void Main(string[] args)
        {
            // Create an instance of the file system
            FileOperations.FileSystem fileSystem = new FileOperations.FileSystem(maxOpenFiles: 2, maxProcesses: 3);

            // Start thread 1
            Thread p1 = new Thread(() => SimulateProcess(fileSystem, processId: 1));
            p1.Start();

            // Wait for thread 1 to complete all its processes
            p1.Join();

            // Set the event to signal that thread 1 has completed
            thread1Completed.Set();

            // Start threads 2 and 3 simultaneously
            Thread p2 = new Thread(() => SimulateProcess(fileSystem, processId: 2));
            Thread p3 = new Thread(() => SimulateProcess(fileSystem, processId: 3));
            p2.Start();
            p3.Start();

            // Wait for threads 2 and 3 to complete
            p2.Join();
            p3.Join();

            // Display files in the directory after all operations
            fileSystem.ListFiles();
        }

        // Simulate a process (thread) interacting with the file system
        public static void SimulateProcess(FileOperations.FileSystem fileSystem, int processId)
        {
            // Wait for thread 1 to complete its processes if not thread 1
            if (processId != 1)
            {
                thread1Completed.WaitOne();
            }

            // Simulate process operations...
            Console.WriteLine($"Thread {processId} started.");

            // Example operations: create files, open files, write data, etc.
            fileSystem.CreateFile($"file_process{processId}_1.txt", fileSize: 2);
            fileSystem.CreateFile($"file_process{processId}_2.txt", fileSize: 3);

            // Example: open files
            int handle1 = fileSystem.OpenFile($"file_process{processId}_1.txt", processId);
            int handle2 = fileSystem.OpenFile($"file_process{processId}_2.txt", processId);

            // Simulate process operations...

            // Example: close files
            fileSystem.CloseFile($"file_process{processId}_1.txt", processId);
            fileSystem.CloseFile($"file_process{processId}_2.txt", processId);

            Console.WriteLine($"Thread {processId} completed.");
        }
    }
}
