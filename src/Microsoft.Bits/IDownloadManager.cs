using System;
using System.Collections.Generic;

namespace Microsoft.Bits
{
    public interface IDownloadManager
    {
        IDownloadJob CreateJob(string displayName, string remoteUrl, string localFile, DownloadPriority priority = DownloadPriority.Normal);
        IDownloadJob FindJob(Guid id);
        IEnumerable<IDownloadJob> GetAll();
    }
}
