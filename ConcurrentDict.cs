using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class Message : IComparable<Message>
{
    public int id { get; set; }
    public DateTime update_time { get; set; }
    public string content { get; set; }

    public int CompareTo(Message other)
    {
        if (id == other.id)
            return update_time.CompareTo(other.update_time);
        else
            return id.CompareTo(other.id);
    }
}

public class MQReader
{
    private ConcurrentDictionary<int, List<Message>> dictMessages = new ConcurrentDictionary<int, List<Message>>();
    private int numberOfThreads = 5; // Example number of reading threads, configurable
    private AutoResetEvent dataReadyEvent = new AutoResetEvent(false);

    public MQReader()
    {
        Task.Run(() => HandleMessage()); // Start the handling thread
    }

    public void StartReading()
    {
        for (int i = 0; i < numberOfThreads; i++)
        {
            Task.Run(() =>
            {
                while (true) // Replace with actual condition
                {
                    Message message = ReadMessageFromMQ(); // Implement this method
                    AddMessageToDictionary(message);
                }
            });
        }
    }

    private void AddMessageToDictionary(Message message)
    {
        dictMessages.AddOrUpdate(message.id,
            new List<Message> { message },
            (key, existingList) =>
            {
                lock (existingList) // Ensure thread-safe updates
                {
                    existingList.Add(message);
                }
                return existingList;
            });

        dataReadyEvent.Set(); // Signal that new data is available
    }

    private void HandleMessage()
    {
    while (true) // Use a proper condition to exit the loop as needed
    {
        dataReadyEvent.WaitOne(); // Wait for signal

        // Take a snapshot of keys to process current messages only
        var keysSnapshot = dictMessages.Keys.ToList();
        
        List<string> sortedList = new List<string>();
        foreach (var key in keysSnapshot)
        {
            if (dictMessages.TryRemove(key, out List<Message> listMessages))
            {
                lock (listMessages) // Ensure thread-safe access to list
                {
                    listMessages.Sort();
                    sortedList.AddRange(listMessages.Select(m => m.content));
                }
            }
        }

        foreach (var content in sortedList)
        {
            InsertToDatabase(content); // Implement this database insertion logic
        }

        // Optionally: Notify MQ manager the data is committed, and the data can be removed from the queue
    }
}


    private Message ReadMessageFromMQ()
    {
        // This method should be implemented to read messages from IBM MQ
        return new Message(); // Placeholder
    }

    private void InsertToDatabase(string content)
    {
        // This method should be implemented to insert the message content into the database
    }
}
