using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Shin.Framework.Extensions;
using Shin.Framework.Threading;

namespace CoreSandbox
{
    internal sealed class LockSlimTests
    {
        private readonly ReaderWriterLockSlim m_lock;
        private readonly SynchronizationAccess m_access;

        public LockSlimTests(SynchronizationAccess access = SynchronizationAccess.Read)
        {
            m_access = access;
            m_lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
            
        }

        public LockSlimTests Start()
        {
            Console.WriteLine("--------------------");
            Console.WriteLine("Starting Tests...");
            PrintLockState();
            Debug.Assert(!m_lock.IsReadLockHeld);
            m_lock.TryEnter(SynchronizationAccess.Read);
            PrintLockState();
            Debug.Assert(m_lock.IsReadLockHeld);
            Console.WriteLine("--------------------");

            return this;
        }

        public void Finish()
        {
            Console.WriteLine("--------------------");
            Console.WriteLine("Finishing Tests...");
            PrintLockState();
            Debug.Assert(m_lock.IsLockHeld());
            m_lock.TryExitAll();
            PrintLockState();
            Debug.Assert(!m_lock.IsLockHeld());
            Console.WriteLine("--------------------");
        }

        public LockSlimTests StartReadRecursion()
        {
            Console.WriteLine("--------------------");
            Console.WriteLine("Starting Read Recursion Tests...");
            //try
            //{
                PrintLockState();
                var count = m_lock.RecursiveReadCount;
                //Debug.Assert(!m_lock.IsReadLockHeld);
                m_lock.TryEnter(SynchronizationAccess.Read);
                PrintLockState();
                Debug.Assert(m_lock.RecursiveReadCount > count);
                Console.WriteLine("--------------------");

                return this;
            //}
            //finally
            //{
            //    Finish();
            //}
        }

        public LockSlimTests StartWriteRecursion()
        {
            Console.WriteLine("--------------------");
            Console.WriteLine("Starting Write Recursion Tests...");
            //try
            //{
                PrintLockState();
                var count = m_lock.RecursiveWriteCount;
            //Debug.Assert(m_lock.IsLockHeld());
            m_lock.TryEnter(SynchronizationAccess.Write);
                PrintLockState();
            Debug.Assert(m_lock.RecursiveWriteCount > count);
            Console.WriteLine("--------------------");

                return this;
            //}
            //finally
            //{
            //    Finish();
            //}
        }

        private void PrintLockState()
        {
            Console.WriteLine("====================");
            Console.WriteLine($@"Is write locked: {m_lock.IsWriteLockHeld}");
            Console.WriteLine($@"Is read locked: {m_lock.IsReadLockHeld}");
            Console.WriteLine($@"Is Upgrade locked: {m_lock.IsUpgradeableReadLockHeld}");
            Console.WriteLine("~~~~~~~~~~~~~~~~~~");
            Console.WriteLine($@"Write count: {m_lock.RecursiveWriteCount}");
            Console.WriteLine($@"Read count: {m_lock.RecursiveReadCount}");
            Console.WriteLine($@"Upgrade count: {m_lock.RecursiveUpgradeCount}");
            Console.WriteLine("====================");
        }
    }
}
