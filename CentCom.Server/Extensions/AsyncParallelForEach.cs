using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace CentCom.Server.Extensions;

// Taken from https://scatteredcode.net/parallel-foreach-async-in-c/
public static class Extensions
{
    public static Task AsyncParallelForEach<T>(this IEnumerable<T> source, Func<T, Task> body, int maxDegreeOfParallelism = DataflowBlockOptions.Unbounded, TaskScheduler scheduler = null)
    {
        var options = new ExecutionDataflowBlockOptions
        {
            MaxDegreeOfParallelism = maxDegreeOfParallelism
        };
        if (scheduler != null)
            options.TaskScheduler = scheduler;
        var block = new ActionBlock<T>(body, options);
        foreach (var item in source)
            block.Post(item);
        block.Complete();
        return block.Completion;
    }

    public static async Task AsyncParallelForEach<T>(this IAsyncEnumerable<T> source, Func<T, Task> body, int maxDegreeOfParallelism = DataflowBlockOptions.Unbounded, TaskScheduler scheduler = null)
    {
        var options = new ExecutionDataflowBlockOptions
        {
            MaxDegreeOfParallelism = maxDegreeOfParallelism
        };
        if (scheduler != null)
            options.TaskScheduler = scheduler;
        var block = new ActionBlock<T>(body, options);
        await foreach (var item in source)
            block.Post(item);
        block.Complete();
        await block.Completion;
    }
}