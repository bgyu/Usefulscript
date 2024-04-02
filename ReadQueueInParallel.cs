using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    static void Main(string[] args)
    {
        var processor = new MessageProcessor();

        // Specify the number of threads for enqueuing messages
        int numberOfThreads = 4;

        // Simulate adding messages from multiple threads
        SimulateEnqueueMessages(processor, numberOfThreads);

        // Waiting for a bit to ensure all messages have been enqueued and processing starts
        Thread.Sleep(2000); // This is for demonstration; in a real app, you'd use a more robust sync mechanism

        // Simulating the end of message enqueueing for specific IDs, in a real scenario this should be called when you know no more messages will be added.
        // It might be based on application logic outside the scope of this demonstration.
        processor.CompleteAddingForAllIds();

        Console.WriteLine("All messages have been enqueued. Processing is underway.");

        // Wait for a key press to exit the program, in a real application, you would have a more robust way to wait for all processing to complete
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }

    static void SimulateEnqueueMessages(MessageProcessor processor, int numberOfThreads)
    {
        var tasks = new List<Task>();

        for (int i = 0; i < numberOfThreads; i++)
        {
            int threadId = i; // Local copy for lambda capture
            tasks.Add(Task.Run(() =>
            {
                var random = new Random();
                
                // Each thread enqueues a series of messages
                for (int j = 0; j < 5; j++) // Assuming each thread produces 5 messages for simplicity
                {
                    var messageId = random.Next(1, 4); // Randomly assigns a message ID between 1 and 3
                    var message = new Message
                    {
                        Id = messageId,
                        Timestamp = DateTime.UtcNow.AddSeconds(random.Next(1, 5)),
                        Content = $"Thread {threadId} Message {j} for ID {messageId}"
                    };
                    processor.EnqueueMessage(message);
                    Thread.Sleep(100); // Simulate delay between message production
                }
            }));
        }

        Task.WaitAll(tasks.ToArray()); // Wait for all enqueueing tasks to complete
    }
}


public class Message
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string Content { get; set; }
}

public class MessageProcessor
{
    private readonly ConcurrentDictionary<int, BlockingCollection<Message>> _messageQueues = new ConcurrentDictionary<int, BlockingCollection<Message>>();
    private readonly Dictionary<int, Task> _processingTasks = new Dictionary<int, Task>();

    // Enqueue a message for processing
    public void EnqueueMessage(Message message)
    {
        var queue = _messageQueues.GetOrAdd(message.Id, _ => new BlockingCollection<Message>(new ConcurrentQueue<Message>()));
        queue.Add(message);

        lock (_processingTasks)
        {
            // Ensure only one task per ID
            if (!_processingTasks.ContainsKey(message.Id))
            {
                _processingTasks[message.Id] = Task.Run(() => ProcessQueue(message.Id));
            }
        }
    }

    // Process messages for a specific ID
    private void ProcessQueue(int id)
    {
        var queue = _messageQueues[id];

        foreach (var message in queue.GetConsumingEnumerable())
        {
            try
            {
                ProcessMessage(message);
            }
            catch (Exception ex)
            {
                // Implement appropriate error handling
                Console.WriteLine($"Error processing message: {ex.Message}");
            }
        }
    }

    // Simulate processing a message
    private void ProcessMessage(Message message)
    {
        Console.WriteLine($"Processing message ID: {message.Id}, Content: {message.Content}, Timestamp: {message.Timestamp}");
        // Simulate some work
        Task.Delay(1000).Wait();
    }

    // Signal that no more messages will be added for a specific ID
    public void CompleteAdding(int id)
    {
        if (_messageQueues.TryGetValue(id, out var queue))
        {
            queue.CompleteAdding();
        }
    }
}
