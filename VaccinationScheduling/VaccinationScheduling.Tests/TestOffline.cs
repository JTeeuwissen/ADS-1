using System.IO;
using Xunit;

namespace VaccinationScheduling.Tests
{
    public class TestOffline
    {
        const string InputFolderPath = @"..\..\..\Input\offline\";

        [Fact]
        public void Test1()
        {
            foreach (string filePath in Directory.EnumerateFileSystemEntries(InputFolderPath))
            {
                Extensions.SetInput(filePath);
                Offline.Program.Main();
            }
        }
    }
}