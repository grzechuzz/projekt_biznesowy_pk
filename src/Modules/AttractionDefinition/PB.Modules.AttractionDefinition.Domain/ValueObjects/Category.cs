namespace PB.Modules.AttractionDefinition.Domain.ValueObjects;

using PB.Shared.Domain;

public class Category : ValueObject
{
    public string Value { get; }

    public static readonly Category Landmark = new("Landmark");
    public static readonly Category Sport = new("Sport");
    public static readonly Category Gastronomy = new("Gastronomy");
    public static readonly Category Culture = new("Culture");
    public static readonly Category Entertainment = new("Entertainment");
    public static readonly Category Nature = new("Nature");

    public Category(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Category cannot be empty.");
        Value = value;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
