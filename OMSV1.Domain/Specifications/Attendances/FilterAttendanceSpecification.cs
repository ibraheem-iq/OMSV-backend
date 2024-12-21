using System;
using OMSV1.Domain.Entities.Attendances;
using OMSV1.Domain.Enums;

namespace OMSV1.Domain.Specifications.Attendances;

public class FilterAttendanceSpecification : BaseSpecification<Attendance>
{
    public FilterAttendanceSpecification(
        int? workingHours = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int? officeId = null,
        int? governorateId = null,
        int? profileId = null)
        : base(x =>
            //(string.IsNullOrEmpty(workingHours) || x.WorkingHours.Contains(workingHours)) &&
            x.WorkingHours == (WorkingHours)workingHours.Value  &&
            (!startDate.HasValue || x.Date >= startDate.Value) &&
            (!endDate.HasValue || x.Date <= endDate.Value) &&
            (!officeId.HasValue || x.OfficeId == officeId.Value) &&
            (!governorateId.HasValue || x.GovernorateId == governorateId.Value) &&
            (!profileId.HasValue || x.ProfileId == profileId.Value))
    {
        AddInclude(x => x.Governorate);
        //AddInclude(x => x.WorkingHours); // Remove this line
        AddInclude(x => x.Office);
        AddInclude(x => x.Profile);
    }
}