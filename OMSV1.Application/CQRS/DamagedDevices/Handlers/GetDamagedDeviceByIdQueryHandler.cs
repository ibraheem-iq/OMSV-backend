using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OMSV1.Application.Dtos.DamagedDevices;
using OMSV1.Application.Queries.DamagedDevices;
using OMSV1.Domain.Entities.DamagedDevices;
using OMSV1.Domain.SeedWork;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OMSV1.Application.Handlers.DamagedDevices
{
    public class GetDamagedDeviceByIdQueryHandler : IRequestHandler<GetDamagedDeviceByIdQuery, DamagedDeviceDto?>
    {
        private readonly IGenericRepository<DamagedDevice> _repository;
        private readonly IMapper _mapper;

        public GetDamagedDeviceByIdQueryHandler(IGenericRepository<DamagedDevice> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<DamagedDeviceDto?> Handle(GetDamagedDeviceByIdQuery request, CancellationToken cancellationToken)
        {
            // Retrieve the specific damaged device as an IQueryable
            var damagedDeviceQuery = _repository.GetAllAsQueryable()
                .Where(dd => dd.Id == request.Id); // Filter by the given ID

            // Map to DamagedDeviceDto using AutoMapper's ProjectTo
            var damagedDeviceDto = await damagedDeviceQuery
                .ProjectTo<DamagedDeviceDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken); // Fetch the first result or null

            return damagedDeviceDto; // Return the mapped DTO
        }
    }
}
