using MongoDB.Bson.Serialization.Attributes;

namespace EAD_Project.Data
{
    public class Traveller
    {
        [BsonId]
       //BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string _id { get; set; }
        public string travellerEmail { get; set; }
        public string travellerPassword { get; set; }
        public string travellerName { get; set; }
        public int travellerAge { get; set; }
        public string travellerCity { get; set; }
        public string travellerPhone { get; set; }
        public int travellerAccStatus { get; set; }

    }
}
