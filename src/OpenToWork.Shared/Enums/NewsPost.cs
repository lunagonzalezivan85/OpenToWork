namespace OpenToWork.Shared.Enums;

public enum NewsPostType
{
    Article = 0,
    Video = 1,
    /// <summary>Aviso corto: evento, novedad de la plataforma.</summary>
    Announcement = 2
}

public enum NewsPostStatus
{
    Draft = 0,
    Published = 1,
    Archived = 2
}
