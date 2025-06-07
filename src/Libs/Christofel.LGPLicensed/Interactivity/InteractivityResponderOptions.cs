namespace Christofel.LGPLicensed.Interactivity;

/// <summary>
/// Options for <see cref="InteractivityResponder"/>.
/// </summary>
/// This code is from Remora.Discord by @Nihlus
/// see https://github.com/Remora/Remora.Discord/blob/main/Remora.Discord.Interactivity/Responders/InteractivityResponderOptions.cs
public class InteractivityResponderOptions
{
    /// <summary>
    /// Gets or sets whether to suppress automatic responses.
    /// </summary>
    public bool SuppressAutomaticResponses { get; set; } = false;
}