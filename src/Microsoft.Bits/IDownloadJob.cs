using System;
using System.ComponentModel;

namespace Microsoft.Bits
{
	/// <summary>
	/// Interface implemented by transfer jobs.
	/// </summary>
	/// <remarks>
	/// Supports property change notifications for easy display on UIs.
	/// </remarks>
    public interface IDownloadJob : INotifyPropertyChanged
    {
		/// <summary>
		/// Occurs when the job was cancelled by invoking <see cref="Cancel"/>.
		/// </summary>
        event EventHandler Cancelled;

		/// <summary>
		/// Occurs when the job was completed by invoking <see cref="Complete"/> once the 
		/// payload has been fully transferred.
		/// </summary>
        event EventHandler Completed;

		/// <summary>
		/// Occurs when the transfer job was suspended by invoking <see cref="Suspend"/>.
		/// </summary>
        event EventHandler Suspended;

		/// <summary>
		/// Occurs when the transfer job was resumed by invoking <see cref="Resume"/>.
		/// </summary>
        event EventHandler Resumed;

		/// <summary>
		/// Occurs when the job has finished transferring all bytes from the <see cref="RemoteUrl"/>.
		/// </summary>
        event EventHandler Transferred;

		/// <summary>
		/// Gets the identifier of the job in the queue
		/// </summary>
        Guid Id { get; }

		/// <summary>
		/// Gets or sets the display name that identifies the job
		/// </summary>
        string DisplayName { get; set; }

		/// <summary>
		/// Gets or sets the description of the job.
		/// </summary>
        string Description { get; set; }
        
		/// <summary>
		/// Gets or sets the priority level you have set for the job.
		/// </summary>
        DownloadPriority Priority { get; set; }

		/// <summary>
		/// Specifies the minimum length of time that BITS waits after 
		/// encountering a transient error condition before trying to 
		/// transfer the file
		/// </summary>
		/// <remarks>Minimum length of time, in seconds, that BITS waits 
        /// after encountering a transient error before trying to transfer 
        /// the file. The default retry delay is 600 seconds (10 minutes). 
        /// The minimum retry delay that you can specify is 60 seconds. 
        /// If you specify a value less than 60 seconds, BITS changes the 
        /// value to 60 seconds. If the value exceeds the no-progress-timeout 
        /// value retrieved from the GetNoProgressTimeout method, BITS will 
        /// not retry the transfer and moves the job to the 
        /// <see cref="DownloadStatus.Error"/> state.</remarks>
        uint MinimumRetryDelay { get; set; }

		/// <summary>
		/// Specifies the length of time, in seconds, that BITS continues to try to 
		/// transfer the file after encountering a transient error 
		/// condition
		/// </summary>
        /// <remarks>
		/// The default retry period is 2 days.
        /// Set the retry period to 0 to prevent retries and to force the job into 
        /// the <see cref="DownloadStatus.Error"/> state for all errors. If the retry period value 
        /// exceeds the JobInactivityTimeout Group Policy value (90-day default), 
        /// BITS cancels the job after the policy value is exceeded.
        /// </remarks>
        uint NoProgressTimeout { get; set; }

		/// <summary>
		/// Retrieves the local name of the file to download or being downloaded.
		/// </summary>
        string LocalFile { get; }

        /// <summary>
		/// Retrieves the remote Url of the file to download or being downloaded.
		/// </summary>
        string RemoteUrl { get; }

		/// <summary>
		/// Retrieves the state of the job.
		/// </summary>
        DownloadStatus Status { get; }

        /// <summary>
        /// If the <see cref="Status"/> is <see cref="DownloadStatus.Error"/>, 
        /// this property will contain the error description.
        /// </summary>
        string StatusMessage { get; }

		/// <summary>
		/// Total number of bytes to transfer for the job.
		/// </summary>
        ulong BytesTotal { get; }

		/// <summary>
		/// Number of bytes transferred.
		/// </summary>
        ulong BytesTransferred { get; }
        
		/// <summary>
		/// Cancels the job and removes temporary files from the client.
		/// </summary>
        void Cancel();

		/// <summary>
		/// Ends the job and saves the transferred files on the client.
		/// </summary>
        void Complete();

		/// <summary>
		/// Restarts a suspended job.
		/// </summary>
        void Resume();

		/// <summary>
		/// Pauses the job.
		/// </summary>
        void Suspend();
    }
}
