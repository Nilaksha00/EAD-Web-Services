using EAD_Project.Data.Reservations;
using MongoDB.Bson.Serialization.Attributes;

namespace EAD_Project.Data.TrainSchedules
{
    public class TrainSchedule
    {
        [BsonId]
        // BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string? trainScheduleID { get; set; }
        public string? trainScheduleDate { get; set; }
        public string? trainScheduleDept { get; set; }
        public string? trainScheduleArr { get; set; }
        public string? trainScheduleTrainID { get; set; }
        public string? trainScheduleDestinationPoint { get; set; }
        public string? trainScheduleDeparturePoint { get; set; }
        public int? trainScheduleTicketPrice { get; set; }
        public List<Reservation>? reservations { get; set; }
    }
}
