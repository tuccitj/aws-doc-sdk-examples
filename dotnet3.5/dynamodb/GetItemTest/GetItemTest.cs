// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved. 
// SPDX - License - Identifier: Apache - 2.0
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

using Moq;

using Xunit;
using Xunit.Abstractions;

namespace GetItemTest
{
    public class GetItemTest
    {
        private readonly ITestOutputHelper _output;

        public GetItemTest(ITestOutputHelper output)
        {
            this._output = output;
        }

        readonly string _tableName = "testtable";
        readonly string _id = "25";

        private IAmazonDynamoDB CreateMockDynamoDbClient()
        {
            var mockDynamoDbClient = new Mock<IAmazonDynamoDB>();

            mockDynamoDbClient.Setup(client => client.QueryAsync(
                It.IsAny<QueryRequest>(),
                It.IsAny<CancellationToken>()))
                .Callback<QueryRequest, CancellationToken>((request, token) =>
                {
                    if (!string.IsNullOrEmpty(_tableName))
                    {
                        bool areEqual = _tableName == request.TableName;                        
                        Assert.True(areEqual, "The provided table name is not the one used to access the table");
                    }
                })
                .Returns((QueryRequest r, CancellationToken token) =>
                {
                    return Task.FromResult(new QueryResponse { HttpStatusCode = HttpStatusCode.OK });
                });

            return mockDynamoDbClient.Object;
        }

        [Fact]
        public async Task CheckGetItem()
        {
            IAmazonDynamoDB client = CreateMockDynamoDbClient();

            var result = await GetItem.GetItem.GetItemAsync(client, _tableName, _id);

            bool gotResult = result != null;
            Assert.True(gotResult, "Could NOT get result from querying table " + _tableName);

            bool ok = result.HttpStatusCode == HttpStatusCode.OK;
            Assert.True(ok, "Could NOT get item # " + _id + " from table " + _tableName);

            _output.WriteLine("Got item from table");
        }
    }
}
