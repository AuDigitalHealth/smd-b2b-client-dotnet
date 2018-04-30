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
using Nehta.SMD2010.TRD;
using System.Xml;

namespace Nehta.VendorLibrary.SM.SMD.Sample
{
    /// <summary>
    /// Requirements for using the TransportResponseDeliveryClient:
    /// a) A Transport Layer Security (TLS) X509Certificate2 certificate.
    ///    All clients require this key pair and certificate in order to authenticate the client to the TRD
    ///    Web Service providers during the Transport Layer Security (TLS) handshake.
    /// b) The digital certificate of the Certificate Authority (CA) which signed the SMD Web Service providers TLS certificate.
    ///    This certificate is used to authenticate the SMD Web Service provider to the clients during the TLS handshake.
    /// c) Your organisation's fully qualified Healthcare Provider Identifier or HPI-O and those to whom you wish to 
    ///    send and receive messages from.
    /// d) The endpoint URLs for your Transport Response Delivery Web Service providers i.e. the endpoint URL of the
    ///    the receiver of the Transport Response or that of its intermediary.
    /// e) A valid TransportResponseType object to be delivered. This object's metadata should include a valid digest value
    ///    and Transport Response code to deliver the Transport Response.
    /// </summary>
    public class TransportResponseDeliveryClientSample
    {
        public void Sample()
        {
            // This example shows a Transport Response Delivery (TRD) message creation after a message
            // is received via Sealed Message Delivery (SMD). A static helper method is provided on
            // the client to help generate a TransportResponseType instance from a SealedMessageType
            // instance received via SMD, SIMD, or SMR.

            // TLS certificate used to authenticate the client to the TRD service during TLS connection.
            X509Certificate2 tlsCert = X509CertificateUtil.GetCertificate("TlsCertificateSerialNumber", X509FindType.FindBySerialNumber, StoreName.My, StoreLocation.CurrentUser, true);

            // Certificate used to decrypt a received SealedMessageType payload.
            X509Certificate2 payloadDecryptionCert = X509CertificateUtil.GetCertificate("PayloadDecryptionCertificateSerialNumber", X509FindType.FindBySerialNumber, StoreName.My, StoreLocation.CurrentUser, true);

            // Set up client.
            TransportResponseDeliveryClient trdClient = new TransportResponseDeliveryClient(tlsCert);

            // The SealedMessageType instance to generate a Transport Response for.
            // Typically, this would be available after an SMD, SIMD or SMR service invocation.
            Nehta.SMD2010.SMD.SealedMessageType sealedMessage = new Nehta.SMD2010.SMD.SealedMessageType();

            // Generate the Transport Response for the Sealed Message.
            TransportResponseType transportResponse = TransportResponseDeliveryClient.GetTransportResponse(
                new CommonSealedMessageType(sealedMessage),
                HIQualifiers.HPIOQualifier + "16 digit HPIO of sender organisation",
                ResponseClassType.Success,
                "DeliveryResponseCode",
                "DeliveryResponseMessage",
                DateTime.Now.ToUniversalTime(),
                new UniqueId().ToString(),
                true,
                null,
                payloadDecryptionCert
            );

            // Add the generated Transport Response to a list.
            List<TransportResponseType> transportResponses = new List<TransportResponseType>();
            transportResponses.Add(transportResponse);

            // Invoke the Deliver operation on the client, passing in the list of Transport Responses.
            deliverResponse response = trdClient.Deliver(transportResponses, new Uri("https://TRDServiceEndpoint"));
        }
    }
}
