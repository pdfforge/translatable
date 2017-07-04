using System;
using System.ComponentModel;
using Translatable.Annotations;

namespace Translatable
{
    public class EnumTranslation<T> : INotifyPropertyChanged
         where T : struct, IConvertible
    {
        private string _translation;

        public EnumTranslation(string translation, T value)
        {
            Translation = translation;
            Value = value;
        }

        public T Value { get; }

        public string Translation
        {
            get
            {
                return _translation;
            }
            internal set
            {
                if (value == _translation) return;
                _translation = value;
                OnPropertyChanged(nameof(Translation));
            }
        }

        public static EnumTranslation<T>[] CreateDefaultEnumTranslation()
        {
            return EnumTranslationFactory.CreateEnumTranslation<T>(null);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
