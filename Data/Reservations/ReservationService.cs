using EAD_Project.Data.BackOfficeUsers;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Collections.Generic;
using System.Threading.Tasks;
using EAD_Project.Data.TrainSchedules;
using System.Globalization;
using MongoDB.Driver.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace EAD_Project.Data.Reservations
{
    public class ReservationService
    {
        private readonly IMongoCollection<Reservation> _reservation;
        private readonly IMongoCollection<Traveller> _traveller;
        private readonly TravellerService _travellerService; 
        private readonly TrainScheduleService _trainScheduleService; 

        public ReservationService(IOptions<EADDatabaseSettings> options, TravellerService travellerService, TrainScheduleService trainScheduleService)
        {
            var mongoClient = new MongoClient(options.Value.ConnectionString);

            _reservation = mongoClient.GetDatabase(options.Value.DatabaseName)
                .GetCollection<Reservation>(options.Value.ReservationCollectionName);

            _traveller = mongoClient.GetDatabase(options.Value.DatabaseName)
                .GetCollection<Traveller>(options.Value.TravellerCollectionName);

            _travellerService = travellerService;
            _trainScheduleService = trainScheduleService;
        }

        // add a new reservation (Is train schedules are published and reservation date is within 30 days from the booking date)
        public async Task CreateReservation(Reservation newReservation)
        {
            // Check if the specified train schedule ID exists
            var trainSchedule = await _trainScheduleService.GetTrainSchedule(newReservation.reservationTrainScheduleID);

            if (trainSchedule == null)
            {
                // Train schedule not found, throw an exception or handle accordingly
                throw new ArgumentException($"Invalid train schedule ID: {newReservation.reservationTrainScheduleID}");
            }

            // Check if the reservation date is within 30 days from the booking date
            if (!IsReservationDateValid(newReservation.reservationDate))
            {
                throw new ArgumentException($"Invalid reservation date: {newReservation.reservationDate}");
            }

            // Continue with creating the reservation
            await _reservation.InsertOneAsync(newReservation);


            // Add the reservation ID to the TrainSchedule's reservations array
            if (trainSchedule.reservations == null)
            {
                trainSchedule.reservations = new List<Reservation>();
            }

            trainSchedule.reservations.Add(new Reservation { reservationID = newReservation.reservationID });

            // Update the TrainSchedule with the new reservation ID
            await _trainScheduleService.UpdateTrainSchedule(trainSchedule.trainScheduleID, trainSchedule);
        }


        // view reservation list with relevant user details
        public async Task<List<ReservationWithTravellerDetails>> GetReservationsWithDetails()
        {
            var reservations = await _reservation.Find(_ => true).ToListAsync();
            var reservationsWithDetails = new List<ReservationWithTravellerDetails>();

            foreach (var reservation in reservations)
            {

                //var travellerDetails = await _travellerService.GetTravellerAccount(reservation.reservationTravellerID);

                var trainScheduleDetails = await _trainScheduleService.GetTrainSchedule(reservation.reservationTrainScheduleID);

                //Console.WriteLine($"Traveller Details: {travellerDetails}");

                reservationsWithDetails.Add(new ReservationWithTravellerDetails
                {
                    Reservation = reservation,
                    //TravellerDetails = travellerDetails,
                    TrainScheduleDetails = new TrainSchedule
                    {
                        trainScheduleDept = trainScheduleDetails.trainScheduleDept,
                        trainScheduleArr = trainScheduleDetails.trainScheduleArr,
                        trainScheduleTrainID = trainScheduleDetails.trainScheduleTrainID,
                        trainScheduleDestinationPoint = trainScheduleDetails.trainScheduleDestinationPoint,
                        trainScheduleDeparturePoint = trainScheduleDetails.trainScheduleDeparturePoint
                    }
                });
            }

            return reservationsWithDetails;
        }

        // get a single reservation with user details by reservation ID
        public async Task<ReservationWithTravellerDetails?> GetReservationWithDetails(string reservationId)
        {
            var reservation = await _reservation.Find(r => r.reservationID == reservationId).FirstOrDefaultAsync();

            if (reservation == null)
            {
                // Reservation not found
                return null;
            }

            //var travellerDetails = await _travellerService.GetTravellerAccount(reservation.reservationTravellerID);
            var trainScheduleDetails = await _trainScheduleService.GetTrainSchedule(reservation.reservationTrainScheduleID);


            return new ReservationWithTravellerDetails
            {
                Reservation = reservation,
                //TravellerDetails = travellerDetails,
                TrainScheduleDetails = new TrainSchedule
                {
                    trainScheduleDept = trainScheduleDetails.trainScheduleDept,
                    trainScheduleArr = trainScheduleDetails.trainScheduleArr,
                    trainScheduleTrainID = trainScheduleDetails.trainScheduleTrainID,
                    trainScheduleDestinationPoint = trainScheduleDetails.trainScheduleDestinationPoint,
                    trainScheduleDeparturePoint = trainScheduleDetails.trainScheduleDeparturePoint
                }
            };
        }


        // update a reservation
        public async Task UpdateReservation(string? reservationId, Reservation updatedReservation)
        {
            // Check if the reservation exists
            var existingReservation = await _reservation.Find(r => r.reservationID == reservationId).FirstOrDefaultAsync();

            if (existingReservation == null)
            {
                // Reservation not found, throw an exception or handle accordingly
                throw new ArgumentException($"Reservation not found with ID: {reservationId}");
            }

            // Calculate the days difference
            int daysDifference = CalculateDaysDifference(existingReservation.reservationDate);

            // Check if the days difference is less than 5
            if (daysDifference >= 5)
            {
                // Continue with the update
                updatedReservation.reservationID = reservationId;

                // Replace the existing reservation with the updated one
                var updateResult = await _reservation.ReplaceOneAsync(r => r.reservationID == reservationId, updatedReservation);

                if (updateResult.ModifiedCount == 0)
                {
                    // Update did not modify any documents, likely due to the reservation not existing
                    throw new InvalidOperationException($"Update failed. Reservation not found with ID: {reservationId}");
                }
            }
            else
            {
                // Reservation date is not valid for update
                throw new InvalidOperationException("Reservation date is not valid for update.");
            }
        }


        // delete a reservation
        public async Task DeleteReservation(string? reservationId)
        {
            // Check if the reservation exists
            var existingReservation = await _reservation.Find(r => r.reservationID == reservationId).FirstOrDefaultAsync();

            if (existingReservation == null)
            {
                // Reservation not found, throw an exception or handle accordingly
                throw new ArgumentException($"Reservation not found with ID: {reservationId}");
            }

            // Calculate the days difference
            int daysDifference = CalculateDaysDifference(existingReservation.reservationDate);

            // Check if the days difference is less than 5
            if (daysDifference >= 5)
            {
             
                // Continue with the deletion
                await _reservation.DeleteOneAsync(r => r.reservationID == reservationId);
            }
            else
            {
                // Reservation date is not valid for update
                throw new InvalidOperationException("Reservation date is not valid for delete.");
            }

        }

   

        // traveller dashboard - get the reservation list of upcoming reservations
        public async Task<List<ReservationWithTravellerDetails>> GetReservationWithDetailsAheadOfToday(string id)
        {
            var currentDate = DateTime.Now;

            var reservations = await _reservation
                .Find(r => r.reservationTravellerID == id && DateTime.Parse(r.reservationDate) > currentDate)
                .ToListAsync();

            var reservationsWithDetails = new List<ReservationWithTravellerDetails>();

            foreach (var reservation in reservations)
            {
                var trainScheduleDetails = await _trainScheduleService.GetTrainSchedule(reservation.reservationTrainScheduleID);

                reservationsWithDetails.Add(new ReservationWithTravellerDetails
                {
                    Reservation = reservation,
                    TrainScheduleDetails = new TrainSchedule
                    {
                        trainScheduleDept = trainScheduleDetails.trainScheduleDept,
                        trainScheduleArr = trainScheduleDetails.trainScheduleArr,
                        trainScheduleTrainID = trainScheduleDetails.trainScheduleTrainID,
                        trainScheduleDestinationPoint = trainScheduleDetails.trainScheduleDestinationPoint,
                        trainScheduleDeparturePoint = trainScheduleDetails.trainScheduleDeparturePoint
                    }
                });
            }

            return reservationsWithDetails;
        }

     

        // traveller dashboard - get the reservation list of upcoming reservations
        public async Task<List<ReservationWithTravellerDetails>> GetReservationOfATraveller(string id)
        {
            var currentDate = DateTime.Now;

            var reservations = await _reservation
                .Find(r => r.reservationTravellerID == id)
                .ToListAsync();

            var reservationsWithDetails = new List<ReservationWithTravellerDetails>();

            foreach (var reservation in reservations)
            {
                var trainScheduleDetails = await _trainScheduleService.GetTrainSchedule(reservation.reservationTrainScheduleID);

                reservationsWithDetails.Add(new ReservationWithTravellerDetails
                {
                    Reservation = reservation,
                    TrainScheduleDetails = new TrainSchedule
                    {
                        trainScheduleDept = trainScheduleDetails.trainScheduleDept,
                        trainScheduleArr = trainScheduleDetails.trainScheduleArr,
                        trainScheduleTrainID = trainScheduleDetails.trainScheduleTrainID,
                        trainScheduleDestinationPoint = trainScheduleDetails.trainScheduleDestinationPoint,
                        trainScheduleDeparturePoint = trainScheduleDetails.trainScheduleDeparturePoint
                    }
                });
            }

            return reservationsWithDetails;
        }





        // Helper function to calculate the days difference
        private int CalculateDaysDifference(string? reservationDate)
        {
            DateTimeOffset currentDate = DateTimeOffset.UtcNow;

            if (DateTimeOffset.TryParse(reservationDate, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out DateTimeOffset reservationDateTime))
            {
                Console.WriteLine($"currentDate : {currentDate}");
                Console.WriteLine($"reservationDateTime Id: {reservationDateTime}");

                // Calculate the days difference
                return (int)(reservationDateTime - currentDate).TotalDays;
            }
            else
            {
                // Invalid date format
                Console.WriteLine("Invalid date format.");
                return int.MaxValue; // or throw an exception depending on your requirements
            }
        }

        // Helper function to check if the reservation date is within 30 days from the booking date
        private bool IsReservationDateValid(string? reservationDate)
        {
            // Implement the logic to check if the reservation date is within 30 days from the booking date
            if (DateTime.TryParse(reservationDate, out DateTime parsedDate))
            {
                DateTime currentDate = DateTime.UtcNow.Date;
                return (parsedDate - currentDate).Days <= 30;
            }

            return false;
        }

       
    }

    public class ReservationWithTravellerDetails
    {
        public Reservation? Reservation { get; set; }
        //public Traveller? TravellerDetails { get; set; }
        public TrainSchedule? TrainScheduleDetails { get; set; }
    }

    public class ReservationWithTrainScheduleDetails
    {
        public Reservation? Reservation { get; set; }
        public TrainSchedule? TrainScheduleDetails { get; set; }
    }
}
