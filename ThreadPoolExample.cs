using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        // Example usage
        int numberOfThreads = 4; // Set the number of threads you want to use
        List<string> itemsToProcess = new List<string> { "Item1", "Item2", "Item3", "Item4", "Item5", "Item6" };

        await ProcessListWithThreadPool(numberOfThreads, itemsToProcess, ProcessItem);
    }

    public static async Task ProcessListWithThreadPool(int maxDegreeOfParallelism, List<string> items, Action<string> processAction)
    {
        using (var semaphore = new SemaphoreSlim(maxDegreeOfParallelism))
        {
            var tasks = new List<Task>();

            foreach (var item in items)
            {
                // Wait until the semaphore allows us to proceed
                await semaphore.WaitAsync();

                tasks.Add(Task.Run(() =>
                {
                    try
                    {
                        processAction(item);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }));
            }

            // Wait for all tasks to complete
            await Task.WhenAll(tasks);
        }
    }

    // Define the action to process each item
    private static void ProcessItem(string item)
    {
        // Simulate some work
        Console.WriteLine($"Processing {item} on Thread {Thread.CurrentThread.ManagedThreadId}");
        Thread.Sleep(1000); // Simulate time-consuming work
    }
}
