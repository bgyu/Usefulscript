public class Message
{
    public int Id { get; set; }
    public string Content { get; set; }
}

public class Producer
{
    private readonly ConcurrentQueue<Message> _queue;

    public Producer(ConcurrentQueue<Message> queue)
    {
        _queue = queue;
    }

    public void Produce(int messageId, string content)
    {
        var message = new Message { Id = messageId, Content = content };
        _queue.Enqueue(message);
        Console.WriteLine($"Produced: {content}");
    }
}

public class Consumer
{
    private readonly ConcurrentQueue<Message> _queue;
    private readonly SemaphoreSlim _semaphore;

    public Consumer(ConcurrentQueue<Message> queue, SemaphoreSlim semaphore)
    {
        _queue = queue;
        _semaphore = semaphore;
    }

    public async Task Consume()
    {
        while (true)
        {
            await _semaphore.WaitAsync();

            if (_queue.TryDequeue(out Message message))
            {
                Console.WriteLine($"Consuming: {message.Content}");
                // Simulate database insertion delay
                await Task.Delay(1000);
                Console.WriteLine($"Consumed: {message.Content}");
            }

            _semaphore.Release();
        }
    }
}

public class Program
{
    public static async Task Main(string[] args)
    {
        var queue = new ConcurrentQueue<Message>();
        var semaphore = new SemaphoreSlim(1, 1); // Ensures one-at-a-time processing
        var producer = new Producer(queue);
        var consumers = new List<Consumer>
        {
            new Consumer(queue, semaphore),
            new Consumer(queue, semaphore) // Add more as needed
        };

        // Simulate producing messages
        for (int i = 1; i <= 10; i++)
        {
            producer.Produce(i, $"Message {i}");
        }

        // Start consuming
        var consumerTasks = consumers.Select(c => c.Consume()).ToList();

        // Wait for all consumer tasks to complete (in a real application, you might run indefinitely)
        await Task.WhenAll(consumerTasks);
    }
}
