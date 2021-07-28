namespace Christofel.BaseLib.Database.Models
{
    /// <summary>
    /// Entity used for storing configuration in Name Value manner
    /// </summary>
    public class ConfigurationEntry
    {
        public string Name { get; set; } = null!;

        public string Value { get; set; } = null!;
    }
}