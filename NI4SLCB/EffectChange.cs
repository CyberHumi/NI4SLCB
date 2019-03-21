using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NI4SLCB {
    [Serializable]
    class EffectChange {
        public enum Type { DEFAULT, FOLLOW, SUB, DONATION, CHEER, HOST, RAID }
        public enum ChatCmd { MASTER, CMD1, CMD2, CMD3, CMD4, CMD5, CMD6, CMD7, CMD8, CMD9, CMD10 }
        public const string CurrentEffect = "--- current light effect ---";

        private string EffectName;
        private Boolean ActiveBrightness;
        private int Brightness;
        private Boolean AllowBrightnessParameter;
        private Boolean ActiveDuration;
        private int Duration;
        private Boolean[] Devices;

        public EffectChange() {
            Devices = new Boolean[4];
        }
        public EffectChange(string EffectName, Boolean ActiveBrightness, int Brightness, Boolean AllowBrightnessParameter, Boolean ActiveDuration, int Duration, Boolean[] Devices) {
            this.EffectName = EffectName;
            this.ActiveBrightness = ActiveBrightness;
            this.Brightness = Brightness;
            this.AllowBrightnessParameter = AllowBrightnessParameter;
            this.ActiveDuration = ActiveDuration;
            this.Duration = Duration;
            this.Devices = Devices;
        }
        public EffectChange(string EffectName, Boolean ActiveBrightness, int Brightness, Boolean ActiveDuration, int Duration, Boolean[] Devices)
            : this(EffectName, ActiveBrightness, Brightness, false, ActiveDuration, Duration, Devices) { 
        }

        public void SetEffectName(string EffectName) {
            this.EffectName = EffectName;
        }
        public void SetActiveBrightness(Boolean ActiveBrightness) {
            this.ActiveBrightness = ActiveBrightness;
        }
        public void SetBrightness(int Brightness) {
            this.Brightness = Brightness;
        }
        public void SetAllowBrightnessParameter(Boolean AllowBrightnessParameter) {
            this.AllowBrightnessParameter = AllowBrightnessParameter;
        }
        public void SetActiveDuration(Boolean ActiveDuration) {
            this.ActiveDuration = ActiveDuration;
        }
        public void SetDuration(int Duration) {
            this.Duration = Duration;
        }
        public void SetDevices(Boolean[] Devices) {
            this.Devices = Devices;
        }
        public void SetDevice(int Device, Boolean isSet) {
            this.Devices[Device] = isSet;
        }

        public string GetEffectName() {
            return EffectName;
        }
        public Boolean IsActiveBrightness() {
            return ActiveBrightness;
        }
        public int GetBrightness() {
            return Brightness;
        }
        public Boolean IsAllowBrightnessParameter() {
            return AllowBrightnessParameter;
        }
        public Boolean IsActiveDuration() {
            return ActiveDuration;
        }
        public int GetDuration() {
            return Duration;
        }
        public Boolean[] GetDevices() {
            return Devices;
        }
        public Boolean GetDevice(int Device) {
            return Devices[Device];
        }
    }
}
