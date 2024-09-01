using SQLite;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace relational_sqlite
{
    public partial class MainPage : ContentPage
    {
        public MainPage()=> InitializeComponent();
    }
    class MainPageBinding : INotifyPropertyChanged
    {
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
        public ICommand TestCommand { get; private set; }

        // If an item is selected, delete it
        private void OnTest(object o) => Groups.Remove(SelectedGroup ?? new());
        public ObservableCollection<Group> Groups { get; } = new ObservableCollection<Group>();
        public ObservableCollection<Site> Sites { get; } = new ObservableCollection<Site>();
        public Group? SelectedGroup
        {
            get => _selectedGroup;
            set
            {
                if (!Equals(_selectedGroup, value))
                {
                    _selectedGroup = value;
                    OnPropertyChanged();
                }
            }
        }

        Group? _selectedGroup = default;

        public bool ButtonVisible => SelectedGroup is not null;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            switch (propertyName)
            {
                case nameof(SelectedGroup):
                    OnPropertyChanged(nameof(ButtonVisible));
                    break;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }

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
        // Consider making this key unique at the point of
        // instantiation rather than the point of insertion.
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
}
