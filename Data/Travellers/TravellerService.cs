using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace EAD_Project.Data
{
    public class TravellerService
    {
        private readonly IMongoCollection<Traveller> _traveller;

        public TravellerService(IOptions<EADDatabaseSettings> options)
        {
            var mongoClient = new MongoClient(options.Value.ConnectionString);

            _traveller = mongoClient.GetDatabase(options.Value.DatabaseName)
                .GetCollection<Traveller>(options.Value.TravellerCollectionName);

        }

        //get traveller list
        public async Task<List<Traveller>> GetTravellerAccounts() =>
            await _traveller.Find(_ => true).ToListAsync();

        // get a single traveller with id
        public async Task<Traveller> GetTravellerAccount(string? id) =>
            await _traveller.Find(m => m._id == id).FirstOrDefaultAsync();

        // add a new traveller acc
        public async Task CreateTravellerAccount(Traveller newTraveller) =>
            await _traveller.InsertOneAsync(newTraveller);

        // update a traveller acc
        public async Task UpdateTravellerAccount(string id, Traveller updateTraveller) =>
            await _traveller.ReplaceOneAsync(m => m._id == id, updateTraveller);

        // delete a traveller acc
        public async Task RemoveTravellerAccount(string id) =>
            await _traveller.DeleteOneAsync(m => m._id == id);

        // activate a traveller acc
        public async Task ActivateTravellerAccount(string id)
        {
            var filter = Builders<Traveller>.Filter.Eq(m => m._id, id);
            var update = Builders<Traveller>.Update.Set("travellerAccStatus", 1);

            await _traveller.UpdateOneAsync(filter, update);
        }

        // activate a traveller acc
        public async Task DeactivateTravellerAccount(string id)
        {
            var filter = Builders<Traveller>.Filter.Eq(m => m._id, id);
            var update = Builders<Traveller>.Update.Set("travellerAccStatus", 0);

            await _traveller.UpdateOneAsync(filter, update);
        }


    }
}
