﻿
namespace Windows.Bits
{
	/// <summary>
	/// Transfer job status.
	/// </summary>
	public enum DownloadStatus
	{
		/// <summary>
		/// Specifies that the job status isn't known yet, such as 
		/// when the download manager has not been initialized yet.
		/// </summary>
		Unknown = -1,

		/// <summary>
		/// Specifies that the job is in the queue and waiting to run. 
		/// If a user logs off while their job is transferring, the job 
		/// transitions to the queued state.
		/// </summary>
		Queued = 0,

		/// <summary>
		/// Specifies that BITS is trying to connect to the server. If the 
		/// connection succeeds, the state of the job becomes 
		/// <see cref="Transferring"/>; otherwise, the state becomes 
		/// <see cref="TransientError"/>.
		/// </summary>
		Connecting = 1,

		/// <summary>
		/// Specifies that BITS is transferring data for the job.
		/// </summary>
		Transferring = 2,

		/// <summary>
		/// Specifies that the job is suspended (paused).
		/// </summary>
		Suspended = 3,

		/// <summary>
		/// Specifies that a non-recoverable error occurred (the service is 
		/// unable to transfer the file). When the error can be corrected, 
		/// such as an access-denied error, call the IBackgroundCopyJob::Resume 
		/// method after the error is fixed. However, if the error cannot be 
		/// corrected, call the IBackgroundCopyJob::Cancel method to cancel 
		/// the job, or call the IBackgroundCopyJob::Complete method to accept 
		/// the portion of a download job that transferred successfully.
		/// </summary>
		Error = 4,

		/// <summary>
		/// Specifies that a recoverable error occurred. The service tries to 
		/// recover from the transient error until the retry time value that 
		/// you specify using the IBackgroundCopyJob::SetNoProgressTimeout method 
		/// expires. If the retry time expires, the job state changes to 
		/// <see cref="Error"/>.
		/// </summary>
		TransientError = 5,

		/// <summary>
		/// Specifies that your job was successfully processed.
		/// </summary>
		Transferred = 6,

		/// <summary>
		/// Specifies that you called the IBackgroundCopyJob::Complete method 
		/// to acknowledge that your job completed successfully
		/// </summary>
		Acknowledged = 7,

		/// <summary>
		/// Specifies that you called the IBackgroundCopyJob::Cancel method to 
		/// cancel the job (remove the job from the transfer queue)
		/// </summary>
		Cancelled = 8,
	}
}
