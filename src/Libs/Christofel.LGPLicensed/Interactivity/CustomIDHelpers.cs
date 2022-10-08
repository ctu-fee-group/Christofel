using Christofel.CommandsLib;
using Humanizer;
using Remora.Discord.API.Abstractions.Objects;

namespace Christofel.LGPLicensed.Interactivity;

/// <summary>
/// Contains various helper methods for creating component ID strings.
/// </summary>
/// This code is from Remora.Discord by @Nihlus
/// see https://github.com/Remora/Remora.Discord/blob/main/Remora.Discord.Interactivity/CustomIDHelpers.cs
public class CustomIDHelpers
{
    /// <summary>
    /// Creates an ID string that can be used with button components.
    /// </summary>
    /// <param name="name">
    /// The name used to identify the component. Must be unique among the components in the message.
    /// </param>
    /// <param name="path">
    /// The group path to the component; that is, the outer groups that must be traversed before reaching the group
    /// where the component's handler is declared.
    /// </param>
    /// <param name="parameters">The parameters to pass to the command.</param>
    /// <returns>The custom ID.</returns>
    public static string CreateButtonID(string name, string path, params string[] parameters)
        => CreateID(ComponentType.Button, name, path, parameters);

    /// <summary>
    /// Creates an ID string that can be used with select menu components.
    /// </summary>
    /// <param name="name">
    /// The name used to identify the component. Must be unique among the components in the message.
    /// </param>
    /// <param name="path">
    /// The group path to the component; that is, the outer groups that must be traversed before reaching the group
    /// where the component's handler is declared.
    /// </param>
    /// <param name="parameters">The parameters to pass to the command.</param>
    /// <returns>The custom ID.</returns>
    public static string CreateSelectMenuID(string name, string path, params string[] parameters)
        => CreateID(ComponentType.SelectMenu, name, path, parameters);

    /// <summary>
    /// Creates an ID string that can be used with button components.
    /// </summary>
    /// <param name="name">
    /// The name used to identify the component. Must be unique among the components in the message.
    /// </param>
    /// <param name="path">
    /// The group path to the component; that is, the outer groups that must be traversed before reaching the group
    /// where the component's handler is declared.
    /// </param>
    /// <param name="parameters">The parameters to pass to the command.</param>
    /// <returns>The custom ID.</returns>
    public static string CreateModalID(string name, string path, params string[] parameters)
        => FormatID("modal", name, path, parameters);

    /// <summary>
    /// Creates an ID string that can be used with message components.
    /// </summary>
    /// <param name="type">The component type that the ID is for.</param>
    /// <param name="name">
    /// The name used to identify the component. Must be unique among the components in the message.
    /// </param>
    /// <param name="path">
    /// The group path to the component; that is, the outer groups that must be traversed before reaching the group
    /// where the component's handler is declared.
    /// </param>
    /// <param name="parameters">The parameters to pass to the command.</param>
    /// <returns>The custom ID.</returns>
    public static string CreateID(ComponentType type, string name, string path, params string[] parameters)
        => FormatID(type.ToString().Kebaberize(), name, path, parameters);

    private static string FormatID(string type, string name, string path, string[] parameters)
    {
        var customID = $"{Constants.InteractivityPrefix}::{path} {type}::{name} {string.Join(' ', parameters)}";
        if (customID.Length <= 100)
        {
            return customID;
        }

        var reductionRequired = customID.Length - 100;
        throw new ArgumentException
        (
            $"The final ID is too long. Reduce your parameter lengths by at least {reductionRequired} characters."
        );
    }
}