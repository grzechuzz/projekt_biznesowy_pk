using PB.Shared.Domain;
using PB.Modules.TripSelection.Domain.Enums;

namespace PB.Modules.TripSelection.Domain.ValueObjects;

public sealed class SelectionIssue : ValueObject
{
    public IssueType Type { get; }
    public string Message { get; }
    public Guid? RelatedItemId { get; }

    public SelectionIssue(IssueType type, string message, Guid? relatedItemId = null)
    {
        if (string.IsNullOrWhiteSpace(message)) throw new DomainException("Issue message cannot be empty");
        Type = type;
        Message = message.Trim();
        RelatedItemId = relatedItemId;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Type;
        yield return Message;
        yield return RelatedItemId;
    }
}
