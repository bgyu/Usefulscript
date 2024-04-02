using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

class Program
{
    static void Main(string[] args)
    {
        var processor = new MessageProcessor();

        // Simulate adding messages with different IDs and timestamps
        processor.EnqueueMessage(new Message { Id = 1, Timestamp = DateTime.UtcNow.AddSeconds(1), Content = "Message 1a" });
        processor.EnqueueMessage(new Message { Id = 1, Timestamp = DateTime.UtcNow.AddSeconds(2), Content = "Message 1b" });
        processor.EnqueueMessage(new Message { Id = 2, Timestamp = DateTime.UtcNow.AddSeconds(1), Content = "Message 2" });
        processor.EnqueueMessage(new Message { Id = 3, Timestamp = DateTime.UtcNow.AddSeconds(1), Content = "Message 3" });

        // Simulating the end of message enqueueing for a specific ID
        processor.CompleteAdding(1);
        processor.CompleteAdding(2);
        processor.CompleteAdding(3);

        // Wait for a key press to exit the program, in a real application, you would have a more robust way to wait for all processing to complete
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
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
