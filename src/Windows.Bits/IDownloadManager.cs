using System;
using System.Collections.Generic;

namespace Windows.Bits
{
	/// <summary>
	/// Main API for Microsoft BITS, exposing a way to create and retrieve jobs managed by the system.
	/// </summary>
	public interface IDownloadManager
	{
		/// <summary>
		/// Creates the job.
		/// </summary>
		/// <param name="displayName">The display name.</param>
		/// <param name="remoteUrl">The remote URL.</param>
		/// <param name="localFile">The local file.</param>
		/// <param name="priority">The priority.</param>
		/// <returns></returns>
		IDownloadJob CreateJob (string displayName, string remoteUrl, string localFile, DownloadPriority priority = DownloadPriority.Normal);

		/// <summary>
		/// Attemps to find the job with the given identifier.
		/// </summary>
		/// <param name="id">The job's identifier.</param>
		/// <returns>The job or <see langword="null"/> if not found.</returns>
		IDownloadJob FindJob (Guid id);

		/// <summary>
		/// Gets all the jobs currently being managed with the system.
		/// </summary>
		IEnumerable<IDownloadJob> GetAll ();
	}
}
