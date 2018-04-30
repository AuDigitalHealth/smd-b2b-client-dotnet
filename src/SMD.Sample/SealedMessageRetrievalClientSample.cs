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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using Nehta.VendorLibrary.Common;
using Nehta.SMD2010.SMR;
using System.Xml;

namespace Nehta.VendorLibrary.SM.SMD.Sample
{
    /// <summary>
    /// Requirements for using the SealedMessageRetrievalClientSample:
    /// a) A Transport Layer Security (TLS) X509Certificate2 certificate.
    ///    All clients require this key pair and certificate in order to authenticate the client to the SMR
    ///    Web Service providers during the Transport Layer Security (TLS) handshake.
    /// b) The digital certificate of the Certificate Authority (CA) which signed the SMD Web Service providers TLS certificate.
    ///    This certificate is used to authenticate the SMD Web Service provider to the clients during the TLS handshake.
    /// c) Your organisation's fully qualified Healthcare Provider Identifier or HPI-O and those to whom you wish to 
    ///    send and receive messages from.
    /// d) The endpoint URL for your Sealed Message Retrieval Web Service providers i.e. the endpoint URL of the client
    ///    system's intermediary.
    /// </summary>
    class SealedMessageRetrievalClientSample
    {
        public void Sample()
        {
            // TLS certificate used to authenticate the client to the SMR service during TLS connection.
            X509Certificate2 tlsCert = X509CertificateUtil.GetCertificate("TlsCertificateSerialNumber", X509FindType.FindBySerialNumber, StoreName.My, StoreLocation.CurrentUser, true);

            // Certificate used to sign the payload.
            X509Certificate2 signingCert = X509CertificateUtil.GetCertificate("SigningCertificateSerialNumber", X509FindType.FindBySerialNumber, StoreName.My, StoreLocation.CurrentUser, true);

            // Certificate used to encrypt the payload.
            X509Certificate2 payloadDecryptionCert = X509CertificateUtil.GetCertificate("DecryptionCertificateSerialNumber", X509FindType.FindBySerialNumber, StoreName.My, StoreLocation.CurrentUser, true);

            // Instantiate client
            SealedMessageRetrievalClient client = new SealedMessageRetrievalClient(tlsCert);

            // ------------------------------------------------------------------------------
            // List
            // ------------------------------------------------------------------------------

            // Set up request
            list listRequest = new list()
            {
                allAvailable = true,
                limit = 0,
                receiverOrganisation = HIQualifiers.HPIOQualifier + "16 digit HPIO of receiver organisation"
            };

            // Invoke the List operation
            MessageListType messageList = client.List(listRequest, new Uri("https://SMRServiceEndpointUri"));

            // ------------------------------------------------------------------------------
            // Retrieve
            // ------------------------------------------------------------------------------

            // Get list of invocation IDs obtained from List operation
            string[] invocationIds = messageList.retrievalRecord.Select(r => r.metadata.invocationId).ToArray();

            // Set up request
            retrieve retrieveRequest = new retrieve()
            {
                invocationId = invocationIds,
                receiverOrganisation = HIQualifiers.HPIOQualifier + "16 digit HPIO of receiver organisation"
            };

            // Invoke the Retrieve operation
            SealedMessageType[] sealedMessages = client.Retrieve(retrieveRequest, new Uri("https://SMRServiceEndpointUri"));

            // ------------------------------------------------------------------------------
            // Obtaining the payload from the Sealed Messages
            // ------------------------------------------------------------------------------

            // Obtain the first Sealed Message to decrypt
            SealedMessageType sealedMessage = sealedMessages[0];

            // Serialize the encrypted payload
            XmlDocument encryptedPayload = sealedMessage.encryptedPayload.SerializeToXml("encryptedPayload");

            // Decrypt the payload, obtaining the signed payload
            XmlDocument decryptedPayload = encryptedPayload.XspDecrypt(payloadDecryptionCert);

            // Obtain the original payload from the signed payload
            XmlDocument originalPayload = decryptedPayload.XspGetPayloadFromSignedDocument();
        }
    }
}
