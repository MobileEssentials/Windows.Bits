using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Windows.Bits.Tests
{
	public class DownloadManagerSpec : IDisposable
	{
		IDownloadManager manager;
		IDownloadJob job;

		public DownloadManagerSpec ()
		{
			manager = new DownloadManager ();
			job = manager.CreateJob ("name", "http://xvs.xamarin.com/Tests/Windows.Bits.Tests-DO-NOT-DELETE.bin", "blob.bin");
		}

		[Fact]
		public void when_creating_job_then_returns_created_job ()
		{
			Assert.NotEqual (Guid.Empty, job.Id);
			Assert.Equal ("name", job.DisplayName);
			Assert.Equal ("http://xvs.xamarin.com/Tests/Windows.Bits.Tests-DO-NOT-DELETE.bin", job.RemoteUrl);
			Assert.Equal ("blob.bin", Path.GetFileName (job.LocalFile));
		}

		[Fact]
		public void when_creating_job_then_can_cancel_it ()
		{
			job.Resume ();
			job.Cancel ();

			Assert.Equal (DownloadStatus.Cancelled, job.Status);
		}

		[Fact]
		public void when_creating_job_then_can_get_from_getall ()
		{
			var all = manager.GetAll ().ToDictionary (x => x.Id);

			Assert.True (all.ContainsKey (job.Id));
		}

		[Fact]
		public void when_creating_job_then_can_find_it ()
		{
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
		public async Task when_finding_created_job_then_can_resume_and_finish ()
		{
			job = manager.FindJob (job.Id);

			job.Resume ();

			await Task.Run (() => {
				while (job.Status != DownloadStatus.Transferred)
					Thread.Sleep (50);
			}).TimeoutAfter (20);

			Assert.Equal (DownloadStatus.Transferred, job.Status);
		}

		[Fact]
		public async Task when_finding_created_job_then_can_find_it_completed_and_acknowledge_it ()
		{
			job.Resume ();
			await Task.Run (() => {
				while (job.Status != DownloadStatus.Transferred)
					Thread.Sleep (50);
			}).TimeoutAfter (20);

			job = manager.FindJob (job.Id);
			Assert.Equal (DownloadStatus.Transferred, job.Status);

			job.Complete ();

			Assert.Equal (DownloadStatus.Acknowledged, job.Status);

			// From now on, finding returns null.
			Assert.Null (manager.FindJob (job.Id));
		}

		public void when_creating_blob_then_succeeds ()
		{
			var size = 1024 * 512;
			File.WriteAllBytes (@"..\..\blob.bin", new byte[size]);
		}

		public void Dispose ()
		{
			try {
				job.Cancel ();
			} catch { }
		}
	}
}
