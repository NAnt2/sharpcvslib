/////////////////////////////////////////////////////////////
//
//	This class manages the appearance of the solution
//	explorer window.
//
//////////////////////////////////////////////////////////////
using System;
using System.Drawing;
using System.Windows.Forms;
using EnvDTE;
using log4net;
using SharpCvsAddIn.Win32;

namespace SharpCvsAddIn.UI
{
	/// <summary>
	/// Summary description for SolutionExplorer.
	/// </summary>
	public class SolutionExplorer : ISolutionExplorer
	{
		//private UIHierarchy uiHierarchy;
		private const string VSNETWINDOW = "wndclass_desked_gsk";
		private const string GENERICPANE = "GenericPane";
		private const string UIHIERARCHY = "VsUIHierarchyBaseWin";
		private const string TREEVIEW = "SysTreeView32";
		private const string VBFLOATINGPALETTE = "VBFloatingPalette";
		
		private const string STATUS_IMAGES = "SharpCvsAddIn.status_icons.bmp";

		private static readonly ILog log_ = LogManager.GetLogger( typeof(SolutionExplorer) );
		Controller controller_;
		TreeView treeview_;
		ImageList statusImageList_;
		IntPtr originalImageList_ = IntPtr.Zero;
		StatusGraph.SolutionNode root_ = null;

		public SolutionExplorer(Controller controller)
		{
			controller_ = controller;

		}

		public IStatusNode Root
		{
			get { return root_ ; }
		}

		public TreeView TreeView
		{
			get{ return treeview_; }
		}

		public void Refresh()
		{
			root_ = StatusGraph.GetRoot( controller_ );
		}

		/// <summary>
		/// Get the underlying TreeView control in solution explorer and do our thing to it
		/// </summary>
		public void Initialize()
		{
			log_.Info( "Initialize" );
			Window explorerWindow = controller_.DTE.Windows.Item( 
				EnvDTE.Constants.vsWindowKindSolutionExplorer );

			// we need to make sure its not hidden and that it is dockable
			bool linkable = explorerWindow.Linkable;
			bool hidden = explorerWindow.AutoHides;
			bool isFloating = explorerWindow.IsFloating;

			try
			{

				// these two operations need to be done in an exact order, 
				// depending on whether it is initially hidden
				if ( hidden )
				{
					explorerWindow.AutoHides = false;
					explorerWindow.IsFloating = false;
					explorerWindow.Linkable = true;
				}
				else
				{
					explorerWindow.IsFloating = false;
					explorerWindow.Linkable = true;
					explorerWindow.AutoHides = false;
				}



			}
			catch(Exception e)
			{
				log_.Error( e.Message );
			}

			// find the solution explorer window
			// Get the caption of the solution explorer            
			string slnExplorerCaption = explorerWindow.Caption;
			log_.Info( "Caption of solution explorer window is " + slnExplorerCaption );

			IntPtr vsnet = (IntPtr)controller_.DTE.MainWindow.HWnd;

			// first try finding it as a child of the main VS.NET window
			IntPtr slnExplorer = Functions.FindWindowEx( vsnet, IntPtr.Zero, GENERICPANE, 
				slnExplorerCaption );

			// not there? Try looking for a floating palette. These are toplevel windows for 
			// some reason
			if ( slnExplorer == IntPtr.Zero )
			{
				log_.Info( "Solution explorer not a child of VS.NET window. Searching floating windows" );
				// we need to search for the caption of any of the potentially linked windows
				IntPtr floatingPalette = IntPtr.Zero;
				foreach( Window win in explorerWindow.LinkedWindowFrame.LinkedWindows )
				{
					floatingPalette = Functions.FindWindow( VBFLOATINGPALETTE, 
						win.Caption );
					if ( floatingPalette != IntPtr.Zero )
						break;
				}
                
				// the solution explorer should be a direct child of the palette
				slnExplorer = Functions.FindWindowEx( floatingPalette, IntPtr.Zero, GENERICPANE,
					slnExplorerCaption );
			}

			IntPtr uiHierarchy = Functions.FindWindowEx( slnExplorer, IntPtr.Zero, 
				UIHIERARCHY, null );
			IntPtr treeHwnd = Functions.FindWindowEx( uiHierarchy, IntPtr.Zero, TREEVIEW, 
				null );         
 
			if ( treeHwnd == IntPtr.Zero )
				throw new ApplicationException( 
					"Could not attach to solution explorer treeview" );

			treeview_ = new TreeView( treeHwnd );

			// reset back to the original hiding-state and dockable state            
			explorerWindow.Linkable = linkable;
			explorerWindow.IsFloating = isFloating;
			if ( explorerWindow.Linkable )
				explorerWindow.AutoHides = hidden;



				
			// load the status images image strip
			log_.Info( "Loading status images" );
			Bitmap statusImages = (Bitmap)Image.FromStream( 
				this.GetType().Assembly.GetManifestResourceStream( STATUS_IMAGES ) );

			statusImages.MakeTransparent( statusImages.GetPixel(0,0) );

			this.statusImageList_ = new ImageList();
			this.statusImageList_.ImageSize = new Size(7, 16);
			this.statusImageList_.Images.AddStrip( statusImages );  
			originalImageList_ = treeview_.StatusImageList;
			treeview_.StatusImageList = this.statusImageList_.Handle;
		}

		public void Cleanup()
		{
			if( originalImageList_ != IntPtr.Zero )
			{
				treeview_.StatusImageList = originalImageList_ ;
			}
		}


	}
}
