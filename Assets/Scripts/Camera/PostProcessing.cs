using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;

public class PostProcessing : MonoBehaviour
{
    private PostProcessVolume postProcess;
    private Bloom bloom;
    [SerializeField] private float maxBloom;
    [SerializeField] private Slider slider;
    private void Start()
    {
        postProcess = GetComponent<PostProcessVolume>();
        postProcess.profile.TryGetSettings(out bloom);
        bloom.active = true;
        maxBloom = bloom.intensity.value;
        if(!PlayerPrefs.HasKey("bloomValue")) PlayerPrefs.SetFloat("bloomValue", 1);
        if (slider)
        {
            slider.value = PlayerPrefs.GetFloat("bloomValue");
        }
        bloom.intensity.value = PlayerPrefs.GetFloat("bloomValue") * maxBloom;
    }
    public void Graphics()
    {
        bloom.intensity.value = slider.value*maxBloom;
        PlayerPrefs.SetFloat("bloomValue", slider.value);
    }
}
