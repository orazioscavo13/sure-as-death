using System;
namespace SureAsDeath.App.Hangfire.Jobs.Interfaces
{
    public interface IRetrieveDataJob
    {
        void SyncData(CancellationToken cancellationToken);
    }
}

