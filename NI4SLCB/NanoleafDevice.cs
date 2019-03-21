using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NI4SLCB {
    [Serializable]
    class NanoleafDevice {
        private string location;
        private string authToken;
        private string[] effectList;
        private string defaultEffect;
        private int defaultBrightness;
        private int defaultColorTemperature;

        public NanoleafDevice() : this("","") {
        }
        public NanoleafDevice(string location, string authToken) {
            this.location = location;
            this.authToken = authToken;
        }

        public void SetLocation(string location) {
            this.location = location;
        }
        public void SetAuthToken(string authToken) {
            this.authToken = authToken;
        }
        public void SetEffectList(string[] effectList) {
            this.effectList = effectList;
        }
        public void SetDefaultEffect(string defaultEffect) {
            this.defaultEffect = defaultEffect;
        }
        public void SetDefaultBrightness(int defaultBrightness) {
            this.defaultBrightness = defaultBrightness;
        }
        public void SetDefaultColorTemperature(int defaultColorTemperature) {
            this.defaultColorTemperature = defaultColorTemperature;
        }

        public string GetLocation() {
            return location;
        }
        public string GetAuthToken() {
            return authToken;
        }
        public string[] GetEffectList() {
            return effectList;
        }
        public string GetDefaultEffect() {
            return defaultEffect;
        }
        public int GetDefaultBrightness() {
            return defaultBrightness;
        }
        public int GetDefaultColorTemperature() {
            return defaultColorTemperature;
        }
    }
}
