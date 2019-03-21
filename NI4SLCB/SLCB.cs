using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NI4SLCB {
    class SLCB {
        private ClientWebSocket webSocket = null;
        private CancellationTokenSource cts;
        private string url;
        private string apiKey;
        private MainForm mainForm;
        private Task t;
        private Boolean debug = false;

        public static string[] events = {
            "EVENT_SUB",            // Twitch sub
            "EVENT_MX_SUB",         // Mixer sub
            "EVENT_YT_SUB",         // YouTube sub
            "EVENT_FOLLOW",         // Twitch follow
            "EVENT_MX_FOLLOW",      // Mixer follow
            "EVENT_HOST",           // Twitch host
            "EVENT_MX_HOST",        // Mixer host
            "EVENT_RAID",           // Twitch raid
            "EVENT_DONATION",       // Mixer/Twitch/YouTube donation
            "EVENT_CHEER",          // Twitch cheer
            "EVENT_CHATCMD"         //  chat command
        };

        public SLCB(MainForm mainForm) {
            this.mainForm = mainForm;
            ReadApiKey();
        }

        public async Task ConnectAsync() {
            webSocket = new ClientWebSocket();
            try {
                await webSocket.ConnectAsync(new Uri(url), CancellationToken.None);
                var encoded = Encoding.UTF8.GetBytes(GetJsonAuth());
                var buffer = new ArraySegment<Byte>(encoded, 0, encoded.Length);
                try {
                    await webSocket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
                    cts = new CancellationTokenSource();
                    t = WebSocketRequestHandler(cts.Token);
                } catch (Exception ex) {
                    if (ex.GetType().Equals(typeof(InvalidOperationException)))
                        MainForm.ShowAlert(ex.GetType() + "\n\n" + ex.Message, "Cannot start \"SLCB\" task");
                    else
                        MainForm.ShowAlert(ex.Message, "Wrong API Key");
                    return;
                }
            } catch (Exception ex) {
                MainForm.ShowAlert("WebSocket connection to Streamlabs Chatbot failed.\n\n" + ex.Message, "Connection error");
                return;
            }
        }

        public async Task DisconnectAsync() {
            if (cts != null)
                cts.Cancel();
            mainForm.SetDisconnctStatus();
            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Done", CancellationToken.None);
        }

        public Boolean IsConnected() {
            return webSocket != null && webSocket.State == WebSocketState.Open;
        }

        public Boolean IsAborted() {
            return webSocket != null && webSocket.State == WebSocketState.Aborted;
        }

        private string GetJsonAuth() {
            JSONAuthMessage jsonAuth = new JSONAuthMessage {
                author = "CyberHumi",
                website = "https://www.twitch.tv/CyberHumi",
                api_key = apiKey,
                events = events
            };
            return JsonConvert.SerializeObject(jsonAuth);
        }

        //Asynchronous request handler. 
        public async Task WebSocketRequestHandler(CancellationToken t) {
            t.ThrowIfCancellationRequested();

            /*We define a certain constant which will represent 
            size of received data. It is established by us and  
            we can set any value. We know that in this case the size of the sent 
            data is very small. 
            */
            const int maxMessageSize = 1024;

            //Buffer for received bits. 
            var receivedDataBuffer = new ArraySegment<Byte>(new Byte[maxMessageSize]);

            //Checks WebSocket state. 
            while (webSocket.State == WebSocketState.Open) {
                receivedDataBuffer = new ArraySegment<Byte>(new Byte[maxMessageSize]);
                //Reads data. 
                WebSocketReceiveResult webSocketReceiveResult = await webSocket.ReceiveAsync(receivedDataBuffer, t);

                //If input frame is cancelation frame, send close command. 
                if (webSocketReceiveResult.MessageType == WebSocketMessageType.Close) {
                    //await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, String.Empty, cancellationToken);
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, String.Empty, t);
                } else {
                    byte[] request = receivedDataBuffer.Array.Where(b => b != 0).ToArray();

                    //Because we know that is a string, we convert it. 
                    string receiveString = System.Text.Encoding.UTF8.GetString(request, 0, request.Length);
                    if (debug)
                        MessageBox.Show(receiveString, "Message", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    try {
                        /* *** SLCB Message *** */
                        dynamic json = JsonConvert.DeserializeObject<JSONSLCBEvent>(receiveString);
                        if (debug)
                            MessageBox.Show("[" + json.@event + "]\n\n[" + json.data + "]", "Event", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        dynamic data = JsonConvert.DeserializeObject<JSONSLCBEventData>(json.data);
                        json.EventData = data;
                        json.Date = DateTime.Now;

                        /* print json object */
                        if (debug) {
                            MessageBox.Show("[" + JsonConvert.SerializeObject(json, Formatting.Indented) + "]", "Message Event", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            MessageBox.Show("[" + JsonConvert.SerializeObject(data, Formatting.Indented) + "]", "Message Data", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }

                        /* add item to listView */
                        NanoleafEventHandler.Instance.AddSLCBEvent((JSONSLCBEvent)json);
                        mainForm.AddListViewEventsItem((JSONSLCBEvent)json);
                        if (debug)
                            MessageBox.Show("[fertig]", "Add item", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    } catch (Exception ee) {
                        try {
                            if (debug)
                                MessageBox.Show("[" + ee.Message + "]\n\n" + ee.Source, "Message ee", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            /* *** Connect *** */
                            dynamic json = JsonConvert.DeserializeObject<JSONMessage>(receiveString);
                            if (debug)
                                MessageBox.Show("[" + json.@event + "]", "Message Event", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            /* print json object */
                            if (debug)
                                MessageBox.Show("[" + JsonConvert.SerializeObject(json, Formatting.Indented) + "]", "Connect Message", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            if (string.Compare(json.@event, "EVENT_CONNECTED") == 0)
                                mainForm.SetConnctStatus();
                        } catch (Exception ee2) {
                            MainForm.ShowAlert(ee2.Message, "JSON import error");
                        }
                    }
                }
            }
            mainForm.SetDisconnctStatus();
            MainForm.ShowAlert("Websocket task has ended.", "Message");
        }

        // read file: API_Key.js
        private void ReadApiKey() {
            try {
                using (StreamReader sr = new StreamReader("API_Key.js")) {
                    string[] parts = sr.ReadToEnd().Split(';');
                    for (int i = 0; i < parts.Length; i++) {
                        if (parts[i].Contains("API_Key"))
                            apiKey = parts[i].Split('"')[1];
                        if (parts[i].Contains("API_Socket"))
                            url = parts[i].Split('"')[1];
                    }
                }
                mainForm.UpdateTabpage21_websocket(url, apiKey);
            } catch (Exception e) {
                MainForm.ShowAlert(e.Message, "Cannot read API Key");
            }
        }

    }
}

