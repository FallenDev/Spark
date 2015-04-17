﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Microsoft.Win32;

using Spark.Dialogs;
using Spark.Input;
using Spark.Interop;
using Spark.Models;
using Spark.Security;

namespace Spark.ViewModels
{
    public sealed class MainViewModel : WorkspaceViewModel
    {
        ICommand locateClientPathCommand;
        ICommand testConnectionCommand;
        ICommand launchClientCommand;

        UserSettings userSettings;
        IEnumerable<ClientVersion> clientVersions;

        UserSettingsViewModel userSettingsViewModel;
        ObservableCollection<ClientVersionViewModel> clientVersionViewModels;

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
                        onCanExecute: x => !string.IsNullOrWhiteSpace(this.UserSettings.ServerHostname) && this.UserSettings.ServerPort > 0);

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
                        onCanExecute: x => File.Exists(this.UserSettings.ClientExecutablePath) && (!string.IsNullOrWhiteSpace(this.UserSettings.ClientVersion) || this.UserSettings.ShouldAutoDetectClientVersion));
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
            this.userSettingsViewModel = new UserSettingsViewModel(userSettings);

            // Create client version view model for each client settings object (via LINQ projection)
            var clientVersionViewModels = clientVersions.Select(x => new ClientVersionViewModel(x));
            this.clientVersionViewModels = new ObservableCollection<ClientVersionViewModel>(clientVersionViewModels);
        }

        #region Execute Methods
        void OnLocateClientPath()
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
                this.UserSettings.ClientExecutablePath = dialog.FileName;
            }
        }

        void OnTestConnection()
        {
            Debug.WriteLine("OnTestConnection");

            Debug.WriteLine(string.Format("ServerHostname = {0},  ServerPort = {1}", this.UserSettings.ServerHostname, this.UserSettings.ServerPort));

            var result = this.DialogService.ShowOKDialog("Not Implemented", "Sorry, this feature is not currently implemented.", "It will be implemented in a future update.");
            Debug.WriteLine(string.Format("Result = {0}", result));
        }

        void OnLaunchClient()
        {
            Debug.WriteLine("OnLaunchClient");

            Debug.WriteLine(string.Format("ClientExecutablePath = {0}, ClientVersion = {1}, Auto-Detect = {2}, ServerHostname = {3}, ServerPort = {4}, ShouldRedirectClient = {5}, ShouldSkipIntro = {6}, ShouldAllowMultipleInstances = {7}, ShouldHideWalls = {8}",
                this.UserSettings.ClientExecutablePath,
                this.UserSettings.ClientVersion,
                this.UserSettings.ShouldAutoDetectClientVersion,
                this.UserSettings.ServerHostname,
                this.UserSettings.ServerPort,
                this.UserSettings.ShouldRedirectClient,
                this.UserSettings.ShouldSkipIntro,
                this.UserSettings.ShouldAllowMultipleInstances,
                this.UserSettings.ShouldHideWalls));

            LaunchClientWithSettings(userSettings, clientVersions);
        }
        #endregion

        #region Client Launch Methods
        void LaunchClientWithSettings(UserSettings settings, IEnumerable<ClientVersion> clientVersions)
        {
            if (settings == null)
                throw new ArgumentNullException("settings");

            if (clientVersions == null)
                throw new ArgumentNullException("clientVersions");

            IPAddress serverIPAddress = null;
            int serverPort = settings.ServerPort;
            ClientVersion clientVersion = null;

            #region Verify Client Executable
            if (!File.Exists(settings.ClientExecutablePath))
            {
                Debug.WriteLine("ClientExecutableNotFound: {0}", settings.ClientExecutablePath);

                this.DialogService.ShowOKDialog("File Not Found",
                    "Unable to locate the client executable.",
                    string.Format("Ensure the file exists at the following location:\n{0}", settings.ClientExecutablePath));

                return;
            }
            #endregion

            #region Determine Client Version
            if (settings.ShouldAutoDetectClientVersion)
            {
                try
                {
                    // Auto-detect using the computed MD5 hash
                    using (var md5 = MD5.Create())
                    {
                        var hashString = md5.ComputeHashString(settings.ClientExecutablePath);

                        // Find the client version by hash
                        Debug.WriteLine(string.Format("ClientHash = {0}", hashString));
                        clientVersion = clientVersions.FirstOrDefault(x => x.Hash.Equals(hashString, StringComparison.OrdinalIgnoreCase));
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(string.Format("UnableToDetectClient: {0}", ex.Message));

                    this.DialogService.ShowOKDialog("Unable to Detect Version",
                        "Unable to detect the client version.",
                        ex.Message);
                }
            }
            else
            {
                // Manually select client version by name
                clientVersion = clientVersions.FirstOrDefault(x => x.Name.Equals(settings.ClientVersion, StringComparison.OrdinalIgnoreCase));
            }

            // Check that a client version was determined
            if (clientVersion == null)
            {
                Debug.WriteLine("ClientVersionNotFound: Auto-Detect={0}, VersionName={1}", settings.ShouldAutoDetectClientVersion, settings.ClientVersion);

                this.DialogService.ShowOKDialog("Unknown Client Version",
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
                    // Lookup the server hostname (via DNS)
                    var hostEntry = Dns.GetHostEntry(settings.ServerHostname);

                    // Find the IPv4 address
                    foreach (var ipAddress in hostEntry.AddressList)
                    {
                        if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
                        {
                            serverIPAddress = ipAddress;
                            break;
                        }
                    }

                    // An error occured when trying to resolve the hostname to an IPv4 address
                    if (serverIPAddress == null)
                    {
                        Debug.WriteLine(string.Format("NoIPv4AddressFoundForHost: {0}", settings.ServerHostname));

                        this.DialogService.ShowOKDialog("Unable to Resolve Hostname",
                            "Unable to resolve the server hostname to an IPv4 address.",
                            "Check your network connection and try again.");

                        return;
                    }
                }
                catch (Exception ex)
                {
                    // An error occured when trying to resolve the hostname

                    Debug.WriteLine(string.Format("UnableToResolveHostname: {0}", ex.Message));

                    this.DialogService.ShowOKDialog("Unable to Resolve Hostname",
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

                        this.DialogService.ShowOKDialog("Failed to Patch",
                            "Unable to patch the client executable.",
                            ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                // An error occured trying to launch the client
                Debug.WriteLine(string.Format("UnableToLaunchClient: {0}", ex.Message));

                this.DialogService.ShowOKDialog("Failed to Launch",
                    "Unable to launch the client executable.",
                    ex.Message);
            }
            #endregion
        }

        void PatchClient(UserSettings settings, SuspendedProcess process, ClientVersion clientVersion, IPAddress serverIPAddress, int serverPort)
        {
            if (settings == null)
                throw new ArgumentNullException("settings");

            if (process == null)
                throw new ArgumentNullException("process");

            if (clientVersion == null)
                throw new ArgumentNullException("clientVersion");

            if (settings.ShouldRedirectClient && serverIPAddress == null)
                throw new ArgumentNullException("serverIPAddress", "Server IP address must be specified when redirecting the client");

            if (settings.ShouldRedirectClient && serverPort <= 0)
                throw new ArgumentOutOfRangeException("Server port number must be greater than zero when redirecting the client");

            using (var stream = new ProcessMemoryStream(process.ProcessId))
            using (var writer = new BinaryWriter(stream, Encoding.ASCII, leaveOpen: true))
            {
                // Apply server hostname/port patch
                if (settings.ShouldRedirectClient && clientVersion.ServerAddressPatchAddress > 0 && clientVersion.ServerPortPatchAddress > 0)
                {
                    Debug.WriteLine("Applying server redirect patch...");

                    // Write server IP address (bytes are reversed)
                    stream.Position = clientVersion.ServerAddressPatchAddress;
                    
                    foreach (byte ipByte in serverIPAddress.GetAddressBytes().Reverse())
                    {
                        writer.Write((byte)0x6A); // PUSH
                        writer.Write((byte)ipByte);
                    }

                    // Write server port (lo and hi bytes)
                    stream.Position = clientVersion.ServerPortPatchAddress;
                    var portHiByte = (serverPort >> 8) & 0xFF;
                    var portLoByte = serverPort & 0xFF;

                    writer.Write((byte)portLoByte);
                    writer.Write((byte)portHiByte);
                }

                // Apply intro video patch
                if (settings.ShouldSkipIntro && clientVersion.IntroVideoPatchAddress > 0)
                {
                    Debug.WriteLine("Applying intro video patch...");

                    stream.Position = clientVersion.IntroVideoPatchAddress;

                    writer.Write((byte)0x83);   // CMP
                    writer.Write((byte)0xFA);   // EDX
                    writer.Write((byte)0x00);   // 0
                    writer.Write((byte)0x90);   // NOP
                    writer.Write((byte)0x90);   // NOP
                    writer.Write((byte)0x90);   // NOP
                }

                // Apply multiple instances patch
                if (settings.ShouldAllowMultipleInstances && clientVersion.MultipleInstancePatchAddress > 0)
                {
                    Debug.WriteLine("Applying multiple instance patch...");

                    stream.Position = clientVersion.MultipleInstancePatchAddress;

                    writer.Write((byte)0x31); // XOR
                    writer.Write((byte)0xC0); // EAX, EAX
                    writer.Write((byte)0x90); // NOPs
                    writer.Write((byte)0x90); // NOP
                    writer.Write((byte)0x90); // NOP
                    writer.Write((byte)0x90); // NOP
                }

                // Apply hide walls patch
                if (settings.ShouldHideWalls && clientVersion.HideWallsPatchAddress > 0)
                {
                    Debug.WriteLine("Applying hide walls patch...");

                    stream.Position = clientVersion.HideWallsPatchAddress;

                    writer.Write((byte)0xEB);   // JMP SHORT
                    writer.Write((byte)0x17);   // +17
                    writer.Write((byte)0x90);   // NOP
                }
            }
        }
        #endregion
    }
}
