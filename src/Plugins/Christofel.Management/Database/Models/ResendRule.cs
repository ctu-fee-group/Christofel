//
//   ResendRule.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel.DataAnnotations.Schema;
using Remora.Rest.Core;

namespace Christofel.Management.Database.Models
{
    /// <summary>
    /// Represents table ResendRule holding data about channels to resend messages from to another channel.
    /// </summary>
    [Table("ResendRule", Schema = ManagementContext.SchemaName)]
    public class ResendRule
    {
        /// <summary>
        /// Gets or sets primary key of <see cref="ResendRule" />.
        /// </summary>
        public int ResendRuleId { get; set; }

        /// <summary>
        /// Gets or sets the channel from which to resend.
        /// </summary>
        public Snowflake FromChannel { get; set; }

        /// <summary>
        /// Gets or sets the channel to which to resend.
        /// </summary>
        public Snowflake ToChannel { get; set; }
    }
}