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


        // add a new back office user acc
        public async Task CreateBackOfficeUserAccount(BackOfficeUser newBackOfficeUser)
        {
            newBackOfficeUser.backOfficerPassword = HashPassword(newBackOfficeUser.backOfficerPassword);
            await _backOfficeUser.InsertOneAsync(newBackOfficeUser);
        }       


        // login back office user
        public async Task<BackOfficeUser> LoginUser(Login login)
        {
            var backOfficeUser = await _backOfficeUser.Find(m => m.backOfficerEmail == login.email).FirstOrDefaultAsync();

            if (backOfficeUser != null)
            {
                if (BCrypt.Net.BCrypt.Verify(login.password, backOfficeUser.backOfficerPassword))
                {
                    return backOfficeUser;
                }
            }
            return null;
        }

        // helper function to hash the password
        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
    }

   
}
