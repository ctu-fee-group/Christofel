using Remora.Commands.Conditions;

namespace Christofel.CommandsLib.Permissions
{
    public class RequirePermissionAttribute : ConditionAttribute
    {
        public RequirePermissionAttribute(string permission)
        {
            Permission = permission;
        }
        
        public string Permission { get; set; }
    }
}