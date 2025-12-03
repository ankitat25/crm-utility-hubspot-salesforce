using System.Threading.Tasks;
using CrmUtility.Backend.Data;
using CrmUtility.Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace CrmUtility.Backend.Services
{
    public class TokenService
    {
        private readonly AppDbContext _db;

        public TokenService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<OAuthConnection> UpsertConnectionAsync(OAuthConnection connection)
        {
            var existing = await _db.OAuthConnections
                .FirstOrDefaultAsync(x => x.UserId == connection.UserId && x.Crm == connection.Crm);

            if (existing == null)
            {
                _db.OAuthConnections.Add(connection);
            }
            else
            {
                existing.AccessToken = connection.AccessToken;
                existing.RefreshToken = connection.RefreshToken;
                existing.ExpiresAtUtc = connection.ExpiresAtUtc;
                existing.InstanceUrl = connection.InstanceUrl;
                existing.HubSpotPortalId = connection.HubSpotPortalId;
            }

            await _db.SaveChangesAsync();
            return connection;
        }

        public async Task<OAuthConnection> GetConnectionAsync(string userId, CrmType crm)
        {
            return await _db.OAuthConnections
                .FirstOrDefaultAsync(x => x.UserId == userId && x.Crm == crm);
        }
    }
}
