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
using Nehta.SMD2010.TRR;

namespace Nehta.VendorLibrary.SM.SMD.Sample
{
    /// <summary>
    /// Requirements for using the TransportResponseRetrievalClient:
    /// a) A Transport Layer Security (TLS) X509Certificate2 certificate.
    ///    All clients require this key pair and certificate in order to authenticate the client to the TRR
    ///    Web Service providers during the Transport Layer Security (TLS) handshake.
    /// b) The digital certificate of the Certificate Authority (CA) which signed the SMD Web Service providers TLS certificate.
    ///    This certificate is used to authenticate the SMD Web Service provider to the clients during the TLS handshake.
    /// c) Your organisation's fully qualified Healthcare Provider Identifier or HPI-O and those to whom you wish to 
    ///    send and receive messages from.
    /// d) The endpoint URLs for your Transport Response Retrieval Web Service providers i.e. the endpoint URL of the
    ///    client's intermediary.
    /// </summary>
    public class TransportResponseRetrievalClientSample
    {
        public void Sample()
        {
            // TLS certificate used to authenticate the client to the TRR service during TLS connection.
            X509Certificate2 tlsCert = X509CertificateUtil.GetCertificate("TlsCertificateSerialNumber", X509FindType.FindBySerialNumber, StoreName.My, StoreLocation.CurrentUser, true);

            // Instantiate client
            TransportResponseRetrievalClient client = new TransportResponseRetrievalClient(tlsCert);

            // ------------------------------------------------------------------------------
            // Retrieve
            // ------------------------------------------------------------------------------

            // Set up request
            retrieve retrieveRequest = new retrieve()
            {
                allAvailable = true,
                limit = 0,
                organisation = HIQualifiers.HPIOQualifier + "16 digit HPIO of receiver organisation"
            };

            // Invoke the Retrieve operation
            TransportResponseListType responseList = client.Retrieve(retrieveRequest, new Uri("https://TRREndpointUri"));

            // ------------------------------------------------------------------------------
            // Remove
            // ------------------------------------------------------------------------------

            // Build list of retrieved transport response IDs
            string[] responseIds = responseList.response.Select(r => r.metadata.responseId).ToArray();

            // Set up request
            remove removeRequest = new remove()
            {
                force = true,
                organisation = HIQualifiers.HPIOQualifier + "16 digit HPIO of receiver organisation",
                responseId = responseIds
            };

            // Invoke the Remove operation
            RemoveResultType[] removeResults = client.Remove(removeRequest, new Uri("https://TRREndpointUri"));
        }
    }
}
