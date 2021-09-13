//
//  Program.cs
//
//  Copyright (c) Christofel authors. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Christofel.Scheduler.Abstractions;
using Christofel.Scheduler.Example.Data;
using Christofel.Scheduler.Example.Jobs;
using Christofel.Scheduler.Extensions;
using Christofel.Scheduler.Triggers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Christofel.Scheduler.Example
{
    /// <summary>
    /// The entry point class of Scheduler example.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// The entry point of Scheduler example.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task Main(string[] args)
        {
            var services = BuildServiceProvider();
            IScheduler scheduler = services.GetRequiredService<IScheduler>();

            var nonConcurrencyState = new NonConcurrentTrigger.State();

            await scheduler.StartAsync();

            // hello world that will be executed only once (that is using simple trigger)
            var helloWorldJobData = new TypedJobData<HelloWorldJob>(new JobKey("HelloWorld", "Simple"))
                .AddData("Where", "Simple"); // Any key is accepted, all values will be passed to the constructor, only types matter.
            await scheduler.ScheduleAsync(helloWorldJobData, new SimpleTrigger());

            var helloWorldRecurringData = new TypedJobData<HelloWorldJob>(new JobKey("HelloWorld", "Recurring"))
                .AddData("Where", "Recurring");
            await scheduler.ScheduleAsync(helloWorldRecurringData, new RecurringTrigger(TimeSpan.FromSeconds(10)));

            // Execute job that will have to wait for blocking job to finish
            var blockingJobData = new TypedJobData<BlockingJob>(new JobKey("NonConcurrent", "Blocking"));
            var waitJobData = new TypedJobData<PassDataJob>(new JobKey("NonConcurrent", "WaitForBlocking"))
                .AddData("Data", new MyJobData("Running after blocking job", 1));

            await scheduler.ScheduleAsync
                (blockingJobData, new NonConcurrentTrigger(new SimpleTrigger(), nonConcurrencyState));
            await scheduler.ScheduleAsync
                (waitJobData, new NonConcurrentTrigger(new SimpleTrigger(), nonConcurrencyState));

            await Task.Delay(-1);
        }

        private static IServiceProvider BuildServiceProvider()
        {
            return new ServiceCollection()
                .AddLogging(b => b.AddConsole())
                .AddScheduler()
                .Replace(ServiceDescriptor.Singleton<IJobStore, ApplicationJobStore>())
                .BuildServiceProvider();
        }
    }
}