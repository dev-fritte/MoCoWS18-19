using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using uPLibrary.Networking.M2Mqtt.Messages;
using UnityEngine.UI;

public class MqttLinearMapping : MonoBehaviour {

    #region enums

    /// <summary>
    /// Three different send modis:
    ///     - default:      direct sending
    ///     - delay:        send the latest value after delay
    ///     - queuedDelay:  send all messages from delay interval
    /// </summary>
    private enum MqttSendModis { Default, Delay, QueuedDelay}

    #endregion enums

    #region constant values

    private const float MINVALUE = 0;
    private const float MAXVALUE = 1;

    #endregion constant values

    #region SerializedFields

    [SerializeField]
    private float Value;

    [SerializeField, Header("Mapping"), Tooltip("lower boarder for linear mapping")]
    private float minMappingValue = 0;

    [SerializeField, Tooltip("upper boarder for linear mapping")]
    private float maxMappingValue = 100;

    [SerializeField, Tooltip("changes min and max value")]
    private bool inverse = false;

    [SerializeField, Tooltip("UI Text which displays the current mapping value")]
    private Text mappedText;

    [SerializeField, Header("MQTT"), Tooltip("MQTT Topic to send the value\n If EMPTY topic = gameObject.name")]
    private string topic = "";

    [SerializeField, Tooltip("- default: direct sending\n- delay: send the latest value after delay\n- queuedDelay: send all messages from delay interval")]
    private MqttSendModis sendMode;

    [SerializeField, Tooltip("Not used for default mode")]
    private float delayTime = 0.1f;

    #endregion SerializedFields

    #region getter & setter
    // Getter and Setter functions
    public float value
    {
        get
        {
            return Value;
        }
        set
        {
            if (MINVALUE <= value && value <= MAXVALUE)
            {
                this.Value = value;
                SendMessage();
            }

        }
    }

    #endregion getter & setter

    //MQTT Script 
    private Mqtt mqtt;

    //save value for delay mode
    private float currentMessage = 0;

    //flag for delay mode
    private bool currentMessageChanged = false;

    //List which stores the messages for queuedDelay mode
    private List<float> queuedMessage;
    
    private float lastTimeStamp = 0;
    
    private void Start()
    {
        mqtt = FindObjectOfType<Mqtt>();
        queuedMessage = new List<float>();
        lastTimeStamp = Time.time;

        if (topic == "")
            topic = gameObject.name;
                
    }
    
    /// <summary>
    /// calculates the linear mapping and sends or saves the value depending on Sendmode
    /// </summary>
    private void SendMessage()
    {
        float factor = value;

        if (inverse)
            factor = MAXVALUE - value;

        float message = minMappingValue + (maxMappingValue - minMappingValue) * factor; // mapping
        
        switch (sendMode)
        {
            case MqttSendModis.Default:
                mqtt.Publish(topic, ((int)message).ToString()); //send message instant
                break;
            case MqttSendModis.Delay:
                currentMessage = message;   //save message for next send cycle
                currentMessageChanged = true;
                break;
            case MqttSendModis.QueuedDelay: 
                queuedMessage.Add(message); //add message to list of actions
                break;
            default:
                break;
        }

        //write value to ui text
        if (mappedText != null)
        {
            mappedText.text = ((int)message).ToString();
        }
    }

    /// <summary>
    /// Send messages with in delay or queuedDelay mode
    /// </summary>
    private void FixedUpdate() //called from unity in every physics cycle
    {
        if (Time.time - lastTimeStamp > delayTime) //check for delay time
        {
            if (MqttSendModis.Delay == sendMode && 
                currentMessageChanged)
            {
                mqtt.Publish(topic, ((int)currentMessage).ToString());
                currentMessageChanged = false;
            }
            else if (MqttSendModis.QueuedDelay == sendMode && 
                     queuedMessage.Count > 0)
            {
                foreach (float message in queuedMessage)
                {
                    mqtt.Publish(topic, ((int)message).ToString());
                }
                queuedMessage.Clear();
            }

            lastTimeStamp = Time.time;
        }
    }
}