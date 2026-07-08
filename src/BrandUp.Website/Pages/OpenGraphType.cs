namespace BrandUp.Website.Pages
{
    /// <summary>
    /// Стандартные типы Open Graph (og:type), см. https://ogp.me/#types.
    /// </summary>
    public enum OpenGraphType
    {
        Website,
        Article,
        Profile,
        Book,
        MusicSong,
        MusicAlbum,
        MusicPlaylist,
        MusicRadioStation,
        VideoMovie,
        VideoEpisode,
        VideoTvShow,
        VideoOther
    }

    public static class OpenGraphTypeExtensions
    {
        /// <summary>Возвращает строковое значение og:type (например, "music.song").</summary>
        public static string ToOpenGraphString(this OpenGraphType type) => type switch
        {
            OpenGraphType.Website => "website",
            OpenGraphType.Article => "article",
            OpenGraphType.Profile => "profile",
            OpenGraphType.Book => "book",
            OpenGraphType.MusicSong => "music.song",
            OpenGraphType.MusicAlbum => "music.album",
            OpenGraphType.MusicPlaylist => "music.playlist",
            OpenGraphType.MusicRadioStation => "music.radio_station",
            OpenGraphType.VideoMovie => "video.movie",
            OpenGraphType.VideoEpisode => "video.episode",
            OpenGraphType.VideoTvShow => "video.tv_show",
            OpenGraphType.VideoOther => "video.other",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown Open Graph type.")
        };

        /// <summary>Разбирает строковое значение og:type в <see cref="OpenGraphType"/>.</summary>
        public static OpenGraphType ParseOpenGraphType(string value)
        {
            ArgumentNullException.ThrowIfNull(value);

            return value.Trim().ToLowerInvariant() switch
            {
                "website" => OpenGraphType.Website,
                "article" => OpenGraphType.Article,
                "profile" => OpenGraphType.Profile,
                "book" => OpenGraphType.Book,
                "music.song" => OpenGraphType.MusicSong,
                "music.album" => OpenGraphType.MusicAlbum,
                "music.playlist" => OpenGraphType.MusicPlaylist,
                "music.radio_station" => OpenGraphType.MusicRadioStation,
                "video.movie" => OpenGraphType.VideoMovie,
                "video.episode" => OpenGraphType.VideoEpisode,
                "video.tv_show" => OpenGraphType.VideoTvShow,
                "video.other" => OpenGraphType.VideoOther,
                _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown Open Graph type.")
            };
        }
    }
}
