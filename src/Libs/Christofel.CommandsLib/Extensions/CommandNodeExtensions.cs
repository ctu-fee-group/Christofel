using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Christofel.CommandsLib.Permissions;
using Remora.Commands.Trees.Nodes;
using Remora.Discord.Commands.Extensions;

namespace Christofel.CommandsLib.Extensions
{
    public static class CommandNodeExtensions
    {
        public static string? GetChristofelPermission(this CommandNode commandNode)
        {
            return commandNode.FindCustomAttributeOnLocalTree<RequirePermissionAttribute>()?.Permission;
        }

        public static string? GetChristofelPermission(this GroupNode groupNode)
        {
            return groupNode.GroupTypes.Select(x => x.GetCustomAttribute<RequirePermissionAttribute>())
                .FirstOrDefault()?.Permission;
        }
    }
}