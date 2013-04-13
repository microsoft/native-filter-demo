using System;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace NativeFilterDemo
{
    public partial class SecondPage : PhoneApplicationPage
    {
        public SecondPage()
        {
            InitializeComponent();
            BuildApplicationBar();
        }

        private void BuildApplicationBar()
        {
            ApplicationBar = new ApplicationBar();

            ApplicationBarIconButton button = new ApplicationBarIconButton(new Uri("/Assets/Icons/back.png", UriKind.Relative));
            button.Text = "Camera page";
            button.Click += BackButtonClick;
            ApplicationBar.Buttons.Add(button);
        }

        private void BackButtonClick(object sender, EventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}