using MongoDB.Driver;
using Testcontainers.MongoDb;

namespace Poll.N.Quiz.NuGet.IntegrationTests.EventStore.WriteOnly;

public class MongoDbFixture : IAsyncDisposable
{
    public MongoDbFixture()
    {
        _mongoContainer = new MongoDbBuilder().Build();
    }

    public MongoClient? MongoClient { get; private set; }

    private readonly MongoDbContainer _mongoContainer;

    public virtual async Task InitializeAsync()
    {
        if (MongoClient is not null)
            return;

        await _mongoContainer.StartAsync();

        MongoClient = new MongoClient(_mongoContainer.GetConnectionString());
    }

    public async ValueTask DisposeAsync()
    {
        if(MongoClient is null)
            return;

        await _mongoContainer.StopAsync();
        await _mongoContainer.DisposeAsync();

        MongoClient = null;
        GC.SuppressFinalize(this);
    }
}
