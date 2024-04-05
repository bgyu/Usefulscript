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
        while (true) // Replace with actual condition
        {
            dataReadyEvent.WaitOne(); // Wait for signal

            List<string> sortedList = new List<string>();
            foreach (var kvp in dictMessages)
            {
                List<Message> listMessages;
                if (dictMessages.TryRemove(kvp.Key, out listMessages))
                {
                    listMessages.Sort();
                    foreach (var m in listMessages)
                    {
                        sortedList.Add(m.content);
                    }
                }
            }

            foreach (var str in sortedList)
            {
                InsertToDatabase(str); // Implement this method
            }

            // Optionally: Notify MQ manager the data is committed and can be removed from the queue
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
