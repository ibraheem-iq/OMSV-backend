using MediatR;

namespace OMSV1.Application.CQRS.Lov.DamagedDevice
{
    public class AddDamagedDeviceTypeCommand : IRequest<bool>
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }
}