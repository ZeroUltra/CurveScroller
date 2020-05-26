using UnityEngine;

namespace ZeroUltra.CurveScroller
{
    /// <summary>
    /// Component 
    /// </summary>
    public abstract class CurveScrollerItemComponent : MonoBehaviour
    {
        public abstract void SetData(CurveScrollerItemData t);
    }
}
