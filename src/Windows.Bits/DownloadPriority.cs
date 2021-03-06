﻿
namespace Windows.Bits
{
	/// <summary>
	/// Priority of the transfer job.
	/// </summary>
	public enum DownloadPriority
	{
		/// <summary>
		/// Transfers the job in the foreground
		/// </summary>
		Foreground = 0,

		/// <summary>
		/// Transfers the job in the background. This is the highest background 
		/// priority level. 
		/// </summary>
		High = 1,

		/// <summary>
		/// Transfers the job in the background. This is the default priority 
		/// level for a job
		/// </summary>
		Normal = 2,

		/// <summary>
		/// Transfers the job in the background. This is the lowest background 
		/// priority level
		/// </summary>
		Low = 3,
	}
}
