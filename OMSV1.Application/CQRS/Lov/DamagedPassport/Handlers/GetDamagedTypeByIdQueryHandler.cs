using MediatR;
using OMSV1.Application.Dtos.LOV;
using OMSV1.Domain.Entities.DamagedPassport; // For DamagedType entity
using OMSV1.Domain.SeedWork;
using OMSV1.Application.Helpers; // Assuming HandlerException is defined here

namespace OMSV1.Application.CQRS.Lov.DamagedPassport
{
    public class GetDamagedTypeByIdQueryHandler : IRequestHandler<GetDamagedTypeByIdQuery, DamagedTypeDto>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetDamagedTypeByIdQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<DamagedTypeDto> Handle(GetDamagedTypeByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // Get the damaged type by id
                var damagedType = await _unitOfWork.Repository<DamagedType>()
                    .GetByIdAsync(request.Id);

                 if (damagedType == null)
                {
                    throw new KeyNotFoundException($"DamagedType with ID {request.Id} not found.");
                }

                // Map to DTO (use AutoMapper if set up)
                var damagedTypeDto = new DamagedTypeDto
                {
                    Id = damagedType.Id,
                    Name = damagedType.Name,
                    Description = damagedType.Description
                };

                return damagedTypeDto;
            }
            catch (Exception ex)
            {
                // Log and throw a custom exception if an error occurs
                throw new HandlerException("An error occurred while fetching the damaged type by ID.", ex);
            }
        }
    }
}
