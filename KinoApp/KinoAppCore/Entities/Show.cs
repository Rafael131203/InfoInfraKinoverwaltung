namespace KinoAppCore.Entities;

public sealed class Show
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Title { get; private set; } = default!;
    public DateTime StartTime { get; private set; }
    public DateTime EndTime { get; private set; }
    public decimal BasePrice { get; private set; }

    // Navigation
    public ICollection<Ticket> Tickets { get; private set; } = new List<Ticket>();

    private Show() { }

    public Show(string title, DateTime start, DateTime end, decimal basePrice)
    {
        Title = title;
        StartTime = start;
        EndTime = end;
        BasePrice = basePrice;
    }

    public bool IsRunning(DateTime now) => now >= StartTime && now <= EndTime;
}
