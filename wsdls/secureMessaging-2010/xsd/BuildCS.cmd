@echo OFF
cls
@echo ============================================================================
@echo --- DELETE FILES NOT REQUIRED                                            ---
@echo ============================================================================

DEL *.cs

@echo ============================================================================
@echo --- Building WSDL proxies                                                ---
@echo ============================================================================

xsd smd-Message-2010.xsd xsp-SignedPayload-2010.xsd xmldsig-core-schema.xsd /c /l:CS /n:nehta.smd2010.smd_Message

rename smd-Message-2010*.cs smd-Message-2010.cs

PAUSE
