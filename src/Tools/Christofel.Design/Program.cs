//
//   Program.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Christofel.Api;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Christofel.Design
{
    /// <summary>
    /// The startup class.
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// Main entrypoint of the design .
        /// </summary>
        /// <param name="args">The program arguments.</param>
        public static void Main(string[] args)
            => CreateHostBuilder(args).Build().Run();

        private static IHostBuilder CreateHostBuilder(string[] args)
            => Host.CreateDefaultBuilder(args);
    }
}