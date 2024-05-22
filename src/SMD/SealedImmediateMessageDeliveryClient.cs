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
using Nehta.SMD2010.SIMD;
using System.ServiceModel.Channels;
using System.Xml;

namespace Nehta.VendorLibrary.SM.SMD
{
    /// <summary>
    /// An implementation of a client for the Sealed Immediate Message Delivery (SIMD) service.  
    /// </summary>
    public class SealedImmediateMessageDeliveryClient : IDisposable
    {
        internal SealedImmediateMessageDelivery simdClient;
        internal SoapInspector.SoapMessages  simdMessages;

        /// <summary>
        /// Gets the soap request xml for the last service call.
        /// </summary>
        public string LastSoapRequest
        {
            get
            {
                return simdMessages.SoapRequest;
            }
        }

        /// <summary>
        /// Gets the soap response xml for the last service call.
        /// </summary>
        public string LastSoapResponse
        {
            get
            {
                return simdMessages.SoapResponse;
            }
        }

        #region Constructors

        /// <summary>
        /// Initializes an instance of the SealedImmediateMessageDeliveryClient.
        /// </summary>
        /// <param name="tlsCert">The client certificate to establish the TLS connection with.</param>
        public SealedImmediateMessageDeliveryClient(X509Certificate2 tlsCert)
        {
            Validation.ValidateArgumentRequired("tlsCert", tlsCert);
            InitializeClient(null, tlsCert);
        }

        /// <summary>
        /// Initializes an instance of the SealedImmediateMessageDeliveryClient.
        /// </summary>
        /// <param name="configurationName">Endpoint configuration name for the SIMD endpoint.</param>
        /// <param name="tlsCert">The certificate to establish the TLS connection with.</param>
        public SealedImmediateMessageDeliveryClient(string configurationName, X509Certificate2 tlsCert)
        {
            Validation.ValidateArgumentRequired("tlsCert", tlsCert);
            InitializeClient(configurationName, tlsCert);
        }

        #endregion

        /// <summary>
        /// Validate a MessageMetadataType object.
        /// </summary>
        /// <param name="metadata">The object to be validated.</param>
        public static void ValidateMetadata(MessageMetadataType metadata)
        {
            // Validate metadata
            Validation.ValidateArgumentRequired("metadata", metadata);
            Validation.ValidateDateTime("metadata.creationTime", metadata.creationTime);
            Validation.ValidateArgumentRequired("metadata.invocationId", metadata.invocationId);
            Validation.ValidateArgumentRequired("metadata.receiverOrganisation", metadata.receiverOrganisation);
            Validation.ValidateArgumentRequired("metadata.senderOrganisation", metadata.senderOrganisation);
            Validation.ValidateArgumentRequired("metadata.serviceCategory", metadata.serviceCategory);
            Validation.ValidateArgumentRequired("metadata.serviceInterface", metadata.serviceInterface);
            Validation.ValidateStringMatch("metadata.serviceInterface", metadata.serviceInterface, ServiceInterfaces.SimdServiceInterface);

            // Validate that route record hasn't been specified
            Validation.ValidateArgumentNotAllowed("metadata.routeRecord", metadata.routeRecord);

            // Other transport metadata
            if (metadata.otherTransportMetadata != null && metadata.otherTransportMetadata.Length > 0)
            {
                for (int x = 0; x < metadata.otherTransportMetadata.Length; x++)
                {
                    OtherTransportMetadataEntryType omt = metadata.otherTransportMetadata[x];
                    Validation.ValidateArgumentRequired(string.Format("metadata.otherTransportMetadata[{0}].metadataType", x), omt.metadataType);
                    Validation.ValidateArgumentRequired(string.Format("metadata.otherTransportMetadata[{0}].metadataValue", x), omt.metadataValue);
                }
            }
        }

        /// <summary>
        /// Create and returns a SealedMessageType instance encapsulating payload signing and encryption.
        /// </summary>
        /// <param name="payload">The xml document payload.</param>
        /// <param name="metadata">The metadata of the message.</param>
        /// <param name="signingCert">The certificate to sign the payload with.</param>
        /// <param name="encryptionCert">The certificate to encrypt the payload with.</param>
        /// <returns>The SealedMessageType instance.</returns>
        public static SealedMessageType GetSealedMessage(XmlDocument payload, MessageMetadataType metadata, X509Certificate2 signingCert, X509Certificate2 encryptionCert)
        {
            // Create SealedMessageType
            SealedMessageType sealedMessage = new SealedMessageType();

            // Create signed payload container
            XmlDocument signedPayload = payload.XspSign(signingCert);

            // Create the encrypted payload container
            XmlDocument encryptedPayload = signedPayload.XspEncrypt(encryptionCert);

            // Set encrypted payload
            sealedMessage.encryptedPayload = encryptedPayload.Deserialize<EncryptedPayloadType>();

            // Set metadata
            ValidateMetadata(metadata);
            sealedMessage.metadata = metadata;

            return sealedMessage;
        }

        /// <summary>
        /// Create and returns a SealedMessageType instance encapsulating payload signing and encryption.
        /// </summary>
        /// <param name="messageData">The non xml document payload.</param>
        /// <param name="metadata">The metadata of the message.</param>
        /// <param name="signingCert">The certificate to sign the payload with.</param>
        /// <param name="encryptionCert">The certificate to encrypt the payload with.</param>
        /// <returns>The SealedMessageType instance.</returns>
        public static SealedMessageType GetSealedMessage(byte[] messageData, MessageMetadataType metadata, X509Certificate2 signingCert, X509Certificate2 encryptionCert)
        {
            Validation.ValidateArgumentRequired("messageData", messageData);

            MessageType message = new MessageType();
            message.data = messageData;

            return GetSealedMessage(message.SerializeToXml("message"), metadata, signingCert, encryptionCert);
        }

        /// <summary>
        /// Deliver a Sealed Message to a Receiver or Intermediary and receive an immediate response in return.
        /// </summary>
        /// <param name="message">The encrypted payload to deliver.</param>
        /// <param name="endpointUrl">The endpoint of the destination endpoint service.</param>
        /// <returns>Acknowledgement on the deliver operation.</returns>
        public deliverResponse Deliver(SealedMessageType message, Uri endpointUrl)
        {
            Validation.ValidateArgumentRequired("message", message);
            Validation.ValidateArgumentRequired("endpointUrl", endpointUrl);

            // Validate metadata
            ValidateMetadata(message.metadata);

            // Validate encrypted payload
            Validation.ValidateArgumentRequired("message.encryptedPayload", message.encryptedPayload);

            if (simdClient is Nehta.SMD2010.SIMD.SealedImmediateMessageDeliveryClient)
            {
                Nehta.SMD2010.SIMD.SealedImmediateMessageDeliveryClient client = (Nehta.SMD2010.SIMD.SealedImmediateMessageDeliveryClient)simdClient;
                client.Endpoint.Address = new EndpointAddress(endpointUrl);
            }

            deliverRequest request = new deliverRequest();
            request.deliver = new deliver();
            request.deliver.message = message;

            deliverResponse1 response = simdClient.deliver(request);

            if (response != null && response.deliverResponse != null)
                return response.deliverResponse;
            else
                throw new ApplicationException(ServiceInterfaces.UnexpectedServiceResponse);
        }

        #region Private and internal methods

        /// <summary>
        /// Initializes an instance of the SealedImmediateMessageDeliveryClient.
        /// </summary>
        /// <param name="configurationName">Endpoint configuration name for the SIMD endpoint.</param>
        /// <param name="tlsCert">The client certificate to establish the TLS connection with.</param>
        private void InitializeClient(string configurationName, X509Certificate2 tlsCert)
        {
            this.simdMessages = new SoapInspector.SoapMessages();

            Nehta.SMD2010.SIMD.SealedImmediateMessageDeliveryClient client = null;
            if (!string.IsNullOrEmpty(configurationName))
            {
                client = new Nehta.SMD2010.SIMD.SealedImmediateMessageDeliveryClient(configurationName);
            }
            else
            {
                EndpointAddress address = new EndpointAddress("http://ns.electronichealth.net.au");
                CustomBinding tlsBinding = GetBinding();
                client = new Nehta.SMD2010.SIMD.SealedImmediateMessageDeliveryClient(tlsBinding, address);
            }

            if (client != null)
            {
                SoapInspector.InspectEndpoint(client.Endpoint, simdMessages);
                client.ClientCredentials.ClientCertificate.Certificate = tlsCert;
                simdClient = client;
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
            ClientBase<SealedImmediateMessageDelivery> client;

            if (simdClient is ClientBase<SealedImmediateMessageDelivery>)
            {
                client = (ClientBase<SealedImmediateMessageDelivery>)simdClient;
                if (client.State != CommunicationState.Closed)
                    client.Close();
            }
        }

        #endregion
    }
}
