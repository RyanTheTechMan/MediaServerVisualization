using System.Collections.Generic;

public abstract class MediaLibrary {
    internal MediaServer Server;
    public List<MediaData> MediaData;
    public virtual string name { get; protected set; } = "N/A";
    public LibraryType libraryType { get; protected set; } = LibraryType.UNKNOWN;

    public virtual void UpdateMediaList() {
        throw new System.NotImplementedException();
    }
}
public enum LibraryType {
    UNKNOWN,
    MOVIE,
    SHOW,
}