using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace vpet
{
    public abstract class Manipulator : MonoBehaviour
    {
        public abstract void Init(AbstractParameter parameter);
    }
}