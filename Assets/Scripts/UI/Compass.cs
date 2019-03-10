using UnityEngine;
using UnityEngine.UI;

public class Compass : MonoBehaviour
{
    public Slider slider;

    public void OnGUI()
    {
        transform.GetChild(0).localEulerAngles = new Vector3(0, 0, -slider.value);
    }
}
