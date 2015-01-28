using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Bits.Tests
{
	public class DownloadManagerSpec
	{
		[Fact]
		public void when_creating_job_then_returns_created_job ()
		{
			var manager = new DownloadManager ();

			var job = manager.CreateJob ("name", "http://cdn.cazzulino.com/blob.bin", "blob.bin");

			Assert.NotEqual (Guid.Empty, job.Id);
			Assert.Equal ("name", job.DisplayName);
			Assert.Equal ("http://cdn.cazzulino.com/blob.bin", job.RemoteUrl);
			Assert.Equal ("blob.bin", Path.GetFileName (job.LocalFile));
		}

		[Fact]
		public void when_setting_job_properties_then_updates_job ()
		{
			var manager = new DownloadManager ();

			var job = manager.CreateJob ("name", "http://cdn.cazzulino.com/blob.bin", "blob.bin");
			job.Priority = DownloadPriority.Low;

			var retryDelay = job.MinimumRetryDelay * 2;
			var timeout = job.NoProgressTimeout * 2;

			job.Description = "Description";
			job.Priority = DownloadPriority.Foreground;
			job.MinimumRetryDelay = retryDelay;
			job.NoProgressTimeout = timeout;

			var saved = manager.FindJob (job.Id);

			Assert.Equal ("Description", saved.Description);
			Assert.Equal (DownloadPriority.Foreground, saved.Priority);
			Assert.Equal (retryDelay, saved.MinimumRetryDelay);
			Assert.Equal (timeout, saved.NoProgressTimeout);
		}

		[Fact]
		public void when_setting_job_properties_then_raises_property_changed ()
		{
			var manager = new DownloadManager ();
			var job = manager.CreateJob ("name", "http://cdn.cazzulino.com/blob.bin", "blob.bin");
			var properties = new HashSet<string> ();

			job.PropertyChanged += (sender, args) => properties.Add (args.PropertyName);

			job.DisplayName = "DisplayName";
			job.Description = "Description";
			job.Priority = DownloadPriority.Foreground;
			job.MinimumRetryDelay = 0;
			job.NoProgressTimeout = 0;

			job.Cancel ();

			Assert.Contains ("DisplayName", properties);
			Assert.Contains ("Description", properties);
			Assert.Contains ("Priority", properties);
			Assert.Contains ("MinimumRetryDelay", properties);
			Assert.Contains ("NoProgressTimeout", properties);
			Assert.Contains ("Status", properties);
		}

		[Fact]
		public void when_creating_job_then_can_cancel_it ()
		{
			var manager = new DownloadManager ();

			var job = manager.CreateJob ("name", "http://cdn.cazzulino.com/blob.bin", "blob.bin");

			job.Resume ();
			job.Cancel ();

			Assert.Equal (DownloadStatus.Cancelled, job.Status);
		}

		[Fact]
		public void when_resuming_job_then_gets_resumed_event ()
		{
			var manager = new DownloadManager ();
			var job = manager.CreateJob ("name", "http://cdn.cazzulino.com/blob.bin", "blob.bin");
			var called = false;
			job.Resumed += (sender, args) => called = true;

			job.Resume ();

			for (int i = 0; i < 10; i++) {
				if (called)
					break;

				Thread.Sleep (500);
			}

			Assert.True (called);
		}

		[Fact]
		public void when_cancelling_job_then_gets_cancelled_event ()
		{
			var manager = new DownloadManager ();
			var job = manager.CreateJob ("name", "http://cdn.cazzulino.com/blob.bin", "blob.bin");
			var called = false;
			job.Cancelled += (sender, args) => called = true;

			job.Resume ();
			job.Cancel ();

			for (int i = 0; i < 10; i++) {
				if (called)
					break;

				Thread.Sleep (500);
			}

			Assert.True (called);
		}

		[Fact]
		public void when_transferred_job_then_gets_transferred_event ()
		{
			var manager = new DownloadManager ();
			var job = manager.CreateJob ("name", "http://cdn.cazzulino.com/blob.bin", "blob.bin");
			var called = false;
			job.Transferred += (sender, args) => called = true;

			job.Resume ();

			while (job.Status != DownloadStatus.Transferred) {
				Thread.Sleep (50);
			}

			Assert.True (called);
		}

		[Fact]
		public void when_completing_job_then_gets_completed_event ()
		{
			var manager = new DownloadManager ();
			var job = manager.CreateJob ("name", "http://cdn.cazzulino.com/blob.bin", "blob.bin");
			var called = false;
			job.Completed += (sender, args) => called = true;

			job.Resume ();

			while (job.Status != DownloadStatus.Transferred) {
				Thread.Sleep (100);
			}

			job.Complete ();

			Assert.True (called);
		}

		[Fact]
		public void when_completing_non_transferred_job_then_throws()
		{
			var manager = new DownloadManager ();

			var job = manager.CreateJob ("name", "http://cdn.cazzulino.com/blob.bin", "blob.bin");
			
			Assert.Throws<InvalidOperationException> (() => job.Complete ());
		}

		[Fact]
		public void when_creating_job_then_can_get_from_getall ()
		{
			var manager = new DownloadManager ();
			var job = manager.CreateJob ("name", "http://cdn.cazzulino.com/blob.bin", "blob.bin");

			var all = manager.GetAll ().ToDictionary (x => x.Id);

			Assert.True (all.ContainsKey (job.Id));
		}

		[Fact]
		public void when_creating_job_then_can_find_it ()
		{
			var manager = new DownloadManager ();

			var job = manager.CreateJob ("name", "http://cdn.cazzulino.com/blob.bin", "blob.bin");

			job.Resume ();
			job.Suspend ();

			var job1 = manager.FindJob (job.Id);

			Assert.NotNull (job1);
			Assert.Equal (job.DisplayName, job1.DisplayName);
			Assert.Equal (job.BytesTotal, job1.BytesTotal);
			Assert.Equal (job.BytesTransferred, job1.BytesTransferred);
			Assert.Equal (job.LocalFile, job1.LocalFile);

			Assert.Equal (job.Priority, job1.Priority);
			Assert.Equal (job.RemoteUrl, job1.RemoteUrl);
			Assert.Equal (job.Status, job1.Status);
			Assert.Equal (job.StatusMessage, job1.StatusMessage);
		}

		[Fact]
		public void when_creating_job_then_can_resume_and_finish ()
		{
			var manager = new DownloadManager ();
			var job = manager.CreateJob ("name", "http://cdn.cazzulino.com/blob.bin", "blob.bin");

			job.Resume ();

			while (job.Status != DownloadStatus.Transferred) {
				Thread.Sleep (500);
			}

			Assert.Equal (DownloadStatus.Transferred, job.Status);
		}

		[Fact]
		public void when_finding_created_job_then_can_resume_and_finish ()
		{
			var manager = new DownloadManager ();

			var job = manager.CreateJob ("name", "http://cdn.cazzulino.com/blob.bin", "blob.bin");
			job = manager.FindJob (job.Id);

			job.Resume ();

			while (job.Status != DownloadStatus.Transferred) {
				Thread.Sleep (500);
			}

			Assert.Equal (DownloadStatus.Transferred, job.Status);
		}

		[Fact]
		public void when_finding_created_job_then_can_find_it_completed ()
		{
			var manager = new DownloadManager ();

			var job = manager.CreateJob ("name", "http://cdn.cazzulino.com/blob.bin", "blob.bin");

			job.Resume ();

			while (job.Status != DownloadStatus.Transferred) {
				Thread.Sleep (500);
			}

			job = manager.FindJob (job.Id);

			Assert.Equal (DownloadStatus.Transferred, job.Status);

			job.Complete ();

			Assert.Equal (DownloadStatus.Acknowledged, job.Status);

			// From now on, finding returns null.
			Assert.Null (manager.FindJob (job.Id));
		}

		[Fact (Skip = "Creates an empty 1MB binary file.")]
		public void when_creating_blob_then_succeeds ()
		{
			var size = 1024 * 1024;
			File.WriteAllBytes (@"..\..\blob.bin", new byte[size]);
		}
	}
}
