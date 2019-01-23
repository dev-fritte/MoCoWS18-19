using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {

    [SerializeField, Tooltip("GameObject of the view to connect to the broker")]
    private GameObject connectionView;

    [SerializeField, Tooltip("GameObject which automatically enables after connecting to the broker")]
    private GameObject productiveView;

    [SerializeField, Tooltip("Text which displays the current control mode")]
    private Text selectedModeText;

    // Use this for initialization
    void Start () {
        Mqtt.OnClientConnected.AddListener(OpenProductiveView);
	}
	
    /// <summary>
    /// Change UI from connection view to productive view
    /// </summary>
    private void OpenProductiveView()
    {
        Debug.Log("Change to productive View");

        if (connectionView != null && productiveView != null)
        {
            connectionView.SetActive(false);
            productiveView.SetActive(true);
        }
        else
            Debug.Log("Nicht zugewiesene Views!!");
    }

    /// <summary>
    /// Enables or disables GameObject of second control mode
    /// </summary>
    /// <param name="view"></param>
    public void SwitchMqttInputMode(GameObject view)
    {
        if (view.activeSelf == true)
        {
            view.SetActive(false);
            if (selectedModeText != null)
                selectedModeText.text = "Slider Mode";
        }
        else
        {
            view.SetActive(true);
            if (selectedModeText != null)
                selectedModeText.text = "Sensor Mode";
        }
    }
}
