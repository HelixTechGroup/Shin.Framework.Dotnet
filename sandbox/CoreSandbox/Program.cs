#region Usings
using System;
#endregion

namespace CoreSandbox
{
    class Program
    {
        #region Methods
        private static void Main(string[] args)
        {
            //Throw.If(args.Length > 0).InvalidOperationException();

            //var lockSlimTests = new LockSlimTests();
            //for (var i = 0; i < 30; i++)
            //{
            //    lockSlimTests.Start()
            //                 .StartWriteRecursion()
            //                 .StartReadRecursion()
            //                 .StartWriteRecursion()
            //                 .Finish();
            //}

            //Console.WriteLine("Lockslim Tests Finished");
            //Console.WriteLine("Press q to Quit or any other key to continue...");
            //if (Console.ReadKey().Key == ConsoleKey.Q)
            //    return;

            var containerTests = new IoCContainerTests();
            containerTests.Start()
                          .ParentCreationTests()
                          .ParentRegisterTests()
                          .ParentLifetimeTests()
                          .ChildCreationTests()
                          .ChildRegisterTests()
                          .ChildLifetimeTests()
                          .GrandchildCreationTests()
                          .GrandchildRegisterTests()
                          .GrandchildLifetimeTests()
                           //.AsyncLifetimeTests()
                          .Finish();

            Console.WriteLine("Tests Finished");
            Console.WriteLine("Please press any key to continue...");
            Console.ReadKey();
        }
        #endregion
    }
}