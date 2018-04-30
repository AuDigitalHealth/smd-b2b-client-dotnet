using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nehta.VendorLibrary.SM.SMD
{
    /// <summary>
    /// Service interface values for Secure Message Delivery operations.
    /// </summary>
    public class ServiceInterfaces
    {
        /// <summary>
        /// Sealed Immediate Message Delivery
        /// </summary>
        public const string SimdServiceInterface = "http://ns.electronichealth.net.au/smd/intf/SealedImmediateMessageDelivery/TLS/2010";
        
        /// <summary>
        /// Sealed Message Delivery
        /// </summary>
        public const string SmdServiceInterface = "http://ns.electronichealth.net.au/smd/intf/SealedMessageDelivery/TLS/2010";
        
        /// <summary>
        /// Transport Response Delivery
        /// </summary>
        public const string TrdServiceInterface = "http://ns.electronichealth.net.au/smd/intf/TransportResponseDelivery/TLS/2010";
        
        /// <summary>
        /// Transport Response Retrieval
        /// </summary>
        public const string TrrServiceInterface = "http://ns.electronichealth.net.au/smd/intf/TransportResponseRetrieval/TLS/2010";

        /// <summary>
        /// Sealed Message Retrieval
        /// </summary>
        public const string SmrServiceInterface = "http://ns.electronichealth.net.au/smd/intf/SealedMessageRetrieval/TLS/2010";
    }
}
