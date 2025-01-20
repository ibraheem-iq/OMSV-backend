using MediatR;
using OMSV1.Application.Dtos.DamagedDevices;

namespace OMSV1.Application.CQRS.Queries.DamagedDevices;

public class GetDamagedDeviceBySerialNumberQuery : IRequest<DamagedDeviceDto>
{
    public required string SerialNumber { get; set; }
}
