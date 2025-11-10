namespace KinoAppCore.Entities;

public sealed class Ticket
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid ShowId { get; private set; }
    public Show Show { get; private set; } = default!;
    public int SeatId { get; private set; }             // e.g. seat number
    public string Row { get; private set; } = default!;
    public decimal Price { get; private set; }
    public bool IsSold { get; private set; }

    // PostgreSQL will use this for optimistic concurrency
    public uint xmin { get; private set; }

    private Ticket() { }

    public Ticket(Guid showId, int seatId, string row, decimal price)
    {
        ShowId = showId;
        SeatId = seatId;
        Row = row;
        Price = price;
    }

    public void MarkAsSold()
    {
        if (IsSold)
            throw new InvalidOperationException("Ticket already sold.");
        IsSold = true;
    }
}
