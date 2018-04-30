/*
 * Copyright 2011 NEHTA
 *
 * Licensed under the NEHTA Open Source (Apache) License; you may not use this
 * file except in compliance with the License. A copy of the License is in the
 * 'license.txt' file, which should be provided with this work.
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
 * License for the specific language governing permissions and limitations
 * under the License.
 */

using System;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using Nehta.VendorLibrary.Common;
using Nehta.SMD2010.TRR;
using System.ServiceModel.Channels;

namespace Nehta.VendorLibrary.SM.SMD
{
    /// <summary>
    /// An implementation of a client for the Transport Response Retrieval (TRR) service.  
    /// </summary>
    public class TransportResponseRetrievalClient : IDisposable
    {
        internal TransportResponseRetrieval trrClient;
        internal SoapInspector.SoapMessages  trrMessages;

        /// <summary>
        /// Gets the soap request xml for the last service call.
        /// </summary>
        public string LastSoapRequest
        {
            get
            {
                return trrMessages.SoapRequest;
            }
        }

        /// <summary>
        /// Gets the soap response xml for the last service call.
        /// </summary>
        public string LastSoapResponse
        {
            get
            {
                return trrMessages.SoapResponse;
            }
        }

        #region Constructors

        /// <summary>
        /// Initializes an instance of the TransportResponseRetrievalClient.
        /// </summary>
        /// <param name="tlsCert">The client certificate to be used to establish the TLS connection.</param> 
        public TransportResponseRetrievalClient(X509Certificate2 tlsCert)
        {
            Validation.ValidateArgumentRequired("tlsCert", tlsCert);
            InitializeClient(null, tlsCert);
        }

        /// <summary>
        /// Initializes an instance of the TransportResponseRetrievalClient.
        /// </summary>
        /// <param name="configurationName">Endpoint configuration name for the TRR endpoint.</param>
        /// <param name="tlsCert">The certificate to be used to establish the TLS connection.</param>
        public TransportResponseRetrievalClient(string configurationName, X509Certificate2 tlsCert)
        {
            Validation.ValidateArgumentRequired("tlsCert", tlsCert);
            InitializeClient(configurationName, tlsCert);
        }

        #endregion

        /// <summary>
        /// Retrieve a list of Transport Responses.
        /// </summary>
        /// <param name="retrieveRequest">The parameters for the retrieve operation.</param>
        /// <param name="endpointUrl">The endpoint URL for the TRR service.</param>
        /// <returns>A list of available responses.</returns>
        public TransportResponseListType Retrieve(retrieve retrieveRequest, Uri endpointUrl)
        {
            Validation.ValidateArgumentRequired("retrieveRequest", retrieveRequest);
            Validation.ValidateArgumentRequired("endpointUrl", endpointUrl);

            Validation.ValidateArgumentRequired("retrieveRequest.organisation", retrieveRequest.organisation);

            if (trrClient is Nehta.SMD2010.TRR.TransportResponseRetrievalClient)
            {
                Nehta.SMD2010.TRR.TransportResponseRetrievalClient client = (Nehta.SMD2010.TRR.TransportResponseRetrievalClient)trrClient;
                client.Endpoint.Address = new EndpointAddress(endpointUrl);
            }

            retrieveRequest request = new retrieveRequest();
            request.retrieve = retrieveRequest;

            retrieveResponse1 response = trrClient.retrieve(request);

            if (response != null && response.retrieveResponse != null)
                return response.retrieveResponse.list;
            else
                throw new ApplicationException(Properties.Resources.UnexpectedServiceResponse);
        }

        /// <summary>
        /// Mark a set of Transport Responses as retrieved. Retrieved transport should not be returned
        /// in subsequent retrieve operations unless the <i>allAvailable</i> flag is specified.
        /// </summary>
        /// <param name="removeRequest">Specify the transport responses to be removed.</param>
        /// <param name="endpointUrl">The endpoint URL for the TRR service.</param>
        /// <returns>A list of results indicating if the operation is successful.</returns>
        public RemoveResultType[] Remove(remove removeRequest, Uri endpointUrl)
        {
            Validation.ValidateArgumentRequired("removeRequest", removeRequest);
            Validation.ValidateArgumentRequired("endpointUrl", endpointUrl);

            Validation.ValidateArgumentRequired("removeRequest.organisation", removeRequest.organisation);
            Validation.ValidateArgumentRequired("removeRequest.responseId", removeRequest.responseId);

            if (trrClient is Nehta.SMD2010.TRR.TransportResponseRetrievalClient)
            {
                Nehta.SMD2010.TRR.TransportResponseRetrievalClient client = (Nehta.SMD2010.TRR.TransportResponseRetrievalClient)trrClient;
                client.Endpoint.Address = new EndpointAddress(endpointUrl);
            }

            removeRequest request = new removeRequest();
            request.remove = removeRequest;

            removeResponse response = trrClient.remove(request);

            if (response != null)
                return response.removeResponse1;
            else
                throw new ApplicationException(Properties.Resources.UnexpectedServiceResponse);
        }

        #region Private and internal methods

        /// <summary>
        /// Initializes an instance of the TransportResponseRetrievalClient.
        /// </summary>
        /// <param name="configurationName">Endpoint configuration name for the SMR endpoint.</param>
        /// <param name="tlsCert">The client certificate to be used to establish the TLS connection.</param>
        private void InitializeClient(string configurationName, X509Certificate2 tlsCert)
        {
            this.trrMessages = new SoapInspector.SoapMessages();

            Nehta.SMD2010.TRR.TransportResponseRetrievalClient client = null;
            if (!string.IsNullOrEmpty(configurationName))
            {
                client = new Nehta.SMD2010.TRR.TransportResponseRetrievalClient(configurationName);
            }
            else
            {
                EndpointAddress address = new EndpointAddress("http://ns.electronichealth.net.au");
                CustomBinding tlsBinding = GetBinding();
                client = new Nehta.SMD2010.TRR.TransportResponseRetrievalClient(tlsBinding, address);
            }

            if (client != null)
            {
                SoapInspector.InspectEndpoint(client.Endpoint, trrMessages);
                client.ClientCredentials.ClientCertificate.Certificate = tlsCert;
                trrClient = client;
            }
        }

        /// <summary>
        /// Gets the binding configuration for the client.
        /// </summary>
        /// <returns>Configured CustomBinding instance</returns>
        internal CustomBinding GetBinding()
        {
            // Set up binding
            CustomBinding tlsBinding = new CustomBinding();

            TextMessageEncodingBindingElement tlsEncoding = new TextMessageEncodingBindingElement();
            tlsEncoding.ReaderQuotas.MaxDepth = 2147483647;
            tlsEncoding.ReaderQuotas.MaxStringContentLength = 2147483647;
            tlsEncoding.ReaderQuotas.MaxArrayLength = 2147483647;
            tlsEncoding.ReaderQuotas.MaxBytesPerRead = 2147483647;
            tlsEncoding.ReaderQuotas.MaxNameTableCharCount = 2147483647;

            HttpsTransportBindingElement httpsTransport = new HttpsTransportBindingElement();
            httpsTransport.RequireClientCertificate = true;
            httpsTransport.MaxReceivedMessageSize = 2147483647;
            httpsTransport.MaxBufferSize = 2147483647;

            tlsBinding.Elements.Add(tlsEncoding);
            tlsBinding.Elements.Add(httpsTransport);

            return tlsBinding;
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Closes and disposes the client.
        /// </summary>
        public void Dispose()
        {
            ClientBase<TransportResponseRetrieval> client;

            if (trrClient is ClientBase<TransportResponseRetrieval>)
            {
                client = (ClientBase<TransportResponseRetrieval>)trrClient;
                if (client.State != CommunicationState.Closed)
                    client.Close();
            }
        }

        #endregion
    }
}
