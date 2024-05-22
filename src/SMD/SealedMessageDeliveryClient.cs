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
using Nehta.SMD2010.SMD;
using System.ServiceModel.Channels;
using System.Xml;
 
namespace Nehta.VendorLibrary.SM.SMD
{
    /// <summary>
    /// An implementation of a client for the Sealed Message Delivery (SMD) service.  
    /// </summary>
    public class SealedMessageDeliveryClient : IDisposable
    {
        internal SealedMessageDelivery smdClient;
        internal SoapInspector.SoapMessages  smdMessages;

        /// <summary>
        /// Gets the soap request xml for the last service call.
        /// </summary>
        public string LastSoapRequest
        {
            get
            {
                return smdMessages.SoapRequest;
            }
        }

        /// <summary>
        /// Gets the soap response xml for the last service call.
        /// </summary>
        public string LastSoapResponse
        {
            get
            {
                return smdMessages.SoapResponse;
            }
        }

        #region Constructors

        /// <summary>
        /// Initializes an instance of the SealedMessageDeliveryClient.
        /// </summary>
        /// <param name="tlsCert">The client certificate to be used to establish the TLS connection.</param>
        public SealedMessageDeliveryClient(X509Certificate2 tlsCert)
        {
            Validation.ValidateArgumentRequired("tlsCert", tlsCert);
            InitializeClient(null, tlsCert);
        }

        /// <summary>
        /// Initializes an instance of the SealedMessageDeliveryClient.
        /// </summary>
        /// <param name="configurationName">Endpoint configuration name for the SMD endpoint.</param>
        /// <param name="tlsCert">The certificate to be used to establish the TLS connection.</param>
        public SealedMessageDeliveryClient(string configurationName, X509Certificate2 tlsCert)
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
            Validation.ValidateStringMatch("metadata.serviceInterface", metadata.serviceInterface, ServiceInterfaces.SmdServiceInterface);

            if (metadata.routeRecord != null && metadata.routeRecord.Length > 0)
            {
                for (int x = 0; x < metadata.routeRecord.Length; x++)
                {
                    // Validate interaction type
                    InteractionType it = metadata.routeRecord[x].interaction;
                    Validation.ValidateArgumentRequired(string.Format("metadata.routeRecord[{0}].interaction", x), it);
                    Validation.ValidateArgumentRequired(string.Format("metadata.routeRecord[{0}].interaction.serviceCategory", x), it.serviceCategory);
                    Validation.ValidateArgumentRequired(string.Format("metadata.routeRecord[{0}].interaction.serviceEndpoint", x), it.serviceEndpoint);
                    Validation.ValidateArgumentRequired(string.Format("metadata.routeRecord[{0}].interaction.serviceInterface", x), it.serviceInterface);
                    Validation.ValidateArgumentRequired(string.Format("metadata.routeRecord[{0}].interaction.serviceProvider", x), it.serviceProvider);
                    Validation.ValidateArgumentRequired(string.Format("metadata.routeRecord[{0}].interaction.target", x), it.target);
                    Validation.ValidateStringMatch(string.Format("metadata.routeRecord[{0}].interaction.serviceInterface", x), it.serviceInterface, ServiceInterfaces.TrdServiceInterface);

                    // Validate cert refs if available
                    if (it.certRef != null && it.certRef.Length > 0)
                    {
                        for (int y = 0; y < it.certRef.Length; y++)
                        {
                            Validation.ValidateArgumentRequired(string.Format("metadata.routeRecord[{0}].interaction.certRef[{1}].useQualifier", x, y), it.certRef[y].useQualifier);
                            Validation.ValidateArgumentRequired(string.Format("metadata.routeRecord[{0}].interaction.certRef[{1}].qualifiedCertRef", x, y), it.certRef[y].qualifiedCertRef);
                            Validation.ValidateArgumentRequired(string.Format("metadata.routeRecord[{0}].interaction.certRef[{1}].qualifiedCertRef.type", x, y), it.certRef[y].qualifiedCertRef.type);
                            Validation.ValidateArgumentRequired(string.Format("metadata.routeRecord[{0}].interaction.certRef[{1}].qualifiedCertRef.value", x, y), it.certRef[y].qualifiedCertRef.value);
                        }
                    }
                }
            }

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
        /// Deliver a Sealed Message to a Receiver or Intermediary.
        /// </summary>
        /// <param name="message">The message to deliver.</param>
        /// <param name="endpointUrl">The endpoint of the SMD service.</param>
        /// <returns>A value indicating if the operation is successful.</returns>
        public deliverResponse Deliver(SealedMessageType message, Uri endpointUrl)
        {
            Validation.ValidateArgumentRequired("message", message);
            Validation.ValidateArgumentRequired("endpointUrl", endpointUrl);

            // Validate metadata
            ValidateMetadata(message.metadata);

            // Validate encrypted payload
            Validation.ValidateArgumentRequired("message.encryptedPayload", message.encryptedPayload);

            if (smdClient is Nehta.SMD2010.SMD.SealedMessageDeliveryClient)
            {
                Nehta.SMD2010.SMD.SealedMessageDeliveryClient client = (Nehta.SMD2010.SMD.SealedMessageDeliveryClient)smdClient;
                client.Endpoint.Address = new EndpointAddress(endpointUrl);
            }

            deliverRequest request = new deliverRequest();
            request.deliver = new deliver();
            request.deliver.message = message;

            deliverResponse1 response = smdClient.deliver(request);

            if (response != null && response.deliverResponse != null)
                return response.deliverResponse;
            else
                throw new ApplicationException(ServiceInterfaces.UnexpectedServiceResponse);
        }

        #region Private and internal methods

        /// <summary>
        /// Initializes an instance of the SealedMessageDeliveryClient.
        /// </summary>
        /// <param name="configurationName">Endpoint configuration name for the SMD endpoint.</param>
        /// <param name="tlsCert">The client certificate to be used to establish the TLS connection.</param>
        private void InitializeClient(string configurationName, X509Certificate2 tlsCert)
        {
            this.smdMessages = new SoapInspector.SoapMessages();

            Nehta.SMD2010.SMD.SealedMessageDeliveryClient client = null;
            if (!string.IsNullOrEmpty(configurationName))
            {
                client = new Nehta.SMD2010.SMD.SealedMessageDeliveryClient(configurationName);
            }
            else
            {
                EndpointAddress address = new EndpointAddress("https://ns.electronichealth.net.au");
                CustomBinding tlsBinding = GetBinding();
                client = new Nehta.SMD2010.SMD.SealedMessageDeliveryClient(tlsBinding, address);
            }

            if (client != null)
            {
                SoapInspector.InspectEndpoint(client.Endpoint, smdMessages);
                client.ClientCredentials.ClientCertificate.Certificate = tlsCert;
                smdClient = client;
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
            ClientBase<SealedMessageDelivery> client;

            if (smdClient is ClientBase<SealedMessageDelivery>)
            {
                client = (ClientBase<SealedMessageDelivery>)smdClient;
                if (client.State != CommunicationState.Closed)
                    client.Close();
            }
        }

        #endregion
        
    }
}
