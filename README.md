One could make a case for defining this behavior in the bound context, deleting the relational `Site` objects as a response to `CollectionChanged.Remove` events fired by the `Groups` collection.

```
public MainPageBinding()
{
    TestCommand = new Command(OnTest);
    Groups.CollectionChanged += (sender, e) =>
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Remove:
                if (e.OldItems != null)
                {
                    foreach (Group group in e.OldItems)
                    {
                        // MAUI seems to leave group selected even after it's removed
                        // from the collection, which seems weird but let's fix that
                        // (for the benefit of button visibility binding)
                        if(Equals(group, SelectedGroup))
                        {
                            SelectedGroup = null;
                        }
                        InMemoryDatabase.Delete(group);
                        foreach(
                            var site 
                            in InMemoryDatabase.Table<Site>()
                            .Where(_=>_.GrpId == group.Id)
                            .ToArray())
                        {
                            Sites.Remove(site);
                            InMemoryDatabase.Delete(site);
                        }
                    }
                }
                break;
        }
    };
    // Query the database to populate the CollectionView controls on MainView
    foreach (Group group in InMemoryDatabase.Table<Group>()) Groups.Add(group);
    foreach (Site site in InMemoryDatabase.Table<Site>())Sites.Add(site);
}
```

___

Now, using an auto-increment id seemed to add an extra element of danger because it's not assigned until the Group is inserted. For this demo, I went ahead and used a guid instead so that the `Id` is unique from the moment of creation.

```
[Table("groups")]
public class Group : IEquatable<Group>
{
    // Consider making this key unique at the point of
    // instantiation rather than the point of insertion.
    [PrimaryKey]
    public string Id { get; set; } = Guid.NewGuid().ToString().ToUpper();
    [Unique]
    public string? GrpName { get; set; }
    public bool Equals(Group? other) =>
        Id == other?.Id;
    public override string ToString() => GrpName ?? "Default";
}

[Table("site")]
public class Site : IEquatable<Site>
{
    [PrimaryKey]
    public string Id { get; set; } = Guid.NewGuid().ToString().ToUpper();
    [Unique]
    public string? Title { get; set; }
    public string? SiteAddr { get; set; }
    public string? GrpId { get; set; }    // The Id number matching the Group Id
    public string? Notes { get; set; }
    public bool Equals(Site? other) =>
        Id == other?.Id;
    public override string ToString() => Title ?? "Default";
}
```