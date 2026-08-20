using UnityEngine;
using UnityEngine.UI;

public class InteractionSlider : MonoBehaviour
{
    [SerializeField] Slider slider;
    public float waitTimer;
    public float timer;
    void Start(){
        slider.maxValue = waitTimer;
        slider.value = 0;
    }
    public void SetSliderValue(){
        slider.value = timer;
    }
}
