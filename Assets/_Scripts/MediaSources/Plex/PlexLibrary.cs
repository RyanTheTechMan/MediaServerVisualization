using Newtonsoft.Json.Linq;

public class PlexLibrary {
    internal PlexServer plexServer;
    internal JObject libraryData;

    public PlexLibrary(PlexServer plexServer, JObject libraryData) {
        this.plexServer = plexServer;
        this.libraryData = libraryData;
    }

}
