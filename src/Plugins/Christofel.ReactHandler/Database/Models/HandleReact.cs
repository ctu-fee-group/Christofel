//
//   HandleReact.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel.DataAnnotations.Schema;
using Remora.Rest.Core;

namespace Christofel.ReactHandler.Database.Models
{
    /// <summary>
    /// Database model representing data about reactions that should assign channels or roles.
    /// </summary>
    [Table("HandleReact", Schema = ReactHandlerContext.SchemaName)]
    public class HandleReact
    {
        /// <summary>
        /// Primary key of the model.
        /// </summary>
        public int HandleReactId { get; set; }

        /// <summary>
        /// Id of the channel that the message is in.
        /// </summary>
        public Snowflake ChannelId { get; set; }

        /// <summary>
        /// Id of the message that should be watched.
        /// </summary>
        public Snowflake MessageId { get; set; }

        /// <summary>
        /// Reaction that should be watched on the specified message.
        /// </summary>
        public string Emoji { get; set; } = null!;

        /// <summary>
        /// Type of the entity to be added/removed.
        /// </summary>
        public HandleReactType Type { get; set; }

        /// <summary>
        /// Id of the entity to be added/removed.
        /// </summary>
        public Snowflake EntityId { get; set; }
    }
}