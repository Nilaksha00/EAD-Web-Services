using EAD_Project.Data.Reservations;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace EAD_Project.Data.TrainSchedules
{
    public class TrainScheduleService
    {
        private readonly IMongoCollection<TrainSchedule> _trainSchedule;

        public TrainScheduleService(IOptions<EADDatabaseSettings> options)
        {
            var mongoClient = new MongoClient(options.Value.ConnectionString);

            _trainSchedule = mongoClient.GetDatabase(options.Value.DatabaseName)
                .GetCollection<TrainSchedule>(options.Value.TrainScheduleCollectionName);

        }

        // testing
    }
}
