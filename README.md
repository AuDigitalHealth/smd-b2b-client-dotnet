# Secure Messaging

This is a software library that provides example client implementations of the
NEHTA Secure Message Delivery (SMD) specifications using .NET.

The Secure Message Delivery specification is documented in Standards Australia's

Australian Technical Specification ATS 5822-2010 and is available from the
following URL:
    
http://infostore.saiglobal.com/store/portal.aspx?portal=Informatics


Setup
=====

- To build and test the distributable package, Visual Studio 2008 must be installed.
- Start up SMD.sln.
- For documentation on the HI library, refer to Help/Index.html.



Solution
========

The solution consists of two projects:

SMD: The 'Nehta.VendorLibrary.SM.SMD' project contains the implementation of Secure Messaging Delivery (SMD)
clients. 

Nehta.Xsp: The 'Nehta.Xsp' project contains helper implementations of the Xml Secured Payload Profiles (XSPP).

Common: The 'Nehta.VendorLibrary.Common' project contains helper libraries common across
all NEHTA vendor library components.



Building and using the library
==============================

The solution can be built using 'ctrl-shift-b'. The compiled assembly can then be referenced
where the clients will be available.



Client instantiation
=====================
The SMD client library consists of the following five distinct Web Service clients classes:

  1. SealedMessageDeliveryClient
  2. SealedMessageRetrievalClient
  3. TransportResponseDeliveryClient
  4. TransportResponseRetrievalClient
  5. SealedImmediateMessageDeliveryClient

1. Requirements:

   a) A Transport Layer Security (TLS) X509Certificate2 certificate.
      All clients require this key pair and certificate in order to authenticate the client to the SMD
      Web Service providers during the Transport Layer Security (TLS) handshake.

   b) The digital certificate of the Certificate Authority (CA) which signed the SMD Web Service providers TLS certificate.
      This certificate is used to authenticate the SMD Web Service provider to the clients during the TLS handshake.

   c) Your organisation's fully qualified Healthcare Provider Identifier or HPI-O and those to whom you wish to 
      send and receive messages from.

2. Code example:

   The following code snippets demonstrate example instantiations of the five SMD Clients:

   -----------------------------------------------------------------------------------------------------------------
   // Sealed Message Delivery Client:
   SealedMessageDeliveryClient smdClient = new SealedMessageDeliveryClient(tlsCert);

   // Sealed Message Retrieval Client:
   SealedMessageRetrievalClient smrClient = new SealedMessageRetrievalClient(tlsCert);

   // Transport Response Delivery Client:
   TransportResponseDeliveryClient trdClient = new TransportResponseDeliveryClient(tlsCert);
   
   // Transport Response Retrieval Client:   
   TransportResponseRetrievalClient trrClient = new TransportResponseRetrievalClient(tlsCert);

   // Sealed Immediate Message Delivery Client:
   SealedImmediateMessageDeliveryClient simdClient = new SealedImmediateMessageDeliveryClient(tlsCert);

   -----------------------------------------------------------------------------------------------------------------


   
Client Usage
============

This section describes how the five different clients may be used. This section assumes that you have met the
requirements above and have successfully instantiated the relevant clients. Please refer to the help file (Help/Index.html)
for more detailed descriptions of the client classes, methods and their arguments.

Sealed Message Delivery Client
------------------------------

1. Requirements
   a) The endpoint URLs for your Sealed Message Delivery Web Service providers i.e. the endpoint URL of the receiver of
      the Sealed Message or that of the receiver's intermediary.

   b) A valid SealedMessageType object to be delivered. This object's metadata should include a valid route record
      to receive transport responses related to this message.

2. Code example:

   The following code snippets demonstrates an example usage of the SealedMessageDeliveryClient's deliver method:

   -----------------------------------------------------------------------------------------------------------------
   DeliverStatusType deliverStatusType = smdClient.Deliver(sealedMessage,
                                                           smdEndpointURI);
   -----------------------------------------------------------------------------------------------------------------

Sealed Message Retrieval Client
-------------------------------

1. Requirements
   a) The endpoint URL for your Sealed Message Retrieval Web Service providers i.e. the endpoint URL of the client
      system's intermediary.

2. Code examples:

   The following code snippets demonstrates an example usage of the SealedMessageRetrievalClient's list method:

   --------------------------------------------------------------------------------------------------------------------
   MessageListType messageListType = smrClient.List(listRequest,
                                                    smrEndpointURI);
   --------------------------------------------------------------------------------------------------------------------

   The following code snippets demonstrates an example usage of the SealedMessageRetrievalClient's retrieve method:

   -----------------------------------------------------------------------------------------------------------------
   SealedMessageType retrievedMessage = smrClient.Retrieve(retrieveRequest,
                                                           smrEndpointURI);
   -----------------------------------------------------------------------------------------------------------------

Transport Response Delivery Client
----------------------------------

1. Requirements
   a) The endpoint URLs for your Transport Response Delivery Web Service providers i.e. the endpoint URL of the
      the receiver of the Transport Response or that of its intermediary.

   b) A valid TransportResponseType object to be delivered. This object's metadata should include a valid digest value
      and Transport Response code to deliver the Transport Response.

2. Code example:

   The following code snippets demonstrates an example usage of the TransportResponseDeliveryClient's deliver method:

   -----------------------------------------------------------------------------------------------------------------
   DeliverStatusType deliverStatusType = trdClient.Deliver(transportResponseTypeList,
                                                           trdEndpointURL);
   -----------------------------------------------------------------------------------------------------------------   
   
Transport Response Retrieval Client
------------------------------

1. Requirements
   a) The endpoint URLs for your Transport Response Retrieval Web Service providers i.e. the endpoint URL of the
      client's intermediary.

2. Code example:

   The following code snippets demonstrates an example usage of the TransportResponseRetrievalClient's remove method:

   --------------------------------------------------------------------------------------------------------------------
   List<RemoveResultType> removeResultTypes = trrClient.Remove(removeRequest,
                                                               trrEndpointURL);
   --------------------------------------------------------------------------------------------------------------------
   
   The following code snippets demonstrates an example usage of the TransportResponseRetrievalClient's retrieve method:
   --------------------------------------------------------------------------------------------------------------------
   TransportResponseListType transportResponseListType = trrClient.Retrieve(retrieveRequest,
                                                                            trrEndpointURL);
   --------------------------------------------------------------------------------------------------------------------
      
Sealed Immediate Message Delivery Client
----------------------------------------

1. Requirements
   a) The endpoint URLs for your Sealed Immediate Message Delivery Web Service providers i.e. the receiver of your
      sealed message who will synchronously respond with a Sealed Message.

2. Code examples:

   The following code snippets demonstrates an example usage of the SealedImmediateMessageDeliveryClient's deliver
   method:
   ---------------------------------------------------------------------------------------------------------------
   SealedMessageType responseMessage = simdClient.Deliver(sealedMessage,
                                                          simdEndpointURL)
   ---------------------------------------------------------------------------------------------------------------



Generating service interfaces and classes from the WSDL and XSD files
=====================================================================

1. With the installation of VS2008, the SvcUtil.exe tool should be installed.

2. Run the following command (or compile it into an executable *.cmd file):

   -------------------------
   REM : This command has to be executed in the "WsdlAndXsd" directory
   REM : Set WSDLTOOL to the location of the local SvcUtil.exe installation

   SET WSDLTOOL="c:\Program Files\Microsoft SDKs\Windows\v6.0A\Bin\SvcUtil.exe" /noLogo

   %WSDLTOOL%  /useSerializerForFaults /serializer:XmlSerializer wsdl\smd-SealedImmediateMessageDelivery-TLS-2010.wsdl wsdl\smd-SealedImmediateMessageDelivery-Interface-2010.wsdl xsd\*.xsd /out:smd-SealedImmediateMessageDelivery2010.cs /noconfig /namespace:*,nehta.smd2010.SIMD
   %WSDLTOOL%  /useSerializerForFaults /serializer:XmlSerializer wsdl\smd-SealedMessageDelivery-TLS-2010.wsdl wsdl\smd-SealedMessageDelivery-Interface-2010.wsdl xsd\*.xsd /out:smd-SealedMessageDelivery2010.cs /noconfig /namespace:*,nehta.smd2010.SMD
   %WSDLTOOL%  /useSerializerForFaults /serializer:XmlSerializer wsdl\smd-SealedMessageRetrieval-TLS-2010.wsdl wsdl\smd-SealedMessageRetrieval-Interface-2010.wsdl xsd\*.xsd /out:smd-SealedMessageRetrieval2010.cs /noconfig /namespace:*,nehta.smd2010.SMR
   %WSDLTOOL%  /useSerializerForFaults /serializer:XmlSerializer wsdl\smd-TransportResponseDelivery-TLS-2010.wsdl wsdl\smd-TransportResponseDelivery-Interface-2010.wsdl xsd\*.xsd /out:smd-TransportResponseDelivery2010.cs /noconfig /namespace:*,nehta.smd2010.TRD
   %WSDLTOOL%  /useSerializerForFaults /serializer:XmlSerializer wsdl\smd-TransportResponseRetrieval-TLS-2010.wsdl wsdl\smd-TransportResponseRetrieval-Interface-2010.wsdl xsd\*.xsd /out:smd-TransportResponseRetrieval2010.cs /noconfig /namespace:*,nehta.smd2010.TRR
   PAUSE
   --------------------------



License Agreement
=================

See [LICENSE](LICENSE.txt) file.

