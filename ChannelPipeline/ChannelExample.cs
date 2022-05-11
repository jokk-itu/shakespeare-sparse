using System.Threading.Channels;

namespace ChannelPipeline;

public static class ChannelExample
{
    private static async IAsyncEnumerable<string> ReadAllLinesAsync(string path)
    {
        using var sr = System.IO.File.OpenText(path);
        string? line;
        while ((line = await sr.ReadLineAsync()) is not null)
            yield return line;
    }

    private static async Task ReadAsync(ChannelWriter<string> channel, string path)
    {
        await foreach (var line in ReadAllLinesAsync(path))
        {
            await channel.WriteAsync(line);
            Console.WriteLine($"Writing from {path}: {line}");
        }
    }

    public static Task<ICollection<Channel<string>>> ReadShakespeareAsync()
    {
        ICollection<Channel<string>> writers = new List<Channel<string>> { Channel.CreateUnbounded<string>(), Channel.CreateUnbounded<string>(), Channel.CreateUnbounded<string>() };
        Task.Run(() =>
        {
            Task.WhenAll(
                ReadAsync(writers.ElementAt(0), "hamlet.txt"),
                ReadAsync(writers.ElementAt(1), "coriolanus.txt"),
                ReadAsync(writers.ElementAt(2), "romeoandjuliet.txt"));
            
            foreach (var writer in writers)
            {
                writer.Writer.Complete();
            }
            return Task.CompletedTask;
        });

        return Task.FromResult(writers);
    }

    public static Task<Channel<string>> WriteShakespeareAsync(ICollection<Channel<string>> readers)
    {
        var writer = Channel.CreateUnbounded<string>();

        Task.Run(() =>
        {
            Task.WaitAll(readers.Select(reader => Task.Run(async () =>
            {
                await foreach (var line in reader.Reader.ReadAllAsync())
                {
                    Console.WriteLine($"Read: {line}");
                    await writer.Writer.WriteAsync(line);
                }
            })).ToArray());

            writer.Writer.Complete();
        });
        
        return Task.FromResult(writer);
    }
}