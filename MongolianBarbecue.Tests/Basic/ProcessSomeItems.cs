using NUnit.Framework;

namespace MongolianBarbecue.Tests.Basic
{
    [TestFixture]
    public class ProcessSomeItems : FixtureBase
    {
        protected override void SetUp()
        {
            var database = GetCleanTestDatabase();


        }
    }
}