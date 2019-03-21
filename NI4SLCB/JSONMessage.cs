using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NI4SLCB {

    /* StreamLabs Chatbot */

    [Serializable]
    public class JSONAuthMessage {
        public string author { get; set; }
        public string website { get; set; }
        public string api_key { get; set; }
        public string[] events { get; set; }
    }

    [Serializable]
    public class JSONMessage {
        public string @event { get; set; }
        public JSONData data { get; set; }
    }

    [Serializable]
    public class JSONData {
        public string message { get; set; }
    }

    [Serializable]
    public class JSONSLCBEvent {
        public string command { get; set; }
        public string effectName { get; set; }
        public int brightness { get; set; }
        public string name { get; set; }
        public string display_name { get; set; }
        public string @event { get; set; }
        public string data { get; set; }
        public JSONSLCBEventData EventData { get; set; }
        public DateTime Date { get; set; }
    }

    [Serializable]
    public class JSONSLCBEventData {
        public string command { get; set; }
        public string effectName { get; set; }
        public int brightness { get; set; }
        public string userId { get; set; }
        public string name { get; set; }
        public string display_name { get; set; }
        public string tier { get; set; }
        public Boolean is_resub { get; set; }
        public int months { get; set; }
        public string message { get; set; }
        public string gift_target { get; set; }
        public int bits { get; set; }
        public int total_bits { get; set; }
        public int viewers { get; set; }
        public string amount { get; set; }
        public string currency { get; set; }
    }

    /* Nanoleaf */

    [Serializable]
    public class JSONNanoleaf {
        public string author { get; set; }
    }

    [Serializable]
    public class JSONNanoleafAuthToken {
        public string auth_token { get; set; }
    }

    [Serializable]
    public class JSONNanoleafState {
        public Boolean value { get; set; }
    }

    [Serializable]
    public class JSONNanoleafGetBrightness {
        public int value { get; set; }
        public int max { get; set; }
        public int min { get; set; }
    }
}
