﻿#region Usings
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Shin.Framework.Collections.Concurrent;
using Shin.Framework.Extensions;
using Shin.Framework.IoC.DependencyInjection;
using Shin.Framework.IoC.Native.DependencyInjection;
using Shin.Framework.IoC.Native.DependencyInjection.Exceptions;
#endregion

namespace CoreSandbox
{
    internal sealed class IoCContainerTests : ShinUnitTest
    {
        #region Members
        private static IDIContainer m_childContainer;
        private static IDIContainer m_childContainer2;
        private static IDIContainer m_container;
        private static IDIContainer m_grandChildContainer;
        private static IDIContainer m_grandChildContainer2;
        #endregion

        #region Methods
        public IoCContainerTests Start()
        {
            Console.WriteLine("--------------------");
            Console.WriteLine("Starting Tests...");
            Console.WriteLine("--------------------");

            return this;
        }

        public IoCContainerTests ParentCreationTests()
        {
            PrintTestStart();

            Console.WriteLine("Creating Parent Container...");
            m_container = new ShinDIContainer();

            Debug.Assert(m_container != null);

            Console.WriteLine(m_container.ToString());

            PrintTestEnd();
            return this;
        }

        public IoCContainerTests ParentRegisterTests()
        {
            PrintTestStart();

            Debug.Assert(m_container != null);

            m_container.Register<IoCTestClassA>();
            m_container.Register<IoCTestClassB>();


            Debug.Assert(m_container.IsTypeRegistered<IoCTestClassA>());
            Debug.Assert(m_container.IsTypeRegistered<IoCTestClassB>());

            //Console.WriteLine(m_container.ToString());
            PrintTestEnd();

            return this;
        }

        public IoCContainerTests ParentLifetimeTests()
        {
            PrintTestStart();

            Debug.Assert(m_container != null);

            var testA = m_container.Resolve<IoCTestClassA>();
            Console.WriteLine(@$"test A id: {testA.Id}");

            var testA2 = m_container.Resolve<IoCTestClassA>();
            Console.WriteLine(@$"test2 A id: {testA2.Id}");

            var testA3 = m_container.Resolve<IIoCTestInterface>();
            Console.WriteLine(@$"test2 A id: {testA3.Id}");

            Console.WriteLine(@$"test, test2, test3 A id Equal: {testA.Id == testA2.Id && testA.Id == testA3.Id}");
            Debug.Assert(testA.Id == testA2.Id && testA.Id == testA3.Id);

            PrintTestEnd();

            return this;
        }

        public IoCContainerTests ChildCreationTests()
        {
            PrintTestStart();
            Console.WriteLine("--------------------");
            Console.WriteLine("Starting Create Child Tests...");
            Console.WriteLine("--------------------");
            Console.WriteLine("Creating Child Container...");
            Debug.Assert(m_container != null);

            m_childContainer = m_container.Resolve<IDIChildContainer>();
            //m_container.CreateChildContainer();
            Debug.Assert(m_childContainer != null);
            Console.WriteLine(m_childContainer.ToString());
            
            PrintTestEnd();
            return this;
        }

        public IoCContainerTests GrandchildCreationTests()
        {
            Console.WriteLine("--------------------");
            Console.WriteLine("Starting Create Child Tests...");
            Console.WriteLine("--------------------");
            Debug.Assert(m_container != null);
            Debug.Assert(m_childContainer != null);

            Console.WriteLine("Creating Grandchild Container...");
            m_grandChildContainer = m_childContainer.CreateChildContainer();
            Debug.Assert(m_grandChildContainer != null);
            Console.Write(m_grandChildContainer.ToString());

            return this;
        }

        public IoCContainerTests ChildRegisterTests()
        {
            Console.WriteLine("--------------------");
            Console.WriteLine("Starting Child Register Tests...");
            Console.WriteLine("--------------------");
            Debug.Assert(m_container != null);
            Debug.Assert(m_childContainer != null);

            m_childContainer.Register<IoCTestClassC>();

            Debug.Assert(m_container.IsTypeRegistered<IoCTestClassC>());
            Debug.Assert(m_childContainer.IsTypeRegistered<IoCTestClassC>());

            Debug.Assert(m_childContainer.IsTypeRegistered<IoCTestClassA>());
            Debug.Assert(m_childContainer.IsTypeRegistered<IoCTestClassB>());

            //Console.WriteLine(m_childContainer.ToString());

            return this;
        }

        public IoCContainerTests ChildLifetimeTests()
        {
            Console.WriteLine("--------------------");
            Console.WriteLine("Starting Child Lifetime Tests...");
            Console.WriteLine("--------------------");
            Debug.Assert(m_container != null);
            Debug.Assert(m_childContainer != null);

            var testA = m_container.Resolve<IoCTestClassA>();
            var testA2 = m_childContainer.Resolve<IoCTestClassA>();
            Console.WriteLine(@$"test A id(Root): {testA.Id}");
            Console.WriteLine(@$"test A id(Root->Child): {testA2.Id}");
            Console.WriteLine(@$"test A id and test2 A id Equal: {testA.Id == testA2.Id}");
            Debug.Assert(testA.Id == testA2.Id);

            var testB = m_container.Resolve<IoCTestClassB>();
            var testB2 = m_childContainer.Resolve<IoCTestClassB>();
            Console.WriteLine(@$"test B id(Root): {testB.Id}");
            Console.WriteLine(@$"test B id(Root->Child): {testB2.Id}");
            Console.WriteLine(@$"test B id and test2 B id Equal: {testB.Id == testB2.Id}");
            Debug.Assert(testB.Id == testB2.Id);

            var testC = m_container.Resolve<IoCTestClassC>();
            var testC2 = m_childContainer.Resolve<IoCTestClassC>();
            Console.WriteLine(@$"test C id(Root): {testC.Id}");
            Console.WriteLine(@$"test C id(Root->Child): {testC2.Id}");
            Console.WriteLine(@$"test C id and test2 C id Equal: {testC.Id == testC2.Id}");
            Debug.Assert(testC.Id == testC2.Id);

            //PrintLockState();
            //Console.WriteLine("--------------------");

            return this;
        }

        public IoCContainerTests GrandchildLifetimeTests()
        {
            Console.WriteLine("--------------------");
            Console.WriteLine("Starting Grandchild Lifetime Tests...");
            Console.WriteLine("--------------------");
            Debug.Assert(m_container != null);
            Debug.Assert(m_childContainer != null);
            Debug.Assert(m_grandChildContainer != null);

            var testA = m_container.Resolve<IoCTestClassA>();
            var testA2 = m_childContainer.Resolve<IoCTestClassA>();
            var testA3 = m_grandChildContainer.Resolve<IoCTestClassA>();
            var conditionA = testA.Id == testA2.Id && testA.Id == testA3.Id;
            Console.WriteLine(@$"test A id(Root): {testA.Id}");
            Console.WriteLine(@$"test A id(Root->Child): {testA2.Id}");
            Console.WriteLine(@$"test A id(Root->Child->Grandchild): {testA3.Id}");
            Console.WriteLine(@$"test A id Equal: {conditionA}");
            Debug.Assert(conditionA);

            var testB = m_container.Resolve<IoCTestClassB>();
            var testB2 = m_childContainer.Resolve<IoCTestClassB>();
            var testB3 = m_grandChildContainer.Resolve<IoCTestClassB>();
            var conditionB = testB.Id == testB2.Id && testB.Id == testB3.Id;
            Console.WriteLine(@$"test B id(Root): {testB.Id}");
            Console.WriteLine(@$"test B id(Root->Child): {testB2.Id}");
            Console.WriteLine(@$"test B id(Root->Child->Grandchild): {testB3.Id}");
            Console.WriteLine(@$"test B id Equal: {conditionB}");
            Debug.Assert(conditionB);

            var testC = m_container.Resolve<IoCTestClassC>();
            var testC2 = m_childContainer.Resolve<IoCTestClassC>();
            var testC3 = m_grandChildContainer.Resolve<IoCTestClassC>();
            var conditionC = testC.Id == testC2.Id && testC.Id == testC3.Id;
            Console.WriteLine(@$"test C id(Root): {testC.Id}");
            Console.WriteLine(@$"test C id(Root->Child): {testC2.Id}");
            Console.WriteLine(@$"test C id(Root->Child->Grandchild): {testC3.Id}");
            Console.WriteLine(@$"test C id Equal: {conditionC}");
            Debug.Assert(conditionC);

            var testD = m_container.Resolve<IoCTestClassA>();
            var testD2 = m_childContainer.Resolve<IoCTestClassA>();
            var testD3 = m_grandChildContainer.Resolve<IoCTestClassA>();
            var conditionD = testD.Id == testD2.Id && testD.Id == testD3.Id;
            Console.WriteLine(@$"test D id(Root): {testD.Id}");
            Console.WriteLine(@$"test D id(Root->Child): {testD2.Id}");
            Console.WriteLine(@$"test D id(Root->Child->Grandchild): {testD3.Id}");
            Console.WriteLine(@$"test D id Equal: {conditionD}");
            Debug.Assert(conditionD);


            //PrintLockState();
            Console.WriteLine();
            Console.WriteLine("--------------------");

            return this;
        }

        public IoCContainerTests GrandchildRegisterTests()
        {
            Console.WriteLine("--------------------");
            Console.WriteLine("Starting Grandchild Register Tests...");
            Debug.Assert(m_container != null);
            Debug.Assert(m_childContainer != null);
            Debug.Assert(m_grandChildContainer != null);

            m_grandChildContainer.Register<IoCTestClassD>();

            Debug.Assert(m_container.IsTypeRegistered<IoCTestClassD>());
            Debug.Assert(m_childContainer.IsTypeRegistered<IoCTestClassD>());
            Debug.Assert(m_grandChildContainer.IsTypeRegistered<IoCTestClassD>());

            Debug.Assert(m_grandChildContainer.IsTypeRegistered<IoCTestClassA>());
            Debug.Assert(m_grandChildContainer.IsTypeRegistered<IoCTestClassB>());
            Debug.Assert(m_grandChildContainer.IsTypeRegistered<IoCTestClassC>());

            Console.Write(m_grandChildContainer.ToString());
            Console.WriteLine("--------------------");

            return this;
        }

        public IoCContainerTests TraversalContainerTests()
        {
            Console.WriteLine("--------------------");
            Console.WriteLine("Starting Traversal Tests...");
            Debug.Assert(m_container != null);
            Debug.Assert(m_childContainer != null);

            Console.WriteLine("Creating Grandchild Container...");
            using var child = m_childContainer.CreateChildContainer();
            Debug.Assert(child != null);
            Console.Write(child.ToString());

            child.Register<IoCTestClassD>();

            Debug.Assert(m_container.IsTypeRegistered<IoCTestClassD>());
            Debug.Assert(m_childContainer.IsTypeRegistered<IoCTestClassD>());
            Debug.Assert(child.IsTypeRegistered<IoCTestClassD>());

            Debug.Assert(child.IsTypeRegistered<IoCTestClassA>());
            Debug.Assert(child.IsTypeRegistered<IoCTestClassB>());
            Debug.Assert(child.IsTypeRegistered<IoCTestClassC>());

            var testA = m_container.Resolve<IoCTestClassD>();
            Console.WriteLine(@$"test D id(Root): {testA.Id}");

            var testA2 = m_childContainer.Resolve<IoCTestClassD>();
            Console.WriteLine(@$"test D id(Root->Child): {testA2.Id}");

            //PrintLockState();
            var testA3 = child.Resolve<IoCTestClassD>();
            Console.WriteLine(@$"test D id(Root->Child->Child): {testA3.Id}");

            var condition = testA.Id == testA2.Id && testA.Id == testA3.Id;
            Debug.Assert(condition);
            Console.WriteLine(@$"All IoCTestClassD Id Equal: {condition}");


            //PrintContainerState();
            Console.WriteLine("--------------------");

            return this;
        }

        public IoCContainerTests ConstructorInjectionTests()
        {
            PrintTestStart();

            Console.WriteLine("Constructor Injection Tests...");
            Debug.Assert(m_container is not null);

            m_container.Register<IoCTestClassE>();
            m_container.Register<IoCTestClassF>();

            var a = m_container.Resolve<IoCTestClassA>();
            Debug.Assert(a != null);
            Console.WriteLine(@$"test A id(Root): {a.Id}");

            var e = m_container.Resolve<IoCTestClassE>();
            Debug.Assert(e != null);
            Console.WriteLine(@$"test E id(Root): {e.Id}");

            var f = m_container.Resolve<IoCTestClassF>();
            Debug.Assert(f != null);
            Console.WriteLine(@$"test F id(Root): {f.Id}");

            Console.WriteLine(@$"test E->Interface id(TestA): {e.Interface.Id}");
            Console.WriteLine(@$"test F->Class id(TestE): {f.Class.Id}");

            var conditionA = e.Interface.Id.Equals(a.Id);
            var conditionB = f.Class.Id.Equals(e.Id);

            Debug.Assert(conditionA);
            Console.WriteLine(@$"IoCTestClassE.Interface Id Equal: {conditionA}");

            Debug.Assert(conditionB);
            Console.WriteLine(@$"IoCTestClassF.Class Id Equal: {conditionB}");

            PrintTestEnd();

            return this;
        }

        public IoCContainerTests InterfaceRegistrationTests()
        {
            PrintTestStart();
            
            Console.WriteLine("Starting Interface Registration Tests...");
            Debug.Assert(m_container is not null);
            
            m_container.Register<IIoCTestInterface2, IoCTestClassG>();
            Debug.Assert(m_container.IsTypeRegistered<IoCTestClassG>());
            Debug.Assert(m_container.IsTypeRegistered<IIoCTestInterface2>());
            
            PrintTestEnd();
            return this;
        }
        
        public IoCContainerTests InterfaceResolutionTests()
        {
            PrintTestStart();

            Console.WriteLine("Starting Interface Resolution Tests...");
            Debug.Assert(m_container is not null);

            var testA = m_container.Resolve<IIoCTestInterface>();
            Debug.Assert(testA is not null);
            Console.WriteLine(@$"test A (IIoCTestInterface) id(Root): {testA.Id}");

            var testA2 = m_container.Resolve<IoCTestClassA>();
            Debug.Assert(testA2 is not null);
            Console.WriteLine(@$"test A (IoCTestClassA) id(Root): {testA2.Id}");

            var condition = testA.Id == testA2.Id;
            Debug.Assert(condition);
            Console.WriteLine(@$"test A Interface and Concrete id Equal: {condition}");

            Debug.Assert(TestInterface<IIoCTestInterface>());
            Debug.Assert(!TestInterface<IIoCTestInterfaceNoConcrete>());

            PrintTestEnd();

            return this;
        }

        private bool TestInterface<T>()
        {
            var result = false;
            
            try
            {
                var condition2 = m_container.Resolve<T>();
                //Console.WriteLine(@$"test {typeof(T).Name} has no implementation: {condition2}");
                Debug.Assert(condition2 is not null);
            }
            catch (IoCResolutionException e)
            {
                //Console.WriteLine($@"");
                return false;
                //Debug.Assert(false);
            }

            try
            {
                var condition3 = m_container.ResolveAll<T>();
                //Console.WriteLine(@$"testI has no implementation: {condition3}");
                Debug.Assert(condition3 is not null);
            }
            catch (IoCResolutionException e)
            {
                Console.WriteLine(e);
                return false;
                //Debug.Assert(false);
            }

            var condition4 = m_container.TryResolve<T>(out var testI);
            Console.WriteLine(@$"{typeof(T).Name} has implementation: {condition4}");
            result = condition4;
            if (!result)
                return false;
            //Debug.Assert(condition4);
            //Debug.Assert(testI is not null);

            var condition5 = m_container.TryResolveAll<T>(out var testI2);
            Console.WriteLine(@$"{typeof(T).Name} has implementation: {condition5}");
            result = condition5;
            if (!result) return false;
            
            //Debug.Assert(condition5);
            //Debug.Assert(testI2 is not null);
            
            return true;
        }
        
        public void Finish()
        {
            Console.WriteLine("--------------------");
            Console.WriteLine("Finishing Tests...");
            PrintContainerState();

            m_grandChildContainer?.Dispose();
            m_grandChildContainer = null;

            m_childContainer?.Dispose();
            m_childContainer = null;

            m_container?.Dispose();
            m_container = null;

            Console.WriteLine("--------------------");
        }

        public IoCContainerTests AsyncLifetimeTests()
        {
            var threadLimit = 10;

            Console.WriteLine("--------------------");
            Console.WriteLine("Starting Async Lifetime Tests...");

            for (var i = 0; i < threadLimit; i++)
            {
                var task = Task.Run(() => m_container.Resolve<IoCTestClassA>());
                var task2 = Task.Run(() => m_container.Resolve<IoCTestClassA>());
                task.ConfigureAwait(false);
                task2.ConfigureAwait(false);
                Task.WaitAll(task, task2);
                var testA = task.Result;
                var testA2 = task.Result;
                Console.WriteLine(@$"test A id: {testA.Id}");
                Console.WriteLine(@$"test2 A id: {testA2.Id}");
                Console.WriteLine(@$"test A id and test2 A id Equal: {testA.Id == testA2.Id}");
                Debug.Assert(testA.Id == testA2.Id);
                Console.WriteLine("--------------------");
            }

            var tasks = new ConcurrentList<Task>();
            for (var i = 0; i < threadLimit; i++)
            {
                Console.WriteLine(@$"Starting Task {i}");
                tasks.Add(Task.Run(() =>
                                   {
                                       var a = m_container.Resolve<IoCTestClassA>();
                                       Console.WriteLine(@$"test A id: {a.Id}");
                                       Console.WriteLine("--------------------");
                                   }));
                //task.ConfigureAwait(false);
                //task.Start();
                //tasks.Add(task);
            }

            Console.WriteLine("Waiting for tasks to complete...");
            Task.WaitAll(tasks.ToArray());
            //var testA = Task.Run(() => m_container.Resolve<IoCTestClassA>()).Result;
            //Console.WriteLine(@$"test A id: {testA.Id}");

            //var testA2 = Task.Run(() => m_container.Resolve<IoCTestClassA>()).Result;
            //Console.WriteLine(@$"test2 A id: {testA2.Id}");
            //Console.WriteLine(@$"test A id and test2 A id Equal: {testA.Id == testA2.Id}");
            //Debug.Assert(testA.Id == testA2.Id);
            Console.WriteLine("--------------------");

            //PrintLockState();
            //Console.WriteLine("--------------------");

            return this;
        }

        private void PrintContainerState()
        {
            Console.WriteLine("====================");
            Console.WriteLine("~~~~~~~~~~~~~~~~~~");
            Console.WriteLine($@"Root Container: {m_container.Root.Id}");
            Console.WriteLine("====================");

            //Console.WriteLine($@"Is Test Class A Registered: {m_container.IsTypeRegistered<IoCTestClassA>()}");
            //Console.WriteLine($@"Is Test Class B Registered: {m_container.IsTypeRegistered<IoCTestClassB>()}");
            var all = m_container.ResolveAll<IIoCTestClass>();
            Debug.Assert(all.Any());
            Console.WriteLine($@"All Tests Registered: {all.Count()}");
            foreach (var c in all)
            {
                Console.WriteLine(@$"Class Id: {c.Id}");
                Console.WriteLine(@$"Class Interface: {c.ContainsInterface<IIoCTestClass>()}");
            }

            Console.WriteLine("~~~~~~~~~~~~~~~~~~");
            Console.WriteLine(m_container.ToString());
            Console.WriteLine("====================");

            //foreach (var c in ((IParentDIContainer)m_container).ChildContainers)
            //{
            //    Console.WriteLine("~~~~~~~~~~~~~~~~~~");
            //    Console.WriteLine(@$">>>>Child Container: {c.Id}<<<<");
            //    Console.WriteLine("====================");

            //    var allChild = c.ResolveAll<IIoCTestClass>();
            //    Console.WriteLine($@"All Tests Registered: {allChild.Count()}");
            //    foreach (var cc in allChild)
            //    {
            //        Console.WriteLine(@$"Class Id: {c.Id}");
            //        Console.WriteLine(@$"Class Interface: {c.ContainsInterface<IIoCTestClass>()}");
            //    }

            //    Console.WriteLine("~~~~~~~~~~~~~~~~~~");
            //    Console.WriteLine(c.ToString());
            //    Console.WriteLine("====================");
            //}
        }
        #endregion

        #region Nested Types
        private abstract class IoCTestClass : IIoCTestClass
        {
            #region Properties
            public Guid Id { get; protected init; }
            #endregion

            protected IoCTestClass() { Id = Guid.NewGuid(); }
        }

        private class IoCTestClassA : IoCTestClass,
                                      IIoCTestInterface { }

        private class IoCTestClassB : IoCTestClass { }

        private class IoCTestClassC : IoCTestClass { }

        private class IoCTestClassD : IoCTestClass { }

        private class IoCTestClassE : IoCTestClass
        {
            #region Properties
            public IIoCTestInterface Interface { get; protected init; }
            #endregion

            public IoCTestClassE(IIoCTestInterface ioCTest) { Interface = ioCTest; }
        }

        private class IoCTestClassF : IoCTestClass
        {
            #region Properties
            public IoCTestClassE Class { get; protected init; }
            #endregion

            public IoCTestClassF(IoCTestClassE classE) { Class = classE; }
        }

        private class IoCTestClassG : IoCTestClass, IIoCTestInterface2 { }
        
        private interface IIoCTestClass
        {
            #region Properties
            Guid Id { get; }
            #endregion
        }

        private interface IIoCTestInterface : IIoCTestClass { }

        private interface IIoCTestInterfaceNoConcrete : IIoCTestClass { }

        private interface IIoCTestInterface2 : IIoCTestClass { }
        #endregion
    }
}