//
//   WrongParametersEventOptions.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Christofel.CommandsLib.ExecutionEvents.Options
{
    public class WrongParametersEventOptions
    {
        public string CommandExplanationFormat { get; set; } = "Wrong syntax. (Usage: `{Prefix}{Name} {Parameters})`";

        public string GroupExplanationFormat { get; set; } =
            "This is a group of commands. Use one of: `{Prefix}{Name} <{Children}>`";

        public string RequiredParameterExplanationFormat { get; set; } = "<{Name}:{Type}>";

        public string OptionalParameterExplanationFormat { get; set; } = "[{Name}:{Type}]";

        public string Prefix { get; set; } = "!";
    }
}