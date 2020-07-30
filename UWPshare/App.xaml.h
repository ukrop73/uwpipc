//
// App.xaml.h
// Declaration of the App class.
//

#pragma once

#include "App.g.h"

namespace UWPshare
{
	/// <summary>
	/// Provides application-specific behavior to supplement the default Application class.
	/// </summary>
	ref class App sealed
	{
	protected:
		virtual void OnLaunched(Windows::ApplicationModel::Activation::LaunchActivatedEventArgs^ e) override;
		void OnBackgroundActivated(Windows::ApplicationModel::Activation::BackgroundActivatedEventArgs^ args) override;
		

	internal:
		App();
		static Windows::ApplicationModel::AppService::AppServiceConnection^ getConnection();
		static Platform::String^ getVs();

	private:
		void OnSuspending(Platform::Object^ sender, Windows::ApplicationModel::SuspendingEventArgs^ e);
		void OnNavigationFailed(Platform::Object ^sender, Windows::UI::Xaml::Navigation::NavigationFailedEventArgs ^e);
		void OnTaskCanceled(Windows::ApplicationModel::Background::IBackgroundTaskInstance^ sender, Windows::ApplicationModel::Background::BackgroundTaskCancellationReason reason);
		//void OnRequestReceived(Windows::ApplicationModel::AppService::AppServiceConnection^ connection, Windows::ApplicationModel::AppService::AppServiceRequestReceivedEventArgs^ args);
	};
}
