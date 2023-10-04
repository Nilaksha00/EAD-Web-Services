using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace EAD_Project.Data.BackOfficeUsers
{
    public class BackOfficeUserService
    {
        private readonly IMongoCollection<BackOfficeUser> _backOfficeUser;

        public BackOfficeUserService(IOptions<EADDatabaseSettings> options)
        {
            var mongoClient = new MongoClient(options.Value.ConnectionString);

            _backOfficeUser = mongoClient.GetDatabase(options.Value.DatabaseName)
                .GetCollection<BackOfficeUser>(options.Value.BackOfficeUserCollectionName);

        } 
        
        // get a single back office user with id
        public async Task<BackOfficeUser> GetBackOfficeUserAccount(string id) =>
            await _backOfficeUser.Find(m => m.backOfficerID == id).FirstOrDefaultAsync();

        // add a new traveller acc
        public async Task CreateBackOfficeUserAccount(BackOfficeUser newBackOfficeUser) =>
            await _backOfficeUser.InsertOneAsync(newBackOfficeUser);

        
    }

   
}
