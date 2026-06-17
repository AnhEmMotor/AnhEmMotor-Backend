using System;
using System.Collections.Generic;

namespace AnhEmMotor.Application.ApiContracts.Admin.Appointments
{
    public record AdminAppointmentResponse(
        int Id, 
        DateTime AppointmentDate, 
        string CustomerName, 
        string VehicleModel, 
        string Type, // Lái thử / Dịch vụ
        string Status, 
        string AssignedSaleName);

    public record AssignSaleRequest(string SaleId);
    public record DispatchToWorkshopRequest(int BookingId, string TechnicianId);
}