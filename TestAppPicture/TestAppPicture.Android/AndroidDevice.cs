using System;
using Android.OS;
using TestAppPicture.Droid;
using TestAppPicture.Services;
using TestAppPicture.Views;

[assembly: Xamarin.Forms.Dependency(typeof(AndroidDevice))]
namespace TestAppPicture.Droid
{
    public class AndroidDevice : IDevice
    {
        public string GetIdentifier()
        {
            return Android.Provider.Settings.Secure.GetString(Android.App.Application.Context.ContentResolver, Android.Provider.Settings.Secure.AndroidId); ;
        }
    }
}

