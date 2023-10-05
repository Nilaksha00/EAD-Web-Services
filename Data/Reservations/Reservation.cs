using MongoDB.Bson.Serialization.Attributes;

namespace EAD_Project.Data.Reservations
{
    public class Reservation
    {
        [BsonId]
        //    [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string? reservationID { get; set; }
        public string? reservationDate { get; set; }
        public string? reservationTravellerID { get; set; }
        public string? reservationTravellerName { get; set; }
        public string? reservationTrainScheduleID { get; set; }
        public string? reservationTrainScheduleTime { get; set; }
        public string? reservationCreatorID { get; set; }
        public string? reservationCreatorName { get; set; }
    }
}
