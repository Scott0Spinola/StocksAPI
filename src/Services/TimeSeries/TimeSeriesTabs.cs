namespace src.Services.TimeSeries;

public enum TimeSeriesGranularity
{
    Hour,
    Day,
    Month
}

public static class TimeSeriesTabs
{
    public static (DateTime startInclusive, DateTime endInclusive) GetWindow(DateTime dataInicio, DateTime dataFim, int tab)
    {
        var anchorDay = dataFim.Date;

        return tab switch
        {
            0 => (anchorDay, anchorDay.AddDays(1).AddTicks(-1)),
            1 => (anchorDay.AddDays(-6), anchorDay.AddDays(1).AddTicks(-1)),
            2 => (anchorDay.AddDays(-30), anchorDay.AddDays(1).AddTicks(-1)),
            3 => (new DateTime(anchorDay.Year, anchorDay.Month, 1).AddMonths(-11), anchorDay.AddDays(1).AddTicks(-1)),
            4 => (dataInicio, dataFim),
            _ => (dataInicio, dataFim)
        };
    }

    public static (DateTime startInclusive, DateTime endInclusive, TimeSeriesGranularity granularity) GetWindowWithGranularity(
        DateTime dataInicio,
        DateTime dataFim,
        int tab)
    {
        var anchorDay = dataFim.Date;

        return tab switch
        {
            0 => (anchorDay, anchorDay.AddDays(1).AddTicks(-1), TimeSeriesGranularity.Hour),
            1 => (anchorDay.AddDays(-6), anchorDay.AddDays(1).AddTicks(-1), TimeSeriesGranularity.Day),
            2 => (anchorDay.AddDays(-30), anchorDay.AddDays(1).AddTicks(-1), TimeSeriesGranularity.Day),
            3 => (new DateTime(anchorDay.Year, anchorDay.Month, 1).AddMonths(-11), anchorDay.AddDays(1).AddTicks(-1), TimeSeriesGranularity.Month),
            4 => (dataInicio, dataFim, dataInicio.Date == dataFim.Date ? TimeSeriesGranularity.Hour : TimeSeriesGranularity.Day),
            _ => (dataInicio, dataFim, TimeSeriesGranularity.Day)
        };
    }

    public static (DateTime startInclusive, DateTime endInclusive) GetPreviousWindow(DateTime startInclusive, DateTime endInclusive)
    {
        var duration = endInclusive - startInclusive;
        var previousEndInclusive = startInclusive.AddTicks(-1);
        var previousStartInclusive = previousEndInclusive - duration;
        return (previousStartInclusive, previousEndInclusive);
    }

    public static IEnumerable<DateTime> GetBuckets(DateTime startInclusive, DateTime endInclusive, TimeSeriesGranularity granularity)
    {
        if (granularity == TimeSeriesGranularity.Hour)
        {
            var start = new DateTime(startInclusive.Year, startInclusive.Month, startInclusive.Day, 0, 0, 0);
            for (var hour = 0; hour < 24; hour++)
            {
                yield return start.AddHours(hour);
            }

            yield break;
        }

        if (granularity == TimeSeriesGranularity.Day)
        {
            for (var day = startInclusive.Date; day <= endInclusive.Date; day = day.AddDays(1))
            {
                yield return day;
            }

            yield break;
        }

        var startMonth = new DateTime(startInclusive.Year, startInclusive.Month, 1);
        var endMonth = new DateTime(endInclusive.Year, endInclusive.Month, 1);
        for (var month = startMonth; month <= endMonth; month = month.AddMonths(1))
        {
            yield return month;
        }
    }
}
