namespace VaccinationScheduling.Online
{
    /// <summary>
    /// Score values for the sticky algorithm
    /// It always prefers to schedule both on an existing machine. But after that prefers jabs to be neighbouring an existing jab on that same machine.
    /// </summary>
    public enum Score
    {
        NEWMACHINE = 10,
        EXISTINGMACHINE = 15,
        NEIGHBOURSONE = 16,
        FLUSH = 19,
    }
}
