using System.ComponentModel;
using System.Xml.Serialization;
using System;

namespace romsdownload.Data
{
    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "", IsNullable = false)]
    public class Config : INotifyPropertyChanged
    {
        [XmlIgnore] public static Config Instance;

        private string _selectedStyle;
        private string _selectedColor;

        public string SelectedStyle
        {
            get { return _selectedStyle; }
            set
            {
                _selectedStyle = value;
                OnPropertyChanged("SelectedStyle");
            }
        }

        public string SelectedColor
        {
            get { return _selectedColor; }
            set
            {
                _selectedColor = value;
                OnPropertyChanged("SelectedColor");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
