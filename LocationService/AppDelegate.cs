using System;
using System.Collections.Generic;
using System.Linq;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace LocationService
{
	// The UIApplicationDelegate for the application. This class is responsible for launching the 
	// User Interface of the application, as well as listening (and optionally responding) to 
	// application events from iOS.
	[Register ("AppDelegate")]
	public partial class AppDelegate : UIApplicationDelegate
	{
		UIWindow window;
		MainController _controller;

		UIAlertView _notificationAlert;
		UILocalNotification _lastLocalNotification;

		public override bool FinishedLaunching(UIApplication app, NSDictionary options)
		{
			window = new UIWindow(UIScreen.MainScreen.Bounds);

			_controller = new MainController();
			window.RootViewController = _controller;

			_notificationAlert = new UIAlertView();
			_notificationAlert.Title = "App receive local notification";
			_notificationAlert.AddButton("Yes");
			_notificationAlert.AddButton("No");
			_notificationAlert.CancelButtonIndex = 1;
			_notificationAlert.Clicked += OnButtonPressed;

			window.MakeKeyAndVisible();

			return true;
		}

		public override bool WillFinishLaunching(UIApplication application, NSDictionary launchOptions)
		{
			/*
			UILocalNotification tn = new UILocalNotification();
			tn.AlertBody = "WillFinishLaunching";
			UIApplication.SharedApplication.PresentLocationNotificationNow(tn);
			*/

			if (launchOptions != null)
			{
				NSObject launchFromLocations;
				if(launchOptions.TryGetValue(UIApplication.LaunchOptionsLocationKey, out launchFromLocations))
				{
					if(((NSNumber)launchFromLocations).BoolValue)
					{
						UILocalNotification ln = new UILocalNotification();
						ln.AlertBody = "position changed!";
						ln.AlertAction = "просмотреть";
						UIApplication.SharedApplication.PresentLocationNotificationNow(ln);
					}
				}
			}

			return true;
		}

		public override void ReceivedLocalNotification(UIApplication application, UILocalNotification notification)
		{
			_lastLocalNotification = notification;
			_notificationAlert.Message = string.Format(
@"Do you want to cancel it?
Alertbody: {0}", notification.AlertBody);

			_notificationAlert.Show();
		}

		void OnButtonPressed(object sender, UIButtonEventArgs e)
		{
			if (e.ButtonIndex == 0)
			{
				UIApplication.SharedApplication.CancelLocalNotification(_lastLocalNotification);
				Console.WriteLine("local notification canceled");
			}
			else
			{
				Console.WriteLine("local notification is not canceled");
			}

		}
	}
}

