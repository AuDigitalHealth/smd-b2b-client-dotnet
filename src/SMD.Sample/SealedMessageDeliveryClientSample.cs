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
using Nehta.VendorLibrary.Common;
using Nehta.SMD2010.SMD;
using System.Xml;

namespace Nehta.VendorLibrary.SM.SMD.Sample
{
    /// <summary>
    /// Requirements for using the SealedMessageDeliveryClient:
    /// a) A Transport Layer Security (TLS) X509Certificate2 certificate.
    ///    All clients require this key pair and certificate in order to authenticate the client to the SMD
    ///    Web Service providers during the Transport Layer Security (TLS) handshake.
    /// b) The digital certificate of the Certificate Authority (CA) which signed the SMD Web Service providers TLS certificate.
    ///    This certificate is used to authenticate the SMD Web Service provider to the clients during the TLS handshake.
    /// c) Your organisation's fully qualified Healthcare Provider Identifier or HPI-O and those to whom you wish to 
    ///    send and receive messages from.
    /// d) The endpoint URLs for your Sealed Message Delivery Web Service providers i.e. the endpoint URL of the receiver of
    ///    the Sealed Message or that of the receiver's intermediary.
    /// e) A valid SealedMessageType object to be delivered. This object's metadata should include a valid route record
    ///    to receive transport responses related to this message.
    /// </summary>
    public class SealedMessageDeliveryClientSample
    {
        public void Sample()
        {
            // Payload
            XmlDocument payload = new XmlDocument();
            payload.LoadXml("<data>Sample data to deliver.</data>");

            // TLS certificate used to authenticate the client to the SMD service during TLS connection.
            X509Certificate2 tlsCert = X509CertificateUtil.GetCertificate("TlsCertificateSerialNumber", X509FindType.FindBySerialNumber, StoreName.My, StoreLocation.CurrentUser, true);

            // Certificate used to sign the payload.
            X509Certificate2 signingCert = X509CertificateUtil.GetCertificate("SigningCertificateSerialNumber", X509FindType.FindBySerialNumber, StoreName.My, StoreLocation.CurrentUser, true);

            // Certificate used to encrypt the payload.
            X509Certificate2 payloadEncryptionCert = X509CertificateUtil.GetCertificate("EncryptionCertificateSerialNumber", X509FindType.FindBySerialNumber, StoreName.My, StoreLocation.CurrentUser, true);

            // Instantiate the client.
            SealedMessageDeliveryClient client = new SealedMessageDeliveryClient(tlsCert);

            // Set up metadata
            MessageMetadataType metadata = new MessageMetadataType();
            metadata.creationTime = DateTime.Now.ToUniversalTime();
            metadata.expiryTime = DateTime.Now.AddDays(30).ToUniversalTime();
            metadata.expiryTimeSpecified = true;
            metadata.invocationId = new UniqueId().ToString(); 
            metadata.receiverIndividual = HIQualifiers.HPIIQualifier + "16 digit receiver HPII";
            metadata.receiverOrganisation = HIQualifiers.HPIOQualifier + "16 digit receiver organisation HPIO";
            metadata.senderIndividual = HIQualifiers.HPIIQualifier + "16 digit sender HPII";
            metadata.senderOrganisation = HIQualifiers.HPIOQualifier + "16 digit sender organisation HPIO";
            metadata.serviceCategory = "ServiceCategory";
            metadata.serviceInterface = ServiceInterfaces.SmdServiceInterface;
            
            // Set up route record. Route records are used to indicate where transport responses
            // are to be sent to.
            metadata.routeRecord = new RouteRecordEntryType[1];
            metadata.routeRecord[0] = new RouteRecordEntryType();
            metadata.routeRecord[0].sendIntermediateResponses = false;
            metadata.routeRecord[0].interaction = new InteractionType();
            metadata.routeRecord[0].interaction.target = HIQualifiers.HPIOQualifier + "16 digit HPIO of the transport response target";
            metadata.routeRecord[0].interaction.serviceCategory = "ServiceCategory";
            metadata.routeRecord[0].interaction.serviceEndpoint = "https://TransportResponseDeliveryEndpoint";
            metadata.routeRecord[0].interaction.serviceInterface = ServiceInterfaces.TrdServiceInterface;
            metadata.routeRecord[0].interaction.serviceProvider = HIQualifiers.HPIOQualifier + "16 digit HPIO of the TRD service provider";
            
            // Obtain a SealedMessageType instance.
            SealedMessageType sealedMessage = SealedMessageDeliveryClient.GetSealedMessage(payload, metadata, signingCert, payloadEncryptionCert);
            
            // Invoke the Deliver operation on the client.
            deliverResponse response = client.Deliver(sealedMessage, new Uri("https://SMDServiceEndpoint"));
        }
    }
}
