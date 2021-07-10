namespace WoTClanIconDownloadConsole
{
    public enum Region
    {
        NA,
        EU,
        RU,
        ASIA
    }

    public enum ApplicationExitCode
    {
        NoError = 0,
        InvalidCmdArg = 1,
        NoRegionsSpecified = 2,
        FailedToDownloadApi = 3,
        FailedToParseApi = 4,
        FailedToDownloadImages = 5,
        HelpExit = 69,
    }
}