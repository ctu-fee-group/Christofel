//
//   LifetimeHandlerExtensions.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Christofel.Plugins.Lifetime
{
    public static class LifetimeHandlerExtensions
    {
        public static bool MoveToIfLower(this LifetimeHandler lifetimeHandler, LifetimeState state)
        {
            var set = lifetimeHandler.Lifetime.State < state;

            if (set)
            {
                lifetimeHandler.MoveToState(state);
            }

            return set;
        }

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