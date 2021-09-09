//
//   ICtuTokenApi.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading;
using System.Threading.Tasks;
using Christofel.BaseLib.User;

namespace Christofel.Api.OAuth
{
    public interface ICtuTokenApi
    {
        public Task<ICtuUser> CheckTokenAsync(string accessToken, CancellationToken token = default);
    }
}