using System;
using TestAppPicture.Views;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TestAppPicture
{
    public partial class App : Application
    {
        MainPageView pageModel;
        public App ()
        {
            InitializeComponent();
            pageModel = new MainPageView();
            MainPage = new MainPage(pageModel);
        }

        protected override void OnStart ()
        {
            if (pageModel != null)
                pageModel.Start();
        }

        protected override void OnSleep ()
        {
            if (pageModel != null)
                pageModel.Stop();
        }

        protected override void OnResume ()
        {

        }
    }
}

