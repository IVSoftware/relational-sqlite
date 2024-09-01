using SQLite;
using System.Collections.ObjectModel;
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
        public MainPageBinding()
        {
            TestCommand = new Command(OnTest);
        }
        public ICommand TestCommand { get; private set; }
        private void OnTest(object o)
        {
            Groups.Remove(SelectedGroup);
        }
        public ObservableCollection<Group> Groups { get; } = new ObservableCollection<Group>
        {
            new Group{ GrpName = "GroupA" },
            new Group{ GrpName = "GroupB" },
        };
        public Group SelectedGroup
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

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null) => 
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        Group _selectedGroup = default;

        public event PropertyChangedEventHandler? PropertyChanged;
    }
    public class Group
    {
        [PrimaryKey, AutoIncrement]
        public long Id { get; set; }
        public string? GrpName { get; set; }
        public override string ToString() => GrpName ?? "Default";

        public async Task DeleteAsync()
        {
        }
    }

    public class Sites
    {
        [PrimaryKey, AutoIncrement]
        public long Id { get; set; }
        public string? Title { get; set; }
        public string? SiteAddr { get; set; }
        public long GrpId { get; set; }    // The Id number matching the Group Id
        public string? Notes { get; set; }
        public override string ToString() => Title ?? "Default";
    }

}
