using CommunityToolkit.Mvvm.ComponentModel;

namespace AllergyScan.Models
{
    public class Allergy : ObservableObject
    {
        private int id;
        private string name = string.Empty;
        private DateTime createdAt;

        public int Id
        {
            get => id;
            set => SetProperty(ref id, value);
        }

        public string Name
        {
            get => name;
            set
            {
                if (SetProperty(ref name, value))
                {
                    OnPropertyChanged(nameof(DisplayName));
                }
            }
        }

        public DateTime CreatedAt
        {
            get => createdAt;
            set
            {
                if (SetProperty(ref createdAt, value))
                {
                    OnPropertyChanged(nameof(CreatedAtText));
                }
            }
        }

        public string DisplayName => string.IsNullOrWhiteSpace(Name)
            ? string.Empty
            : char.ToUpperInvariant(Name[0]) + (Name.Length > 1 ? Name[1..] : string.Empty);

        public string CreatedAtText => CreatedAt == default
            ? string.Empty
            : CreatedAt.ToLocalTime().ToString("d");
    }
}
