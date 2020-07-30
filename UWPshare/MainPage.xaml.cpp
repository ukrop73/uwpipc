//
// MainPage.xaml.cpp
// Implementation of the MainPage class.
//

#include "pch.h"
#include "MainPage.xaml.h"

#include <Windows.h>
#include <appmodel.h>
#include <malloc.h>
#include <stdio.h>

using namespace UWPshare;

using namespace Platform;
using namespace Windows::Foundation;
using namespace Windows::Foundation::Collections;
using namespace Windows::UI::Xaml;
using namespace Windows::UI::Xaml::Controls;
using namespace Windows::UI::Xaml::Controls::Primitives;
using namespace Windows::UI::Xaml::Data;
using namespace Windows::UI::Xaml::Input;
using namespace Windows::UI::Xaml::Media;
using namespace Windows::UI::Xaml::Navigation;
using namespace Windows::UI::Core;

using namespace Windows::ApplicationModel;
using namespace Windows::ApplicationModel::AppService;
using namespace Concurrency;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

MainPage::MainPage()
{
	InitializeComponent();
}

//void UWPshare::MainPage::Connection_ServiceClosed(AppServiceConnection^ sender, AppServiceClosedEventArgs^ args)
//{
//    Dispatcher->RunAsync(CoreDispatcherPriority::Normal, ref new DispatchedHandler([this]()
//    {
//        // Dispose the connection
//        delete _connection;
//        _connection = nullptr;
//    }));
//}


void UWPshare::MainPage::OpenConnection_Click(Object^ sender, RoutedEventArgs^ e)
{
    tb1->Text = "huhuhuhu";

    //Is a connection already open?
    if (connection != nullptr)
    {
        return;
    }

    Package^ package = Package::Current;
    AppServiceConnection dataService;
    dataService.AppServiceName = "SampleInteropService";
    dataService.PackageFamilyName = package->Id->FamilyName;

    //Set up a new app service connection
    connection = ref new AppServiceConnection();
    connection->AppServiceName = "SampleInteropService";
    connection->PackageFamilyName = package->Id->FamilyName;
    connection->ServiceClosed += ref new TypedEventHandler<AppServiceConnection^, AppServiceClosedEventArgs^>(this, &UWPshare::MainPage::Connection_ServiceClosed);

    // open the connection
    create_task(connection->OpenAsync()).then([this](AppServiceConnectionStatus status)
    {
        //If the new connection opened successfully we're done here
        
        if (status == AppServiceConnectionStatus::Success)
        {
            tb1->Text = "Connection is open";
        }
        else
        {
            // Something went wrong. Show the user a meaningful message.
            switch (status)
            {
            case AppServiceConnectionStatus::AppNotInstalled:
                //rootPage->NotifyUser("The app AppServicesProvider is not installed. Deploy AppServicesProvider to this device and try again.", NotifyType::ErrorMessage);
                break;

            case AppServiceConnectionStatus::AppUnavailable:
                //rootPage->NotifyUser("The app AppServicesProvider is not available. This could be because it is currently being updated or was installed to a removable device that is no longer available.", NotifyType::ErrorMessage);
                break;

            case AppServiceConnectionStatus::AppServiceUnavailable:
                //rootPage->NotifyUser("The app AppServicesProvider is installed but it does not provide the app service " + _connection->AppServiceName, NotifyType::ErrorMessage);
                break;

            default:
            case AppServiceConnectionStatus::Unknown:
                //rootPage->NotifyUser("An unknown error occurred while we were trying to open an AppServiceConnection.", NotifyType::ErrorMessage);
                break;
            }

            // Dispose the connection
            delete connection;
            connection = nullptr;
        }
    }); // create_task()
} // OpenConnection_Click() 

void UWPshare::MainPage::Connection_ServiceClosed(AppServiceConnection^ sender, AppServiceClosedEventArgs^ args)
{
    //tb1->Text = "Connection_ServiceClosed";
    Dispatcher->RunAsync(CoreDispatcherPriority::Normal, ref new DispatchedHandler([this]()
    {
        // Dispose the connection
        delete connection;
        connection = nullptr;
    }));
}

void UWPshare::MainPage::Button_Click(Platform::Object^ sender, Windows::UI::Xaml::RoutedEventArgs^ e)
{
    IAsyncAction^ actionLaunch = FullTrustProcessLauncher::LaunchFullTrustProcessForCurrentAppAsync();
}


void UWPshare::MainPage::Button_Click_1(Platform::Object^ sender, Windows::UI::Xaml::RoutedEventArgs^ e)
{
    //const Windows::UI::Core::CoreDispatcher^ myDispatcher = CoreWindow::GetForCurrentThread()->Dispatcher;

    // Send a message to the app service
    auto inputs = ref new ValueSet();
    inputs->Insert("KEY", App::getVs());


    connection = UWPshare::App::getConnection();

    auto sendMessageTask = create_task(connection->SendMessageAsync(inputs));
    sendMessageTask.then([this](AppServiceResponse^ response)
    {
        // If the service responded display the message. We're done!

        Windows::UI::Popups::MessageDialog msg{ L"App:SendMessageAsync." };
        msg.ShowAsync();

        switch (response->Status)
        {
        case AppServiceResponseStatus::Success:
            tb1->Text = "Success.";
            break;
        case AppServiceResponseStatus::Failure:
            tb1->Text = "The service failed to acknowledge the message we sent it. It may have been terminated because the client was suspended.";
            break;

        case AppServiceResponseStatus::ResourceLimitsExceeded:
            tb1->Text = "The service exceeded the resources allocated to it and had to be terminated.";
            break;

        default:
        case AppServiceResponseStatus::Unknown:
            tb1->Text = "An unknown error occurred while we were trying to send a message to the service.";
            break;
        }
    }); // create_task().then()
}
