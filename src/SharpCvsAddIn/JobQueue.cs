using System;
using System.Collections;
using System.Threading;
using System.Diagnostics;
using System.Windows.Forms;
using log4net ;

namespace SharpCvsAddIn
{
	/// <summary>
	/// If a event handler is provided, it will be triggered when the associated job
	/// completes. jobData can contain data produced in job
	/// </summary>
	public delegate void JobCompletionHandler( object sender, object jobData);

	public interface IJob : ICloneable 
	{
		string Name { get; }
		object DoWork();
	}

	/// <summary>
	/// this class manages jobs that are performed on a seperate thread, this facility exists
	/// to let you perform lengthy operations without locking up the user interface, jobs 
	/// are processed FIFO 
	/// </summary>
	public class JobQueue : IDisposable
	{
		// utility class to manage jobs
		private class JobInfo
		{
			internal JobCompletionHandler OnComplete_ = null;
			internal IJob job_ = null;
		}

		private Queue jobs_ = Queue.Synchronized( new Queue() );
		private ManualResetEvent addJobEvent_ = new ManualResetEvent(false);
		private AutoResetEvent stopEvent_ = new AutoResetEvent(false);
		// this is signalled initially so we won't block if job
		// thread never gets started
		private ManualResetEvent jobRunnerExitEvent_ = new ManualResetEvent(true);
		private static readonly ILog log_ = LogManager.GetLogger(typeof(JobQueue));
		private bool disposed = false;
		private IController controller_ = null;
		private const int WAIT_PERIOD = 10000 ; // 10 seconds

		private void JobRunner()
		{
			log_.Debug( "JobRunner started" );
			try
			{
				jobRunnerExitEvent_.Reset();
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

					if( jobs_.Count > 0 )
					{
						JobInfo ji = (JobInfo)jobs_.Dequeue();
						log_.Debug( string.Format("Processing job {0}", ji.job_.Name));

						try
						{					
							object data = ji.job_.DoWork();	

							if(ji.OnComplete_ != null)
							{
								ji.OnComplete_(this, data );
							}
						}
						catch(Exception e)
						{
							log_.Error("A job threw an exception", e );

						}
					}
					
					// this will cause the thread to wait 
					// until we get more jobs
					if( jobs_.Count == 0 ) addJobEvent_.Reset();

				} // end thread loop
			}
			finally
			{
				log_.Debug("job thread exiting");
				// send signal to things that may be blocking that we 
				// are done
				jobRunnerExitEvent_.Set();
			}

		}


		public JobQueue(IController controller)
		{	
			controller_ = controller;


		}

		public void AddJob( IJob job )
		{
			this.AddJob( job, null );
		}



		/// <summary>
		///  Add a job to the job queue
		/// </summary>
		/// <param name="job">A job, override Work </param>
		/// <param name="handler">The event, if supplied will be triggered on successful completion of the job</param>
		/// <param name="data">Information to pass to the job</param>
		public void AddJob( IJob job, JobCompletionHandler handler )
		{
			Debug.Assert( job != null );
			log_.Debug( string.Format("Adding job {0}", job.Name) );
			JobInfo ji = new JobInfo();
			ji.job_ = (IJob)job.Clone();
			ji.OnComplete_ += handler;
			jobs_.Enqueue(ji);
			addJobEvent_.Set();

		}

		/// <summary>
		/// stops the processing of more jobs
		/// </summary> 
		public void Stop()
		{
			log_.Debug("Called Stop");
			stopEvent_.Set();
			// block for up to 10 seconds to all jobs to finish up 
			while( !jobRunnerExitEvent_.WaitOne( JobQueue.WAIT_PERIOD, true ))
			{
				DialogResult result = controller_.UIShell.ShowMessageBox(
					"MSGBOX_QUERY_WAITFORCLOSE",
					MessageBoxButtons.YesNo,
					MessageBoxIcon.Question );

				if( result == DialogResult.No ) break;
			}
		}
	
		/// <summary>
		/// Starts the job queue
		/// </summary>
		public void Start()
		{
			log_.Debug("Called Start");
			Thread t = new Thread( new ThreadStart( this.JobRunner ) );
			t.Start();
		}
		#region IDisposable Members

		public void Dispose()
		{
			if( !disposed )
			{
				disposed = true;
				stopEvent_.Set();
				jobRunnerExitEvent_.WaitOne( 3000, true );
			}

		}

		#endregion
	}
}