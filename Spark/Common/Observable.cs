using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Spark.Common;

public abstract class Observable : INotifyPropertyChanged, INotifyPropertyChanging
{
    #region INotifyPropertyChanged
    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        var handler = PropertyChanged;

        handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    #endregion

    #region INotifyPropertyChanging
    public event PropertyChangingEventHandler PropertyChanging;

    protected virtual void OnPropertyChanging([CallerMemberName] string propertyName = "")
    {
        var handler = PropertyChanging;

        handler?.Invoke(this, new PropertyChangingEventArgs(propertyName));
    }
    #endregion

    protected Observable() { }

    protected virtual bool SetProperty<T>(ref T backingStore, T newValue, [CallerMemberName] string propertyName = "", Action onChanged = null, Action<T> onChanging = null)
    {
        // Check if the new value is the same as the existing value
        if (EqualityComparer<T>.Default.Equals(newValue, backingStore))
            return false;

        // OnChanging acton invoked prior to OnPropertyChanging
        onChanging?.Invoke(newValue);

        OnPropertyChanging(propertyName);
            
        // Replace the existing value with the new value
        backingStore = newValue;

        // OnChanged action invoked prior to OnPropertyChanged
        onChanged?.Invoke();

        OnPropertyChanged(propertyName);
        return true;
    }
}