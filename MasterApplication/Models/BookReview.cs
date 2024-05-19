using MasterApplication.Models.Enums;

namespace MasterApplication.Models;

public class BookReview
{
    public string Id { get; set; } = null!;

    public BookReviewType ReviewType { get; set; }

    public string Summary { get; set; } = null!;

    public string MainStory { get; set; } = null!;

    public bool IsLikedMainStory;

    public string? SideStories { get; set; }

    public bool IsLikedSideStories;

    public string Characters { get; set; } = null!;

    public bool IsLikedCharacters;

    public string SettingsAndAmbiance { get; set; } = null!;

    public bool IsLikedSettingsAndAmbiance;

    public string Ending { get; set; } = null!;

    public bool IsLikedEnding;

    public string? ExtensiveReview { get; set; }


    public BookReview()
    {

    }
}
