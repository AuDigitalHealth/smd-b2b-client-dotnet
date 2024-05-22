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
using Nehta.SMD2010.SMR;
using System.ServiceModel.Channels;

namespace Nehta.VendorLibrary.SM.SMD
{
    /// <summary>
    /// An implementation of a client for the Sealed Message Retrieval (SMR) service.  
    /// </summary>
    public class SealedMessageRetrievalClient : IDisposable
    {
        internal SealedMessageRetrieval smrClient;
        internal SoapInspector.SoapMessages smrMessages;

        /// <summary>
        /// Gets the soap request xml for the last service call.
        /// </summary>
        public string LastSoapRequest
        {
            get
            {
                return smrMessages.SoapRequest;
            }
        }

        /// <summary>
        /// Gets the soap response xml for the last service call.
        /// </summary>
        public string LastSoapResponse
        {
            get
            {
                return smrMessages.SoapResponse;
            }
        }

        #region Constructors

        /// <summary>
        /// Initializes an instance of the SealedMessageRetrievalClient.
        /// </summary>
        /// <param name="tlsCert">The client certificate to be used to establish the TLS connection.</param>
        public SealedMessageRetrievalClient(X509Certificate2 tlsCert)
        {
                        Validation.ValidateArgumentRequired("tlsCert", tlsCert);
            Validation.ValidateArgumentRequired("tlsCert", tlsCert);
            InitializeClient(null, tlsCert);
        }

        /// <summary>
        /// Initializes an instance of the SealedMessageRetrievalClient.
        /// </summary>
        /// <param name="configurationName">Endpoint configuration name for the SMR endpoint.</param>
        /// <param name="tlsCert">The certificate to be used to establish the TLS connection.</param>
        public SealedMessageRetrievalClient(string configurationName, X509Certificate2 tlsCert)
        {
            Validation.ValidateArgumentRequired("tlsCert", tlsCert);
            InitializeClient(configurationName, tlsCert);
        }

        #endregion

        /// <summary>
        /// Get a list of messages that are available for retrieval.
        /// </summary>
        /// <param name="listRequest">The parameters for the list operation.</param>
        /// <param name="endpointUrl">The endpoint URL for the SMR service.</param>
        /// <returns>A list of available messages.</returns>
        public MessageListType List(list listRequest, Uri endpointUrl)
        {
            Validation.ValidateArgumentRequired("listRequest", listRequest);
            Validation.ValidateArgumentRequired("endpointUrl", endpointUrl);
            Validation.ValidateArgumentRequired("listRequest.receiverOrganisation", listRequest.receiverOrganisation);

            if (smrClient is Nehta.SMD2010.SMR.SealedMessageRetrievalClient)
            {
                Nehta.SMD2010.SMR.SealedMessageRetrievalClient client = (Nehta.SMD2010.SMR.SealedMessageRetrievalClient)smrClient;
                client.Endpoint.Address = new EndpointAddress(endpointUrl);
            }

            listRequest request = new listRequest();
            request.list = listRequest;

            listResponse1 response = smrClient.list(request);

            if (response != null && response.listResponse != null)
                return response.listResponse.list;
            else
                throw new ApplicationException(ServiceInterfaces.UnexpectedServiceResponse);
        }

        /// <summary>
        /// Retrieve a list of Sealed Messages.
        /// </summary>
        /// <param name="retrieveRequest">The parameters for the retrieve operation.</param>
        /// <param name="endpointUrl">The endpoint URL for the SMR service.</param>
        /// <returns>A list of Sealed Messages.</returns>
        public SealedMessageType[] Retrieve(retrieve retrieveRequest, Uri endpointUrl)
        {
            Validation.ValidateArgumentRequired("retrieveRequest", retrieveRequest);
            Validation.ValidateArgumentRequired("endpointUrl", endpointUrl);

            Validation.ValidateArgumentRequired("retrieveRequest.receiverOrganisation", retrieveRequest.receiverOrganisation);
            Validation.ValidateArgumentRequired("retrieveRequest.invocationId", retrieveRequest.invocationId);

            if (smrClient is Nehta.SMD2010.SMR.SealedMessageRetrievalClient)
            {
                Nehta.SMD2010.SMR.SealedMessageRetrievalClient client = (Nehta.SMD2010.SMR.SealedMessageRetrievalClient)smrClient;
                client.Endpoint.Address = new EndpointAddress(endpointUrl);
            }

            retrieveRequest request = new retrieveRequest();
            request.retrieve = retrieveRequest;

            retrieveResponse response = smrClient.retrieve(request);

            if (response != null)
                return response.retrieveResponse1;
            else
                throw new ApplicationException(ServiceInterfaces.UnexpectedServiceResponse);
        }

        #region Private and internal methods

        /// <summary>
        /// Initializes an instance of the SealedMessageRetrievalClient.
        /// </summary>
        /// <param name="configurationName">Endpoint configuration name for the SMR endpoint.</param>
        /// <param name="tlsCert">The client certificate to be used to establish the TLS connection.</param>
        private void InitializeClient(string configurationName, X509Certificate2 tlsCert)
        {
            this.smrMessages = new SoapInspector.SoapMessages();

            Nehta.SMD2010.SMR.SealedMessageRetrievalClient client = null;
            if (!string.IsNullOrEmpty(configurationName))
            {
                client = new Nehta.SMD2010.SMR.SealedMessageRetrievalClient(configurationName);
            }
            else
            {
                EndpointAddress address = new EndpointAddress("http://ns.electronichealth.net.au");
                CustomBinding tlsBinding = GetBinding();
                client = new Nehta.SMD2010.SMR.SealedMessageRetrievalClient(tlsBinding, address);
            }

            if (client != null)
            {
                SoapInspector.InspectEndpoint(client.Endpoint, smrMessages);
                client.ClientCredentials.ClientCertificate.Certificate = tlsCert;
                smrClient = client;
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
            ClientBase<SealedMessageRetrieval> client;

            if (smrClient is ClientBase<SealedMessageRetrieval>)
            {
                client = (ClientBase<SealedMessageRetrieval>)smrClient;
                if (client.State != CommunicationState.Closed)
                    client.Close();
            }
        }

        #endregion
    }
}
