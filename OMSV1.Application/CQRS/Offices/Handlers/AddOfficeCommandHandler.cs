﻿using AutoMapper;
using MediatR;
using OMSV1.Domain.Entities.Offices;
using OMSV1.Domain.SeedWork;
using System.Threading;
using System.Threading.Tasks;

namespace OMSV1.Application.Commands.Offices
{
    public class AddOfficeCommandHandler : IRequestHandler<AddOfficeCommand, int>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public AddOfficeCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<int> Handle(AddOfficeCommand request, CancellationToken cancellationToken)
    {
        // Map the command to the Office entity
        var office = _mapper.Map<Office>(request);

        // Use the generic repository to add the Office entity
        await _unitOfWork.Repository<Office>().AddAsync(office);

        // Save the changes using Unit of Work
        if (!await _unitOfWork.SaveAsync(cancellationToken))
        {
            throw new Exception("Failed to save the office to the database.");
        }

        return office.Id; // Return the new entity ID
    }
}

}
