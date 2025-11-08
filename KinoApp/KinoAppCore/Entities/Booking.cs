namespace KinoAppCore.Entities;

public sealed class Booking
{
    public Guid Id { get; }
    public Guid ShowId { get; }
    public bool IsPaid { get; private set; }
    public decimal AmountPaid { get; private set; }

    public Booking(Guid id, Guid showId) { Id = id; ShowId = showId; }

    public void Pay(decimal amount)
    {
        if (IsPaid) throw new InvalidOperationException("Booking already paid.");
        if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount));
        IsPaid = true;
        AmountPaid = amount;
    }
}
