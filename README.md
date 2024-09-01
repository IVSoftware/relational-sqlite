As I understand it, if a UI action deletes a `Group` object from the view, the relational `Site` objects should be permanently deleted from the database (or perhaps just removed from visibility in the view). 

[![delete action][1]][1]

___

Either way, one could make a case for defining this behavior in the bound context, adding and removing the relational `Site` objects in their view as a response to `CollectionChanged.Add` `CollectionChanged.Remove` events fired by the `Groups` collection.

```
// <PackageReference Include="sqlite-net-pcl" Version="1.9.172" />
using SQLite;
class MainPageBinding : INotifyPropertyChanged
{
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
                            InMemoryDatabase.Delete(group);
                            foreach(
                                var site 
                                in InMemoryDatabase.Table<Site>()
                                .Where(_=>_.GrpId == group.Id))
                            {
                                Sites.Remove(site);
                                InMemoryDatabase.Delete(site);
                            }
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Add:
                    if (e.NewItems != null)
                    {
                        foreach (Group group in e.NewItems)
                        {
                            foreach (
                                Site site in
                                InMemoryDatabase.Table<Site>()
                                .Where(_ => _.GrpId == group.Id))
                            {
                                Sites.Add(site);
                            }
                        }
                    }
                    break;
            }
        };
        // Query the database to populate the CollectionView controls on MainView
        foreach (Group group in InMemoryDatabase.Table<Group>()) Groups.Add(group);
    }

    #region T E S T I N G
    // For testing purposes:
    // Using an in-memory SQLite database instead of .db file
    SQLiteConnection InMemoryDatabase 
    { 
        get
        {
            if(_singleton == null)
            {
                _singleton = new SQLiteConnection(":memory:");
                InMemoryDatabase.CreateTable<Group>();
                InMemoryDatabase.CreateTable<Site>();
                var groupA = new Group { GrpName = "GroupA" };
                InMemoryDatabase.Insert(groupA);
                InMemoryDatabase.Insert(new Site { Title = "GroupA.Site1", GrpId = groupA.Id });
                InMemoryDatabase.Insert(new Site { Title = "GroupA.Site2", GrpId = groupA.Id });

                var groupB = new Group { GrpName = "GroupB" };
                InMemoryDatabase.Insert(groupB);
                InMemoryDatabase.Insert(new Site { Title = "GroupB.Site1", GrpId = groupB.Id });
                InMemoryDatabase.Insert(new Site { Title = "GroupB.Site2", GrpId = groupB.Id });
            }
            return _singleton;
        }
    } 
    SQLiteConnection? _singleton = null;
    #endregion T E S T I N G
}
```

___



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


  [1]: https://i.sstatic.net/3aokWjlD.png