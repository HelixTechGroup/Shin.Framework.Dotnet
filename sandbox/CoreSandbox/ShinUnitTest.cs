using System;
using System.Runtime.CompilerServices;
using Shin.Framework;

namespace CoreSandbox
{
    internal abstract class ShinUnitTest : Disposable
    {
        #region Methods
        protected static void PrintTestStart([CallerMemberName] string testName = "")
        {
            Console.WriteLine("--------------------");
            Console.WriteLine(@$"Starting {testName} Tests...");
            Console.WriteLine("--------------------");
            Console.WriteLine();
        }

        protected static void PrintTestEnd([CallerMemberName] string testName = "")
        {
            Console.WriteLine();
            Console.WriteLine("--------------------");
            Console.WriteLine(@$"Finishing {testName} Tests...");
            Console.WriteLine("--------------------");
            Console.WriteLine();
        }
        #endregion
    }
}