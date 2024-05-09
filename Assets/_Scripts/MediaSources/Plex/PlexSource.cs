public class PlexSource : MediaSource {
    // https://www.plex.tv/about/privacy-legal/plex-trademarks-and-guidelines/ - Plex Trademarks and Guidelines
    // https://brand.plex.tv/d/sCoLEtmF5hWG/style-guide#/visual/logo - Official Plex logo
    // NOTE: Art/Sources/Plex/icon.svg - Is unofficial and not from Plex
    
    public override string name { get; set; } = "Plex";
    public override string iconPath { get; set; } = "Icons/PlexIcon.png";
    public override string bannerPath { get; set; } = "Icons/PlexBanner.png";
}