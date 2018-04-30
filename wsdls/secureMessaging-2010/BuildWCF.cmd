SET WSDLTOOL="C:\Program Files (x86)\Microsoft SDKs\Windows\v7.0A\Bin\SvcUtil.exe" /noLogo

%WSDLTOOL%  /useSerializerForFaults /serializer:XmlSerializer wsdl\smd-SealedImmediateMessageDelivery-TLS-2010.wsdl wsdl\smd-SealedImmediateMessageDelivery-Interface-2010.wsdl xsd\*.xsd /out:smd-SealedImmediateMessageDelivery2010.cs /noconfig /namespace:*,Nehta.SMD2010.SIMD

%WSDLTOOL%  /useSerializerForFaults /serializer:XmlSerializer wsdl\smd-SealedMessageDelivery-TLS-2010.wsdl wsdl\smd-SealedMessageDelivery-Interface-2010.wsdl xsd\*.xsd /out:smd-SealedMessageDelivery2010.cs /noconfig /namespace:*,Nehta.SMD2010.SMD

%WSDLTOOL%  /useSerializerForFaults /serializer:XmlSerializer wsdl\smd-SealedMessageRetrieval-TLS-2010.wsdl wsdl\smd-SealedMessageRetrieval-Interface-2010.wsdl xsd\*.xsd /out:smd-SealedMessageRetrieval2010.cs /noconfig /namespace:*,Nehta.SMD2010.SMR

%WSDLTOOL%  /useSerializerForFaults /serializer:XmlSerializer wsdl\smd-TransportResponseDelivery-TLS-2010.wsdl wsdl\smd-TransportResponseDelivery-Interface-2010.wsdl xsd\*.xsd /out:smd-TransportResponseDelivery2010.cs /noconfig /namespace:*,Nehta.SMD2010.TRD

%WSDLTOOL%  /useSerializerForFaults /serializer:XmlSerializer wsdl\smd-TransportResponseRetrieval-TLS-2010.wsdl	wsdl\smd-TransportResponseRetrieval-Interface-2010.wsdl xsd\*.xsd /out:smd-TransportResponseRetrieval2010.cs /noconfig /namespace:*,Nehta.SMD2010.TRR

PAUSE
