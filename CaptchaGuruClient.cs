using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

    public class CaptchaGuruClient
    {
        public string APIKey { get; private set; }
        public CaptchaGuruClient(string apiKey)
        {
            APIKey = apiKey;
        }

        /// <summary>
        /// Sends a solve request and waits for a response
        /// </summary>
        /// <param name="googleKey">The "sitekey" value from site your captcha is located on</param>
        /// <param name="pageUrl">The page the captcha is located on</param>
        /// <param name="proxy">The proxy used, format: "username:password@ip:port</param>
        /// <param name="proxyType">The type of proxy used</param>
        /// <param name="result">If solving was successful this contains the answer</param>
        /// <returns>Returns true if solving was successful, otherwise false</returns>
        public bool SolveRecaptchaV2(string googleKey, string pageUrl, out string result)
        {
            string requestUrl = "http://api.captcha.guru/in.php?key=" + APIKey + "&method=userrecaptcha&googlekey=" + googleKey + "&pageurl=" + pageUrl;

            

            try
            {
                WebRequest req = WebRequest.Create(requestUrl);

                using (WebResponse resp = req.GetResponse())
                using (StreamReader read = new StreamReader(resp.GetResponseStream()))
                {
                    string response = read.ReadToEnd();

                    if (response.Length < 3)
                    {
                        result = response;
                        return false;
                    }
                    else
                    {
                        if (response.Substring(0, 3) == "OK|")
                        {
                            string captchaID = response.Remove(0, 3);

                            for (int i = 0; i < 24; i++)
                            {
                                WebRequest getAnswer = WebRequest.Create("http://api.captcha.guru/res.php?key=" + APIKey + "&action=get&id=" + captchaID);

                                using (WebResponse answerResp = getAnswer.GetResponse())
                                using (StreamReader answerStream = new StreamReader(answerResp.GetResponseStream()))
                                {
                                    string answerResponse = answerStream.ReadToEnd();

                                    if (answerResponse.Length < 3)
                                    {
                                        result = answerResponse;
                                        return false;
                                    }
                                    else
                                    {
                                        if (answerResponse.Substring(0, 3) == "OK|")
                                        {
                                            result = answerResponse.Remove(0, 3);
                                            return true;
                                        }
                                        else if (answerResponse != "CAPCHA_NOT_READY")
                                        {
                                            result = answerResponse;
                                            return false;
                                        }
                                    }
                                }

                                Thread.Sleep(5000);
                            }

                            result = "Timeout";
                            return false;
                        }
                        else
                        {
                            result = response;
                            return false;
                        }
                    }
                }
            }
            catch { }

            result = "Unknown error";
            return false;
        }
    }

    public enum ProxyType
    {
        HTTP,
        HTTPS,
        SOCKS4,
        SOCKS5
    }

