using ShuaibTransportation.Data.Entities;

namespace ShuaibTransportation.Services.DTOs;

public record ScheduleListDto(
    int Id, string RouteName, string VehicleName, string VehicleRegistration,
    DateTime DepartureTime, decimal Price, int AvailableSeats, int TotalSeats);

public record SeatMapDto(
    int ScheduleId, string RouteName, string VehicleName, string VehicleRegistration,
    DateTime DepartureTime, decimal Price, int TotalSeats, int AvailableSeats,
    IReadOnlyList<int> BookedSeatNumbers);

public record BookingDto(
    Guid Id, string Reference, string RouteName, DateTime DepartureTime,
    string VehicleName, int SeatsBooked, string SeatNumbers, BookingStatus Status,
    DateTime CreatedAt, string CustomerName, string CustomerEmail);
