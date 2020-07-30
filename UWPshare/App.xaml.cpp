//
// App.xaml.cpp
// Implementation of the App class.
//

#include "pch.h"
#include "MainPage.xaml.h"
using namespace Concurrency;


using namespace UWPshare;

using namespace Platform;
using namespace Windows::ApplicationModel;
using namespace Windows::ApplicationModel::Activation;
using namespace Windows::Foundation;
using namespace Windows::Foundation::Collections;
using namespace Windows::UI::Xaml;
using namespace Windows::UI::Xaml::Controls;
using namespace Windows::UI::Xaml::Controls::Primitives;
using namespace Windows::UI::Xaml::Data;
using namespace Windows::UI::Xaml::Input;
using namespace Windows::UI::Xaml::Interop;
using namespace Windows::UI::Xaml::Media;
using namespace Windows::UI::Xaml::Navigation;

using namespace Windows::ApplicationModel::AppService;
using namespace Windows::ApplicationModel::Background;

static Platform::Agile<Windows::ApplicationModel::Background::BackgroundTaskDeferral^> _serviceDeferral;
static Windows::ApplicationModel::AppService::AppServiceConnection^ _connection;
static Platform::String^ vs = "1";

/// <summary>
/// Initializes the singleton application object.  This is the first line of authored code
/// executed, and as such is the logical equivalent of main() or WinMain().
/// </summary>
App::App()
{
    InitializeComponent();
    Suspending += ref new SuspendingEventHandler(this, &App::OnSuspending);
}

/// <summary>
/// Invoked when the application is launched normally by the end user.  Other entry points
/// will be used such as when the application is launched to open a specific file.
/// </summary>
/// <param name="e">Details about the launch request and process.</param>
void App::OnLaunched(Windows::ApplicationModel::Activation::LaunchActivatedEventArgs^ e)
{
    auto rootFrame = dynamic_cast<Frame^>(Window::Current->Content);

    // Do not repeat app initialization when the Window already has content,
    // just ensure that the window is active
    if (rootFrame == nullptr)
    {
        // Create a Frame to act as the navigation context and associate it with
        // a SuspensionManager key
        rootFrame = ref new Frame();

        rootFrame->NavigationFailed += ref new Windows::UI::Xaml::Navigation::NavigationFailedEventHandler(this, &App::OnNavigationFailed);

        if (e->PreviousExecutionState == ApplicationExecutionState::Terminated)
        {
            // TODO: Restore the saved session state only when appropriate, scheduling the
            // final launch steps after the restore is complete

        }

        if (e->PrelaunchActivated == false)
        {
            if (rootFrame->Content == nullptr)
            {
                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                rootFrame->Navigate(TypeName(MainPage::typeid), e->Arguments);
            }
            // Place the frame in the current Window
            Window::Current->Content = rootFrame;
            // Ensure the current window is active
            Window::Current->Activate();
        }
    }
    else
    {
        if (e->PrelaunchActivated == false)
        {
            if (rootFrame->Content == nullptr)
            {
                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                rootFrame->Navigate(TypeName(MainPage::typeid), e->Arguments);
            }
            // Ensure the current window is active
            Window::Current->Activate();
        }
    }
}

/// <summary>
/// Invoked when application execution is being suspended.  Application state is saved
/// without knowing whether the application will be terminated or resumed with the contents
/// of memory still intact.
/// </summary>
/// <param name="sender">The source of the suspend request.</param>
/// <param name="e">Details about the suspend request.</param>
void App::OnSuspending(Object^ sender, SuspendingEventArgs^ e)
{
    (void) sender;  // Unused parameter
    (void) e;   // Unused parameter

    //TODO: Save application state and stop any background activity
}

/// <summary>
/// Invoked when Navigation to a certain page fails
/// </summary>
/// <param name="sender">The Frame which failed navigation</param>
/// <param name="e">Details about the navigation failure</param>
void App::OnNavigationFailed(Platform::Object ^sender, Windows::UI::Xaml::Navigation::NavigationFailedEventArgs ^e)
{
    throw ref new FailureException("Failed to load Page " + e->SourcePageType.Name);
}

void OnRequestReceived(AppServiceConnection^ connection, AppServiceRequestReceivedEventArgs^ args);

void App::OnBackgroundActivated(Windows::ApplicationModel::Activation::BackgroundActivatedEventArgs ^args)
{
    __super::OnBackgroundActivated(args);

    Windows::UI::Popups::MessageDialog msg{ L"App:OnBackgroundActivated." };
    msg.ShowAsync();

    IBackgroundTaskInstance^ taskInstance = args->TaskInstance;
    // Take a deferral so the service isn't terminated
    _serviceDeferral = taskInstance->GetDeferral();

    taskInstance->Canceled += ref new BackgroundTaskCanceledEventHandler(this, &App::OnTaskCanceled);

    auto details = dynamic_cast<AppServiceTriggerDetails^>(taskInstance->TriggerDetails);
    _connection = details->AppServiceConnection;

    // Listen for incoming app service requests
    _connection->RequestReceived += ref new TypedEventHandler<AppServiceConnection^, AppServiceRequestReceivedEventArgs^>(OnRequestReceived);
    //_connection->RequestReceived += ref new TypedEventHandler<AppServiceConnection^, AppServiceRequestReceivedEventArgs^>(this, &App::OnRequestReceived);
}

Windows::ApplicationModel::AppService::AppServiceConnection^ UWPshare::App::getConnection()
{
    return _connection;
}

Platform::String^ UWPshare::App::getVs()
{
    return vs;
}

void OnRequestReceived(AppServiceConnection^ sender, AppServiceRequestReceivedEventArgs^ args)
{
    
    // Get a deferral so we can use an awaitable API to respond to the message
    auto messageDeferral = args->GetDeferral();

    

    auto input = args->Request->Message;
    /*int32 d1 = safe_cast<int32>(input->Lookup("D1"));
    int32 d2 = safe_cast<int32>(input->Lookup("D2"));*/

    auto d1 = input->Lookup("D1");
    auto d2 = input->Lookup("D2");

    vs = d1->ToString();

    Windows::UI::Popups::MessageDialog msg{ L"App:OnRequestReceived." };
    msg.ShowAsync();
    //return;

    // Create the response
    auto result = ref new ValueSet();
    result->Insert("RESULT", d1);

    // Send the response asynchronously
    create_task(args->Request->SendResponseAsync(result)).then([messageDeferral](AppServiceResponseStatus status)
    {
        // Complete the message deferral so the platform knows we're done responding
        messageDeferral->Complete();
    });

    
}

void App::OnTaskCanceled(Windows::ApplicationModel::Background::IBackgroundTaskInstance^ sender, Windows::ApplicationModel::Background::BackgroundTaskCancellationReason reason)
{
    Windows::UI::Popups::MessageDialog msg{ L"App:OnTaskCanceled." };
    msg.ShowAsync();

    if (_serviceDeferral != nullptr)
    {
        // Complete the service deferral
        _serviceDeferral->Complete();
        _serviceDeferral = nullptr;
    }

    delete _connection;
    _connection = nullptr;
}