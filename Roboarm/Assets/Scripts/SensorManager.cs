using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SensorManager : MonoBehaviour {

    [SerializeField]
    private List<Slider> sliders;

    [SerializeField]
    private Image lowerAreaButton;

    [SerializeField]
    private Image upperAreaButton;

    public enum SensorControlMode
    {
        LowerPart, //control the two servos at the bottom
        UpperPart  //control the two servos at the top
    }

    private SensorControlMode controlMode = SensorControlMode.LowerPart;

    private Gyroscope gyro;

    private float timeToWait = .15f;
    private float savedTime = 0f;

    private Vector3 calibratedPosition;
    
    private void Awake()
    {
        gyro = Input.gyro;
        gyro.enabled = true;
        calibratedPosition = gyro.attitude.eulerAngles;
    }

    private void OnEnable()
    {
        foreach (Slider slider in sliders)
        {
            slider.interactable = false;
        }
        
        Awake();
    }

    private void OnDisable()
    {
        foreach(Slider slider in sliders)
        {
            slider.interactable = true;
        }
    }

    // Update is called once per frame
    private void Update () {
        if (Time.time > savedTime + timeToWait)
        {
            Quaternion currentMotion = gyro.attitude;

            float xDiff = Mathf.DeltaAngle(calibratedPosition.x, currentMotion.eulerAngles.x);
            float yDiff = Mathf.DeltaAngle(calibratedPosition.y, currentMotion.eulerAngles.y);

            if (xDiff > 50 || xDiff < -50)
                xDiff = 50f;
            if (yDiff > 50 || yDiff < -50)
                yDiff = 50f;

            if (xDiff < 15 && xDiff > -15)
                xDiff = 0;
            if (yDiff < 15 && yDiff > -15)
                yDiff = 0;
             

            xDiff /= 1800;
            yDiff /= 1800;
            /*

            xDiff = (Mathf.Round(xDiff * 10) / 10) / 10;
            yDiff = (Mathf.Round(yDiff * 10) / 10) / 10;

            if (xDiff < 2 && xDiff > -2) xDiff = 0;
            if (yDiff < 2 && yDiff > -2) yDiff = 0;*/

            SendValueToSlider(xDiff, yDiff);
            
            savedTime = Time.time;
        }
    }

    public void CalibrateAngles()
    {
        calibratedPosition = gyro.attitude.eulerAngles;
    }

    private void SetSensorControlMode(SensorControlMode mode)
    {
        this.controlMode = mode;

        CalibrateAngles();
    }

    public void SetSensorControlMode(bool lowerPart)
    {
        if (lowerPart)
        {
            SetSensorControlMode(SensorControlMode.LowerPart);
            lowerAreaButton.color = Color.black;
            upperAreaButton.color = Color.gray;
        }
        else
        {
            SetSensorControlMode(SensorControlMode.UpperPart);
            lowerAreaButton.color = Color.gray;
            upperAreaButton.color = Color.black;
        }
    }

    private void SendValueToSlider(float xDiff, float yDiff)
    {
        if(controlMode == SensorControlMode.LowerPart)
        {
            sliders[0].value = sliders[0].value + xDiff;
            sliders[1].value = sliders[1].value + yDiff;
        }
        else
        {
            sliders[2].value = sliders[2].value + yDiff;
            sliders[3].value = sliders[3].value + xDiff;
        }
    }
}
