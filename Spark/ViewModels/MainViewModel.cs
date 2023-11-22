﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows.Input;

using Microsoft.Win32;

using Spark.Dialogs;
using Spark.Input;
using Spark.Interop;
using Spark.Models;
using Spark.Net;
using Spark.Runtime;
using Spark.Security;

namespace Spark.ViewModels;

public sealed class MainViewModel : WorkspaceViewModel
{
    private ICommand locateClientPathCommand;
    private ICommand testConnectionCommand;
    private ICommand launchClientCommand;

    private UserSettings userSettings;
    private IEnumerable<ClientVersion> clientVersions;

    private string testConnectionButtonTitle = "_Test Connection";
    private bool isTestingConnection;

    private UserSettingsViewModel userSettingsViewModel;
    private ObservableCollection<ClientVersionViewModel> clientVersionViewModels;

    #region Properties
    public ICommand LocateClientPathCommand
    {
        get
        {
            if (locateClientPathCommand == null)
                locateClientPathCommand = new DelegateCommand(parameter => OnLocateClientPath());

            return locateClientPathCommand;
        }
    }

    public ICommand TestConnectionCommand
    {
        get
        {
            // Lazy-initialized
            if (testConnectionCommand == null)
                testConnectionCommand = new DelegateCommand(x => OnTestConnection(),
                    onCanExecute: x => !string.IsNullOrWhiteSpace(UserSettings.ServerHostname) && UserSettings.ServerPort > 0 && !IsTestingConnection);

            return testConnectionCommand;
        }
    }

    public ICommand LaunchClientCommand
    {
        get
        {
            // Lazy-initialized
            if (launchClientCommand == null)
            {
                launchClientCommand = new DelegateCommand(x => OnLaunchClient(),
                    onCanExecute: x => File.Exists(UserSettings.ClientExecutablePath) && (!string.IsNullOrWhiteSpace(UserSettings.ClientVersion) || UserSettings.ShouldAutoDetectClientVersion));
            }

            return launchClientCommand;
        }
    }

    public UserSettingsViewModel UserSettings
    {
        get { return userSettingsViewModel; }
        set { SetProperty(ref userSettingsViewModel, value); }
    }

    public ObservableCollection<ClientVersionViewModel> ClientVersions
    {
        get { return clientVersionViewModels; }
        set { SetProperty(ref clientVersionViewModels, value); }
    }

    public string TestConnectionButtonTitle
    {
        get { return testConnectionButtonTitle; }
        set { SetProperty(ref testConnectionButtonTitle, value); }
    }

    public bool IsTestingConnection
    {
        get { return isTestingConnection; }
        set { SetProperty(ref isTestingConnection, value); }
    }
    #endregion

    public MainViewModel(UserSettings userSettings, IEnumerable<ClientVersion> clientVersions, IDialogService dialogService)
        : base(App.ApplicationName, dialogService)
    {
        if (userSettings == null)
            throw new ArgumentNullException("userSettings");

        if (clientVersions == null)
            throw new ArgumentNullException("clientVersions");

        if (dialogService == null)
            throw new ArgumentNullException("dialogService");

        this.userSettings = userSettings;
        this.clientVersions = clientVersions;

        // Create user settings view model
        userSettingsViewModel = new UserSettingsViewModel(userSettings);

        // Create client version view model for each client settings object (via LINQ projection)
        var viewModels = from v in clientVersions
            orderby v.VersionCode descending
            select new ClientVersionViewModel(v);

        clientVersionViewModels = new ObservableCollection<ClientVersionViewModel>(viewModels);
    }

    #region Execute Methods

    private void OnLocateClientPath()
    {
        Debug.WriteLine("OnLocateClientPath");

        // Create open file dialog to show user
        var dialog = new OpenFileDialog()
        {
            FileName = "Darkages.exe",
            DefaultExt = ".exe",
            Filter = "Dark Ages Game Clients|Darkages.exe|All Executables (*.exe)|*.exe"
        };

        // Show dialog to user
        if (dialog.ShowDialog() == true)
        {
            // Set selected filename
            UserSettings.ClientExecutablePath = dialog.FileName;
        }
    }

    private async void OnTestConnection()
    {
        // Begin testing state
        IsTestingConnection = true;
        TestConnectionButtonTitle = "Connecting to Server...";

        Debug.WriteLine("OnTestConnection");
        Debug.WriteLine(string.Format("ServerHostname = {0},  ServerPort = {1}", UserSettings.ServerHostname, UserSettings.ServerPort));

        ClientVersion clientVersion;

        #region Get Client Version
        try
        {
            clientVersion = DetectClientVersion(userSettings, clientVersions);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(string.Format("UnableToDetectClient: {0}", ex.Message));

            DialogService.ShowOKDialog("Unable to Detect Version",
                "Unable to detect the client version.",
                ex.Message);

            return;
        }

        if (clientVersion == null)
        {
            Debug.WriteLine("ClientVersionNotFound: Auto-Detect={0}, VersionName={1}", userSettings.ShouldAutoDetectClientVersion, userSettings.ClientVersion);
            DialogService.ShowOKDialog("Unknown Client Version",
                "Unable to determine the client version.",
                "You may manually select a client version by disabling auto-detection.");

            return;
        }
        #endregion

        Debug.WriteLine(string.Format("ClientVersion = {0} ({1})", clientVersion.Name, clientVersion.VersionCode));

        // Test connection asynchronously
        await TestConnectionToServerAsync(userSettings.ServerHostname, userSettings.ServerPort, clientVersion.VersionCode);

        // Completed testing
        TestConnectionButtonTitle = "_Test Connection";
        IsTestingConnection = false;
    }

    private void OnLaunchClient()
    {
        Debug.WriteLine("OnLaunchClient");

        Debug.WriteLine(string.Format("ClientExecutablePath = {0}, ClientVersion = {1}, Auto-Detect = {2}, ServerHostname = {3}, ServerPort = {4}, ShouldRedirectClient = {5}, ShouldSkipIntro = {6}, ShouldAllowMultipleInstances = {7}, ShouldHideWalls = {8}",
            UserSettings.ClientExecutablePath,
            UserSettings.ClientVersion,
            UserSettings.ShouldAutoDetectClientVersion,
            UserSettings.ServerHostname,
            UserSettings.ServerPort,
            UserSettings.ShouldRedirectClient,
            UserSettings.ShouldSkipIntro,
            UserSettings.ShouldAllowMultipleInstances,
            UserSettings.ShouldHideWalls));

        LaunchClientWithSettings(userSettings, clientVersions);
    }
    #endregion

    private void LaunchClientWithSettings(UserSettings settings, IEnumerable<ClientVersion> clientVersions)
    {
        if (settings == null)
            throw new ArgumentNullException("settings");

        if (clientVersions == null)
            throw new ArgumentNullException("clientVersions");

        IPAddress serverIPAddress = null;
        int serverPort = settings.ServerPort;
        ClientVersion clientVersion = null;

        #region Validate Client Executable Path
        if (!File.Exists(settings.ClientExecutablePath))
        {
            Debug.WriteLine("ClientExecutableNotFound: {0}", settings.ClientExecutablePath);

            DialogService.ShowOKDialog("File Not Found",
                "Unable to locate the client executable.",
                string.Format("Ensure the file exists at the following location:\n{0}", settings.ClientExecutablePath));

            return;
        }
        #endregion

        #region Determine Client Version

        try
        {
            // Detect client version
            clientVersion = DetectClientVersion(settings, clientVersions);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(string.Format("UnableToDetectClient: {0}", ex.Message));

            DialogService.ShowOKDialog("Unable to Detect Version",
                "Unable to detect the client version.",
                ex.Message);

            return;
        }

        // Check that a client version was determined
        if (clientVersion == null)
        {
            Debug.WriteLine("ClientVersionNotFound: Auto-Detect={0}, VersionName={1}", settings.ShouldAutoDetectClientVersion, settings.ClientVersion);

            DialogService.ShowOKDialog("Unknown Client Version",
                "Unable to determine the client version.",
                "You may manually select a client version by disabling auto-detection.");

            return;
        }
        #endregion

        #region Lookup Hostname for Redirect
        if (settings.ShouldRedirectClient)
        {
            try
            {
                // Resolve the hostname via DNS
                serverIPAddress = ResolveHostname(settings.ServerHostname);
                Debug.WriteLine(string.Format("Resolved '{0}' -> {1}", settings.ServerHostname, serverIPAddress));

                // An error occured when trying to resolve the hostname to an IPv4 address
                if (serverIPAddress == null)
                {
                    Debug.WriteLine(string.Format("NoIPv4AddressFoundForHost: {0}", settings.ServerHostname));

                    DialogService.ShowOKDialog("Unable to Resolve Hostname",
                        "Unable to resolve the server hostname to an IPv4 address.",
                        "Check your network connection and try again.");

                    return;
                }
            }
            catch (Exception ex)
            {
                // An error occured when trying to resolve the hostname
                Debug.WriteLine(string.Format("UnableToResolveHostname: {0}", ex.Message));

                DialogService.ShowOKDialog("Unable to Resolve Hostname",
                    "Unable to resolve the server hostname.",
                    "Check your network connection and try again.");

                return;
            }
        }
        #endregion

        #region Launch and Patch Client
        try
        {
            Debug.WriteLine(string.Format("ClientVersion = {0}", clientVersion.Name));

            // Try to launch the client
            using (var process = SuspendedProcess.Start(settings.ClientExecutablePath))
            {
                try
                {
                    PatchClient(settings, process, clientVersion, serverIPAddress, serverPort);
                }
                catch (Exception ex)
                {
                    // An error occured trying to patch the client
                    Debug.WriteLine(string.Format("UnableToPatchClient: {0}", ex.Message));

                    DialogService.ShowOKDialog("Failed to Patch",
                        "Unable to patch the client executable.",
                        ex.Message);
                }
            }
        }
        catch (Exception ex)
        {
            // An error occured trying to launch the client
            Debug.WriteLine(string.Format("UnableToLaunchClient: {0}", ex.Message));

            DialogService.ShowOKDialog("Failed to Launch",
                "Unable to launch the client executable.",
                ex.Message);
        }
        #endregion
    }

    private async Task TestConnectionToServerAsync(string serverHostname, int serverPort, int versionCode)
    {
        var shouldRetry = false;
        IPAddress serverIPAddress = null;

        #region Lookup Hostname
        try
        {
            // Resolve the hostname via DNS
            serverIPAddress = ResolveHostname(serverHostname);
            Debug.WriteLine(string.Format("Resolved '{0}' -> {1}", serverHostname, serverIPAddress));

            // An error occured when trying to resolve the hostname to an IPv4 address
            if (serverIPAddress == null)
            {
                Debug.WriteLine(string.Format("NoIPv4AddressFoundForHost: {0}", serverHostname));

                DialogService.ShowOKDialog("Unable to Resolve Hostname",
                    "Unable to resolve the server hostname to an IPv4 address.",
                    "Check your network connection and try again.");

                return;
            }
        }
        catch (Exception ex)
        {
            // An error occured when trying to resolve the hostname
            Debug.WriteLine(string.Format("UnableToResolveHostname: {0}", ex.Message));

            DialogService.ShowOKDialog("Unable to Resolve Hostname",
                "Unable to resolve the server hostname.",
                "Check your network connection and try again.");

            return;
        }
        #endregion

        try
        {
            using (var serverTester = new ServerTester())
            {
                await serverTester.ConnectToServerAsync(serverIPAddress, serverPort);

                var isVersionOK = await serverTester.CheckClientVersionCodeAsync(versionCode);
                Debug.WriteLine(string.Format("CheckClientVersionOK: {0}", isVersionOK));

                if (isVersionOK)
                {
                    // The server has accepted this client version
                    DialogService.ShowOKDialog("Connection Successful",
                        "The server appears to be up and running.",
                        string.Format("Connected to {0}:{1} successfully.", serverIPAddress, serverPort));
                }
                else
                {
                    // This server has rejected this client version
                    DialogService.ShowOKDialog("Connection Failed",
                        "The server appears to be running, but requires a different version of the client.",
                        null);
                }
            }
        }
        catch (Exception ex)
        {
            // An error occured when trying to connect to the hostname
            Debug.WriteLine(string.Format("UnableToConnect: {0}", ex.Message));

            var result = DialogService.ShowYesNoDialog("Connection Failed",
                "Unable to connect to the server.",
                ex.Message,
                yesButtonTitle: "T_ry Again",
                noButtonTitle: "_OK");

            // Retry the connection
            if (result.HasValue && result.Value)
                shouldRetry = true;
        }

        if (shouldRetry)
            await TestConnectionToServerAsync(serverHostname, serverPort, versionCode);
    }

    #region Helper Methods

    private static ClientVersion DetectClientVersion(UserSettings settings, IEnumerable<ClientVersion> availableVersions)
    {
        if (settings.ShouldAutoDetectClientVersion)
        {
            // Auto-detect using the computed MD5 hash
            using (var md5 = MD5.Create())
            {
                var hashString = md5.ComputeHashString(settings.ClientExecutablePath);

                // Find the client version by hash
                Debug.WriteLine(string.Format("ClientHash = {0}", hashString));
                return availableVersions.FirstOrDefault(x => x.Hash.Equals(hashString, StringComparison.OrdinalIgnoreCase));
            }
        }
        else
        {
            return availableVersions.FirstOrDefault(x => x.Name.Equals(settings.ClientVersion, StringComparison.OrdinalIgnoreCase));
        }
    }

    private static IPAddress ResolveHostname(string hostname)
    {
        // Lookup the server hostname (via DNS)
        var hostEntry = Dns.GetHostEntry(hostname);

        // Find the IPv4 addresses
        var ipAddresses = from ip in hostEntry.AddressList
            where ip.AddressFamily == AddressFamily.InterNetwork
            select ip;

        return ipAddresses.FirstOrDefault();
    }

    private static void PatchClient(UserSettings settings, SuspendedProcess process, ClientVersion clientVersion, IPAddress serverIPAddress, int serverPort)
    {
        if (settings.ShouldRedirectClient && serverIPAddress == null)
            throw new ArgumentNullException("serverIPAddress", "Server IP address must be specified when redirecting the client");

        if (settings.ShouldRedirectClient && serverPort <= 0)
            throw new ArgumentOutOfRangeException("Server port number must be greater than zero when redirecting the client");

        using (var stream = new ProcessMemoryStream(process.ProcessId))
        using (var patcher = new RuntimePatcher(clientVersion, stream, leaveOpen: true))
        {
            // Apply server hostname/port patch
            if (settings.ShouldRedirectClient && clientVersion.ServerHostnamePatchAddress > 0 && clientVersion.ServerPortPatchAddress > 0)
            {
                Debug.WriteLine("Applying server redirect patch...");
                patcher.ApplyServerHostnamePatch(serverIPAddress);
                patcher.ApplyServerPortPatch(serverPort);
            }

            // Apply intro video patch
            if (settings.ShouldSkipIntro && clientVersion.IntroVideoPatchAddress > 0)
            {
                Debug.WriteLine("Applying intro video patch...");
                patcher.ApplySkipIntroVideoPatch();
            }

            // Apply multiple instances patch
            if (settings.ShouldAllowMultipleInstances && clientVersion.MultipleInstancePatchAddress > 0)
            {
                Debug.WriteLine("Applying multiple instance patch...");
                patcher.ApplyMultipleInstancesPatch();
            }

            // Apply hide walls patch
            if (settings.ShouldHideWalls && clientVersion.HideWallsPatchAddress > 0)
            {
                Debug.WriteLine("Applying hide walls patch...");
                patcher.ApplyHideWallsPatch();
            }
        }
    }
    #endregion
}