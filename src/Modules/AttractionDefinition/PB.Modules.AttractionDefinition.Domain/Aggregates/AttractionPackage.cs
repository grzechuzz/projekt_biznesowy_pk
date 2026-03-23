using PB.Modules.AttractionDefinition.Domain.Enums;
using PB.Modules.AttractionDefinition.Domain.ValueObjects;
using PB.Shared.Domain;

namespace PB.Modules.AttractionDefinition.Domain.Aggregates;

public class AttractionPackage : AttractionComponent
{
    public SelectionRule SelectionRule { get; private set; }
    private readonly List<Guid> _componentIds = new();

    public IReadOnlyList<Guid> ComponentIds => _componentIds.AsReadOnly();

    public AttractionPackage(string name, string description, SelectionRule selectionRule)
        : base(name, description)
    {
        SelectionRule = selectionRule ?? throw new DomainException("Selection rule cannot be null");
    }

    public void Update(string name, string description, SelectionRule selectionRule)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Package name cannot be empty");
        Name = name.Trim();
        Description = description?.Trim() ?? "";
        SelectionRule = selectionRule ?? throw new DomainException("Selection rule cannot be null");
    }

    public void AddComponent(Guid componentId)
    {
        if (_componentIds.Contains(componentId)) throw new DomainException("Component already in package");
        _componentIds.Add(componentId);
    }

    public void RemoveComponent(Guid componentId)
    {
        if (!_componentIds.Remove(componentId)) throw new DomainException("Component not found in package");
    }

    public void SetSelectionRule(SelectionRule selectionRule) =>
        SelectionRule = selectionRule ?? throw new DomainException("Selection rule cannot be null");
}
