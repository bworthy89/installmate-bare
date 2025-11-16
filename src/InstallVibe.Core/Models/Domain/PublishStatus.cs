namespace InstallVibe.Core.Models.Domain;

/// <summary>
/// Represents the publishing status of a guide.
/// </summary>
public enum PublishStatus
{
    /// <summary>
    /// Guide is in draft state and not visible to technicians.
    /// </summary>
    Draft = 0,

    /// <summary>
    /// Guide is published and visible to all technicians.
    /// </summary>
    Published = 1,

    /// <summary>
    /// Guide is archived and hidden from technicians (but retained in database).
    /// </summary>
    Archived = 2
}
