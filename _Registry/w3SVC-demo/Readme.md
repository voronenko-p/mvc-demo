Check Environment keys for the services below

A control set contains system configuration information such as device drivers and services. You may notice several instances of control sets when viewing the Registry. Some are duplicates or mirror images of others and some are unique. This article describes how to find control sets, which ones are important, and why.

Control sets are stored in the HKEY_LOCAL_MACHINE subtree, under the SYSTEM key. There may be several control sets depending on how often you change system settings or have problems with the settings you choose. A typical installation of Windows NT will contain four:

\ControlSet001
\ControlSet002
\CurrentControlSet
\Clone
ControlSet001 may be the last control set you booted with.

ControlSet002 could be what is known as the last known good control set, or the control set that last successfully booted Windows NT. The CurrentControlSet subkey is really a pointer to one of the ControlSetXXX keys.

Clone is a clone of CurrentControlSet, and is created each time you boot your computer by the kernel initialization process. This will not be visible

 In order to better understand how these control sets are used, you need to be aware of another subkey, Select.

Select is also under the SYSTEM key. Select contains the following values: HKLM\System\Select

Current
Default
Failed
LastKnownGood
Each of these values contain a REG_DWORD data type and refer to specifically to a control set. For example, if the Current value is set to 0x1, then CurrentControlSet is pointing to ControlSet001. Similarly, if LastKnownGood is set to 0x2, then the last known good control set is ControlSet002. The Default value usually agrees with Current, and Failed refers to a control set that was unable to boot Windows NT successfully.