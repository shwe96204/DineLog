using Shared.Models;

namespace Shared.Services;

public class RestaurantTagService
{
    private readonly List<RestaurantTagItem> _tags =
    [
        new()
        {
            Id = Guid.Parse("8f918746-20ff-4730-aa4a-c18f41fc8c60"),
            Name = "cozy",
            Category = "Mood",
            Description = "Warm, comfortable, and intimate dining atmosphere."
        },
        new()
        {
            Id = Guid.Parse("cf5b1afc-918e-4bf8-a10e-36743de7f8e0"),
            Name = "romantic",
            Category = "Occasion",
            Description = "A place suitable for date nights and soft evening ambience."
        },
        new()
        {
            Id = Guid.Parse("9384c614-c4fd-409b-b26a-344d949a40ab"),
            Name = "comfort-food",
            Category = "Food Style",
            Description = "Familiar dishes that feel comforting and satisfying."
        },
        new()
        {
            Id = Guid.Parse("8207ca28-7a0f-43a0-920a-461205786855"),
            Name = "celebration",
            Category = "Occasion",
            Description = "Best used for birthdays, milestones, and special gatherings."
        },
        new()
        {
            Id = Guid.Parse("66055306-2f01-4da2-9d81-265226257ac8"),
            Name = "rooftop",
            Category = "Setting",
            Description = "Dining with a view or open-air elevated seating."
        }
    ];

    public IReadOnlyList<RestaurantTagItem> GetTags() =>
        _tags.OrderBy(x => x.Name).ToList();

    public RestaurantTagItem? GetTag(Guid id) =>
        _tags.FirstOrDefault(x => x.Id == id);

    public void SaveTag(RestaurantTagItem tag)
    {
        var normalized = NormalizeName(tag.Name);
        tag.Name = normalized;

        var existing = _tags.FirstOrDefault(x => x.Id == tag.Id);
        if (existing is null)
        {
            tag.Id = tag.Id == Guid.Empty ? Guid.NewGuid() : tag.Id;
            _tags.Add(new RestaurantTagItem
            {
                Id = tag.Id,
                Name = tag.Name,
                Category = tag.Category.Trim(),
                Description = tag.Description.Trim()
            });
            return;
        }

        existing.Name = tag.Name;
        existing.Category = tag.Category.Trim();
        existing.Description = tag.Description.Trim();
    }

    public void DeleteTag(Guid id)
    {
        var tag = _tags.FirstOrDefault(x => x.Id == id);
        if (tag is not null)
        {
            _tags.Remove(tag);
        }
    }

    private static string NormalizeName(string value) =>
        value.Trim().ToLowerInvariant().Replace(' ', '-');
}
