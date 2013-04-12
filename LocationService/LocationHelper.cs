	using System;
using System.Collections.Generic;

using MonoTouch.CoreLocation;
using MonoTouch.UIKit;

namespace LocationService
{
	public sealed class LocationHelper
	{
		public event EventHandler LocationObjectAdded;
		private Action _onSuccessEnableLocationCallback;

		CLLocationManager _locationManager;

		List<CLLocation> _locations;
		public List<CLLocation> Locations
		{
			get {return _locations;}
		}

		public List<object> LocationObjects {get; private set;}

		public LocationHelper()
		{
			_locations = new List<CLLocation>();
			LocationObjects = new List<object>();
			
			_locationManager = new CLLocationManager();
			_locationManager.Purpose = "надо включить!";
			_locationManager.DesiredAccuracy = CLLocation.AccuracyThreeKilometers;
			_locationManager.DistanceFilter = CLLocation.AccuracyNearestTenMeters;
			
			_locationManager.Delegate = new LocationManagerDelegate(this);
		}

		public void PromtUserToEnableLocationService(Action callcack)
		{
			if (CLLocationManager.Status == CLAuthorizationStatus.Authorized)
			{
				return;
			}

			_onSuccessEnableLocationCallback = callcack;
			_locationManager.AuthorizationChanged += HandleAuthorizationChanged;

			if (CLLocationManager.Status == CLAuthorizationStatus.NotDetermined)
			{
				_locationManager.StartMonitoringSignificantLocationChanges();
				_locationManager.StopMonitoringSignificantLocationChanges();
			}
			else if (
				   CLLocationManager.Status == CLAuthorizationStatus.Restricted
				|| CLLocationManager.Status == CLAuthorizationStatus.Denied
				)
			{
				UIAlertView alert = new UIAlertView("Need to enable location service", "To use this feature you must enable location service in settings", null, "Ok");
				alert.Show();
			}
		}

		void HandleAuthorizationChanged(object sender, CLAuthorizationChangedEventArgs e)
		{
			_locationManager.AuthorizationChanged -= HandleAuthorizationChanged;

			Action a = _onSuccessEnableLocationCallback;
			_onSuccessEnableLocationCallback = null;

			if (
				e.Status == CLAuthorizationStatus.Authorized
				&& a != null
				)
			{
				a();
			}
		}

		public void RaiseLocationObjectAdded()
		{
			if (LocationObjectAdded != null)
			{
				LocationObjectAdded(this, new EventArgs());
			}
		}

		public void StartLocationUpdates()
		{
			if (CLLocationManager.LocationServicesEnabled)
			{
				if(_locationManager != null)
				{
					_locationManager.StartUpdatingLocation();
				}
			}
			else
			{
				UIAlertView alert = new UIAlertView("cannot determine location", "locatio service are disabled", null, "OK");
				alert.Show();
			}
		}

		public bool StartSignificantLocationChangesUpdate()
		{
			bool result = false;

			// Чтобы следить за местоположением пользователя необхоидмо чтобы это было технически возможно с помощью данного девайса
			// и чтобы пользователь явно разрешил определять его местоположение
			if (   CLLocationManager.SignificantLocationChangeMonitoringAvailable
			    && CLLocationManager.Status == CLAuthorizationStatus.Authorized
			    )
			{
				_locationManager.StartMonitoringSignificantLocationChanges();
				result = true;
			}

			return result;
		}

		public bool StartRegionUpdates(CLRegion region)
		{
			bool result = false;

			// Чтобы следить за пересечением границ облостей необходимо узнать:
			// поддерживает ли телефон слежение за областями
			// разрешено ли данному приложения следить за облостями
			if (   CLLocationManager.RegionMonitoringAvailable
			    && CLLocationManager.Status == CLAuthorizationStatus.Authorized
			    )
			{
				_locationManager.StartMonitoring(region);
				result = true;
			}

			return result;
		}
	}

	public class LocationManagerDelegate : CLLocationManagerDelegate
	{
		LocationHelper _locationHelper;

		public LocationManagerDelegate(LocationHelper locationHelper)
		{
			_locationHelper = locationHelper;
		}

		public override void UpdatedLocation(CLLocationManager manager, CLLocation newLocation, CLLocation oldLocation)
		{
			Console.WriteLine("New location data = {0}", newLocation.Description());

			_locationHelper.Locations.Insert(0, newLocation);
			_locationHelper.LocationObjects.Insert(0, newLocation.Description());

			_locationHelper.RaiseLocationObjectAdded();
		}

		public override void Failed(CLLocationManager manager, MonoTouch.Foundation.NSError error)
		{
			Console.WriteLine("Location failed");

			if (error.Code == (int)CLError.Denied)
			{
				Console.WriteLine("Access to location services denied");

				manager.StopUpdatingLocation();
				manager.Delegate = null;
			}
		}

		public override void RegionEntered(CLLocationManager manager, CLRegion region)
		{
			_locationHelper.LocationObjects.Insert(0, string.Format("RegionEntered: {0}", region.Identifier));
			NotifyUser("RegionEntered", region.Identifier);
		}

		public override void RegionLeft(CLLocationManager manager, CLRegion region)
		{
			_locationHelper.LocationObjects.Insert(0, string.Format("RegionLeft: {0}", region.Identifier));
			NotifyUser("RegionLeft", region.Identifier);
		}

		public override void MonitoringFailed(CLLocationManager manager, CLRegion region, MonoTouch.Foundation.NSError error)
		{
			_locationHelper.LocationObjects.Insert(0, string.Format("MonitoringFailed: {0}", region.Identifier));
			NotifyUser("MonitoringFailed", region.Identifier);
		}

		public void NotifyUser(string title, string message)
		{
			if (UIApplication.SharedApplication.ApplicationState == UIApplicationState.Active)
			{
				UIAlertView alert = new UIAlertView(title, message, null, "OK");
				alert.Show();
			}
			else
			{
				UILocalNotification tn = new UILocalNotification();
				tn.AlertBody = string.Format("{0}: {1}", title, message);
				UIApplication.SharedApplication.PresentLocationNotificationNow(tn);
			}
		}
	}
}

