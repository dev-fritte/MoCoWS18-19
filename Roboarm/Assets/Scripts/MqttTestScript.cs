using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MqttTestScript : MonoBehaviour {

    private Mqtt mqtt;

    private float timeBetweenMessage = .1f;
    private float currentTime;
    
    private int messageCounter = 0;

	// Use this for initialization
	void Start () {
        mqtt = FindObjectOfType<Mqtt>();
        currentTime = Time.time;
	}
	
	// Update is called once per frame
	void Update () {
		if (Time.time - currentTime > timeBetweenMessage)
        {
            mqtt.Publish("Servo_0", (messageCounter++).ToString());
            currentTime = Time.time;
        }
	}
}
