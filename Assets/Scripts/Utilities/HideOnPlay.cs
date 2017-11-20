using UnityEngine;
using System.Collections;

namespace Pamux.Lib.Procedural.Utilities
{
    public class HideOnPlay : MonoBehaviour
    {
        void Start()
        {
            gameObject.SetActive(false);
        }
    }
}