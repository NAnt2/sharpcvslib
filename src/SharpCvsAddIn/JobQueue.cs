using System;
using System.Collections;
using System.Threading;

namespace SharpCvsAddIn
{
	public interface IJob
	{
		void DoWork();
	}

	public class JobQueue
	{
		private Queue jobs_ = Queue.Synchronized( new Queue() );
		private AutoResetEvent addJobEvent_ = new AutoResetEvent(false);
		private AutoResetEvent stopEvent_ = new AutoResetEvent(false);

		private void JobRunner()
		{
			// note stop event has to come first because
			// if duplicate events get triggered, the lowest
			// index is returned, we want stop to take precedence
			// over add job
			WaitHandle[] evts = { stopEvent_, addJobEvent_ };
			while(true)
			{
				int eventId = WaitHandle.WaitAny( evts );
				// stop was signalled, exit thread
				if( eventId == 0 ) break;

				while( jobs_.Count > 0 )
				{
					IJob job = (IJob)jobs_.Dequeue();
					job.DoWork();
				}
			}
			
		}


		public JobQueue()
		{	
			Thread t = new Thread( new ThreadStart( this.JobRunner ) );
			t.Start();

		}

		public void AddJob( IJob job )
		{
			jobs_.Enqueue( job );
			addJobEvent_.Set();
		}

		public void Stop()
		{
			stopEvent_.Set();
		}


	}
}