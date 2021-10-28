namespace VaccinationScheduling.Online
{
    /// <summary>
    /// CommandTypes used for enumerating the redblacktree
    /// </summary>
    public enum CommandType
    {
        ExpandRight,
        ExpandLeft,
        ExpandAndYield,
        Yield,
        FreeExpand
    }
}
