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

        // add a new Train Schedule
        public async Task CreateTrainSchedule(TrainSchedule newTrainSchedule) =>
            await _trainSchedule.InsertOneAsync(newTrainSchedule);

        //get Train Schedule list
        public async Task<List<TrainSchedule>> GetTrainSchedules() =>
            await _trainSchedule.Find(_ => true).ToListAsync();

        // get a single Train Schedule with id
        public async Task<TrainSchedule> GetTrainSchedule(string? id) =>
            await _trainSchedule.Find(m => m.trainScheduleID == id).FirstOrDefaultAsync();

        // update a Train Schedule
        public async Task UpdateTrainSchedule(string? id, TrainSchedule updateTrainSchedule) =>
            await _trainSchedule.ReplaceOneAsync(m => m.trainScheduleID == id, updateTrainSchedule);


        //Delete a Train Schedule (Are there are no reservations )
        public async Task DeleteTrainSchedule(string? id)
        {
            // Check if the train schedule exists
            var trainSchedule = await _trainSchedule.Find(m => m.trainScheduleID == id).FirstOrDefaultAsync();

            if (trainSchedule == null)
            {
                // Train schedule not found, throw an exception or handle accordingly
                throw new ArgumentException($"Train schedule not found with ID: {id}");
            }

            // Check if there are reservations associated with the train schedule
            if (HasReservations(trainSchedule))
            {
                // Train schedule has reservations, prevent deletion
                throw new InvalidOperationException($"Train schedule cannot be deleted as it has associated reservations.");
            }

            // Continue with deleting the train schedule
            await _trainSchedule.DeleteOneAsync(m => m.trainScheduleID == id);
        }

        // Helper function to check if there are reservations associated with the train schedule
        private bool HasReservations(TrainSchedule trainSchedule)
        {
            return trainSchedule.reservations != null && trainSchedule.reservations.Any();
        }
    }
}
