<!-- default badges list -->
![](https://img.shields.io/endpoint?url=https://codecentral.devexpress.com/api/v1/VersionRange/128585729/19.2.7%2B)
[![](https://img.shields.io/badge/Open_in_DevExpress_Support_Center-FF7200?style=flat-square&logo=DevExpress&logoColor=white)](https://supportcenter.devexpress.com/ticket/details/E4606)
[![](https://img.shields.io/badge/📖_How_to_use_DevExpress_Examples-e9f6fc?style=flat-square)](https://docs.devexpress.com/GeneralInformation/403183)
<!-- default badges end -->
<!-- default file list -->
*Files to look at*:

* [Program.cs](./CS/XpoConsoleApplication/Program.cs) (VB: [Program.vb](./VB/XpoConsoleApplication/Program.vb))
* **[XPComplexCustomMemberInfo.cs](./CS/XpoConsoleApplication/XPComplexCustomMemberInfo.cs) (VB: [XPComplexCustomMemberInfo.vb](./VB/XpoConsoleApplication/XPComplexCustomMemberInfo.vb))**
<!-- default file list end -->
# How to create persistent classes mapped to tables with a composite primary key at runtime 


<p>When you have a table with a composite key, the standard solution to map an XPO persistent class to it is to declare a structure for the key member as described in the <a href="https://www.devexpress.com/Support/Center/p/A2615">How to create a persistent object for a database table with a compound key</a> article. If the database schema is not known as design time, and you need to create persistent classes at runtime, the built-in <a href="https://documentation.devexpress.com/CoreLibraries/clsDevExpressXpoMetadataXPDictionarytopic.aspx">XPDictionary</a> methods won't help, because structure-like members are supported only for real types via Reflection. This example demonstrates how to create a custom <a href="https://documentation.devexpress.com/CoreLibraries/clsDevExpressXpoMetadataXPCustomMemberInfotopic.aspx">XPCustomMemberInfo</a> descendant to support nested persistent members.</p>
<p>See also:<br> <a href="https://www.devexpress.com/Support/Center/p/K18482">K18482: How to create persistent metadata on the fly and load data from an arbitrary table</a><br><a href="https://documentation.devexpress.com/CoreLibraries/clsDevExpressXpoMetadataXPMemberInfotopic.aspx">XPMemberInfo</a> </p>

<br/>


