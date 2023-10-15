using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Newtonsoft.Json;
using NLog;

namespace XTuleap
{
    /// <summary>
    ///     This class is used to stores the connection on Tuleap.
    /// </summary>
    public class Connection
    {
        /// <summary>
        /// Logger of the class.
        /// </summary>
        private static readonly Logger msLogger = LogManager.GetCurrentClassLogger();

        /// <summary>
        ///     Default constructor.
        /// </summary>
        public Connection(string pUrl, string pSshKey)
        {
            this.Timeout = 600000;
            this.Url = pUrl;
            this.SSHKey = pSshKey;
            this.TrackerStructures = new List<TrackerStructure>();
            ServicePointManager.ServerCertificateValidationCallback = this.AcceptAllCertifications;
        }

        /// <summary>
        ///     Gets the url.
        /// </summary>
        public string Url
        {
            get;
        }

        /// <summary>
        ///     Gets the SSH key.
        /// </summary>
        public string SSHKey
        {
            get;
        }

        /// <summary>
        ///     Timeout for the connection.
        /// </summary>
        public int Timeout
        {
            get;
            set;
        }

        /// <summary>
        ///     Gets all structures.
        /// </summary>
        public List<TrackerStructure> TrackerStructures
        {
            get;
        }

        /// <summary>
        ///     Adds a new tracker structure.
        /// </summary>
        /// <param name="pId">The identifier of the structure.</param>
        /// <returns>True if tracker structure has been retrieve, false otherwise.</returns>
        public TrackerStructure AddTrackerStructure(int pId)
        {
            TrackerStructure lStructure = this.TrackerStructures.FirstOrDefault(pStructure => pStructure.Id == pId);
            if (lStructure == null)
            {
                string lStructureRequest = this.GetRequest("trackers/" + pId, "");
                if (lStructureRequest != null)
                {
                    lStructure = JsonConvert.DeserializeObject<TrackerStructure>(lStructureRequest);
                    this.TrackerStructures.Add(lStructure);
                    return lStructure;
                }

                return null;
            }

            return lStructure;
        }

        /// <summary>
        ///     Send a GET request on Tuleap.
        /// </summary>
        /// <param name="pRelative">The relative URI according to the URI of the connection.</param>
        /// <param name="pData">The data (optional)</param>
        /// <returns>The response of the server, null if an exception occurred.</returns>
        public string GetRequest(string pRelative, string pData, int pTimeout = 60000)
        {
            try
            {
                string lUrl = this.Url + pRelative;
                WebRequest lRequest = WebRequest.Create(lUrl);
                lRequest.Method = "GET";
                lRequest.ContentLength = 0;
                lRequest.ContentType = "application/json; charset=UTF-8";
                lRequest.Timeout = pTimeout;
                lRequest.Headers.Add("X-Auth-AccessKey", this.SSHKey);
                WebResponse lWebResponse = lRequest.GetResponse();
                Stream lReceiveStream = lWebResponse.GetResponseStream();
                StreamReader lReader = new StreamReader(lReceiveStream, Encoding.UTF8);
                string lContent = lReader.ReadToEnd();
                lWebResponse.Close();
                return lContent;
            }
            catch (WebException lException)
            {
                if (lException.Response != null)
                {
                    using HttpWebResponse lErrorResponse = (HttpWebResponse)lException.Response;
                    using StreamReader lReader = new StreamReader(lErrorResponse.GetResponseStream());
                    string lError = lReader.ReadToEnd();
                    msLogger.Log(LogLevel.Error, lError);
                }

                msLogger.Log(LogLevel.Error, lException);
            }
            catch (Exception lException)
            {
                msLogger.Log(LogLevel.Error, lException);
            }

            return null;
        }

        /// <summary>
        ///     Send a PUT request on Tuleap.
        /// </summary>
        /// <param name="pRelative">The relative URI according to the URI of the connection.</param>
        /// <param name="pData">The data (optional)</param>
        /// <param name="pTimeout">The timeout in milliseconds</param>
        public string? PutRequest(string pRelative, string pData, int pTimeout = 60000)
        {
            try
            {
                string lUrl = this.Url + pRelative;
                HttpWebRequest lRequest = (HttpWebRequest) WebRequest.Create(lUrl);
                lRequest.ContentType = "application/json; charset=UTF-8";
                lRequest.Headers.Add("X-Auth-AccessKey", this.SSHKey);
                byte[] lData = Encoding.UTF8.GetBytes(pData);
                lRequest.ContentLength = lData.Length;
                lRequest.Method = "PUT";
                lRequest.Timeout = pTimeout;
                Stream lRequestStream = lRequest.GetRequestStream();
                lRequestStream.Write(lData, 0, lData.Length);
                lRequestStream.Close();

                HttpWebResponse lWebResponse = (HttpWebResponse) lRequest.GetResponse();
                Stream lReceiveStream = lWebResponse.GetResponseStream();
                StreamReader lReader = new StreamReader(lReceiveStream, Encoding.UTF8);
                string lContent = lReader.ReadToEnd();
                lWebResponse.Close();
                return lContent;
            }
            catch (WebException lException)
            {
                if (lException.Response != null)
                {
                    using HttpWebResponse lErrorResponse = (HttpWebResponse)lException.Response;
                    using StreamReader lReader = new StreamReader(lErrorResponse.GetResponseStream());
                    string lError = lReader.ReadToEnd();
                    msLogger.Log(LogLevel.Error, lError);
                }

                msLogger.Log(LogLevel.Error, lException);
            }
            catch (Exception lException)
            {
                msLogger.Log(LogLevel.Error, lException);
            }

            return null;
        }

        /// <summary>
        ///     Send a PUT request on Tuleap.
        /// </summary>
        /// <param name="pRelative">The relative URI according to the URI of the connection.</param>
        /// <param name="pData">The data (optional)</param>
        /// <param name="pTimeout">The timeout in milliseconds</param>
        public string? PostRequest(string pRelative, string pData, int pTimeout = 60000)
        {
            try
            {
                string lUrl = this.Url + pRelative;
                HttpWebRequest lRequest = (HttpWebRequest) WebRequest.Create(lUrl);
                lRequest.Accept = "application/json";
                lRequest.ContentType = "application/json; charset=UTF-8";
                lRequest.Headers.Add("X-Auth-AccessKey", this.SSHKey);
                byte[] lData = Encoding.UTF8.GetBytes(pData);
                lRequest.ContentLength = lData.Length;
                lRequest.Method = "POST";
                lRequest.Timeout = pTimeout;
                Stream lRequestStream = lRequest.GetRequestStream();
                lRequestStream.Write(lData, 0, lData.Length);
                lRequestStream.Close();

                HttpWebResponse lWebResponse = (HttpWebResponse) lRequest.GetResponse();
                Stream lReceiveStream = lWebResponse.GetResponseStream();
                StreamReader lReader = new StreamReader(lReceiveStream, Encoding.UTF8);
                string lContent = lReader.ReadToEnd();
                lWebResponse.Close();
                return lContent;
            }
            catch (WebException lException)
            {
                if (lException.Response != null)
                {
                    using HttpWebResponse lErrorResponse = (HttpWebResponse) lException.Response;
                    using StreamReader lReader = new StreamReader(lErrorResponse.GetResponseStream());
                    string lError = lReader.ReadToEnd();
                    msLogger.Log(LogLevel.Error, lError);
                }

                msLogger.Log(LogLevel.Error, lException);
            }

            return null;
        }

        /// <summary>
        ///     Send a PUT request on Tuleap.
        /// </summary>
        /// <param name="pRelative">The relative URI according to the URI of the connection.</param>
        /// <param name="pData">The data (optional)</param>
        /// <param name="pTimeout">The timeout in milliseconds</param>
        public bool DeleteRequest(string pRelative, string pData, int pTimeout = 60000)
        {
            try
            {
                string lUrl = this.Url + pRelative;
                HttpWebRequest lRequest = (HttpWebRequest) WebRequest.Create(lUrl);
                lRequest.ContentType = "application/json; charset=UTF-8";
                lRequest.Headers.Add("X-Auth-AccessKey", this.SSHKey);
                byte[] lData = Encoding.UTF8.GetBytes(pData);
                lRequest.ContentLength = lData.Length;
                lRequest.Method = "DELETE";
                lRequest.Timeout = pTimeout;
                Stream lRequestStream = lRequest.GetRequestStream();
                lRequestStream.Write(lData, 0, lData.Length);
                lRequestStream.Close();

                HttpWebResponse lWebResponse = (HttpWebResponse) lRequest.GetResponse();
                Stream lReceiveStream = lWebResponse.GetResponseStream();
                StreamReader lReader = new StreamReader(lReceiveStream, Encoding.UTF8);
                string lContent = lReader.ReadToEnd();
                lWebResponse.Close();

                return true;
            }
            catch (WebException lException)
            {
                if (lException.Response != null)
                {
                    using HttpWebResponse lErrorResponse = (HttpWebResponse)lException.Response;
                    using StreamReader lReader = new StreamReader(lErrorResponse.GetResponseStream());
                    string lError = lReader.ReadToEnd();
                    msLogger.Log(LogLevel.Error, lError);
                }

                msLogger.Log(LogLevel.Error, lException);
            }
            catch (Exception lException)
            {
                msLogger.Log(LogLevel.Error, lException);
            }

            return false;
        }

        /// <summary>
        ///     This method is defined to force all certifications.
        /// </summary>
        private bool AcceptAllCertifications(object sender, X509Certificate certification, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
    }
}