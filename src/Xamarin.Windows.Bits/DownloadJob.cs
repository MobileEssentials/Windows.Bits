using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Xamarin.Windows.Bits.Interop;

namespace Xamarin.Windows.Bits
{
	internal class DownloadJob : IDownloadJob, INotifyPropertyChanged, IBackgroundCopyCallback
	{
		public event EventHandler Cancelled = (sender, args) => { };
		public event EventHandler Completed = (sender, args) => { };
		public event EventHandler Suspended = (sender, args) => { };
		public event EventHandler Resumed = (sender, args) => { };
		public event EventHandler Transferred = (sender, args) => { };

		public event PropertyChangedEventHandler PropertyChanged = (sender, args) => { };

		public const uint DefaultMiniumRetryDelay = 600; // 10' matching BITS default.
		public const uint DefaultNoProgressTimeout = 48 * 60 * 60; // 2 days in seconds.

		Guid id;
		string displayName;
		string description;
		DownloadPriority priority;
		DownloadStatus status;
		string statusMessage;
		ulong bytesTotal;
		ulong bytesTransferred;

		uint minimumRetryDelay = DefaultMiniumRetryDelay;
		uint noProgressTimeout = DefaultNoProgressTimeout;

		internal DownloadJob (Guid id, string displayName, string remoteUrl, string localFile, DownloadPriority priority)
		{
			this.id = id;
			DisplayName = displayName;
			RemoteUrl = remoteUrl;
			LocalFile = localFile;
			Priority = priority;
			Status = DownloadStatus.Unknown;
		}

		internal DownloadJob (Guid id, IBackgroundCopyJob bitsJob)
		{
			this.id = id;

			string name;
			bitsJob.GetDisplayName (out name);
			DisplayName = name;

			string description;
			bitsJob.GetDescription (out description);
			Description = description;

			BG_JOB_PRIORITY priority;
			bitsJob.GetPriority (out priority);
			Priority = (DownloadPriority)(int)priority;

			bitsJob.GetMinimumRetryDelay (out minimumRetryDelay);
			bitsJob.GetNoProgressTimeout (out noProgressTimeout);

			BG_JOB_STATE state;
			bitsJob.GetState (out state);
			Status = (DownloadStatus)(int)state;

			_BG_JOB_PROGRESS progress;
			bitsJob.GetProgress (out progress);
			BytesTotal = progress.BytesTotal;
			BytesTransferred = progress.BytesTransferred;

			bitsJob.SetNotifyInterface (this);

			IEnumBackgroundCopyFiles enumFiles = null;
			try {
				bitsJob.EnumFiles (out enumFiles);
				uint fetched;
				IBackgroundCopyFile file;
				enumFiles.Next (1, out file, out fetched);
				if (fetched == 1) {
					string remoteUrl;
					file.GetRemoteName (out remoteUrl);
					RemoteUrl = remoteUrl;

					string localName;
					file.GetLocalName (out localName);
					LocalFile = localName;
				}
			} finally {
				if (enumFiles != null)
					Marshal.ReleaseComObject (enumFiles);
			}
		}

		public Guid Id { get { return id; } }

		public string DisplayName
		{
			get { return displayName; }
			set { BitsProperty (job => job.SetDisplayName (value), "DisplayName", value, ref displayName); }
		}

		public string Description
		{
			get { return description; }
			set { BitsProperty (job => job.SetDescription (value), "Description", value, ref description); }
		}

		public DownloadPriority Priority
		{
			get { return priority; }
			set { BitsProperty (job => job.SetPriority ((BG_JOB_PRIORITY)(int)value), "Priority", value, ref priority); }
		}

		public uint MinimumRetryDelay
		{
			get { return minimumRetryDelay; }
			set { BitsProperty (job => job.SetMinimumRetryDelay (value), "MinimumRetryDelay", value, ref minimumRetryDelay); }
		}

		public uint NoProgressTimeout
		{
			get { return noProgressTimeout; }
			set { BitsProperty (job => job.SetNoProgressTimeout (value), "NoProgressTimeout", value, ref noProgressTimeout); }
		}

		public DownloadStatus Status
		{
			get { return status; }
			internal set { this.Set ("Status", value, ref status); }
		}

		public string StatusMessage
		{
			get { return statusMessage; }
			internal set { this.Set ("StatusMessage", value, ref statusMessage); }
		}

		public ulong BytesTotal
		{
			get { return bytesTotal; }
			internal set { this.Set ("BytesTotal", value, ref bytesTotal); }
		}

		public ulong BytesTransferred
		{
			get { return bytesTransferred; }
			internal set { this.Set ("BytesTransferred", value, ref bytesTransferred); }
		}

		public string RemoteUrl { get; private set; }

		public string LocalFile { get; private set; }

		public void Cancel ()
		{
			if (status == DownloadStatus.Cancelled)
				throw new InvalidOperationException ("Job has already been cancelled.");

			BitsAction (job => job.Cancel ());
			// This might be needed if the job was never started in the first place.
			Status = DownloadStatus.Cancelled;
		}

		public void Complete ()
		{
			if (Status != DownloadStatus.Transferred)
				throw new InvalidOperationException ("Job is not in the Transferred state required to complete it.");

			BitsAction (job => job.Complete ());

			Status = DownloadStatus.Acknowledged;
		}

		public void Resume ()
		{
			BitsAction (job => job.Resume ());
		}

		public void Suspend ()
		{
			BitsAction (job => job.Suspend ());
		}

		private void BitsProperty<T> (Action<IBackgroundCopyJob> action, string property, T value, ref T storage)
		{
			BitsAction (action);
			Set (property, value, ref storage);
		}

		private void BitsAction (Action<IBackgroundCopyJob> action)
		{
			IBackgroundCopyManager bitsManager = null;
			IBackgroundCopyJob bitsJob = null;
			try {
				bitsManager = (IBackgroundCopyManager)new BackgroundCopyManager ();
				bitsManager.GetJob (ref id, out bitsJob);
				if (bitsJob != null)
					action (bitsJob);
			} catch (COMException cex) {
				string error;
				bitsManager.GetErrorDescription (cex.ErrorCode, 1033, out error);
				throw new InvalidOperationException (error, cex);
			} finally {
				if (bitsJob != null)
					Marshal.ReleaseComObject (bitsJob);
				if (bitsManager != null)
					Marshal.ReleaseComObject (bitsManager);
			}
		}

		private void Set<T> (string property, T value, ref T storage)
		{
			if (!object.Equals (value, storage)) {
				storage = value;
				PropertyChanged (this, new PropertyChangedEventArgs (property));

				if (property == "Status") {
					var status = (DownloadStatus)Convert.ChangeType (value, typeof (DownloadStatus));
					switch (status) {
						case DownloadStatus.Suspended:
							Suspended (this, EventArgs.Empty);
							break;
						case DownloadStatus.Transferring:
							Resumed (this, EventArgs.Empty);
							break;
						case DownloadStatus.Transferred:
							Transferred (this, EventArgs.Empty);
							break;
						case DownloadStatus.Acknowledged:
							Completed (this, EventArgs.Empty);
							break;
						case DownloadStatus.Cancelled:
							Cancelled (this, EventArgs.Empty);
							break;
						default:
							break;
					}
				}
			}
		}

		void IBackgroundCopyCallback.JobTransferred (IBackgroundCopyJob bitsJob)
		{
			Status = DownloadStatus.Transferred;
		}

		void IBackgroundCopyCallback.JobError (IBackgroundCopyJob bitsJob, IBackgroundCopyError error)
		{
			try {
				// if the error hasn't been reported, try to get it
				if (error == null)
					bitsJob.GetError (out error);
			} catch (COMException) { }

			// If we've got the native error, extract values and populate the 
			// status message.
			if (error != null)
				StatusMessage = FormatError (error);

			BG_JOB_STATE state;
			bitsJob.GetState (out state);
			if (state != BG_JOB_STATE.BG_JOB_STATE_ACKNOWLEDGED &&
				state != BG_JOB_STATE.BG_JOB_STATE_CANCELLED)
				bitsJob.Cancel ();

			Status = DownloadStatus.Error;
		}

		void IBackgroundCopyCallback.JobModification (IBackgroundCopyJob bitsJob, uint reserved)
		{
			_BG_JOB_PROGRESS progress;
			bitsJob.GetProgress (out progress);
			BytesTotal = progress.BytesTotal;
			BytesTransferred = progress.BytesTransferred;

			BG_JOB_STATE state;
			bitsJob.GetState (out state);
			Status = (DownloadStatus)(int)state;
		}

		private string FormatError (IBackgroundCopyError error)
		{
			string contextDescription;
			string errorDescription;

			error.GetErrorContextDescription (1033, out contextDescription);
			error.GetErrorDescription (1033, out errorDescription);

			/* 
			 * Additional info we might want to add to the status
			 * 
			BG_ERROR_CONTEXT contextForError;
			int errorCode;
			IBackgroundCopyFile file;
			string protocol;
			string fileLocalName;
			string fileRemoteName;

			error.GetError(out contextForError, out errorCode);
			error.GetFile(out file);
			error.GetProtocol(out protocol);

			file.GetLocalName(out fileLocalName);
			file.GetRemoteName(out fileRemoteName);
            
			 */

			var message = errorDescription;
			if (!string.IsNullOrEmpty (contextDescription))
				message += Environment.NewLine + contextDescription;

			return message;
		}
	}
}
