﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using NUnit.Framework;
using Neo4jClient.ApiModels.Cypher;
using Neo4jClient.Cypher;
using RestSharp;

namespace Neo4jClient.Test.GraphClientTests.Cypher
{
    [TestFixture]
    public class ExecuteGetCypherResultsTests
    {
        public class SimpleResultDto
        {
            public string RelationshipType { get; set; }
            public string Name { get; set; }
            public long? UniqueId { get; set; }
        }

        [Test]
        public void ShouldDeserializeSimpleTableStructure()
        {
            // Arrange
            const string queryText = @"
                START x = node({p0})
                MATCH x-[r]->n
                RETURN type(r) AS RelationshipType, n.Name? AS Name, n.UniqueId? AS UniqueId
                LIMIT 3";
            var query = new CypherQuery(
                queryText,
                new Dictionary<string, object>
                {
                    {"p0", 123}
                });

            var httpFactory = MockHttpFactory.Generate("http://foo/db/data", new Dictionary<RestRequest, HttpResponse>
            {
                {
                    new RestRequest
                    {
                        Resource = "/",
                        Method = Method.GET
                    },
                    new HttpResponse
                    {
                        StatusCode = HttpStatusCode.OK,
                        ContentType = "application/json",
                        Content =
                            @"{
                                'cypher' : 'http://foo/db/data/cypher',
                                'batch' : 'http://foo/db/data/batch',
                                'node' : 'http://foo/db/data/node',
                                'node_index' : 'http://foo/db/data/index/node',
                                'relationship_index' : 'http://foo/db/data/index/relationship',
                                'reference_node' : 'http://foo/db/data/node/0',
                                'extensions_info' : 'http://foo/db/data/ext',
                                'extensions' : {
                                'GremlinPlugin' : {
                                    'execute_script' : 'http://foo/db/data/ext/GremlinPlugin/graphdb/execute_script'
                                }
                                }
                            }".Replace('\'', '"')
                    }
                },
                {
                    new RestRequest
                    {
                        Resource = "/cypher",
                        Method = Method.POST,
                        RequestFormat = DataFormat.Json
                    }.AddBody(new CypherApiQuery(query)),
                    new HttpResponse
                    {
                        StatusCode = HttpStatusCode.OK,
                        ContentType = "application/json",
                        Content =
                            @"{
                                'data' : [ [ 'HOSTS', 'foo', 44321 ], [ 'LIKES', 'bar', 44311 ], [ 'HOSTS', 'baz', 42586 ] ],
                                'columns' : [ 'RelationshipType', 'Name', 'UniqueId' ]
                            }".Replace('\'', '"')
                    }
                }
            });
            var graphClient = new GraphClient(new Uri("http://foo/db/data"), httpFactory);
            graphClient.Connect();

            //Act
            var results = graphClient.ExecuteGetCypherResults<SimpleResultDto>(query);

            //Assert
            Assert.IsInstanceOf<IEnumerable<SimpleResultDto>>(results);

            var resultsArray = results.ToArray();
            Assert.AreEqual(3, resultsArray.Count());

            var firstResult = resultsArray[0];
            Assert.AreEqual("HOSTS", firstResult.RelationshipType);
            Assert.AreEqual("foo", firstResult.Name);
            Assert.AreEqual(44321, firstResult.UniqueId);

            var secondResult = resultsArray[1];
            Assert.AreEqual("LIKES", secondResult.RelationshipType);
            Assert.AreEqual("bar", secondResult.Name);
            Assert.AreEqual(44311, secondResult.UniqueId);

            var thirdResult = resultsArray[2];
            Assert.AreEqual("HOSTS", thirdResult.RelationshipType);
            Assert.AreEqual("baz", thirdResult.Name);
            Assert.AreEqual(42586, thirdResult.UniqueId);
        }

        public class FooData
        {
            public string Bar { get; set; }
            public string Baz { get; set; }
        }

        public class ResultWithNodeDto
        {
            public Node<FooData> Fooness { get; set; }
            public string RelationshipType { get; set; }
            public string Name { get; set; }
            public long? UniqueId { get; set; }
        }

        public class ResultWithRelationshipDto
        {
            public RelationshipInstance<FooData> Fooness { get; set; }
            public string RelationshipType { get; set; }
            public string Name { get; set; }
            public long? UniqueId { get; set; }
        }

        [Test]
        public void ShouldDeserializeTableStructureWithNodes()
        {
            // Arrange
            const string queryText = @"
                START x = node({p0})
                MATCH x-[r]->n
                RETURN x AS Fooness, type(r) AS RelationshipType, n.Name? AS Name, n.UniqueId? AS UniqueId
                LIMIT 3";
            var query = new CypherQuery(
                queryText,
                new Dictionary<string, object>
                {
                    {"p0", 123}
                });

            var httpFactory = MockHttpFactory.Generate("http://foo/db/data", new Dictionary<RestRequest, HttpResponse>
            {
                {
                    new RestRequest
                    {
                        Resource = "/",
                        Method = Method.GET
                    },
                    new HttpResponse
                    {
                        StatusCode = HttpStatusCode.OK,
                        ContentType = "application/json",
                        Content =
                            @"{
                                'cypher' : 'http://foo/db/data/cypher',
                                'batch' : 'http://foo/db/data/batch',
                                'node' : 'http://foo/db/data/node',
                                'node_index' : 'http://foo/db/data/index/node',
                                'relationship_index' : 'http://foo/db/data/index/relationship',
                                'reference_node' : 'http://foo/db/data/node/0',
                                'extensions_info' : 'http://foo/db/data/ext',
                                'extensions' : {
                                'GremlinPlugin' : {
                                    'execute_script' : 'http://foo/db/data/ext/GremlinPlugin/graphdb/execute_script'
                                }
                                }
                            }".Replace('\'', '"')
                    }
                },
                {
                    new RestRequest
                    {
                        Resource = "/cypher",
                        Method = Method.POST,
                        RequestFormat = DataFormat.Json
                    }.AddBody(new CypherApiQuery(query)),
                    new HttpResponse
                    {
                        StatusCode = HttpStatusCode.OK,
                        ContentType = "application/json",
                        Content =
                            @"{
                                'data' : [ [ {
                                'outgoing_relationships' : 'http://foo/db/data/node/0/relationships/out',
                                'data' : {
                                    'Bar' : 'bar',
                                    'Baz' : 'baz'
                                },
                                'traverse' : 'http://foo/db/data/node/0/traverse/{returnType}',
                                'all_typed_relationships' : 'http://foo/db/data/node/0/relationships/all/{-list|&|types}',
                                'property' : 'http://foo/db/data/node/0/properties/{key}',
                                'self' : 'http://foo/db/data/node/0',
                                'properties' : 'http://foo/db/data/node/0/properties',
                                'outgoing_typed_relationships' : 'http://foo/db/data/node/0/relationships/out/{-list|&|types}',
                                'incoming_relationships' : 'http://foo/db/data/node/0/relationships/in',
                                'extensions' : {
                                },
                                'create_relationship' : 'http://foo/db/data/node/0/relationships',
                                'paged_traverse' : 'http://foo/db/data/node/0/paged/traverse/{returnType}{?pageSize,leaseTime}',
                                'all_relationships' : 'http://foo/db/data/node/0/relationships/all',
                                'incoming_typed_relationships' : 'http://foo/db/data/node/0/relationships/in/{-list|&|types}'
                                }, 'HOSTS', 'foo', 44321 ], [ {
                                'outgoing_relationships' : 'http://foo/db/data/node/0/relationships/out',
                                'data' : {
                                    'Bar' : 'bar',
                                    'Baz' : 'baz'
                                },
                                'traverse' : 'http://foo/db/data/node/0/traverse/{returnType}',
                                'all_typed_relationships' : 'http://foo/db/data/node/0/relationships/all/{-list|&|types}',
                                'property' : 'http://foo/db/data/node/0/properties/{key}',
                                'self' : 'http://foo/db/data/node/2',
                                'properties' : 'http://foo/db/data/node/0/properties',
                                'outgoing_typed_relationships' : 'http://foo/db/data/node/0/relationships/out/{-list|&|types}',
                                'incoming_relationships' : 'http://foo/db/data/node/0/relationships/in',
                                'extensions' : {
                                },
                                'create_relationship' : 'http://foo/db/data/node/0/relationships',
                                'paged_traverse' : 'http://foo/db/data/node/0/paged/traverse/{returnType}{?pageSize,leaseTime}',
                                'all_relationships' : 'http://foo/db/data/node/0/relationships/all',
                                'incoming_typed_relationships' : 'http://foo/db/data/node/0/relationships/in/{-list|&|types}'
                                }, 'LIKES', 'bar', 44311 ], [ {
                                'outgoing_relationships' : 'http://foo/db/data/node/0/relationships/out',
                                'data' : {
                                    'Bar' : 'bar',
                                    'Baz' : 'baz'
                                },
                                'traverse' : 'http://foo/db/data/node/0/traverse/{returnType}',
                                'all_typed_relationships' : 'http://foo/db/data/node/0/relationships/all/{-list|&|types}',
                                'property' : 'http://foo/db/data/node/0/properties/{key}',
                                'self' : 'http://foo/db/data/node/12',
                                'properties' : 'http://foo/db/data/node/0/properties',
                                'outgoing_typed_relationships' : 'http://foo/db/data/node/0/relationships/out/{-list|&|types}',
                                'incoming_relationships' : 'http://foo/db/data/node/0/relationships/in',
                                'extensions' : {
                                },
                                'create_relationship' : 'http://foo/db/data/node/0/relationships',
                                'paged_traverse' : 'http://foo/db/data/node/0/paged/traverse/{returnType}{?pageSize,leaseTime}',
                                'all_relationships' : 'http://foo/db/data/node/0/relationships/all',
                                'incoming_typed_relationships' : 'http://foo/db/data/node/0/relationships/in/{-list|&|types}'
                                }, 'HOSTS', 'baz', 42586 ] ],
                                'columns' : [ 'Fooness', 'RelationshipType', 'Name', 'UniqueId' ]
                            }".Replace('\'', '"')
                    }
                }
            });
            var graphClient = new GraphClient(new Uri("http://foo/db/data"), httpFactory);
            graphClient.Connect();

            //Act
            var results = graphClient.ExecuteGetCypherResults<ResultWithNodeDto>(query);

            //Assert
            Assert.IsInstanceOf<IEnumerable<ResultWithNodeDto>>(results);

            var resultsArray = results.ToArray();
            Assert.AreEqual(3, resultsArray.Count());

            var firstResult = resultsArray[0];
            Assert.AreEqual(0, firstResult.Fooness.Reference.Id);
            Assert.AreEqual("bar", firstResult.Fooness.Data.Bar);
            Assert.AreEqual("baz", firstResult.Fooness.Data.Baz);
            Assert.AreEqual("HOSTS", firstResult.RelationshipType);
            Assert.AreEqual("foo", firstResult.Name);
            Assert.AreEqual(44321, firstResult.UniqueId);

            var secondResult = resultsArray[1];
            Assert.AreEqual(2, secondResult.Fooness.Reference.Id);
            Assert.AreEqual("bar", secondResult.Fooness.Data.Bar);
            Assert.AreEqual("baz", secondResult.Fooness.Data.Baz);
            Assert.AreEqual("LIKES", secondResult.RelationshipType);
            Assert.AreEqual("bar", secondResult.Name);
            Assert.AreEqual(44311, secondResult.UniqueId);

            var thirdResult = resultsArray[2];
            Assert.AreEqual(12, thirdResult.Fooness.Reference.Id);
            Assert.AreEqual("bar", thirdResult.Fooness.Data.Bar);
            Assert.AreEqual("baz", thirdResult.Fooness.Data.Baz);
            Assert.AreEqual("HOSTS", thirdResult.RelationshipType);
            Assert.AreEqual("baz", thirdResult.Name);
            Assert.AreEqual(42586, thirdResult.UniqueId);
        }

        [Test]
        public void ShouldDeserializeTableStructureWithRelationships()
        {
            // Arrange
            const string queryText = @"
                START x = node({p0})
                MATCH x-[r]->n
                RETURN x AS Fooness, type(r) AS RelationshipType, n.Name? AS Name, n.UniqueId? AS UniqueId
                LIMIT 3";
            var query = new CypherQuery(
                queryText,
                new Dictionary<string, object>
                {
                    {"p0", 123}
                });

            var httpFactory = MockHttpFactory.Generate("http://foo/db/data", new Dictionary<RestRequest, HttpResponse>
            {
                {
                    new RestRequest
                    {
                        Resource = "/",
                        Method = Method.GET
                    },
                    new HttpResponse
                    {
                        StatusCode = HttpStatusCode.OK,
                        ContentType = "application/json",
                        Content =
                            @"{
                                'cypher' : 'http://foo/db/data/cypher',
                                'batch' : 'http://foo/db/data/batch',
                                'node' : 'http://foo/db/data/node',
                                'node_index' : 'http://foo/db/data/index/node',
                                'relationship_index' : 'http://foo/db/data/index/relationship',
                                'reference_node' : 'http://foo/db/data/node/0',
                                'extensions_info' : 'http://foo/db/data/ext',
                                'extensions' : {
                                'GremlinPlugin' : {
                                    'execute_script' : 'http://foo/db/data/ext/GremlinPlugin/graphdb/execute_script'
                                }
                                }
                            }".Replace('\'', '"')
                    }
                },
                {
                    new RestRequest
                    {
                        Resource = "/cypher",
                        Method = Method.POST,
                        RequestFormat = DataFormat.Json
                    }.AddBody(new CypherApiQuery(query)),
                    new HttpResponse
                    {
                        StatusCode = HttpStatusCode.OK,
                        ContentType = "application/json",
                        Content =
                            @"{
                                'data' : [ [ {
                                'start' : 'http://foo/db/data/node/0',
                                'data' : {
                                    'Bar' : 'bar',
                                    'Baz' : 'baz'
                                },
                                'property' : 'http://foo/db/data/relationship/0/properties/{key}',
                                'self' : 'http://foo/db/data/relationship/0',
                                'properties' : 'http://foo/db/data/relationship/0/properties',
                                'type' : 'HAS_REFERENCE_DATA',
                                'extensions' : {
                                },
                                'end' : 'http://foo/db/data/node/1'
                                }, 'HOSTS', 'foo', 44321 ], [ {
                                'start' : 'http://foo/db/data/node/1',
                                'data' : {
                                    'Bar' : 'bar',
                                    'Baz' : 'baz'
                                },
                                'property' : 'http://foo/db/data/relationship/1/properties/{key}',
                                'self' : 'http://foo/db/data/relationship/1',
                                'properties' : 'http://foo/db/data/relationship/1/properties',
                                'type' : 'HAS_REFERENCE_DATA',
                                'extensions' : {
                                },
                                'end' : 'http://foo/db/data/node/1'
                                }, 'LIKES', 'bar', 44311 ], [ {
                                'start' : 'http://foo/db/data/node/2',
                                'data' : {
                                    'Bar' : 'bar',
                                    'Baz' : 'baz'
                                },
                                'property' : 'http://foo/db/data/relationship/2/properties/{key}',
                                'self' : 'http://foo/db/data/relationship/2',
                                'properties' : 'http://foo/db/data/relationship/2/properties',
                                'type' : 'HAS_REFERENCE_DATA',
                                'extensions' : {
                                },
                                'end' : 'http://foo/db/data/node/1'
                                }, 'HOSTS', 'baz', 42586 ] ],
                                'columns' : [ 'Fooness', 'RelationshipType', 'Name', 'UniqueId' ]
                            }".Replace('\'', '"')
                    }
                }
            });
            var graphClient = new GraphClient(new Uri("http://foo/db/data"), httpFactory);
            graphClient.Connect();

            //Act
            var results = graphClient.ExecuteGetCypherResults<ResultWithRelationshipDto>(query);

            //Assert
            Assert.IsInstanceOf<IEnumerable<ResultWithRelationshipDto>>(results);

            var resultsArray = results.ToArray();
            Assert.AreEqual(3, resultsArray.Count());

            var firstResult = resultsArray[0];
            Assert.AreEqual(0, firstResult.Fooness.Reference.Id);
            Assert.AreEqual("bar", firstResult.Fooness.Data.Bar);
            Assert.AreEqual("baz", firstResult.Fooness.Data.Baz);
            Assert.AreEqual("HOSTS", firstResult.RelationshipType);
            Assert.AreEqual("foo", firstResult.Name);
            Assert.AreEqual(44321, firstResult.UniqueId);

            var secondResult = resultsArray[1];
            Assert.AreEqual(1, secondResult.Fooness.Reference.Id);
            Assert.AreEqual("bar", secondResult.Fooness.Data.Bar);
            Assert.AreEqual("baz", secondResult.Fooness.Data.Baz);
            Assert.AreEqual("LIKES", secondResult.RelationshipType);
            Assert.AreEqual("bar", secondResult.Name);
            Assert.AreEqual(44311, secondResult.UniqueId);

            var thirdResult = resultsArray[2];
            Assert.AreEqual(2, thirdResult.Fooness.Reference.Id);
            Assert.AreEqual("bar", thirdResult.Fooness.Data.Bar);
            Assert.AreEqual("baz", thirdResult.Fooness.Data.Baz);
            Assert.AreEqual("HOSTS", thirdResult.RelationshipType);
            Assert.AreEqual("baz", thirdResult.Name);
            Assert.AreEqual(42586, thirdResult.UniqueId);
        }
    }
}