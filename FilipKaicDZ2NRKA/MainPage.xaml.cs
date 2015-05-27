using FilipKaicDZ2NRKA.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace FilipKaicDZ2NRKA
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        public MainPage()
        {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
        }

        /// <summary>
        /// Gets the <see cref="NavigationHelper"/> associated with this <see cref="Page"/>.
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        /// <summary>
        /// Gets the view model for this <see cref="Page"/>.
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session.  The state will be null the first time a page is visited.</param>
        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            if (e.PageState != null && e.PageState.ContainsKey("firstNumber"))
            {
                firstNumber.Text = e.PageState["firstNumber"].ToString();
            }
            if (e.PageState != null && e.PageState.ContainsKey("secondNumber"))
            {
                secondNumber.Text = e.PageState["secondNumber"].ToString();
            }

            Windows.Storage.ApplicationDataContainer settings =
                Windows.Storage.ApplicationData.Current.LocalSettings;
            if (settings.Values.ContainsKey("time"))
            {
                ShowMessage(settings.Values["time"].ToString());
            }
        }

        private async void ShowMessage(string time)
        {
            MessageDialog md = new MessageDialog(time);

            await md.ShowAsync();
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            e.PageState["firstNumber"] = firstNumber.Text;
            e.PageState["secondNumber"] = secondNumber.Text;
        }

        #region NavigationHelper registration

        /// <summary>
        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// <para>
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="NavigationHelper.LoadState"/>
        /// and <see cref="NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.
        /// </para>
        /// </summary>
        /// <param name="e">Provides data for navigation methods and event
        /// handlers that cannot cancel the navigation request.</param>
        ///
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            RegisterBackgroundTask();
            this.navigationHelper.OnNavigatedTo(e);
        }

        private void RegisterBackgroundTask()
        {
            String name = "MyTask";

            foreach (var task in BackgroundTaskRegistration.AllTasks)
            {
                if (task.Value.Name == name)
                {
                    //
                    // The task is already registered.
                    //

                    return;
                }
            }

            var builder = new BackgroundTaskBuilder();
            builder.Name = name;
            builder.TaskEntryPoint = "WindowsRuntimeComponentDZ.Task";
            SystemTrigger triger = new SystemTrigger(SystemTriggerType.TimeZoneChange, false);
            builder.SetTrigger(triger);

            SystemCondition internetCondition = new SystemCondition(SystemConditionType.InternetAvailable);
            builder.AddCondition(internetCondition);

            BackgroundTaskRegistration myTask = builder.Register();

            foreach (var task in BackgroundTaskRegistration.AllTasks)
            {
                if (task.Value.Name == myTask.Name)
                {
                    AttachProgressAndCompletedHandlers(task.Value);
                }
            }
        }

        private void AttachProgressAndCompletedHandlers(IBackgroundTaskRegistration task)
        {
            task.Progress += new BackgroundTaskProgressEventHandler(OnProgress);
            task.Completed += new BackgroundTaskCompletedEventHandler(OnCompleted);
        }

        private void OnProgress(IBackgroundTaskRegistration task, BackgroundTaskProgressEventArgs args)
        {
            var progress = "Progress... " + args.Progress + "%";
        }

        private void OnCompleted(IBackgroundTaskRegistration task, BackgroundTaskCompletedEventArgs args)
        {
            var settings = ApplicationData.Current.LocalSettings;
            var key = task.TaskId.ToString();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private async void btnAddition_Click(object sender, RoutedEventArgs e)
        {
            int firstNumberInt = 0;
            int secondNumberInt = 0;
            try
            {
                firstNumberInt = Int32.Parse(firstNumber.Text);
                secondNumberInt = Int32.Parse(secondNumber.Text);

                int addition = firstNumberInt + secondNumberInt;
                MessageDialog msgAdd = new MessageDialog(addition.ToString());
                await msgAdd.ShowAsync();
                firstNumber.Text = string.Empty;
                secondNumber.Text = string.Empty;
            }
            catch (Exception exc)
            {
                Error(exc);
            }
        }

        private async void Error(Exception exc)
        {
            MessageDialog msg = new MessageDialog("Molimo vas upišite brojeve.");
            await msg.ShowAsync();
        }

        private void btnUnregister_Click(object sender, RoutedEventArgs e)
        {
            String name = "MyTask";

            foreach (var task in BackgroundTaskRegistration.AllTasks)
            {
                if (task.Value.Name == name)
                {
                    //
                    // The task is already registered.
                    //

                    task.Value.Unregister(true);
                    task.Value.Progress -= OnProgress;
                    task.Value.Completed -= OnCompleted;
                }
            }
        }
    }
}
