using EfCore.Models;
namespace EfCore.Models;
public class TaskFilter
{
    public int Id { get; set; }
    public string Name { get; set; }
    public FilterCriterion<List<int>>? TagIds { get; set; }
    public FilterCriterion<List<int>>? TaskStatusIds { get; set; }
    public FilterCriterion<RelativeDateRange>? DueDate { get; set; }

}

public class FilterCriterion<T>
{
    public T? Value { get; set; }
    public FilterOperator Operator { get; set; }
}
public enum FilterOperator
{
    Equal,
    NotEqual,
    GreaterThan,
    LessThan,
    GreaterThanOrEqual,
    LessThanOrEqual,
    In,
    NotIn
}

public struct RelativeDateRange
{
    private readonly string _expression;

    public RelativeDateRange(string expression)
    {
        _expression = expression ?? string.Empty;
    }

    /// <summary>
    /// Parses the relative date expression and returns the corresponding date range.
    /// Supports relative expressions (T, M, W, Y, etc.) and absolute date or datetime strings.
    /// </summary>
    /// <returns>Tuple of (start, end) dates, or null if invalid.</returns>
    public (DateTime Start, DateTime End)? GetDateRange()
    {
        var expr = _expression.Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(expr))
            return null;

        if (expr.StartsWith("T"))
        {
            if (expr.Length > 1 && TryParseOffset(expr.Substring(1), out int offset))
            {
                var targetDay = DateTime.Today.AddDays(offset);
                return offset < 0 ? (targetDay, DateTime.Today) : (DateTime.Today, targetDay);
            }
            return (DateTime.Today, DateTime.Today);
        }

        if (expr.StartsWith("W"))
        {
            if (expr == "WB")
            {
                var startOfWeek = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
                return (startOfWeek, startOfWeek);
            }
            if (expr == "WE")
            {
                var endOfWeek = DateTime.Today.AddDays(6 - (int)DateTime.Today.DayOfWeek);
                return (endOfWeek, endOfWeek);
            }

            if (expr.Length > 1 && TryParseOffset(expr.Substring(1), out int offset))
            {
                var refWeek = DateTime.Today.AddDays(7 * offset);
                var startOfWeek = refWeek.AddDays(-(int)refWeek.DayOfWeek);
                var endOfWeek = startOfWeek.AddDays(6);
                return (startOfWeek, endOfWeek);
            }

            var thisWeekStart = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
            var thisWeekEnd = thisWeekStart.AddDays(6);
            return (thisWeekStart, thisWeekEnd);
        }

        if (expr.StartsWith("M"))
        {
            if (expr == "MB")
            {
                var monthStart = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                return (monthStart, monthStart);
            }
            if (expr == "ME")
            {
                var monthEnd = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(1).AddDays(-1);
                return (monthEnd, monthEnd);
            }
            if (expr.Length > 1 && TryParseOffset(expr.Substring(1), out int offset))
            {
                var refMonth = DateTime.Today.AddMonths(offset);
                var start = new DateTime(refMonth.Year, refMonth.Month, 1);
                var end = start.AddMonths(1).AddDays(-1);
                return (start, end);
            }

            var thisMonth = DateTime.Today;
            var monthStartDefault = new DateTime(thisMonth.Year, thisMonth.Month, 1);
            var monthEndDefault = monthStartDefault.AddMonths(1).AddDays(-1);
            return (monthStartDefault, monthEndDefault);
        }

        if (expr.StartsWith("Y"))
        {
            if (expr == "YB")
            {
                var yearStart = new DateTime(DateTime.Today.Year, 1, 1);
                return (yearStart, yearStart);
            }
            if (expr == "YE")
            {
                var yearEnd = new DateTime(DateTime.Today.Year, 12, 31);
                return (yearEnd, yearEnd);
            }

            if (expr.Length > 1 && TryParseOffset(expr.Substring(1), out int offset))
            {
                return GetYearDateRange((short)DateTime.Today.Year, offset);
            }

            return GetYearDateRange((short)DateTime.Today.Year);
        }

        if (DateTime.TryParse(_expression, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeLocal, out var parsedDate))
            return (parsedDate, parsedDate);

        return null;
    }

    private static (DateTime start, DateTime end) GetYearDateRange(short year, int offset = 0)
    {
        return offset == 0 ? (new DateTime(year, 1, 1), new DateTime(year, 12, 31)) 
            : (new DateTime(offset < 0 ? year + offset : year, 1, 1), new DateTime(offset > 0 ? year + offset : year, 12, 31));
    }

    private static bool TryParseOffset(string s, out int offset)
    {
        offset = 0;
        if (string.IsNullOrWhiteSpace(s))
            return true;
        return (s.StartsWith("-") || s.StartsWith("+")) && int.TryParse(s, out offset);
    }

    public override string ToString() => _expression;

    // Implicit conversion from string to RelativeDate
    public static implicit operator RelativeDateRange(string expression) => new RelativeDateRange(expression);

    // Implicit conversion from RelativeDate to string
    public static implicit operator string(RelativeDateRange relativeDate) => relativeDate._expression;
}