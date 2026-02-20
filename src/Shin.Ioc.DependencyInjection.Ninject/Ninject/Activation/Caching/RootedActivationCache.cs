// -------------------------------------------------------------------------------------------------
// <copyright file="ChildActivationCache.cs" company="Ninject Project Contributors">
//   Copyright (c) 2010-2011 bbv Software Services AG.
//   Copyright (c) 2011-2017 Ninject Project Contributors. All rights reserved.
//
//   Dual-licensed under the Apache License, Version 2.0, and the Microsoft Public License (Ms-PL).
//   You may not use this file except in compliance with one of the Licenses.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//   or
//       http://www.microsoft.com/opensource/licenses.mspx
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
// </copyright>
// -------------------------------------------------------------------------------------------------

using Shin.IoC.DependencyInjection.Ninject;

using Shinject;
using Shinject.Activation.Caching;
using Shinject.Components;

namespace Shin.IoC.DependencyInjection.Shinject.Activation.Caching
{
    /// <summary>
    /// The activation cache of child kernels.
    /// </summary>
    public class RootedActivationCache : NinjectComponent, IActivationCache
    {
        /// <summary>
        /// The cache of the parent kernel.
        /// </summary>
        private readonly IActivationCache m_rootCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="RootedActivationCache"/> class.
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        public RootedActivationCache(IKernel kernel)
        {
            if (kernel is IRootedKernel r)
                m_rootCache = r.Root.Get<IKernel>().Components.Get<IActivationCache>();
        }

        /// <summary>
        /// Clears the cache.
        /// </summary>
        public void Clear()
        {
        }

        /// <summary>
        /// Adds an activated instance.
        /// </summary>
        /// <param name="instance">The instance to be added.</param>
        public void AddActivatedInstance(object instance)
        {
            m_rootCache.AddActivatedInstance(instance);
        }

        /// <summary>
        /// Adds an deactivated instance.
        /// </summary>
        /// <param name="instance">The instance to be added.</param>
        public void AddDeactivatedInstance(object instance)
        {
            m_rootCache.AddDeactivatedInstance(instance);
        }

        /// <summary>
        /// Determines whether the specified instance is activated.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <returns>
        ///     <c>true</c> if the specified instance is activated; otherwise, <c>false</c>.
        /// </returns>
        public bool IsActivated(object instance)
        {
            return m_rootCache.IsActivated(instance);
        }

        /// <summary>
        /// Determines whether the specified instance is deactivated.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <returns>
        ///     <c>true</c> if the specified instance is deactivated; otherwise, <c>false</c>.
        /// </returns>
        public bool IsDeactivated(object instance)
        {
            return m_rootCache.IsDeactivated(instance);
        }
    }
}