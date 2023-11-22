using System;

using Spark.Models;

namespace Spark.ViewModels;

public sealed class UserSettingsViewModel : ViewModelBase
{
    private UserSettings userSettings;

    #region Model Properties
    public string ClientExecutablePath
    {
        get => userSettings.ClientExecutablePath;
        set
        {
            OnPropertyChanging();
            userSettings.ClientExecutablePath = value;
            OnPropertyChanged();
        }
    }

    public string ClientVersion
    {
        get => userSettings.ClientVersion;
        set
        {
            OnPropertyChanging();
            userSettings.ClientVersion = value;
            OnPropertyChanged();
        }
    }

    public bool ShouldAutoDetectClientVersion
    {
        get => userSettings.ShouldAutoDetectClientVersion;
        set
        {
            OnPropertyChanging();
            userSettings.ShouldAutoDetectClientVersion = value;
            OnPropertyChanged();
        }
    }

    public string ServerHostname
    {
        get => userSettings.ServerHostname;
        set
        {
            OnPropertyChanging();
            userSettings.ServerHostname = value;
            OnPropertyChanged();
        }
    }

    public int ServerPort
    {
        get => userSettings.ServerPort;
        set
        {
            OnPropertyChanging();
            userSettings.ServerPort = value;
            OnPropertyChanged();
        }
    }

    public bool ShouldRedirectClient
    {
        get => userSettings.ShouldRedirectClient;
        set
        {
            OnPropertyChanging();
            userSettings.ShouldRedirectClient = value;
            OnPropertyChanged();
        }
    }

    public bool ShouldSkipIntro
    {
        get => userSettings.ShouldSkipIntro;
        set
        {
            OnPropertyChanging();
            userSettings.ShouldSkipIntro = value;
            OnPropertyChanged();
        }
    }

    public bool ShouldAllowMultipleInstances
    {
        get => userSettings.ShouldAllowMultipleInstances;
        set
        {
            OnPropertyChanging();
            userSettings.ShouldAllowMultipleInstances = value;
            OnPropertyChanged();
        }
    }

    public bool ShouldHideWalls
    {
        get => userSettings.ShouldHideWalls;
        set
        {
            OnPropertyChanging();
            userSettings.ShouldHideWalls = value;
            OnPropertyChanged();
        }
    }
    #endregion

    public UserSettingsViewModel(UserSettings userSettings)
        : base(null, null)
    {
        if (userSettings == null)
            throw new ArgumentNullException("userSettings");

        this.userSettings = userSettings;
    }
}