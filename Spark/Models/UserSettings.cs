﻿using System;
using System.IO;

namespace Spark.Models;

[Serializable]
public sealed class UserSettings
{
    public static readonly string DefaultHostname = "da0.kru.com";
    public static readonly int DefaultPort = 2610;

    #region Properties
    public string ClientExecutablePath { get; set; }
    public string ClientVersion { get; set; }
    public bool ShouldAutoDetectClientVersion { get; set; }
    public string ServerHostname { get; set; }
    public int ServerPort { get; set; }
    public bool ShouldRedirectClient { get; set; }
    public bool ShouldSkipIntro { get; set; }
    public bool ShouldAllowMultipleInstances { get; set; }
    public bool ShouldHideWalls { get; set; }
    #endregion

    public UserSettings() { }

    public void ResetToDefaults()
    {
        // Get "Program Files (x86)" on 64bit OS, else "Program Files" on 32bit OS
        var programFilesPath = Environment.GetFolderPath(Environment.Is64BitOperatingSystem ? Environment.SpecialFolder.ProgramFilesX86 : Environment.SpecialFolder.ProgramFiles);
        ClientExecutablePath = Path.Combine(programFilesPath, "KRU", "Dark Ages", "Darkages.exe");

        ClientVersion = null;
        ShouldAutoDetectClientVersion = true;

        ServerHostname = DefaultHostname;
        ServerPort = DefaultPort;

        ShouldRedirectClient = true;
        ShouldSkipIntro = true;
        ShouldAllowMultipleInstances = true;
        ShouldHideWalls = false;
    }

    public static UserSettings CreateDefaults()
    {
        var settings = new UserSettings();
        settings.ResetToDefaults();

        return settings;
    }
}