namespace LogisticApp.Functions.Models;

public enum DeliveryStatus
{
    Created,
    InsertedOk,
    InsertedError,
    NotInserted,
    Error
}

public class ContainerEntry
{
    public Guid   Id     { get; set; }
    public string Number { get; set; } = string.Empty;
}

public class Client
{
    public Guid   Id          { get; set; }
    public string CompanyName { get; set; } = string.Empty;
}

public class Delivery
{
    public Guid                 Id               { get; set; }
    public List<ContainerEntry> ContainerNumbers { get; set; } = new();
    public Guid                 ClientId         { get; set; }
    public Client               Client           { get; set; } = null!;
    public string               DeliveryOrder    { get; set; } = string.Empty;
    public string               PickupLocation   { get; set; } = string.Empty;
    public string               ReturnLocation   { get; set; } = string.Empty;
    public DateTime             DeliveryDate     { get; set; }
    public DeliveryStatus       Status           { get; set; }
    public DateTime             CreatedAt        { get; set; }
    public DateTime             UpdatedAt        { get; set; }
}
