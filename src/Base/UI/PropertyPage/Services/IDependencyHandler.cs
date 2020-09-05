﻿using Xarial.XCad.UI.PropertyPage.Base;

namespace Xarial.XCad.UI.PropertyPage.Services
{
    /// <summary>
    /// Handling the dynamic control dependencies
    /// </summary>
    /// <remarks>This is asigned via <see cref="Attributes.DependentOnAttribute"/></remarks>
    public interface IDependencyHandler
    {
        /// <summary>
        /// Invokes when any of the dependencies controls changed
        /// </summary>
        /// <param name="source">This control to update state on</param>
        /// <param name="dependencies">List of dependencies controls</param>
        void UpdateState(IControl source, IControl[] dependencies);
    }
}
