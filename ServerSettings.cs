﻿namespace Minecraft;

/// <summary>
/// Main class for server on-boot settings
/// </summary>
public class ServerSettings
{
    /// <summary>
    /// Every incoming network packet will be printed to console when true
    /// </summary>
    public bool ShowIncoming { get; init; }

    /// <summary>
    /// Every outgoing network packet will be printed to console when true
    /// </summary>
    public bool ShowOutgoing { get; init; }

    /// <summary>
    /// Console command prompt will not be enabled on startup when true
    /// </summary>
    public bool DisableConsoleInput { get; init; }
}
