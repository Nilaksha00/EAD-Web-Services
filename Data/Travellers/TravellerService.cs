using Microsoft.Extensions.Options;
using MongoDB.Driver;
using BCrypt.Net;

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

        // login traveller 
        public async Task<Traveller> LoginUser(Login login)
        {
            var traveler = await _traveller.Find(m => m.travellerEmail == login.email).FirstOrDefaultAsync();

            if (traveler != null)
            {
                if (traveler.travellerAccStatus == 1) // Check account status
                {
                    if (BCrypt.Net.BCrypt.Verify(login.password, traveler.travellerPassword))
                    {
                        return traveler;
                    }
                }
                else
                {
                    // Account is not active, return relevant error
                    throw new ApplicationException("User account is not active. Please contact support.");
                }
            }

            // User not found or password incorrect
            return null;
        }



        //get traveller list
        public async Task<List<Traveller>> GetTravellerAccounts() =>
            await _traveller.Find(_ => true).ToListAsync();

        // get a single traveller with id
        public async Task<Traveller> GetTravellerAccount(string? id) =>
            await _traveller.Find(m => m._id == id).FirstOrDefaultAsync();

        // add a new traveller acc
        public async Task CreateTravellerAccount(Traveller newTraveller)
        {
            newTraveller.travellerPassword  = HashPassword(newTraveller.travellerPassword);
            await _traveller.InsertOneAsync(newTraveller);

        }

        // update a traveller acc
        //public async Task UpdateTravellerAccount(string id, Traveller updateTraveller) =>
        //    await _traveller.ReplaceOneAsync(m => m._id == id, updateTraveller);



        //public string? _id { get; set; }
        //public string? travellerEmail { get; set; }
        //public string? travellerPassword { get; set; }
        //public string? travellerName { get; set; }
        //public int? travellerAge { get; set; }
        //public string? travellerCity { get; set; }
        //public string? travellerPhone { get; set; }
        //public int? travellerAccStatus { get; set; }


        public async Task UpdateTravellerAccount(string id, Traveller updateTraveller)
        {
            var filter = Builders<Traveller>.Filter.Eq(m => m._id, id);
            var update = Builders<Traveller>.Update
                .Set("travellerEmail", updateTraveller.travellerEmail)
                .Set("travellerName", updateTraveller.travellerName)
                .Set("travellerAge", updateTraveller.travellerAge)
                .Set("travellerCity", updateTraveller.travellerCity)
                .Set("travellerPhone", updateTraveller.travellerPhone);
            await _traveller.UpdateOneAsync(filter, update);
        }



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

        // helper function to hash the password
        private string HashPassword(string password)
        {
            string salt = BCrypt.Net.BCrypt.GenerateSalt(12);
            return BCrypt.Net.BCrypt.HashPassword(password, salt);
        }


    }
}
