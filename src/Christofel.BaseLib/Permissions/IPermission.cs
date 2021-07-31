namespace Christofel.BaseLib.Permissions
{
    /// <summary>
    /// Permission information that can be used in lists of permissions 
    /// </summary>
    public interface IPermission
    {
        public string PermissionName { get; }
        
        public string DisplayName { get; }
        
        public string Description { get; }
    }
}