namespace PB.Shared.Domain;

public sealed class Tag : ValueObject
{
    public string Name { get; }
    public string? Group { get; }

    public Tag(string name, string? group = null)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Tag name cannot be empty");
        Name = name.Trim().ToLower();
        Group = group?.Trim().ToLower();
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Name;
        yield return Group;
    }

    public override string ToString() => Group != null ? $"{Group}:{Name}" : Name;
}
