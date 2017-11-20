using UnityEngine;
using System.Collections;

namespace Pamux.Lib.Utilities
{
    public class HideOnPlay : MonoBehaviour
    {
        void Start()
        {
            gameObject.SetActive(false);
        }
    }
}