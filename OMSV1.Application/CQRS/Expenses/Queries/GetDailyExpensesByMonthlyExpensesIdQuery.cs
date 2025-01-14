using MediatR;
using OMSV1.Application.DTOs.Expenses;

namespace OMSV1.Application.Queries.Expenses
{
    public class GetDailyExpensesByMonthlyExpensesIdQuery : IRequest<List<DailyExpensesDto>>
    {
        public Guid MonthlyExpensesId { get; set; }

        public GetDailyExpensesByMonthlyExpensesIdQuery(Guid monthlyExpensesId)
        {
            MonthlyExpensesId = monthlyExpensesId;
        }
    }
}
