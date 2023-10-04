using EAD_Project.Data;
using EAD_Project.Data.BackOfficeUsers;
using EAD_Project.Data.Reservations;
using MongoDB.Driver.Core.Operations;

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<EADDatabaseSettings>(builder.Configuration.GetSection("EADDatabaseSettings"));
builder.Services.AddSingleton<TravellerService>();
builder.Services.AddSingleton<BackOfficeUserService>();
builder.Services.AddSingleton<ReservationService>();
var app = builder.Build();



// landing page
app.MapGet("/", () => "Traveller API!");


// ***************************************** TRAVELLER SERVICES **********************************************


//add traveller
app.MapPost("/api/travellers/add-traveller", async (TravellerService travellerService, Traveller traveller) =>
{
    await travellerService.CreateTravellerAccount(traveller);
    return Results.Ok();
});

//get travellers
app.MapGet("/api/travellers", async (TravellerService travellerService) =>
{
    var travellers = await travellerService.GetTravellerAccounts();
    return travellers;
});

//get a traveller acc
app.MapGet("/api/travellers/{id}", async (TravellerService travellerService, string id) =>
{
    var traveler = await travellerService.GetTravellerAccount(id);
    if (traveler != null)
    {
        return Results.Ok(traveler);
    }
    else
    {
        return Results.NotFound($"Traveler with ID {id} not found.");
    }
});

//update traveller
app.MapPut("/api/travellers/{id}", async (TravellerService travellerService, string id, Traveller updatedTraveller) =>
{
    var existingTraveller = await travellerService.GetTravellerAccount(id);
    if (existingTraveller != null)
    {
        await travellerService.UpdateTravellerAccount(id, updatedTraveller);
        return Results.Ok();
    }
    else
    {
        return Results.NotFound($"Traveler with ID {id} not found.");
    }
});

// delete traveller
app.MapDelete("/api/travellers/{id}", async (TravellerService travellerService, string id) =>
{
    var existingTraveller = await travellerService.GetTravellerAccount(id);
    if (existingTraveller != null)
    {
        await travellerService.RemoveTravellerAccount(id);
        return Results.Ok();
    }
    else
    {
        return Results.NotFound($"Traveler with ID {id} not found.");
    }
});

app.MapPut("api/traveller/activate/{id}", async (TravellerService travellerService, string id) =>
{
    var existingTraveller = await travellerService.GetTravellerAccount(id);
    if (existingTraveller != null)
    {
        await travellerService.ActivateTravellerAccount(id);
        return Results.Ok();
    }
    else
    {
        return Results.NotFound($"Traveler with ID {id} not found.");
    }
});

app.MapPut("api/traveller/deactivate/{id}", async (TravellerService travellerService, string id) =>
{
    var existingTraveller = await travellerService.GetTravellerAccount(id);
    if (existingTraveller != null)
    {
        await travellerService.DeactivateTravellerAccount(id);
        return Results.Ok();
    }
    else
    {
        return Results.NotFound($"Traveler with ID {id} not found.");
    }
});




// ***************************************** BACK OFFICE USER SERVICES *****************************************


// get a single back office user with id
app.MapGet("/api/backoffice/{id}", async (BackOfficeUserService backOfficeUserService, string id) =>
{
    var backOfficeUser = await backOfficeUserService.GetBackOfficeUserAccount(id);
    if (backOfficeUser != null)
    {
        return Results.Ok(backOfficeUser);
    }
    else
    {
        return Results.NotFound($"Back Office User with ID {id} not found.");
    }
});

// add back office user
app.MapPost("/api/backoffice/add-backoffice", async (BackOfficeUserService backOfficeUserService, BackOfficeUser backOfficeUser) =>
{
    await backOfficeUserService.CreateBackOfficeUserAccount(backOfficeUser);
    return Results.Ok();
});



// ***************************************** RESERVATION SERVICES *****************************************

// add reservation
app.MapPost("/api/reservation/add-reservation", async (ReservationService reservationService, Reservation reservation) =>
{
    await reservationService.CreateReservation(reservation);
    return Results.Ok();
});

//get reservations
app.MapGet("/api/reservations", async (ReservationService reservationService) =>
{
    var reservations = await reservationService.GetReservations();
    return reservations;
});

app.MapGet("/api/reservation/{id}", async (ReservationService reservationService, string id) =>
{
    var reservations = await reservationService.GetReservation(id);
    return reservations;
});


app.Run();
