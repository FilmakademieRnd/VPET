using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace vpet
{
    public abstract class Manipulator : MonoBehaviour
    {
        public abstract void LinkToParameter(AbstractParameter abstractParam);
    }
}