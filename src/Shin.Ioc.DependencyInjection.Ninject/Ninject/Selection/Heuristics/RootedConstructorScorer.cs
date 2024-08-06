// -------------------------------------------------------------------------------------------------
// <copyright file="ChildKernelConstructorScorer.cs" company="Ninject Project Contributors">
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

using Ninject;
using Ninject.Activation;
using Ninject.Components;
using Ninject.Planning;
using Ninject.Planning.Targets;
using Ninject.Selection.Heuristics;

using Shin.IoC.DependencyInjection.Ninject.Activation;
using Shin.IoC.DependencyInjection.Ninject.Extensions;

namespace Shin.IoC.DependencyInjection.Ninject.Selection.Heuristics
{
    /// <summary>
    /// Scores constructors by either looking for the existence of an injection marker
    /// attribute, or by counting the number of parameters including those defined on parent kernels.
    /// </summary>
    public class RootedConstructorScorer : StandardConstructorScorer
    {
        /// <summary>
        /// Checkes whether a binding exists for a given target on the specified kernel.
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        /// <param name="context">The context.</param>
        /// <param name="target">The target.</param>
        /// <returns>Whether a binding exists for the target in the given context.</returns>
        protected override bool BindingExists(IKernel kernel, IContext context, ITarget target)
        {

            return base.BindingExists(kernel, context, target)
                || BindingExistsOnParentKernel(kernel, context, target)
                || BindingExistsOnChildKernel(kernel, context, target);
        }

        /// <summary>
        /// Checkes whether a binding exists for a given target on the parent of the specified kernel.
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        /// <param name="context">The context.</param>
        /// <param name="target">The target.</param>
        /// <returns>Whether a binding exists for the target in the given context.</returns>
        private bool BindingExistsOnParentKernel(IKernel kernel, IContext context, ITarget target)
        {
            if (kernel is IChildKernel childKernel
                && childKernel.Parent is IKernel parentKernel)
            {
                var c = new Context(kernel,
                                    context.Request.ToRootedRequest(childKernel, RootedResolutionStrategy.Default),
                                    context.Binding,
                                    context.Cache,
                                    kernel.Get<IPlanner>(),
                                    kernel.Get<IPipeline>(),
                                    kernel.Get<IExceptionFormatter>());
                return BindingExists(parentKernel, c, target);
            }

            return false;
        }

        private bool BindingExistsOnChildKernel(IKernel kernel,
                                                IContext context,
                                                ITarget target)
        {
            if (kernel is IParentKernel parentKernel)
            {
                foreach (var childKernel in parentKernel.Children)
                {
                    if (childKernel is IKernelWithId k
                        && context.Request is IRootedRequest r
                        && k.Id != r.RequestingKernelId
                        && BindingExists(k, context, target))
                        return true;
                }
            }

            return false;
        }
    }
}