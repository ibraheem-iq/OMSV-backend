﻿using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using OMSV1.Application.Dtos.DamagedDevices;
using OMSV1.Application.Helpers;
using OMSV1.Application.Queries.DamagedDevices;
using OMSV1.Domain.Entities.DamagedDevices;
using OMSV1.Domain.SeedWork;
using System.Threading;
using System.Threading.Tasks;

namespace OMSV1.Application.Handlers.DamagedDevices
{
    public class GetAllDamagedDevicesQueryHandler : IRequestHandler<GetAllDamagedDevicesQuery, PagedList<DamagedDeviceDto>>
    {
        private readonly IGenericRepository<DamagedDevice> _repository;
        private readonly IMapper _mapper;

        public GetAllDamagedDevicesQueryHandler(IGenericRepository<DamagedDevice> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<PagedList<DamagedDeviceDto>> Handle(GetAllDamagedDevicesQuery request, CancellationToken cancellationToken)
        {
            // Retrieve the damaged devices as IQueryable
            var damagedDevicesQuery = _repository.GetAllAsQueryable();

            // Map to DamagedDeviceDto using AutoMapper's ProjectTo
            var mappedQuery = damagedDevicesQuery.ProjectTo<DamagedDeviceDto>(_mapper.ConfigurationProvider);

            // Apply pagination using PagedList
            var pagedDamagedDevices = await PagedList<DamagedDeviceDto>.CreateAsync(
                mappedQuery,
                request.PaginationParams.PageNumber,
                request.PaginationParams.PageSize
            );

            return pagedDamagedDevices;  // Return the paginated list
        }
    }
}
