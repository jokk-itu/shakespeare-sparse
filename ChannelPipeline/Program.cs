using ChannelPipeline;

await ChannelExample.WriteShakespeareAsync(await ChannelExample.ReadShakespeareAsync());
Console.ReadLine();