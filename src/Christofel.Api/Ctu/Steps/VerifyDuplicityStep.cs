using System.Linq;
using System.Threading.Tasks;
using Christofel.Api.Ctu.Steps.Enums;
using Christofel.Api.Exceptions;
using Christofel.BaseLib.Database;
using Christofel.BaseLib.Database.Models;
using Christofel.BaseLib.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Christofel.Api.Ctu.Steps
{
    /// <summary>
    /// Verifies all types of duplicities, handles them accordingly
    /// </summary>
    /// <remarks>
    /// Tries to match duplicities from the database,
    ///
    /// for CtuSide and DiscordSide duplicities,
    /// DuplicityApproved column is checked,
    /// if it is true, the process continues,
    /// if not, the user is notified about duplicity
    /// and the process ends up in error
    ///
    /// for Both sides duplicity,
    /// old user is updated with new information,
    /// new one is deleted
    /// </remarks>
    public class VerifyDuplicityStep : CtuAuthStep
    {
        private record Duplicity(DuplicityType Type, DbUser User);

        private Duplicity? _foundDuplicity;

        public VerifyDuplicityStep(ILogger<CtuAuthProcess> logger)
            : base(logger)
        {
        }

        protected override async Task<bool> HandleStep(CtuAuthProcessData data)
        {
            Duplicity duplicity = _foundDuplicity = await GetDuplicityAsync(data);
            DuplicityType duplicityType = duplicity.Type;

            switch (duplicityType)
            {
                case DuplicityType.CtuSide:
                case DuplicityType.DiscordSide:
                    data.DbUser.DuplicitUserId = duplicity.User.UserId;

                    if (!data.DbUser.DuplicityApproved)
                    {
                        throw new UserException(
                            "There is a duplicate user stored, contact administrators, if you want to proceed");
                    }

                    break;
            }

            return true;
        }

        protected override Task HandleAfterNext(CtuAuthProcessData data)
        {
            if (data.Finished)
            {
                switch (_foundDuplicity?.Type)
                {
                    case DuplicityType.Both:
                        data.DbContext.Remove(data.DbUser);
                        _foundDuplicity.User.AuthenticatedAt = data.DbUser.AuthenticatedAt;

                        break;
                }
            }

            return Task.CompletedTask;
        }

        private async Task<Duplicity> GetDuplicityAsync(CtuAuthProcessData data)
        {
            ChristofelBaseContext dbContext = data.DbContext;
            DbUser dbUser = data.DbUser;
            
            DbUser? duplicateBoth = await dbContext.Users
                .AsQueryable()
                .Authenticated()
                .Where(x => x.CtuUsername == dbUser.CtuUsername && x.DiscordId == dbUser.DiscordId)
                .FirstOrDefaultAsync();

            if (duplicateBoth != null)
            {
                return new Duplicity(DuplicityType.Both, duplicateBoth);
            }

            DbUser? duplicateDiscord = await dbContext.Users
                .AsQueryable()
                .Authenticated()
                .Where(x => x.DiscordId == dbUser.DiscordId)
                .FirstOrDefaultAsync();

            if (duplicateDiscord != null)
            {
                return new Duplicity(DuplicityType.DiscordSide, duplicateDiscord);
            }
            
            DbUser? duplicateCtu = await dbContext.Users
                .AsQueryable()
                .Authenticated()
                .Where(x => x.CtuUsername == dbUser.CtuUsername)
                .FirstOrDefaultAsync();

            if (duplicateCtu != null)
            {
                return new Duplicity(DuplicityType.CtuSide, duplicateCtu);
            }
            
            return new Duplicity(DuplicityType.None, dbUser);
        }
    }
}