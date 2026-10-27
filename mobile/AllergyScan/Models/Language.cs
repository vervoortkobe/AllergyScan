using CommunityToolkit.Mvvm.ComponentModel;

namespace AllergyScan.Models
{
    public class Language : ObservableObject
    {
        private int id;
        private string code = string.Empty;
        private string name = string.Empty;

        public int Id
        {
            get => id;
            set => SetProperty(ref id, value);
        }

        /// <summary>ISO 639-1 code, for example "nl".</summary>
        public string Code
        {
            get => code;
            set
            {
                if (SetProperty(ref code, value))
                {
                    OnPropertyChanged(nameof(DisplayName));
                }
            }
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

        public string DisplayName => string.IsNullOrWhiteSpace(Code)
            ? Name
            : $"{Name} ({Code.ToUpperInvariant()})";

        public override string ToString() => DisplayName;
    }
}
