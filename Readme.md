# How to create persistent classes mapped to tables with a composite primary key at runtime 


<p>When you have a table with a composite key, the standard solution to map an XPO persistent class to it is to declare a structure for the key member as described in the <a href="https://www.devexpress.com/Support/Center/p/A2615">How to create a persistent object for a database table with a compound key</a> article. If the database schema is not known as design time, and you need to create persistent classes at runtime, the built-in <a href="https://documentation.devexpress.com/CoreLibraries/clsDevExpressXpoMetadataXPDictionarytopic.aspx">XPDictionary</a> methods won't help, because structure-like members are supported only for real types via Reflection. This example demonstrates how to create a custom <a href="https://documentation.devexpress.com/CoreLibraries/clsDevExpressXpoMetadataXPCustomMemberInfotopic.aspx">XPCustomMemberInfo</a> descendant to support nested persistent members.</p>
<p>See also:<br> <a href="https://www.devexpress.com/Support/Center/p/K18482">K18482: How to create persistent metadata on the fly and load data from an arbitrary table</a><br><a href="https://documentation.devexpress.com/CoreLibraries/clsDevExpressXpoMetadataXPMemberInfotopic.aspx">XPMemberInfo</a> </p>

<br/>


