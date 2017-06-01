using System;
using System.Threading.Tasks;

namespace MongolianBarbecue.Tests.Extensions
{
    public static class IntExtensions
    {
        public static async Task Times(this int count, Func<Task> action)
        {
            for (var counter = 0; counter < count; counter++)
            {
                await action();
            }       
        }
    }
}