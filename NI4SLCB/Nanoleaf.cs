using Newtonsoft.Json;
using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace NI4SLCB {
    class Nanoleaf {
        public Nanoleaf() {
        }

        public static string GenerateToken(string location) {
            string link = location + "/api/v1/new";

            WebRequest request = null;
            WebResponse response = null;
            try {
                request = WebRequest.Create(link);
                request.Method = "POST";
            } catch (Exception e) {
                MainForm.ShowAlert(e.Message, "Error");
                return null;
            }
            try {
                response = request.GetResponse();
            } catch (Exception e) {
                MainForm.ShowAlert(e.Message + "\n\n" + link + "\n\nPress and hold the power button for 5-7 seconds first!\n(Light will begin flashing)", "Error 403");
                return null;
            }
            StreamReader reader = new StreamReader(response.GetResponseStream());
            try {
                dynamic json = JsonConvert.DeserializeObject<JSONNanoleafAuthToken>(reader.ReadToEnd());
                try {
                    return json.auth_token;
                } catch (Exception) {
                }
            } catch (Exception e) {
                MainForm.ShowAlert(e.Message, "JSON import error");
            }
            response.Close();
            return null;
        }

        private static string NanoleafRequest(string link) {
            const string method = "GET";
            WebRequest request = null;
            WebResponse response = null;
            try {
                request = WebRequest.Create(link);
                request.Method = method;
            } catch (Exception e) {
                MainForm.ShowAlert(e.Message, "Error");
                return null;
            }
            try {
                response = request.GetResponse();
            } catch (Exception e) {
                MainForm.ShowAlert(e.Message, "Request Error");
                return null;
            }
            StreamReader reader = new StreamReader(response.GetResponseStream());
            string result = reader.ReadToEnd();
            response.Close();
            return result;
        }

        private static Boolean NanoleafRequest(string method, string link, string requestData) {
            WebRequest request = null;
            WebResponse response = null;
            try {
                /* see https://docs.microsoft.com/de-de/dotnet/framework/network-programming/how-to-send-data-using-the-webrequest-class */
                byte[] byteArray = Encoding.UTF8.GetBytes(requestData);
                request = WebRequest.Create(link);
                request.Method = method;
                request.ContentLength = byteArray.Length;
                Stream dataStream = request.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();
            } catch (Exception e) {
                MainForm.ShowAlert(e.Message, "Error");
                return false;
            }
            try {
                response = request.GetResponse();
            } catch (Exception e) {
                MainForm.ShowAlert("Link:\r\n" + link + "\r\n\r\nData:\r\n" + requestData  + "\r\n\r\n" + e.Message, "Request Error");
                return false;
            }
            StreamReader reader = new StreamReader(response.GetResponseStream());
            return true;
        }

        public static string[] GetEffectList(NanoleafDevice device) {
            string link = device.GetLocation() + "/api/v1/" + device.GetAuthToken() + "/effects/effectsList";
            try {
                string[] list = NanoleafRequest(link).Replace("[", "").Replace("]", "").Replace("\"", "").Split(',');
                return list;
            } catch {
            }
            return null;
        }

        public static string GetState(NanoleafDevice device) {
            string link = device.GetLocation() + "/api/v1/" + device.GetAuthToken() + "/state/on";
            return NanoleafRequest(link);
        }

        public static string GetCurrentEffect(NanoleafDevice device) {
            string link = device.GetLocation() + "/api/v1/" + device.GetAuthToken() + "/effects/select";
            return NanoleafRequest(link);
        }

        public static string GetCurrentBrightness(NanoleafDevice device) {
            string link = device.GetLocation() + "/api/v1/" + device.GetAuthToken() + "/state/brightness";
            return NanoleafRequest(link);
        }

        public static Boolean ChangeState(NanoleafDevice device, Boolean on) {
            string link = device.GetLocation() + "/api/v1/" + device.GetAuthToken() + "/state";
            string request = "{ \"on\": {\"value\": " + on.ToString().ToLower() + "} }";
            return NanoleafRequest("PUT", link, request);
        }

        public static Boolean ChangeEffect(NanoleafDevice device, string effect) {
            string link = device.GetLocation() + "/api/v1/" + device.GetAuthToken() + "/effects";
            string request = "{\"select\" : \"" + effect + "\"}";
            return NanoleafRequest("PUT", link, request);
        }

        public static Boolean ChangeBrightness(NanoleafDevice device, int brightness, int duration) {
            string link = device.GetLocation() + "/api/v1/" + device.GetAuthToken() + "/state";
            string request = "{\"brightness\" : {\"value\": " + brightness + (duration>0 ? ", \"duration\": " + duration : "") + "} }";
            return NanoleafRequest("PUT", link, request);
        }

        public static Boolean ChangeBrightness(NanoleafDevice device, int brightness) {
            return ChangeBrightness(device, brightness, -1);
        }
    }
}
