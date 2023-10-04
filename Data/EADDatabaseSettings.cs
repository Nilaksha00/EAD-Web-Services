namespace EAD_Project.Data
{
    public class EADDatabaseSettings
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string DatabaseName { get; set; } = string.Empty;
        public string TravellerCollectionName { get; set; } = string.Empty;
        public string TrainScheduleCollectionName { get; set; } = string.Empty;
        public string ReservationCollectionName { get; set; } = string.Empty;
        public string BackOfficeUserCollectionName { get; set; } = string.Empty;
    }
}
