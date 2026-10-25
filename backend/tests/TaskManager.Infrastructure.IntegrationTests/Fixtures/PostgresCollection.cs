namespace TaskManager.Infrastructure.IntegrationTests.Fixtures;

[CollectionDefinition("Postgres")]
public sealed class PostgresCollection : ICollectionFixture<PostgresFixture>;
