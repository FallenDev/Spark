using System;

using Spark.Models;

namespace Spark.ViewModels;

public sealed class ClientVersionViewModel : ViewModelBase
{
    private ClientVersion clientVersion;

    #region Model Properties
    public string Name
    {
        get => clientVersion.Name;
        set
        {
            OnPropertyChanging();
            clientVersion.Name = value;
            OnPropertyChanged();
        }
    }

    public int VersionCode
    {
        get => clientVersion.VersionCode;
        set
        {
            OnPropertyChanging();
            clientVersion.VersionCode = value;
            OnPropertyChanged();
        }
    }

    public string Hash
    {
        get => clientVersion.Hash;
        set
        {
            OnPropertyChanging();
            clientVersion.Hash = value;
            OnPropertyChanged();
        }
    }

    public long ServerHostnamePatchAddress
    {
        get => clientVersion.ServerHostnamePatchAddress;
        set
        {
            OnPropertyChanged();
            clientVersion.ServerHostnamePatchAddress = value;
            OnPropertyChanging();
        }
    }

    public long ServerPortPatchAddress
    {
        get => clientVersion.ServerPortPatchAddress;
        set
        {
            OnPropertyChanging();
            clientVersion.ServerPortPatchAddress = value;
            OnPropertyChanged();
        }
    }

    public long IntroVideoPatchAddress
    {
        get => clientVersion.IntroVideoPatchAddress;
        set
        {
            OnPropertyChanging();
            clientVersion.IntroVideoPatchAddress = value;
            OnPropertyChanged();
        }
    }

    public long MultipleInstancePatchAddress
    {
        get => clientVersion.MultipleInstancePatchAddress;
        set
        {
            OnPropertyChanging();
            clientVersion.MultipleInstancePatchAddress = value;
            OnPropertyChanged();
        }
    }

    public long HideWallsPatchAddress
    {
        get => clientVersion.HideWallsPatchAddress;
        set
        {
            OnPropertyChanging();
            clientVersion.HideWallsPatchAddress = value;
            OnPropertyChanged();
        }
    }
    #endregion

    public ClientVersionViewModel(ClientVersion clientVersion)
        : base(null, null)
    {
        if (clientVersion == null)
            throw new ArgumentNullException("clientVersion");

        this.clientVersion = clientVersion;
    }
}