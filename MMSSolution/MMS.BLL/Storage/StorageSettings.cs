namespace MMS.BLL.Storage
{
	public record StaticFilesSettings(string Path, string AccessName, string ProfileImagePath);
	public record FileSystemSettings(string Path, StaticFilesSettings StaticFiles, string InitialMeetingMinutesTemplatePath, string FinalMeetingMinutesTemplatePath);

    public record StorageSettings(FileSystemSettings? FileSystem = null, string? Type = null)
    {
        public const string FileSystemType = "FileSystem";
    }
}
