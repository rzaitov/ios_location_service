using System;
using System.Drawing;

using MonoTouch.UIKit;
using MonoTouch.Foundation;
using MonoTouch.CoreLocation;


namespace LocationService
{
	public class MainController : UIViewController
	{
		MainView _mainView;
		LocationHelper _locationHelper;

		public MainController()
		{
			_locationHelper = new LocationHelper();

			_mainView = new MainView(_locationHelper);
			View = _mainView;

			_mainView.StartRegionMonitoringPressed += OnStartRegionMonitoringPressed;
			_mainView.StartSignificantLocationServicePressed += OnStartSignificantLocationServicePressed;
		}

		void OnStartSignificantLocationServicePressed()
		{
			if (CLLocationManager.Status != CLAuthorizationStatus.Authorized)
			{
				_locationHelper.PromtUserToEnableLocationService(StartSignificantLocationChangesUpdates);
			}
			else
			{
				StartSignificantLocationChangesUpdates();
			}
		}

		void OnStartRegionMonitoringPressed()
		{
			if (CLLocationManager.Status != CLAuthorizationStatus.Authorized)
			{
				_locationHelper.PromtUserToEnableLocationService(StartRegionUpdates);
			}
			else
			{
				StartRegionUpdates();
			}
		}

		/// <summary>
		/// Этот метод можно вызывать если известно что пользователь разрешил определение его местоположения
		/// </summary>
		private void StartRegionUpdates()
		{
			// работа
			CLLocationCoordinate2D workCenter = new CLLocationCoordinate2D(59.938396,30.264938);
			
			// Кировский завод
			CLLocationCoordinate2D homeCenter = new CLLocationCoordinate2D(59.875316,30.267856);

			CLRegion[] regions = new CLRegion[]
			{
				new CLRegion(workCenter, 100,  "work100"),
				new CLRegion(workCenter, 200,  "work200"),
				new CLRegion(workCenter, 300,  "work300"),
				new CLRegion(workCenter, 400,  "work400"),
				new CLRegion(workCenter, 500,  "work500"),
				new CLRegion(workCenter, 600,  "work600"),
				new CLRegion(workCenter, 700,  "work700"),
				new CLRegion(workCenter, 800,  "work800"),
				new CLRegion(workCenter, 900,  "work900"),
				new CLRegion(workCenter, 1000, "work1000"),
				
				// дом
				new CLRegion(homeCenter, 100,  "home100"),
				new CLRegion(homeCenter, 200,  "home200"),
				new CLRegion(homeCenter, 300,  "home300"),
				new CLRegion(homeCenter, 400,  "home400"),
				new CLRegion(homeCenter, 500,  "home500"),
				new CLRegion(homeCenter, 600,  "home600"),
				new CLRegion(homeCenter, 700,  "home700"),
				new CLRegion(homeCenter, 800,  "home800"),
				new CLRegion(homeCenter, 900,  "home900"),
				new CLRegion(homeCenter, 1000, "home1000")
			};
			// работа

			foreach (CLRegion r in regions)
			{
				_locationHelper.StartRegionUpdates(r);
				_locationHelper.LocationObjects.Insert(0, string.Format("start region updates: {0}", r.Identifier));
				_locationHelper.RaiseLocationObjectAdded();
			}
		}
	
		/// <summary>
		/// Этот метод можно вызывать если известно что пользователь разрешил определение его местоположения
		/// </summary>  
		private void StartSignificantLocationChangesUpdates()
		{
			_locationHelper.StartSignificantLocationChangesUpdate();
		}
	}

	public class MainView : UIView
	{
		public event Action StartSignificantLocationServicePressed;
		public event Action StartRegionMonitoringPressed;

		LocationHelper _locationHelper;

		UIButton _startSignificantLocationService;
		UIButton _startRegionMonitorring;

		UITableView _table;

		public MainView(LocationHelper locationHelper)
		{
			_locationHelper = locationHelper;

			_startRegionMonitorring = new UIButton(UIButtonType.RoundedRect);
			_startRegionMonitorring.SetTitle("Start RM", UIControlState.Normal);
			_startRegionMonitorring.Frame = new RectangleF(0, 0, 150, 50);
			_startRegionMonitorring.TouchUpInside += StartRegionMonitoringHandler;

			_startSignificantLocationService = new UIButton(UIButtonType.RoundedRect);
			_startSignificantLocationService.SetTitle("Start SLS", UIControlState.Normal);
			_startSignificantLocationService.Frame = new RectangleF(170, 0, 150, 50);
			_startSignificantLocationService.TouchUpInside += StartSignificantLocationServiceHandler;

			RectangleF tableFrame = new RectangleF(0, 50, 320, 410);
			_table = new UITableView(tableFrame, UITableViewStyle.Plain);
			_table.Source = new LocationTableSource(_locationHelper);

			_locationHelper.LocationObjectAdded += HandleLocationAdded;

			AddSubview(_startSignificantLocationService);
			AddSubview(_startRegionMonitorring);
			AddSubview(_table);
		}

		void StartSignificantLocationServiceHandler(object sender, EventArgs e)
		{
			if (StartSignificantLocationServicePressed != null)
			{
				StartSignificantLocationServicePressed();
			}
		}

		void StartRegionMonitoringHandler(object sender, EventArgs e)
		{
			if (StartRegionMonitoringPressed != null)
			{
				StartRegionMonitoringPressed();
			}
		}

		void HandleLocationAdded (object sender, EventArgs e)
		{
			_table.InsertRows( new NSIndexPath[] { NSIndexPath.FromRowSection(0, 0) }, UITableViewRowAnimation.None);
		}

		protected override void Dispose(bool disposing)
		{
			_startRegionMonitorring.TouchUpInside -= StartRegionMonitoringHandler;
			_startSignificantLocationService.TouchUpInside -= StartSignificantLocationServiceHandler;

			base.Dispose(disposing);
		}
	}

	public class LocationTableSource : UITableViewSource
	{
		LocationHelper _locationhelper;

		public LocationTableSource(LocationHelper locationHelper)
		{
			_locationhelper = locationHelper;
		}

		public override int RowsInSection(UITableView tableview, int section)
		{
			return _locationhelper.LocationObjects.Count;
		}

		public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
		{
			UITableViewCell cell = new UITableViewCell(UITableViewCellStyle.Default, null);
			cell.TextLabel.Font = UIFont.SystemFontOfSize(9);
			cell.TextLabel.Text = string.Format("[{0}] {1}", DateTime.Now.ToString("d HH:mm"), _locationhelper.LocationObjects[indexPath.Row].ToString());

			return cell;
		}
	}
}


