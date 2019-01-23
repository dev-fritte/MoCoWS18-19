using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

public class Mqtt : MonoBehaviour
{
    private MqttClient client;
     
    // The connection informatio n
    [SerializeField]
    private string brokerHostname = "141.37.169.17";
    [SerializeField]
    private int brokerPort = 1883;
    [SerializeField]
    private string userName = "";
    [SerializeField]
    private string password = "";
    [SerializeField]
    private TextAsset certificate;

    // listen on all the Topic
    [SerializeField]
    private static string subTopic = "patho/moco/";

    private static List<Action<object, MqttMsgPublishEventArgs>> receives = new List<Action<object, MqttMsgPublishEventArgs>>();

    public static UnityEvent OnClientConnected = new UnityEvent();
    
    // Use this for initialization
    public void Init()
    {
        if (brokerHostname != null && userName != null && password != null)
        {
            Debug.Log("Connecting to " + brokerHostname + ":" + brokerPort);
            Connect();

            client.MqttMsgPublishReceived += Client_MqttMsgPublishReceived;
            byte[] qosLevels = { MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE };
            client.Subscribe(new string[] { subTopic + "#" }, qosLevels);
        }

        DontDestroyOnLoad(this);
    }

    // Update is called once per frame
    private void Connect()
    {
        // Forming a certificate based on a TextAsset
        //X509Certificate cert = new X509Certificate();
        //cert.Import(certificate.bytes);
        //Debug.Log("Using the certificate '" + cert + "'");
        //client = new MqttClient(brokerHostname);
        //IPAddress ip = new IPAddress()
        //client = new MqttClient()
        client = new MqttClient(brokerHostname, brokerPort, false, null, null, MqttSslProtocols.TLSv1_0, MyRemoteCertificateValidationCallback);
        string clientId = Guid.NewGuid().ToString();
        //Debug.Log("About to connect using '" + userName + "' / '" + password + "'");
        try
        {
            client.Connect(clientId, userName, password);
        }
        catch (Exception e)
        {
            Debug.LogError("Connection error: " + e);
            return;
        }

        Debug.Log("Connected;");
        OnClientConnected.Invoke();
    }

    public static bool MyRemoteCertificateValidationCallback(System.Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
    {
        return true;
    }

    private void Client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
    {
        //string msg = Encoding.UTF8.GetString(e.Message);

        foreach(Action<object, MqttMsgPublishEventArgs> act in receives)
        {
            act.Invoke(sender, e);
        }
    }
    
    public void Publish(string _topic, string msg)
    {
        if (client == null)
            return;

        client.Publish(
            subTopic + _topic, Encoding.UTF8.GetBytes(msg),
            MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
        DateTime dateTime = DateTime.Now;

        string time = dateTime.Hour + ":" + dateTime.Minute + ":" + dateTime.Second + "." + dateTime.Millisecond;

        Debug.Log(time + "-> publish message: <" + msg + "> on topic: " + _topic);
    }

    public void RegisterOnReceive(Action<object, MqttMsgPublishEventArgs> function)
    {
        receives.Add(function);
    }
}

