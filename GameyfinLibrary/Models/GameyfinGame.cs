namespace GameyfinLibrary.Models
{
    internal class GameyfinGame
    {
        public string Slug { get; set; }

        public string Title { get; set; }

        public string Summary { get; set; }

        public string ReleaseDate { get; set; }

        public int UserRating { get; set; }

        public int CriticsRating { get; set; }

        public string CoverId { get; set; }

        public bool ConfirmedMatch { get; set; }

        public bool OfflineCoop { get; set; }

        public bool OnlineCoop { get; set; }

        public bool LanSupport { get; set; }

        public GameyfinLibraryDetails Library { get; set; }

        public SlugAndName[] PlayerPerspectives { get; set; }

        public SlugAndName[] Genres { get; set; }

        public SlugNameAndLogo[] Companies { get; set; }

        public SlugNameAndLogo[] Platforms { get; set; }
    }

    internal class GameyfinLibraryDetails
    {
        public string Path { get; set; }

        public SlugNameAndLogo[] Platforms { get; set; }
    }

    internal class SlugAndName
    {
        public string Slug { get; set; }

        public string Name { get; set; }
    }

    internal class SlugNameAndLogo : SlugAndName
    {
        public string LogoId { get; set; }
    }
}
