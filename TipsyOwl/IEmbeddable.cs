using System.Collections.Generic;
using Discord;

namespace TipsyOwl
{
    /// <summary>
    ///     Interface for content representable as either a single embed or an ordered sequence of embeds.
    /// </summary>
    public interface IEmbeddable
    {
        /// <summary>
        ///     A name string identifying the content. This is only intended for display in places where the content itself isn't
        ///     displayed (e.g. a table of contents), so it may be redundant with some part of the content embed(s).
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     Fetch or build a single embed that partially or fully represents the content. This may differ from
        ///     <see cref="GetAllEmbeds" />, even if that list only contains one embed.
        /// </summary>
        Embed GetMainEmbed();

        /// <summary>
        ///     Fetch or build a list of embeds that fully represents the content. This won't be empty, but may contain only one
        ///     embed that may or may not be equivalent to <see cref="GetMainEmbed" />.
        /// </summary>
        IReadOnlyList<Embed> GetAllEmbeds();
    }
}
