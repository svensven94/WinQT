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
                .WithPayload("Offline")
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
                    .WithPayload("Online")
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
            var message = new MqttApplicationMessageBuilder()
                .WithTopic(Settings.Default.Topic)
                .WithPayload("{\"online\":True,\"idle\":" + CheckIdle() + ",\"idletime\":" + LastInput.GetIdleTime() + "}")
                .WithExactlyOnceQoS()
                .WithRetainFlag(false)
                .Build();

            Console.WriteLine("### SENDING MESSAGE ###");
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
                        this.SendMessage();
                        this.SendTimer.Start(); /* optionally restart for periodic work */
                    }
                }
            }
        }
    }
}
