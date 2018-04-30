using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Security.Cryptography.X509Certificates;
using Nehta.Xsp;
using Nehta.VendorLibrary.Common;

namespace Nehta.VendorLibrary.SM.SMD
{
    /// <summary>
    /// Extension methods to assist with SMD clients.
    /// </summary>
    public static class SmdExtensions
    {
        /// <summary>
        /// Sign an XML document according to the XSPP.
        /// </summary>
        /// <param name="document">The XML document to be signed.</param>
        /// <param name="signingCert">The certificate to sign the XML document with.</param>
        /// <returns>The signed XML document.</returns>
        public static XmlDocument XspSign(this XmlDocument document, X509Certificate2 signingCert)
        {
            ISignedContainerProfileService signedContainerService = XspFactory.Instance.GetSignedContainerProfileService(XspVersion.V_2010);
            return signedContainerService.Create(document, signingCert);
        }

        /// <summary>
        /// Encrypt an XML document according to the XSPP.
        /// </summary>
        /// <param name="document">The XML document to be encrypted.</param>
        /// <param name="encryptionCert">The certificate to encrypt the XML document with.</param>
        /// <returns>The encrypted XML document.</returns>
        public static XmlDocument XspEncrypt(this XmlDocument document, X509Certificate2 encryptionCert)
        {
            IEncryptedContainerProfileService encryptedContainerService = XspFactory.Instance.GetEncryptedContainerProfileService(XspVersion.V_2010);
            return encryptedContainerService.Create(document, encryptionCert);
        }

        /// <summary>
        /// Decrypt an XML document according to the XSPP.
        /// </summary>
        /// <param name="encryptedDocument">The XML document to be decrypted.</param>
        /// <param name="decryptionCert">The certificate to decrypt the XML document with.</param>
        /// <returns>The decrypted XML document.</returns>
        public static XmlDocument XspDecrypt(this XmlDocument encryptedDocument, X509Certificate2 decryptionCert)
        {
            IEncryptedContainerProfileService encryptedContainerService = XspFactory.Instance.GetEncryptedContainerProfileService(XspVersion.V_2010);
            return encryptedContainerService.GetData(encryptedDocument, decryptionCert);
        }

        /// <summary>
        /// Verify a signed XML document.
        /// </summary>
        /// <param name="signedDocument">The signed XML document to verify.</param>
        /// <param name="certificateVerifier">An ICertificateVerifier implementation which will verify the certificate in the signed XML document.</param>
        /// <returns>A value to indicate if the document is verified successfully.</returns>
        public static bool XspVerifySignature(this XmlDocument signedDocument, ICertificateVerifier certificateVerifier)
        {
            Validation.ValidateArgumentRequired("signedDocument", signedDocument);
            Validation.ValidateArgumentRequired("certificateVerifier", certificateVerifier);

            ISignedContainerProfileService signedContainerService = XspFactory.Instance.GetSignedContainerProfileService(XspVersion.V_2010);
            signedContainerService.Check(signedDocument, certificateVerifier);

            return true;
        }

        /// <summary>
        /// Extract a payload from a signed document.
        /// </summary>
        /// <param name="signedDocument">The signed XML document to extract the payload from.</param>
        /// <returns>The XML document containing the payload.</returns>
        public static XmlDocument XspGetPayloadFromSignedDocument(this XmlDocument signedDocument)
        {
            ISignedContainerProfileService signedContainerService = XspFactory.Instance.GetSignedContainerProfileService(XspVersion.V_2010);
            return signedContainerService.GetData(signedDocument);
        }

        /// <summary>
        /// Get the digest values from a signed XML Document.
        /// </summary>
        /// <param name="signedDocument">The signed XML document.</param>
        /// <returns>A list of digest values.</returns>
        public static IList<byte[]> XspGetDigestValues(this XmlDocument signedDocument)
        {
            ISignedContainerProfileService signedContainerService = XspFactory.Instance.GetSignedContainerProfileService(XspVersion.V_2010);
            return signedContainerService.GetDigestValues(signedDocument);
        }

        /// <summary>
        /// Adds a new ElsCertRefType to a generic list of ElsCertRefType.
        /// </summary>
        /// <param name="certRefs">The current list of ElsCertRefType.</param>
        /// <param name="useQualifier">The use qualifier of the certificate.</param>
        /// <param name="type">The type of certificate.</param>
        /// <param name="value">The value of the certificate.</param>
        public static void AddCertRef(this List<Nehta.SMD2010.SMD.ElsCertRefType> certRefs, string useQualifier, string type, string value)
        {
            Validation.ValidateArgumentRequired("useQualifier", useQualifier);
            Validation.ValidateArgumentRequired("type", type);
            Validation.ValidateArgumentRequired("value", value);

            certRefs.Add(new Nehta.SMD2010.SMD.ElsCertRefType()
            {
                useQualifier = useQualifier,
                qualifiedCertRef = new Nehta.SMD2010.SMD.QualifiedCertRefType()
                {
                    type = type,
                    value = value
                }
            });
        }

        /// <summary>
        /// Adds a new ElsCertRefType to a generic list of ElsCertRefType.
        /// </summary>
        /// <param name="certRefs">The current list of ElsCertRefType.</param>
        /// <param name="useQualifier">The use qualifier of the certificate.</param>
        /// <param name="type">The type of certificate.</param>
        /// <param name="value">The value of the certificate.</param>
        public static void AddCertRef(this List<Nehta.SMD2010.SMR.ElsCertRefType> certRefs, string useQualifier, string type, string value)
        {
            Validation.ValidateArgumentRequired("useQualifier", useQualifier);
            Validation.ValidateArgumentRequired("type", type);
            Validation.ValidateArgumentRequired("value", value);

            certRefs.Add(new Nehta.SMD2010.SMR.ElsCertRefType()
            {
                useQualifier = useQualifier,
                qualifiedCertRef = new Nehta.SMD2010.SMR.QualifiedCertRefType()
                {
                    type = type,
                    value = value
                }
            });
        }

        /// <summary>
        /// Adds a new ElsCertRefType to a generic list of ElsCertRefType.
        /// </summary>
        /// <param name="certRefs">The current list of ElsCertRefType.</param>
        /// <param name="useQualifier">The use qualifier of the certificate.</param>
        /// <param name="type">The type of certificate.</param>
        /// <param name="value">The value of the certificate.</param>
        public static void AddCertRef(this List<Nehta.SMD2010.TRD.ElsCertRefType> certRefs, string useQualifier, string type, string value)
        {
            Validation.ValidateArgumentRequired("useQualifier", useQualifier);
            Validation.ValidateArgumentRequired("type", type);
            Validation.ValidateArgumentRequired("value", value);

            certRefs.Add(new Nehta.SMD2010.TRD.ElsCertRefType()
            {
                useQualifier = useQualifier,
                qualifiedCertRef = new Nehta.SMD2010.TRD.QualifiedCertRefType()
                {
                    type = type,
                    value = value
                }
            });
        }
    }
}
