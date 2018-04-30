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
using Nehta.SMD2010.TRD;
using System.ServiceModel.Channels;
using System.Xml;
using System.Collections.Generic;
 


namespace Nehta.VendorLibrary.SM.SMD
{
    /// <summary>
    /// An implementation of a client for the Transport Response Delivery (TRD) service.  
    /// </summary>
    public class TransportResponseDeliveryClient : IDisposable
    {
        internal TransportResponseDelivery trdClient;
        internal SoapInspector.SoapMessages trdMessages;

        /// <summary>
        /// Gets the soap request xml for the last service call.
        /// </summary>
        public string LastSoapRequest
        {
            get
            {
                return trdMessages.SoapRequest;
            }
        }

        /// <summary>
        /// Gets the soap response xml for the last service call.
        /// </summary>
        public string LastSoapResponse
        {
            get
            {
                return trdMessages.SoapResponse;
            }
        }

        #region Constructors

        /// <summary>
        /// Initializes an instance of the TransportResponseDeliveryClient.
        /// </summary>
        /// <param name="tlsCert">The client certificate to be used to establish the TLS connection.</param>
        public TransportResponseDeliveryClient(X509Certificate2 tlsCert)
        {
            Validation.ValidateArgumentRequired("tlsCert", tlsCert);
            InitializeClient(null,tlsCert);
        }
        
        /// <summary>
        /// Initializes an instance of the TransportResponseDeliveryClient.
        /// </summary>
        /// <param name="configurationName">Endpoint configuration name for the TRD endpoint.</param>
        /// <param name="tlsCert">The certificate to be used to establish the TLS connection.</param>
        public TransportResponseDeliveryClient(string configurationName, X509Certificate2 tlsCert)
        {
            Validation.ValidateArgumentRequired("tlsCert", tlsCert);
            InitializeClient(configurationName, tlsCert);
        }

        #endregion

        /// <summary>
        /// Construct a transport response based on the received SealedMessageType instance (from SMD, SIMD or SMR).
        /// </summary>
        /// <param name="sealedMessage">The SMD, SMR, SIMD or TRD SealedMessageType instance to create a transport response for.</param>
        /// <param name="sourceOrganisation">The organisation generating the response.</param>
        /// <param name="deliveryResponseClass">The type of response.</param>
        /// <param name="deliveryResponseCode">The response code.</param>
        /// <param name="deliveryResponseMessage">Message containing more details on the response.</param>
        /// <param name="transportResponseTime">The time of the response.</param>
        /// <param name="responseId">A unique ID to identify the response.</param>
        /// <param name="isFinal">A value to indicate if the transport response is the final response.</param>
        /// <param name="otherTransportMetadata">Additional metadata to be included.</param>
        /// <param name="payloadDecryptionCertificate">The certificate to decrypt the sealed message encrypted payload with (the digest value of the message is obtained from the decrypted payload).</param>
        /// <returns>The TransportResponseType instance.</returns>
        public static TransportResponseType GetTransportResponse(
            CommonSealedMessageType sealedMessage,
            string sourceOrganisation,
            ResponseClassType deliveryResponseClass, 
            string deliveryResponseCode,
            string deliveryResponseMessage,
            DateTime transportResponseTime,
            string responseId,
            bool isFinal, 
            List<OtherTransportMetadataEntryType> otherTransportMetadata,
            X509Certificate2 payloadDecryptionCertificate)
        {
            // Validate sealedMessage
            Validation.ValidateArgumentRequired("sealedMessage", sealedMessage);
            Validation.ValidateArgumentRequired("sealedMessage.EncryptedPayload", sealedMessage.EncryptedPayload);
            Validation.ValidateArgumentRequired("sealedMessage.InvocationId", sealedMessage.InvocationId);
            Validation.ValidateArgumentRequired("sealedMessage.ReceiverOrganisation", sealedMessage.ReceiverOrganisation);
            Validation.ValidateArgumentRequired("sealedMessage.SenderOrganisation", sealedMessage.SenderOrganisation);
            Validation.ValidateArgumentRequired("sealedMessage.ServiceCategory", sealedMessage.ServiceCategory);
            
            Validation.ValidateArgumentRequired("sourceOrganisation", sourceOrganisation);
            Validation.ValidateArgumentRequired("deliveryResponseCode", deliveryResponseCode);
            Validation.ValidateArgumentRequired("deliveryResponseMessage", deliveryResponseMessage);
            Validation.ValidateArgumentRequired("responseId", responseId);

            Validation.ValidateArgumentRequired("payloadDecryptionCertificate", payloadDecryptionCertificate);

            // deliveryResponseClass isn't validated as it is an enum and there is no option to omit it.

            TransportResponseType tr = new TransportResponseType();
            tr.deliveryResponse = new DeliveryResponseType();

            tr.deliveryResponse.responseClass = deliveryResponseClass;
            tr.deliveryResponse.responseCode = deliveryResponseCode;
            tr.deliveryResponse.message = deliveryResponseMessage;
            tr.final = isFinal;

            if (deliveryResponseClass == ResponseClassType.Success)
            {
                XmlDocument encryptedPayload = sealedMessage.EncryptedPayload.SerializeToXml("encryptedPayload");
                XmlDocument signedPayload = encryptedPayload.XspDecrypt(payloadDecryptionCertificate);
                IList<byte[]> digestValues = signedPayload.XspGetDigestValues();

                tr.deliveryResponse.digestValue = digestValues[0];
            }

            tr.metadata = new TransportResponseMetadataType();
            tr.metadata.transportResponseTime = transportResponseTime.ToUniversalTime();
            tr.metadata.responseId = responseId;
            tr.metadata.sourceOrganisation = sourceOrganisation;
            tr.metadata.serviceCategory = sealedMessage.ServiceCategory;
            tr.metadata.invocationId = sealedMessage.InvocationId;
            tr.metadata.receiverOrganisation = sealedMessage.ReceiverOrganisation;
            tr.metadata.senderOrganisation = sealedMessage.SenderOrganisation;

            if (otherTransportMetadata != null && otherTransportMetadata.Count > 0)
                tr.metadata.otherTransportMetadata = otherTransportMetadata.ToArray();

            return tr;
        }

        /// <summary>
        /// Deliver one or more Transport Responses to a Sender, Sender Intermediary or Receiver Intermediary.
        /// </summary>
        /// <param name="responses">The list of Transport Responses to deliver.</param>
        /// <param name="endpointUrl">The endpoint of the TRD service.</param>
        /// <returns>A value indicating if the operation is successful.</returns>
        public deliverResponse Deliver(List<TransportResponseType> responses, Uri endpointUrl)
        {
            Validation.ValidateArgumentRequired("responses", responses);
            Validation.ValidateArgumentRequired("endpointUrl", endpointUrl);

            Validation.ValidateArgumentRequired("responses", responses);
            for (int x = 0; x < responses.Count; x++)
            {
                Validation.ValidateArgumentRequired(string.Format("responses[{0}].deliveryResponse", x), responses[x].deliveryResponse);
                Validation.ValidateArgumentRequired(string.Format("responses[{0}].deliveryResponse.message", x), responses[x].deliveryResponse.message);
                Validation.ValidateArgumentRequired(string.Format("responses[{0}].deliveryResponse.responseCode", x), responses[x].deliveryResponse.responseCode);
                if (responses[x].deliveryResponse.responseClass == ResponseClassType.Success)
                    Validation.ValidateArgumentRequired(string.Format("responses[{0}].deliveryResponse.digestValue", x), responses[x].deliveryResponse.digestValue);

                // Validate metadata
                Validation.ValidateArgumentRequired(string.Format("responses[{0}].metadata", x), responses[x].metadata);
                Validation.ValidateArgumentRequired(string.Format("responses[{0}].metadata.invocationId", x), responses[x].metadata.invocationId);
                Validation.ValidateArgumentRequired(string.Format("responses[{0}].metadata.responseId", x), responses[x].metadata.responseId);
                Validation.ValidateArgumentRequired(string.Format("responses[{0}].metadata.receiverOrganisation", x), responses[x].metadata.receiverOrganisation);
                Validation.ValidateArgumentRequired(string.Format("responses[{0}].metadata.senderOrganisation", x), responses[x].metadata.senderOrganisation);
                Validation.ValidateArgumentRequired(string.Format("responses[{0}].metadata.serviceCategory", x), responses[x].metadata.serviceCategory);
                Validation.ValidateArgumentRequired(string.Format("responses[{0}].metadata.sourceOrganisation", x), responses[x].metadata.sourceOrganisation);
                Validation.ValidateDateTime(string.Format("responses[{0}].metadata.transportResponseTime", x), responses[x].metadata.transportResponseTime);

                // Other transport metadata
                if (responses[x].metadata.otherTransportMetadata != null && responses[x].metadata.otherTransportMetadata.Length > 0)
                {
                    for (int y = 0; y < responses[x].metadata.otherTransportMetadata.Length; y++)
                    {
                        OtherTransportMetadataEntryType omt = responses[x].metadata.otherTransportMetadata[y];
                        Validation.ValidateArgumentRequired(string.Format("responses[{0}].metadata.otherTransportMetadata[{1}].metadataType", x, y), omt.metadataType);
                        Validation.ValidateArgumentRequired(string.Format("responses[{0}].metadata.otherTransportMetadata[{1}].metadataValue", x, y), omt.metadataValue);
                    }
                }
            }

            if (trdClient is Nehta.SMD2010.TRD.TransportResponseDeliveryClient)
            {
                Nehta.SMD2010.TRD.TransportResponseDeliveryClient client = (Nehta.SMD2010.TRD.TransportResponseDeliveryClient)trdClient;
                client.Endpoint.Address = new EndpointAddress(endpointUrl.ToString());
            }

            deliverRequest request = new deliverRequest();
            request.deliver = responses.ToArray();

            deliverResponse1 response = trdClient.deliver(request);

            if (response != null && response.deliverResponse != null)
                return response.deliverResponse;
            else
                throw new ApplicationException(Properties.Resources.UnexpectedServiceResponse);
        }

        #region Private and internal methods

        /// <summary>
        /// Initializes an instance of the TransportMessageDeliveryClient.
        /// </summary>
        /// <param name="configurationName">Endpoint configuration name for the TRD endpoint.</param>
        /// <param name="tlsCert">The client certificate to be used to establish the TLS connection.</param>
        private void InitializeClient(string configurationName, X509Certificate2 tlsCert)
        {
            this.trdMessages = new SoapInspector.SoapMessages();

            Nehta.SMD2010.TRD.TransportResponseDeliveryClient client = null;
            if (!string.IsNullOrEmpty(configurationName))
            {
                client = new Nehta.SMD2010.TRD.TransportResponseDeliveryClient(configurationName);
            }
            else
            {
                EndpointAddress address = new EndpointAddress("http://ns.electronichealth.net.au");
                CustomBinding tlsBinding = GetBinding();
                client = new Nehta.SMD2010.TRD.TransportResponseDeliveryClient(tlsBinding, address);
            }

            if (client != null)
            {
                SoapInspector.InspectEndpoint(client.Endpoint, trdMessages);
                client.ClientCredentials.ClientCertificate.Certificate = tlsCert;
                trdClient = client;
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
            ClientBase<TransportResponseDelivery> client;

            if (trdClient is ClientBase<TransportResponseDelivery>)
            {
                client = (ClientBase<TransportResponseDelivery>)trdClient;
                if (client.State != CommunicationState.Closed)
                    client.Close();
            }
        }

        #endregion

    }
}
