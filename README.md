Neo4jClient.Extension
=====================

Extending the awesome of Neo4jClient

Merge and match nodes or relationships using objects instead of typing pseudo Cypher.

Reduces mistakes and simplifies composition of queries. As well as some more advanced features, two key extension methods are provided:

* MergeEntity
* MergeRelationship

Any object can be provided to these two methods, the properties which are utilised in both Entity and Relationship can be controlled in two ways, attributes and explicit property usage, the following attributes are provided:

* CypherLabel For use on a class, it controls the node Label, if unspecified then the class name is used
* CypherMatch Specifies that a property will be used in a MATCH statement
* CypherMerge Specifies that a property will be used in a MERGE statement
* CypherMergeOnCreate Specifies that a property will be used in the On Create SET portion of a MERGE statement
* CypherMergeOnMatch Specifies that a property will be used in the On Match SET portion of a MERGE statement
