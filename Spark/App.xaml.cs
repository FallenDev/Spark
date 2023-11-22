﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using System.Windows;

using Spark.Dialogs;
using Spark.Models;
using Spark.Models.Serializers;
using Spark.ViewModels;
using Spark.Views;

namespace Spark;

public partial class App : Application
{
    public static readonly string ApplicationName = "Spark";

    public static readonly string SettingsFileName = "Settings.xml";
    public static readonly string SettingsFileVersion = "1.0";

    public static readonly string ClientVersionsFileName = "Versions.xml";
    public static readonly string ClientVersionsFileVersion = "1.1";

    #region Properties
    public UserSettings CurrentSettings { get; protected set; }
    public IEnumerable<ClientVersion> ClientVersions { get; protected set; }
    #endregion

    #region Application Lifecycle
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Load settings and client versions from file (or defaults)
        CurrentSettings = LoadSettingsOrDefaults(SettingsFileName);
        ClientVersions = LoadClientVersionsOrDefaults(ClientVersionsFileName);

        // Initialize the main window and view model
        var window = new MainWindow();
        var dialogService = new DialogService(window);
        var viewModel = new MainViewModel(CurrentSettings, ClientVersions, dialogService);

        // Bind the request close event to closing the window
        viewModel.RequestClose += delegate
        {
            window.Close();
        };

        // Assign the view model to the data context and display the main window
        window.DataContext = viewModel;
        window.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        SaveUserSettings(SettingsFileName, CurrentSettings);
        SaveClientVersions(ClientVersionsFileName, ClientVersions);

        base.OnExit(e);
    }
    #endregion

    #region Save/Load User Settings

    private static void SaveUserSettings(string fileName, UserSettings settings)
    {
        if (fileName == null)
            throw new ArgumentNullException("fileName");

        if (settings == null)
            throw new ArgumentNullException("settings");

        try
        {
            var xml = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
                new XElement("Settings",
                    new XAttribute("FileVersion", SettingsFileVersion),
                    settings.Serialize()));

            // Save user settings to file
            xml.Save(fileName);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(string.Format("Unable to save user settings: {0}", ex.Message));
        }
    }

    private static UserSettings LoadSettingsOrDefaults(string fileName)
    {
        if (fileName == null)
            throw new ArgumentNullException("fileName");

        try
        {
            // Load user settings from file
            if (File.Exists(fileName))
            {
                var xml = XDocument.Load(fileName);
                return UserSettingsSerializer.DeserializeAll(xml).FirstOrDefault() ?? UserSettings.CreateDefaults();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(string.Format("Unable to load user settings: {0}", ex.Message));
        }

        return UserSettings.CreateDefaults();
    }
    #endregion

    #region Save/Load Client Versions

    private static void SaveClientVersions(string fileName, IEnumerable<ClientVersion> versions)
    {
        if (fileName == null)
            throw new ArgumentNullException("fileName");

        if (versions == null)
            throw new ArgumentNullException("versions");

        try
        {
            // Save client versions to file
            var xml = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
                new XElement("SupportedClients",
                    new XAttribute("FileVersion", ClientVersionsFileVersion),
                    versions.SerializeAll()));

            xml.Save(fileName);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(string.Format("Unable to save client versions: {0}", ex.Message));
        }
    }

    private static IEnumerable<ClientVersion> LoadClientVersionsOrDefaults(string fileName)
    {
        if (fileName == null)
            throw new ArgumentNullException("fileName");

        IEnumerable<ClientVersion> clientVersions = null;
            
        try
        {
            // Load client versions from file
            if (File.Exists(fileName))
            {
                var xml = XDocument.Load(fileName);
                var root = xml.Descendants("SupportedClients").FirstOrDefault();

                if (root != null)
                {
                    var fileVersionString = (string)root.Attribute("FileVersion");

                    // Parse the versions for comparison
                    var fileVersion = Version.Parse(fileVersionString);
                    var latestVersion = Version.Parse(ClientVersionsFileVersion);

                    clientVersions = ClientVersionSerializer.DeserializeAll(xml);

                    // Check if client version file is out of date
                    if (fileVersion < latestVersion)
                    {
                        Debug.WriteLine(string.Format("Migrating supported client versions... ({0} -> {1})", fileVersion, latestVersion));

                        // Perform a migration (union) of the old and new client versions
                        if (clientVersions != null)
                            clientVersions = clientVersions.Union(ClientVersion.GetDefaultVersions(), ClientVersion.VersionComparer);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(string.Format("Unable to load client versions: {0}", ex.Message));
            clientVersions = null;
        }

        // Use the deserialized client versions (or the defaults)
        return clientVersions ?? ClientVersion.GetDefaultVersions();
    }
    #endregion

    public static Version GetRunningVersion()
    {
        return Assembly.GetExecutingAssembly().GetName().Version;
    }
}