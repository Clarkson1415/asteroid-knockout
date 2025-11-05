using Godot;
using System;

namespace cakegame1idk.scripts.GameObjects
{
    public interface IPoolable
    {
        event Action<IPoolable> OnDestroyed;

        void OnMadeVisibleAgain();

        /// <summary>
        /// When removed from scene put back into pool.
        /// </summary>
        void OnAddedToPool();
    }
}
