using System;
using UnityEngine;
using UnityEngine.UI;

public class SensitivitySettings : MonoBehaviour
{

    private Slider sensitivitySlider;
    private PlayerController playerController = null;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnEnable()
    {
        if (!PlayerConnection.Local) return;
        
        sensitivitySlider = transform.Find("SensitivitySlider").GetComponent<Slider>();
        if (playerController == null)
        {
            playerController = GameObject.FindAnyObjectByType<PlayerController>();    
        }
        sensitivitySlider.value = playerController.cameraSensitivity;
        sensitivitySlider.onValueChanged.AddListener(ChangeSensitivity);
        //controller = GameObject.Find("");
    }

    private void ChangeSensitivity(float newValue)
    {
        if (playerController == null) return;
        playerController.cameraSensitivity = newValue;
    }
}
