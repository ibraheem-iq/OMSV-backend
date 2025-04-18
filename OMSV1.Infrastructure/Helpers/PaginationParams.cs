using System;

namespace OMSV1.Application.Helpers;

public class PaginationParams
{
    private const int MaxPageSize = 100000;
    public int PageNumber { get; set; } = 1;
    private int _pageSize = 10;

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = (value > MaxPageSize) ? MaxPageSize : value;
    }

}
