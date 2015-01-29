﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Threading.Tasks
{
	public static class TimeoutExtension
	{
		public static async Task TimeoutAfter (this Task task, int secondsTimeout)
		{
			if (await Task.WhenAny (task, Task.Delay (secondsTimeout * 1000)) == task)
				await task;
			else
				throw new TimeoutException ();
		}
	}
}
