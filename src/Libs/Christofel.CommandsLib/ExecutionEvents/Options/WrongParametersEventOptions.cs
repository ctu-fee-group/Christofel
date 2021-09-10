//
//   WrongParametersEventOptions.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Remora.Commands.Trees.Nodes;

namespace Christofel.CommandsLib.ExecutionEvents.Options
{
    /// <summary>
    /// Options for <see cref="WrongParametersExecutionEvent"/>.
    /// </summary>
    public class WrongParametersEventOptions
    {
        /// <summary>
        /// Gets format of explanation for matched <see cref="CommandNode"/>.
        /// </summary>
        /// <remarks>
        /// Supports parameters:
        /// - Name - full name of the command.
        /// - Prefix - <see cref="Prefix"/>.
        /// - Parameters - what parameters are needed for the command formatted by <see cref="RequiredParameterExplanationFormat"/> or <see cref="OptionalParameterExplanationFormat"/>.
        /// </remarks>
        public string CommandExplanationFormat { get; set; } = "Wrong syntax. (Usage: `{Prefix}{Name} {Parameters})`";

        /// <summary>
        /// Gets format for explanation for matched <see cref="GroupNode"/>.
        /// </summary>
        /// <remarks>
        /// Supports parameters:
        /// - Name - full name of the group.
        /// - Prefix - <see cref="Prefix"/>.
        /// - Children - the children nodes that may be executed.
        /// </remarks>
        public string GroupExplanationFormat { get; set; } =
            "This is a group of commands. Use one of: `{Prefix}{Name} <{Children}>`";

        /// <summary>
        /// Gets format for required parameters of command.
        /// </summary>
        /// <remarks>
        /// Support parameters:
        /// - Name - name of the parameter.
        /// - Type - type of the parameter.
        /// </remarks>
        public string RequiredParameterExplanationFormat { get; set; } = "<{Name}:{Type}>";

        /// <summary>
        /// Gets format of optional parameters of command.
        /// </summary>
        /// <remarks>
        /// Support parameters:
        /// - Name - name of the parameter.
        /// - Type - type of the parameter.
        /// </remarks>
        public string OptionalParameterExplanationFormat { get; set; } = "[{Name}:{Type}]";

        /// <summary>
        /// Gets prefix of the message commands.
        /// </summary>
        public string Prefix { get; set; } = "!";
    }
}