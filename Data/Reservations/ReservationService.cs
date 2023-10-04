using EAD_Project.Data.BackOfficeUsers;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Bson;

namespace EAD_Project.Data.Reservations
{
    public class ReservationService
    {
        private readonly IMongoCollection<Reservation> _reservation;
        private readonly IMongoCollection<Traveller> _traveller;

        public ReservationService(IOptions<EADDatabaseSettings> options)
        {
            var mongoClient = new MongoClient(options.Value.ConnectionString);

            _reservation = mongoClient.GetDatabase(options.Value.DatabaseName)
                .GetCollection<Reservation>(options.Value.ReservationCollectionName);

            _traveller = mongoClient.GetDatabase(options.Value.DatabaseName)
                .GetCollection<Traveller>(options.Value.TravellerCollectionName);
        }

        // add a new reservation
        public async Task CreateReservation(Reservation newRersevation) =>
            await _reservation.InsertOneAsync(newRersevation);

        // view reservation list
        public async Task<List<Reservation>> GetReservations() =>
            await _reservation.Find(_ => true).ToListAsync();

        // view reservation
        public async Task<Reservation> GetReservation(string id) =>
            await _reservation.Find(m => m.reservationID == id).FirstOrDefaultAsync();

        //
    }
}
