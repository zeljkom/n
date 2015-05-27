using ZMDZ2NRKA.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Popups;
using Windows.Storage;
using Windows.ApplicationModel.Background;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace ZMDZ2NRKA {
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        public MainPage() {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
        }

        /// <summary>
        /// Gets the <see cref="NavigationHelper"/> associated with this <see cref="Page"/>.
        /// </summary>
        public NavigationHelper NavigationHelper {
            get { return this.navigationHelper; }
        }

        /// <summary>
        /// Gets the view model for this <see cref="Page"/>.
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel {
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
        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e) {
            ApplicationDataContainer sett = Windows.Storage.ApplicationData.Current.RoamingSettings;
            if (sett.Values.ContainsKey("txt1"))
                txt1.Text = sett.Values["txt1"].ToString();
            if (sett.Values.ContainsKey("txt2"))
                txt2.Text = sett.Values["txt2"].ToString();
            if (sett.Values.ContainsKey("txtResult"))
                txtResult.Text = sett.Values["txtResult"].ToString();
            
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e) {
            //Niš ne spremamo ovdje jer imamo samo jedan page
        }

        private async void btnAdd_Click(object sender, RoutedEventArgs e) {
            
            decimal num1 = 0;
            decimal num2 = 0;
            decimal res = 0;
            Boolean err = false;

            if (!Decimal.TryParse(txt1.Text, out num1)) {
                MessageDialog dlg = new MessageDialog("Prvi broj nije ispravan");
                await dlg.ShowAsync();
                return;
            } else if (!Decimal.TryParse(txt2.Text, out num2)) {
                MessageDialog dlg = new MessageDialog("Drugi broj nije ispravan");
                await dlg.ShowAsync();
                return;
            }

            try {
                res = num1 + num2;
            } catch (Exception) {
                err = true;
            }

            if (err) {
                MessageDialog dlg = new MessageDialog("Nije moguće zbrojiti");
                await dlg.ShowAsync();
            } else {
                
                string resultText = String.Format("{0}+{1}={2}",txt1.Text, txt2.Text, res.ToString());
                txtResult.Text = resultText;
                Windows.Storage.ApplicationData.Current.RoamingSettings.Values["txtResult"] = resultText;
                txt1.Text = "";
                txt2.Text = "";              
               
            }     
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
        protected async override void OnNavigatedTo(NavigationEventArgs e) {
            this.navigationHelper.OnNavigatedTo(e);

            var sett = Windows.Storage.ApplicationData.Current.RoamingSettings;
            if (sett.Values.ContainsKey("exitTime")) {                
                MessageDialog dialog = new MessageDialog(String.Format("Datum poslj. izlaska: {0}", sett.Values["exitTime"].ToString()));
                await dialog.ShowAsync();
            }


            var taskRegistration = BackgroundTaskRegistration.AllTasks.Values.FirstOrDefault();            
            if (taskRegistration == null) {

                BackgroundTaskBuilder taskBuilder = new BackgroundTaskBuilder();
                taskBuilder.Name = "MyBackgroundTask";                
                SystemTrigger trigger = new SystemTrigger(SystemTriggerType.TimeZoneChange, false);
                taskBuilder.SetTrigger(trigger);
                taskBuilder.AddCondition(new SystemCondition(SystemConditionType.InternetAvailable));                
                taskBuilder.TaskEntryPoint = typeof(MyBackgroundService.MyBackTask).FullName;
                BackgroundTaskRegistration registration = taskBuilder.Register();

                registration.Progress += registration_Progress;
                registration.Completed += registration_Completed;

            }
           

        }

        private void registration_Completed(BackgroundTaskRegistration sender, BackgroundTaskCompletedEventArgs args) {
            
        }

        private void registration_Progress(BackgroundTaskRegistration sender, BackgroundTaskProgressEventArgs args) {
            
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e) {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private void txt_TextChanged(object sender, TextChangedEventArgs e) {
            ApplicationDataContainer setting = Windows.Storage.ApplicationData.Current.RoamingSettings;
            setting.Values["txt1"] = txt1.Text;
            setting.Values["txt2"] = txt2.Text;
            
        }

        private void btnUnregister_Click(object sender, RoutedEventArgs e) {
            var taskRegistration = BackgroundTaskRegistration.AllTasks.Values.FirstOrDefault();
            
            if (taskRegistration != null) {
                taskRegistration.Unregister(true);
                taskRegistration.Progress -= registration_Progress;
                taskRegistration.Completed -= registration_Completed;
            }

        }


        
        
    }
}
