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
            Debug.Assert(!m_lock.IsWriteLockHeld);
            m_lock.TryEnter(SynchronizationAccess.Read);
            PrintLockState();
            Debug.Assert(!m_lock.IsWriteLockHeld);
            Console.WriteLine("--------------------");

            return this;
        }

        public void Finish()
        {
            Console.WriteLine("--------------------");
            Console.WriteLine("Finishing Tests...");
            PrintLockState();
            Debug.Assert(!m_lock.IsWriteLockHeld);
            m_lock.TryExit(SynchronizationAccess.Read);
            PrintLockState();
            Debug.Assert(!m_lock.IsWriteLockHeld);
            Console.WriteLine("--------------------");
        }

        public LockSlimTests StartRecursion()
        {
            Console.WriteLine("--------------------");
            Console.WriteLine("Starting Recursion Tests...");
            try
            {
                PrintLockState();
                Debug.Assert(!m_lock.IsWriteLockHeld);
                m_lock.TryEnter(SynchronizationAccess.Read);
                PrintLockState();
                Debug.Assert(!m_lock.IsWriteLockHeld);
                Console.WriteLine("--------------------");

                return this;
            }
            finally
            {
                Finish();
            }
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
