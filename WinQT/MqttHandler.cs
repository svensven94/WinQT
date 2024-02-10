using MQTTnet;
using MQTTnet.Client.Options;
using MQTTnet.Extensions.ManagedClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinQT.IdleDetection;
using WinQT.Properties;

namespace WinQT
{
    class MqttHandler
    {
        System.Windows.Forms.Timer SendTimer;
        MqttFactory factory;
        IManagedMqttClient mqttClient;

        public MqttHandler() {
            SendTimer = new System.Windows.Forms.Timer();
            factory = new MqttFactory();
            mqttClient = factory.CreateManagedMqttClient();
        }
        public async Task ConnectAsync()
        {
            Console.WriteLine("### TRYING TO CONNECT ###");
            // Create a new MQTT client.
            var lwt = new MqttApplicationMessageBuilder()
                .WithTopic(Settings.Default.LwtTopic)
                .WithPayload("offline")
                //.WithTopic(Settings.Default.Topic)
                //.WithPayload("{\"online\":False}")
                .WithExactlyOnceQoS()
                .WithRetainFlag(true)
                .Build();

            // Create TCP based options using the builder.
            var options = new MqttClientOptionsBuilder()
                .WithClientId(Guid.NewGuid().ToString())
                .WithTcpServer(Settings.Default.ServerIp, Settings.Default.Port)
                .WithCleanSession()
                .WithWillMessage(lwt)
                .WithCredentials(Settings.Default.Username, Settings.Default.Password)
                .Build();

            var managedOptions = new ManagedMqttClientOptionsBuilder()
                .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
                .WithClientOptions(options)
                .Build();

            await mqttClient.StartAsync(managedOptions);

            mqttClient.UseConnectedHandler(async e =>
            {
                Console.WriteLine("### CONNECTED WITH SERVER ###");

                var connectedMessage = new MqttApplicationMessageBuilder()
                    .WithTopic(Settings.Default.LwtTopic)
                    .WithPayload("online")
                    .WithExactlyOnceQoS()
                    .WithRetainFlag(true)
                    .Build();
                await mqttClient.PublishAsync(connectedMessage, CancellationToken.None);
            });

            SendTimer.Interval = Settings.Default.MessageInterval * 1000;
            SendTimer.Tick += new EventHandler(SendTimer_Tick);
            SendTimer.Start();
        }

        public async Task SendMessage()
        {
            string payload = "{\"online\":\"True\",\"idle\":" + "\"" + CheckIdle() + "\"" + ",\"idletime\":" + LastInput.GetIdleTime() / 1000 + "}";
            var message = new MqttApplicationMessageBuilder()
                .WithTopic(Settings.Default.Topic)
                .WithPayload(payload)
                .WithExactlyOnceQoS()
                .WithRetainFlag(false)
                .Build();

            Console.WriteLine("### SENDING MESSAGE ###"  + "\nPayload: " + payload);
            await mqttClient.PublishAsync(message, CancellationToken.None);
        }

        public async Task SendDiscoveryMessages()
        {
            
        }

        public async Task DiscoveryMessageOnline()
        {
            string deviceNameFormatted = Settings.Default.DeviceName.Replace(" ", "_").ToLower();
            string payload = "{\"name\":\"Online\",\"unique_id\":\"" + deviceNameFormatted + "_online\",\"device\":{\"identifiers\":[\"" + deviceNameFormatted + "\"],\"name\":\"" + Settings.Default.DeviceName + "\"},\"state_topic\":\"" + Settings.Default.Topic + "\",\"payload_on\":\"True\",\"payload_off\":\"False\",\"value_template\":\"{{value_json.online}}\",\"availability\":[{\"topic\":\"" + Settings.Default.LwtTopic + "\"}],\"payload_available\":\"online\",\"payload_not_available\":\"offline\"}";
            var message = new MqttApplicationMessageBuilder()
                .WithTopic(Settings.Default.DiscoveryTopic + "/binary_sensor/" + deviceNameFormatted + "_online/config")
                .WithPayload(payload)
                .WithExactlyOnceQoS()
                .WithRetainFlag(false)
                .Build();

            Console.WriteLine("### SENDING DISCOVERY MESSAGE ###" + "\nPayload: " + payload);
            await mqttClient.PublishAsync(message, CancellationToken.None);
        }

        public async Task DiscoveryMessageIdle()
        {
            string deviceNameFormatted = Settings.Default.DeviceName.Replace(" ", "_").ToLower();
            string payload = "{\"name\":\"Idle\",\"unique_id\":\"" + deviceNameFormatted + "_idle\",\"device\":{\"identifiers\":[\"" + deviceNameFormatted + "\"],\"name\":\"" + Settings.Default.DeviceName + "\"},\"state_topic\":\"" + Settings.Default.Topic + "\",\"payload_on\":\"False\",\"payload_off\":\"True\",\"value_template\":\"{{value_json.idle}}\",\"availability\":[{\"topic\":\"" + Settings.Default.LwtTopic + "\"}],\"payload_available\":\"online\",\"payload_not_available\":\"offline\"}";
            var message = new MqttApplicationMessageBuilder()
                .WithTopic(Settings.Default.DiscoveryTopic + "/binary_sensor/" + deviceNameFormatted + "_idle/config")
                .WithPayload(payload)
                .WithExactlyOnceQoS()
                .WithRetainFlag(false)
                .Build();

            Console.WriteLine("### SENDING DISCOVERY MESSAGE ###" + "\nPayload: " + payload);
            await mqttClient.PublishAsync(message, CancellationToken.None);
        }

        public async Task DiscoveryMessageIdleTime()
        {
            string deviceNameFormatted = Settings.Default.DeviceName.Replace(" ", "_").ToLower();
            string payload = "{\"name\":\"Idletime\",\"unique_id\":\"" + deviceNameFormatted + "\",\"device\":{\"identifiers\":[\"" + deviceNameFormatted + "\"],\"name\":\"" + Settings.Default.DeviceName + "\"},\"state_topic\":\"computer/sven\",\"unit_of_measurement\":\"s\",\"value_template\":\"{{value_json.idletime}}\",\"availability\":[{\"topic\":\"" + Settings.Default.LwtTopic + "\"}],\"payload_available\":\"online\",\"payload_not_available\":\"offline\"}";
            var message = new MqttApplicationMessageBuilder()
                .WithTopic(Settings.Default.DiscoveryTopic + "/sensor/" + deviceNameFormatted + "idletime/config")
                .WithPayload(payload)
                .WithExactlyOnceQoS()
                .WithRetainFlag(false)
                .Build();

            Console.WriteLine("### SENDING DISCOVERY MESSAGE ###" + "\nPayload: " + payload);
            await mqttClient.PublishAsync(message, CancellationToken.None);
        }

        public bool CheckIdle()
        {
            if(!CheckFullscreen.IsForegroundFullScreen())
            {
                if(LastInput.GetIdleTime() >= (Settings.Default.IdleTimeout * 60000))
                {
                    return true;
                }
            }
            return false;
        }

        private void SendTimer_Tick(object sender, EventArgs e)
        {
            {
                lock (SendTimer)
                {
                    /* only work when this is no reentry while we are already working */
                    if (this.SendTimer.Enabled)
                    {
                        this.SendTimer.Stop();
                        this.DiscoveryMessageOnline();
                        this.DiscoveryMessageIdle();
                        this.DiscoveryMessageIdleTime();
                        this.SendMessage();
                        this.SendTimer.Start(); /* optionally restart for periodic work */
                    }
                }
            }
        }
    }
}
