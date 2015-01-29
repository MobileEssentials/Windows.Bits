using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Xamarin.Windows.Bits.Tests
{
	public class DownloadJobSpec : IDisposable
	{
		IDownloadManager manager;
		IDownloadJob job;

		public DownloadJobSpec ()
		{
			manager = new DownloadManager ();
			job = manager.CreateJob ("name", "http://xvs.xamarin.com/Tests/Xamarin.Windows.Bits.Tests-DO-NOT-DELETE.bin", "blob.bin");
		}

		[Fact]
		public void when_setting_job_displayname_then_updates_job_and_raises_property_changed ()
		{
			var value = Guid.NewGuid().ToString();
			var changed = false;
			job.PropertyChanged += (sender, args) => changed = (args.PropertyName == "DisplayName" ? true : changed);

			job.DisplayName = value;

			var saved = manager.FindJob (job.Id);

			Assert.Equal (value, saved.DisplayName);
			Assert.True (changed);
		}

		[Fact]
		public void when_setting_job_description_then_updates_job_and_raises_property_changed ()
		{
			var value = Guid.NewGuid().ToString();
			var changed = false;
			job.PropertyChanged += (sender, args) => changed = (args.PropertyName == "Description" ? true : changed);

			job.Description = value;

			var saved = manager.FindJob (job.Id);

			Assert.Equal (value, saved.Description);
			Assert.True (changed);
		}

		[Fact]
		public void when_setting_job_priority_then_updates_job_and_raises_property_changed ()
		{
			var value = DownloadPriority.Low;
			var changed = false;
			job.PropertyChanged += (sender, args) => changed = (args.PropertyName == "Priority" ? true : changed);

			job.Priority = value;

			var saved = manager.FindJob (job.Id);

			Assert.Equal (value, saved.Priority);
			Assert.True (changed);
		}

		[Fact]
		public void when_setting_job_retry_delay_then_updates_job_and_raises_property_changed ()
		{
			uint value = 1000;
			var changed = false;
			job.PropertyChanged += (sender, args) => changed = (args.PropertyName == "MinimumRetryDelay" ? true : changed);

			job.MinimumRetryDelay = value;

			var saved = manager.FindJob (job.Id);

			Assert.Equal (value, saved.MinimumRetryDelay);
			Assert.True (changed);
		}

		[Fact]
		public void when_setting_job_timeout_then_updates_job_and_raises_property_changed ()
		{
			uint value = 1000;
			var changed = false;
			job.PropertyChanged += (sender, args) => changed = (args.PropertyName == "NoProgressTimeout" ? true : changed);

			job.NoProgressTimeout = value;

			var saved = manager.FindJob (job.Id);

			Assert.Equal (value, saved.NoProgressTimeout);
			Assert.True (changed);
		}

		[Fact]
		public void when_changing_job_status_then_raises_property_changed ()
		{
			var changed = false;
			job.PropertyChanged += (sender, args) => changed = (args.PropertyName == "Status" ? true : changed);

			job.Cancel();

			Assert.Equal(DownloadStatus.Cancelled, job.Status);
			Assert.True(changed);
		}

		[Fact]
		public void when_cancelling_cancelled_job_then_throws ()
		{
			job.Cancel();

			var ex = Assert.Throws<InvalidOperationException> (() => job.Cancel());

			Assert.Null(ex.InnerException);
		}

		[Fact]
		public async Task when_resuming_job_then_gets_resumed_event ()
		{
			var called = false;
			job.Resumed += (sender, args) => called = true;

			job.Resume ();

			await Task.Run (() => {
				while (!called)
					Thread.Sleep (50);
				}).TimeoutAfter (20);

			Assert.True (called);
		}

		[Fact]
		public async Task when_cancelling_job_then_gets_cancelled_event ()
		{
			var called = false;
			job.Cancelled += (sender, args) => called = true;

			job.Resume ();
			job.Cancel ();

			await Task.Run (() => {
				while (!called)
					Thread.Sleep (50);
				}).TimeoutAfter (20);

			Assert.True (called);
		}

		[Fact]
		public async Task when_transferred_job_then_gets_transferred_event ()
		{
			var called = false;
			job.Transferred += (sender, args) => called = true;

			job.Resume ();

			await Task.Run (() => {
				while (!called)
					Thread.Sleep (50);
				}).TimeoutAfter (60);

			Assert.True (called);
		}

		[Fact]
		public async Task when_completing_job_then_gets_completed_event ()
		{
			var called = false;
			job.Completed += (sender, args) => called = true;

			job.Resume ();

			await Task.Run (() => {
				while (job.Status != DownloadStatus.Transferred)
					Thread.Sleep (50);
				}).TimeoutAfter (60);

			job.Complete ();

			Assert.True (called);
		}

		[Fact]
		public void when_completing_non_transferred_job_then_throws()
		{
			Assert.Throws<InvalidOperationException> (() => job.Complete ());
		}

		public void Dispose ()
		{
			try
			{
				job.Cancel();
			}
			catch { } 
		}
	}
}
