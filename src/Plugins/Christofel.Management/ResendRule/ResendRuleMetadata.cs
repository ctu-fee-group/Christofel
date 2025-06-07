//
//   ResendRuleMetadata.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Remora.Rest.Core;

namespace Christofel.Management.ResendRule
{
    /// <summary>
    /// The metadata for resend rule containing where was the message sent.
    /// </summary>
    public record ResendRuleMetadata
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResendRuleMetadata"/> class.
        /// </summary>
        /// <param name="channel">The id of the channel where the message was sent.</param>
        /// <param name="sentMessage">The id of the sent message.</param>
        /// <param name="resentMessages">The messages that were resent.</param>
        public ResendRuleMetadata(Snowflake channel, Snowflake sentMessage, (Snowflake Channel, Snowflake Message)[] resentMessages)
        {
            Channel = channel;
            SentMessage = sentMessage;
            ResentMessages = resentMessages;
        }

        /// <summary>
        /// Gets the channel where the message was sent.
        /// </summary>
        public Snowflake Channel { get; }

        /// <summary>
        /// Gets the id of the message that was resent.
        /// </summary>
        public Snowflake SentMessage { get; }

        /// <summary>
        /// Gets the messages that were resent.
        /// </summary>
        public (Snowflake Channel, Snowflake Message)[] ResentMessages { get; }
    }
}