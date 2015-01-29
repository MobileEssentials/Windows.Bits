using Microsoft.Bits.Interop;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;

namespace Microsoft.Bits
{
    public class DownloadManager : IDownloadManager
    {
        public IDownloadJob CreateJob(string displayName, string remoteUrl, string localFile, DownloadPriority priority = DownloadPriority.Normal)
        {
            if (!Path.IsPathRooted(localFile))
                localFile = new FileInfo(localFile).FullName;

            var targetDir = Path.GetDirectoryName(localFile);
            if (!Directory.Exists(targetDir))
                Directory.CreateDirectory(targetDir);
            if (File.Exists(localFile))
                File.Delete(localFile);

            IBackgroundCopyManager bitsManager = null;
            IBackgroundCopyJob bitsJob = null;
            var id = Guid.Empty;

            try
            {
                bitsManager = (IBackgroundCopyManager)new BackgroundCopyManager();
                bitsManager.CreateJob(displayName, BG_JOB_TYPE.BG_JOB_TYPE_DOWNLOAD, out id, out bitsJob);

                //  ***
                //      SET UP BITS JOB SETTINGS--TIMEOUTS/RETRY ETC           
                //      SEE THE FOLLOWING REFERENCES:
                //  **  http://msdn.microsoft.com/library/default.asp?url=/library/en-us/bits/bits/IBackgroundCopyJob2_setminimumretrydelay.asp?frame=true
                //  **  http://msdn.microsoft.com/library/default.asp?url=/library/en-us/bits/bits/IBackgroundCopyJob2_setnoprogresstimeout.asp?frame=true
                //  **  http://msdn.microsoft.com/library/default.asp?url=/library/en-us/bits/bits/bg_job_priority.asp
                //  ***

                //  in constant set to 0; this makes BITS retry as soon as possible after an error
                bitsJob.SetMinimumRetryDelay(DownloadJob.DefaultMiniumRetryDelay);
                //  in constant set to 5 seconds; BITS will set job to Error status if exceeded
                bitsJob.SetNoProgressTimeout(DownloadJob.DefaultNoProgressTimeout);

                bitsJob.SetPriority((BG_JOB_PRIORITY)(int)priority);

                bitsJob.AddFile(remoteUrl, localFile);

                bitsJob.SetNotifyFlags((uint)(
                    BG_JOB_NOTIFICATION_TYPE.BG_NOTIFY_JOB_ERROR |
                    BG_JOB_NOTIFICATION_TYPE.BG_NOTIFY_JOB_MODIFICATION |
                    BG_JOB_NOTIFICATION_TYPE.BG_NOTIFY_JOB_TRANSFERRED));

                var job = new DownloadJob(id, displayName, remoteUrl, localFile, priority);

                // Set the notify interface to get BITS events
                bitsJob.SetNotifyInterface(job);

                return job;
            }
            finally
            {
                if (bitsJob != null)
                    Marshal.ReleaseComObject(bitsJob);
                if (bitsManager != null)
                    Marshal.ReleaseComObject(bitsManager);
            }
        }

        public IDownloadJob FindJob(Guid id)
        {
            IBackgroundCopyManager bitsManager = null;
            IBackgroundCopyJob bitsJob = null;

            try
            {
                bitsManager = (IBackgroundCopyManager)new BackgroundCopyManager();
                bitsManager.GetJob(ref id, out bitsJob);

                if (bitsJob != null)
                    return new DownloadJob(id, bitsJob);

                return null;
            }
            catch (COMException cex)
            {
                if ((uint)cex.ErrorCode == (uint)BG_RESULT.BG_E_NOT_FOUND)
                    return null;

                string error;
                bitsManager.GetErrorDescription(cex.ErrorCode, 1033, out error);
                throw new ArgumentException(error);
            }
            finally
            {
                if (bitsJob != null)
                    Marshal.ReleaseComObject(bitsJob);
                if (bitsManager != null)
                    Marshal.ReleaseComObject(bitsManager);
            }
        }

        public IEnumerable<IDownloadJob> GetAll()
        {
            var jobs = new List<DownloadJob>();
            IBackgroundCopyManager bitsManager = null;
            IEnumBackgroundCopyJobs enumJobs = null;

            try
            {
                bitsManager = (IBackgroundCopyManager)new BackgroundCopyManager();
                bitsManager.EnumJobs(0, out enumJobs);

                uint fetched;
                IBackgroundCopyJob bitsJob = null;
                try
                {
                    enumJobs.Next(1, out bitsJob, out fetched);
                    while (fetched == 1)
                    {
                        Guid id;
                        bitsJob.GetId(out id);
                        jobs.Add(new DownloadJob(id, bitsJob));

                        enumJobs.Next(1, out bitsJob, out fetched);
                    }
                }
                finally
                {
                    if (bitsJob != null)
                        Marshal.ReleaseComObject(bitsJob);
                }

                return jobs.ToArray();
            }
            finally
            {
                if (enumJobs != null)
                    Marshal.ReleaseComObject(enumJobs);
                if (bitsManager != null)
                    Marshal.ReleaseComObject(bitsManager);
            }
        }
    }
}