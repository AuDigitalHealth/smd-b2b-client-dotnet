using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nehta.VendorLibrary.Common;

namespace Nehta.VendorLibrary.SM.SMD
{    
    /// <summary>
    /// A wrapper class for SealedMessageType existing in the SMD, SMR, SIMD and TRD namespaces.
    /// </summary>
    public class CommonSealedMessageType
    {
        /// <summary>
        /// Service category of the sealed message.
        /// </summary>
        public string ServiceCategory { get; private set; }

        /// <summary>
        /// Invocation ID of the sealed message.
        /// </summary>
        public string InvocationId { get; private set; }

        /// <summary>
        /// Receiver organisation of the sealed message.
        /// </summary>
        public string ReceiverOrganisation { get; private set; }

        /// <summary>
        /// Sender organisation of the sealed message.
        /// </summary>
        public string SenderOrganisation { get; private set; }

        /// <summary>
        /// Encrypted payload of the sealed message.
        /// </summary>
        public object EncryptedPayload { get; private set; }

        private CommonSealedMessageType()
        {
        }

        /// <summary>
        /// Create an instance of CommonSealedMessageType.
        /// </summary>
        /// <param name="m">The SealedMessageType instance to copy.</param>
        public CommonSealedMessageType(Nehta.SMD2010.TRD.SealedMessageType m)
        {
            ServiceCategory = m.metadata.serviceCategory;
            InvocationId = m.metadata.invocationId;
            ReceiverOrganisation = m.metadata.receiverOrganisation;
            SenderOrganisation = m.metadata.senderOrganisation;
            EncryptedPayload = (object)m.encryptedPayload;

            ValidateCommonSealedMessageType();
        }

        /// <summary>
        /// Create an instance of CommonSealedMessageType.
        /// </summary>
        /// <param name="m">The SealedMessageType instance to copy.</param>
        public CommonSealedMessageType(Nehta.SMD2010.SMD.SealedMessageType m)
        {
            ServiceCategory = m.metadata.serviceCategory;
            InvocationId = m.metadata.invocationId;
            ReceiverOrganisation = m.metadata.receiverOrganisation;
            SenderOrganisation = m.metadata.senderOrganisation;
            EncryptedPayload = (object)m.encryptedPayload;

            ValidateCommonSealedMessageType();
        }

        /// <summary>
        /// Create an instance of CommonSealedMessageType.
        /// </summary>
        /// <param name="m">The SealedMessageType instance to copy.</param>
        public CommonSealedMessageType(Nehta.SMD2010.SIMD.SealedMessageType m)
        {
            ServiceCategory = m.metadata.serviceCategory;
            InvocationId = m.metadata.invocationId;
            ReceiverOrganisation = m.metadata.receiverOrganisation;
            SenderOrganisation = m.metadata.senderOrganisation;
            EncryptedPayload = (object)m.encryptedPayload;

            ValidateCommonSealedMessageType();
        }

        /// <summary>
        /// Create an instance of CommonSealedMessageType.
        /// </summary>
        /// <param name="m">The SealedMessageType instance to copy.</param>
        public CommonSealedMessageType(Nehta.SMD2010.SMR.SealedMessageType m)
        {
            ServiceCategory = m.metadata.serviceCategory;
            InvocationId = m.metadata.invocationId;
            ReceiverOrganisation = m.metadata.receiverOrganisation;
            SenderOrganisation = m.metadata.senderOrganisation;
            EncryptedPayload = (object)m.encryptedPayload;

            ValidateCommonSealedMessageType();
        }

        /// <summary>
        /// Validate an instantiated CommonSealedMessageType.
        /// </summary>
        private void ValidateCommonSealedMessageType()
        {
            Validation.ValidateArgumentRequired("metadata.serviceCategory", ServiceCategory);
            Validation.ValidateArgumentRequired("metadata.invocationId", InvocationId);
            Validation.ValidateArgumentRequired("metadata.receiverOrganisation", ReceiverOrganisation);
            Validation.ValidateArgumentRequired("metadata.senderOrganisation", SenderOrganisation);
            Validation.ValidateArgumentRequired("encryptedPayload", EncryptedPayload);
        }
    }
}
