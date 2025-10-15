using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayerMenu : MonoBehaviour
{
    public EarthLayerSwitcher target;
    public void OnClickSetLayer(int index) { if (target) target.SetLayer(index); }
    public void CloseSelf() => Destroy(gameObject);
}
