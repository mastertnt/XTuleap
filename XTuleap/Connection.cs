using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace XTuleap
{
    /// <summary>
    /// This class is used to stores the connection on TULEAP.
    /// </summary>
    public class Connection
    {
        /// <summary>
        /// Gets the url.
        /// </summary>
        public string Url
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the SSH key.
        /// </summary>
        public string SSHKey
        {
            get;
        }

        /// <summary>
        /// Timeout for the connection.
        /// </summary>
        public int Timeout
        {
            get;
            set;
        }

        /// <summary>
        /// Default constructor.
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
        /// Adds a new tracker structure.
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
        /// Send a GET request on TULEAP.
        /// </summary>
        /// <param name="pRelative">The relative URI according to the URI of the connection.</param>
        /// <param name="pData">The data (optional)</param>
        /// <returns>The response of the server, null if an exception occured.</returns>
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
            catch (Exception lException)
            {
                Console.WriteLine(lException);
            }

            return null;
        }

        /// <summary>
        /// Send a PUT request on TULEAP.
        /// </summary>
        /// <param name="pRelative">The relative URI according to the URI of the connection.</param>
        /// <param name="pData">The data (optional)</param>
        /// <param name="pTimeout">The timeout in milliseconds</param>

        public string PutRequest(string pRelative, string pData, int pTimeout = 60000)
        {
            try
            {
                string lUrl = this.Url + pRelative;
                HttpWebRequest lRequest = (HttpWebRequest)WebRequest.Create(lUrl);
                lRequest.ContentType = "application/json; charset=UTF-8";
                lRequest.Headers.Add("X-Auth-AccessKey", this.SSHKey);
                Byte[] lData = Encoding.UTF8.GetBytes(pData);
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
            catch (Exception lException)
            {
                Console.WriteLine(lException);
            }

            return null;
        }

        /// <summary>
        /// Send a PUT request on TULEAP.
        /// </summary>
        /// <param name="pRelative">The relative URI according to the URI of the connection.</param>
        /// <param name="pData">The data (optional)</param>
        /// <param name="pTimeout">The timeout in milliseconds</param>

        public string PostRequest(string pRelative, string pData, int pTimeout = 60000)
        {
            try
            {
                string lUrl = this.Url + pRelative;
                HttpWebRequest lRequest = (HttpWebRequest)WebRequest.Create(lUrl);
                lRequest.Accept = "application/json";
                lRequest.ContentType = "application/json; charset=UTF-8";
                lRequest.Headers.Add("X-Auth-AccessKey", this.SSHKey);
                Byte[] lData = Encoding.UTF8.GetBytes(pData);
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
                    using (var errorResponse = (HttpWebResponse)lException.Response)
                    {
                        using (var reader = new StreamReader(errorResponse.GetResponseStream()))
                        {
                            string error = reader.ReadToEnd();
                            //TODO: use JSON.net to parse this string and look at the error message
                        }
                    }
                }
                Console.WriteLine(lException);
            }
            return null;

        }

        /// <summary>
        /// Send a PUT request on TULEAP.
        /// </summary>
        /// <param name="pRelative">The relative URI according to the URI of the connection.</param>
        /// <param name="pData">The data (optional)</param>
        /// <param name="pTimeout">The timeout in milliseconds</param>
        public bool DeleteRequest(string pRelative, string pData, int pTimeout = 60000)
        {
            try
            {
                string lUrl = this.Url + pRelative;
                HttpWebRequest lRequest = (HttpWebRequest)WebRequest.Create(lUrl);
                lRequest.ContentType = "application/json; charset=UTF-8";
                lRequest.Headers.Add("X-Auth-AccessKey", this.SSHKey);
                Byte[] lData = Encoding.UTF8.GetBytes(pData);
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
            catch (Exception lException)
            {
                Console.WriteLine(lException);
            }

            return false;
        }

        /// <summary>
        /// Gets all structures.
        /// </summary>
        public List<TrackerStructure> TrackerStructures
        {
            get;
        }

        /// <summary>
        /// This method is defined to force all certifications.
        /// </summary>
        private bool AcceptAllCertifications(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certification, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
    }
}
