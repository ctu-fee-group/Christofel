//
//   LifetimeHandlerExtensions.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Christofel.Plugins.Lifetime
{
    /// <summary>
    /// Defines extension methods for the type <see cref="LifetimeHandler"/>.
    /// </summary>
    public static class LifetimeHandlerExtensions
    {
        /// <summary>
        /// Moves to specified state if the current state is lower than that.
        /// </summary>
        /// <param name="lifetimeHandler">The lifetime handler to be moved to the state.</param>
        /// <param name="state">State the lifetime should be moved to, if condition is met.</param>
        /// <returns>Whether the state was changed.</returns>
        public static bool MoveToIfLower(this LifetimeHandler lifetimeHandler, LifetimeState state)
        {
            var set = lifetimeHandler.Lifetime.State < state;

            if (set)
            {
                lifetimeHandler.MoveToState(state);
            }

            return set;
        }

        /// <summary>
        /// Moves to specified state if the current state is exactly the previous one.
        /// </summary>
        /// <param name="lifetimeHandler">The lifetime handler to be moved to the state.</param>
        /// <param name="state">State the lifetime should be moved to, if condition is met.</param>
        /// <returns>Whether the state was changed.</returns>
        public static bool MoveToIfPrevious(this LifetimeHandler lifetimeHandler, LifetimeState state)
        {
            var set = lifetimeHandler.Lifetime.State < state;

            if (set)
            {
                lifetimeHandler.MoveToState(state);
            }

            return set;
        }
    }
}