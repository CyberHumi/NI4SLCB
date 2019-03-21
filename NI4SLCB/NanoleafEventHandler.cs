using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NI4SLCB {
    class NanoleafEventHandler {
        private static NanoleafEventHandler _Instance = null;
        private Queue eventQueue;
        private ManualResetEvent _event;
        private Thread t;
        private MainForm mainForm;
        private NanoleafDevice[] devices;
        private EffectChange[] alerts;
        private EffectChange[] chatCmds;

        private NanoleafEventHandler() {
            eventQueue = new Queue();
            _event = new ManualResetEvent(false);
            t = new Thread(new ThreadStart(QueueHandler)) {
                IsBackground = true
            };
            t.Start();
        }

        public static NanoleafEventHandler Instance {
            get {
                if (_Instance == null)
                    _Instance = new NanoleafEventHandler();
                return _Instance;
            }
        }

        public void SetNanoleafConfig(NanoleafDevice[] devices, EffectChange[] alerts, EffectChange[] chatCmds) {
            this.devices = devices;
            this.alerts = alerts;
            this.chatCmds = chatCmds;
        }

        public void SetMainForm(MainForm mainForm) {
            this.mainForm = mainForm;
        }

        public void AddSLCBEvent(JSONSLCBEvent SLCBEvent) {
            eventQueue.Enqueue(SLCBEvent);

            /* continue thread */
            _event.Set();
        }

        private void QueueHandler() {
            JSONSLCBEvent SLCBEvent;
            string eventname;
            EffectChange ec;
            Boolean[] devs;
            int[] currentBrightness;
            string[] currentEffect;
            Boolean useCurrentEffect;
            string newLightEffect;
            int duration;
            int newBrightness;
            Boolean IsActiveCmdBrightness;

            while (true) {
                if (eventQueue.Count > 0) {
                    SLCBEvent = (JSONSLCBEvent)eventQueue.Dequeue();
                    eventname = SLCBEvent.@event.Substring(SLCBEvent.@event.IndexOf("_") + 1);
                    ec = null;
                    duration = 0;
                    newBrightness = 50;
                    IsActiveCmdBrightness = false;

                    try {
                        /* (1) check paramter */
                        if (eventname.Equals("CHATCMD")) {
                            /* identify chat commnand */
                            try {
                                ec = chatCmds[(int)((EffectChange.ChatCmd)Enum.Parse(typeof(EffectChange.ChatCmd), SLCBEvent.EventData.command))];
                            } catch { /* no actions */ }
                        } else {
                            /* identify alert */
                            try {
                                ec = alerts[(int)((EffectChange.Type)Enum.Parse(typeof(EffectChange.Type), eventname))];
                            } catch { /* no actions */ }
                        }
                        if (ec == null && ec.GetEffectName().Length == 0)
                            continue;
                        duration = ec.GetDuration();
                        newBrightness = ec.GetBrightness();
                        if (eventname.Equals("CHATCMD") && SLCBEvent.EventData.brightness != -1) {
                            newBrightness = SLCBEvent.EventData.brightness;
                            IsActiveCmdBrightness = true;
                        }
                        newLightEffect = ec.GetEffectName();
                        if (eventname.Equals("CHATCMD") && SLCBEvent.EventData.command.Equals("MASTER")) {
                            string clf = SLCBEvent.EventData.effectName;
                            foreach (NanoleafDevice nd in devices) {
                                foreach (string lf in nd.GetEffectList()) {
                                    if (lf.ToLower().Equals(clf.ToLower())) {
                                        newLightEffect = lf;
                                        goto go;
                                    }
                                }
                            }
                            go: /* continue */;
                        }
                        if (newLightEffect.Length == 0)
                            continue;

                        devs = ec.GetDevices();
                        currentBrightness = new int[devs.Length];
                        currentEffect = new string[devs.Length];

                        /* (2) change effect and brightness */
                        for (int i = 0; i < devs.Length; i++) {
                            if (!devs[i] || devices[i] == null || devices[i].GetLocation().Length == 0 || devices[i].GetAuthToken().Length == 0)
                                continue;

                            /* set useCurrentEffect */
                            if (devices[i].GetDefaultEffect() == null)
                                useCurrentEffect = true;
                            else
                                useCurrentEffect = devices[i].GetDefaultEffect().Equals(EffectChange.CurrentEffect);

                            /* set currentEffect */
                            if (useCurrentEffect)
                                currentEffect[i] = Nanoleaf.GetCurrentEffect(devices[i]).Replace("\"", "");
                            else
                                currentEffect[i] = alerts[(int)EffectChange.Type.DEFAULT].GetEffectName();

                            /* change effect */
                            Nanoleaf.ChangeEffect(devices[i], newLightEffect);
                            mainForm.AddListViewEventsItem(DateTime.Now, "", "Device #" + (i + 1) + ", change effect to " + newLightEffect + " for " + duration + "s");

                            /* change brightness */
                            if (ec.IsActiveBrightness() || IsActiveCmdBrightness) {
                                if (alerts[(int)EffectChange.Type.DEFAULT].IsActiveBrightness())
                                    currentBrightness[i] = alerts[(int)EffectChange.Type.DEFAULT].GetBrightness();
                                else {
                                    dynamic data = JsonConvert.DeserializeObject<JSONNanoleafGetBrightness>(Nanoleaf.GetCurrentBrightness(devices[i]));
                                    currentBrightness[i] = ((JSONNanoleafGetBrightness)data).value;
                                }
                                Nanoleaf.ChangeBrightness(devices[i], newBrightness, duration);
                                mainForm.AddListViewEventsItem(DateTime.Now, "", "Device #" + (i + 1) + ", change brightness to " + newBrightness + "%" + " for " + duration + "s");
                            }
                        }

                        if (duration <= 0)
                            continue;

                        /* (3) wait */
                        Thread.Sleep(duration * 1000);

                        /* (4) change back effect and brightness */
                        for (int i = 0; i < devs.Length; i++) {
                            if (!devs[i] || devices[i] == null || devices[i].GetLocation().Length == 0 || devices[i].GetAuthToken().Length == 0)
                                continue;

                            /* change effect */
                            Nanoleaf.ChangeEffect(devices[i], currentEffect[i]);
                            mainForm.AddListViewEventsItem(DateTime.Now, "", "Device #" + (i + 1) + ", change effect to " + currentEffect[i]);

                            /* change brightness */
                            if (ec.IsActiveBrightness() || IsActiveCmdBrightness) {
                                Nanoleaf.ChangeBrightness(devices[i], currentBrightness[i]);
                                mainForm.AddListViewEventsItem(DateTime.Now, "", "Device #" + (i + 1) + ", change brightness to " + currentBrightness[i] + "%");
                            }
                        }
                    } catch (Exception e) {
                        mainForm.AddListViewEventsItem(DateTime.Now, eventname, e.ToString());
                    }
                } else {
                    /* pause thread */
                    _event.Reset();
                    _event.WaitOne();
                }
            }
        }
    }
}
