using MongoDB.Bson.Serialization.Attributes;

namespace EAD_Project.Data.BackOfficeUsers
{
    public class BackOfficeUser
    {
        [BsonId]
    //    BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string? backOfficerID { get; set; }
        public string? backOfficerName { get; set; }
        public int? backOfficerAge { get; set; }
        public string? backOfficerCity { get; set; }
        public string? backOfficerPhone { get; set; }
        public string? backOfficerEmail { get; set; }
        public string? backOfficerPassword { get; set; }
        public int? backOfficerRole { get; set; }
    }
}