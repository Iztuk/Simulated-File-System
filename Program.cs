namespace Simulated_File_System
{
    using static Simulated_File_System.FileOperations;
    using System.Threading;
    using System.Text;

    internal class Program
    {
        // Shared ManualResetEvent to synchronize threads
        private static ManualResetEvent thread1Completed = new ManualResetEvent(false);

        public static void Main(string[] args)
        {
            // Create an instance of the file system
            FileOperations.FileSystem fileSystem = new FileOperations.FileSystem(maxOpenFiles: 10, maxProcesses: 3);

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
            // Simulated text for the Write function.
            byte[] dataForFile1 = Encoding.UTF8.GetBytes("This is the sample text written to file1.txt");
            byte[] dataForFile2 = Encoding.UTF8.GetBytes("This is the sample text written to file2.txt");

            // Wait for thread 1 to complete its processes if not thread 1
            if (processId != 1)
            {
                thread1Completed.WaitOne();
            }

            // Simulate process operations...
            Console.WriteLine($"Thread {processId} started.");

            if (processId == 1)
            {
                fileSystem.CreateFile("file1.txt", fileSizeInBytes: 1, processId);

                fileSystem.WriteFile("file1.txt", processId, dataForFile1);

                fileSystem.CloseFile("file1.txt", processId);

                fileSystem.CreateFile("file2.txt", fileSizeInBytes: 1, processId);

                fileSystem.WriteFile("file2.txt", processId, dataForFile2);

                fileSystem.CloseFile("file2.txt", processId);
            }
            else if (processId == 2)
            {
                fileSystem.OpenFile("file1.txt", processId);

                fileSystem.ReadFile("file1.txt", processId);

                fileSystem.CloseFile("file1.txt", processId);
            }
            else if(processId == 3)
            {
                fileSystem.OpenFile("file2.txt", processId);

                fileSystem.ReadFile("file2.txt", processId);

                fileSystem.CloseFile("file2.txt", processId);
            }
            else
            {
                Console.WriteLine("Unexpected process Id.");
            }

            fileSystem.DisplayVCBBitMap();

            Console.WriteLine($"Thread {processId} completed.\n");
        }
    }
}
