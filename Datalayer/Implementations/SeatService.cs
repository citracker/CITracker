using Datalayer.Interfaces;
using DataRepository;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Shared.DTO;
using Shared.Enumerations;
using Shared.Interfaces;
using Shared.Models;
using System.Data;

namespace Datalayer.Implementations
{
    public class SeatService : BaseManager, ISeatService
    {
        private readonly ILogger<SeatService> _logger;
        private readonly IRepository _repo;
        private readonly IAppSettingsManager _connection;
        private readonly IGenericManager _genManager;

        public SeatService(ILogger<SeatService> logger, IRepository repository,
            IAppSettingsManager AppSettingsManager, IGenericManager genManager)
        {
            _logger = logger;
            _repo = repository;
            _connection = AppSettingsManager;
            _genManager = genManager;
        }

        public async Task<ResponseHandler> CanInviteAsync(int organizationId)
        {
            using var db = CreateConnection(DatabaseConnectionType.MicrosoftSQLServer, await _connection.SQLDBConnection());

            var sub = await _repo.GetAsync<Subscription>(db, @"SELECT TOP 1 * FROM Subscription WHERE OrganizationId = @oid AND Status IN ('ACTIVE','TRIALING','PENDING_CONFIRMATION') ORDER BY Id DESC", new { oid = organizationId }, CommandType.Text);

            if (sub == null)
                return new ResponseHandler { StatusCode = 404, Message = "No active subscription for organization." };

            if (sub.SeatsAllocated >= sub.SeatsPurchased)
                return new ResponseHandler
                {
                    StatusCode = 409,
                    Message = $"All {sub.SeatsPurchased} purchased seats are in use. " +
                              $"Increase seats or upgrade your plan before inviting more users."
                };

            return new ResponseHandler { StatusCode = 200, Message = "OK" };
        }

        public async Task<ResponseHandler> ReserveSeatAsync(int organizationId, long newUserId)
        {
            using var db = CreateConnection(DatabaseConnectionType.MicrosoftSQLServer,
                await _connection.SQLDBConnection());
            db.Open();
            using var tx = db.BeginTransaction();
            try
            {
                // Row-lock the subscription to prevent race conditions on concurrent invites
                var sub = await _repo.GetAsync<Subscription>(db, @"SELECT * FROM Subscription WITH (UPDLOCK, ROWLOCK) WHERE OrganizationId = @oid AND Status IN ('ACTIVE','TRIALING','PENDING_CONFIRMATION')",
                    new { oid = organizationId }, CommandType.Text, tx);

                if (sub == null || sub.SeatsAllocated >= sub.SeatsPurchased)
                {
                    tx.Rollback();
                    return new ResponseHandler { StatusCode = 409, Message = "No seats available." };
                }

                sub.SeatsAllocated += 1;
                sub.LastUpdatedDate = DateTime.UtcNow;
                await _repo.UpdateAsync(db, sub, tx);

                tx.Commit();
                return new ResponseHandler { StatusCode = 200, Message = "Seat reserved." };
            }
            catch (Exception ex)
            {
                tx.Rollback();
                _logger.LogError($"Exception at {nameof(ReserveSeatAsync)} - {JsonConvert.SerializeObject(ex)}");
                return new ResponseHandler { StatusCode = 500, Message = "An error occurred." };
            }
            finally { db.Close(); }
        }

        public async Task<ResponseHandler> ReleaseSeatAsync(int organizationId, long userId)
        {
            using var db = CreateConnection(DatabaseConnectionType.MicrosoftSQLServer,
                await _connection.SQLDBConnection());
            db.Open();
            using var tx = db.BeginTransaction();
            try
            {
                var sub = await _repo.GetAsync<Subscription>(db, @"SELECT * FROM Subscription WITH (UPDLOCK, ROWLOCK) WHERE OrganizationId = @oid AND Status IN ('ACTIVE','TRIALING','PENDING_CONFIRMATION')",
                    new { oid = organizationId }, CommandType.Text, tx);

                if (sub == null || sub.SeatsAllocated <= 0) { tx.Rollback(); return new ResponseHandler { StatusCode = 200 }; }

                sub.SeatsAllocated -= 1;
                sub.LastUpdatedDate = DateTime.UtcNow;
                await _repo.UpdateAsync(db, sub, tx);
                tx.Commit();
                return new ResponseHandler { StatusCode = 200, Message = "Seat released." };
            }
            catch (Exception ex)
            {
                tx.Rollback();
                _logger.LogError($"Exception at {nameof(ReleaseSeatAsync)} - {JsonConvert.SerializeObject(ex)}");
                return new ResponseHandler { StatusCode = 500, Message = "An error occurred." };
            }
            finally { db.Close(); }
        }

        public async Task<ResponseHandler> SetSeatsPurchasedAsync(int organizationId, int seatsPurchased)
        {
            using var db = CreateConnection(DatabaseConnectionType.MicrosoftSQLServer,
                await _connection.SQLDBConnection());

            var sub = await _repo.GetAsync<Subscription>(db, "SELECT TOP 1 * FROM Subscription WHERE OrganizationId = @oid ORDER BY Id DESC", new { oid = organizationId }, CommandType.Text);

            if (sub == null) return new ResponseHandler { StatusCode = 404, Message = "Subscription not found." };

            var plan = await _repo.GetAsync<SubscriptionPlan>(db,  "SELECT * FROM SubscriptionPlan WHERE Id = @id", new { id = sub.SubscriptionPlanId }, CommandType.Text);

            if (seatsPurchased < plan.MinSeats || seatsPurchased > plan.NumberOfLicences)
                return new ResponseHandler
                {
                    StatusCode = 400,
                    Message = $"Plan '{plan.Name}' requires between {plan.MinSeats} and {plan.NumberOfLicences} seats."
                };

            if (seatsPurchased < sub.SeatsAllocated)
                return new ResponseHandler
                {
                    StatusCode = 400,
                    Message = $"Cannot reduce to {seatsPurchased} seats while {sub.SeatsAllocated} are allocated."
                };

            sub.SeatsPurchased = seatsPurchased;
            sub.LastUpdatedDate = DateTime.UtcNow;
            await _repo.UpdateAsync(db, sub);

            return new ResponseHandler { StatusCode = 200, Message = "Seats updated." };
        }

        public async Task<ResponseHandler> ScheduleSeatChangeAtRenewalAsync(int organizationId, int? seats, int? newPlanId)
        {
            using var db = CreateConnection(DatabaseConnectionType.MicrosoftSQLServer,
                await _connection.SQLDBConnection());

            var sub = await _repo.GetAsync<Subscription>(db, "SELECT TOP 1 * FROM Subscription WHERE OrganizationId = @oid ORDER BY Id DESC", new { oid = organizationId }, CommandType.Text);

            if (sub == null) return new ResponseHandler { StatusCode = 404, Message = "Subscription not found." };

            sub.PendingSeatsAtRenewal = seats;
            sub.PendingPlanAtRenewalId = newPlanId;
            sub.LastUpdatedDate = DateTime.UtcNow;
            await _repo.UpdateAsync(db, sub);

            return new ResponseHandler { StatusCode = 200, Message = "Change scheduled for next renewal." };
        }
    }
}
