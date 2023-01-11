using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestAppPicture.Views;
using Xamarin.Forms;

namespace TestAppPicture
{
    public partial class MainPage : ContentPage
    {

        public MainPage(MainPageView ModelPage)
        {
            InitializeComponent();
            this.BindingContext = ModelPage;
        }
    }
}

