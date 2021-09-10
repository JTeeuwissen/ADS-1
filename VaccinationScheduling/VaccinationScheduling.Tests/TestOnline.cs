using System.IO;
using Xunit;

namespace VaccinationScheduling.Tests
{
    public class TestOnline
    {
        const string InputFolderPath = @"..\..\..\Input\Online\";

        [Fact]
        public void Test1()
        {
            foreach (string filePath in Directory.EnumerateFileSystemEntries(InputFolderPath))
            {
                Extensions.SetInput(filePath);
                Online.Program.Main();
            }
        }
    }
}