﻿using System;
using Xamarin.Forms;

namespace Tap5050Buyer
{
    public class AccountInfoNavigationPage : NavigationPage
    {
        public AccountInfoNavigationPage()
        {
            Title = "Account Info";

            DatabaseManager.Token = DatabaseManager.GetFirstToken();
            if (DatabaseManager.Token == null)
            {
                this.PushAsync(new LoginPage());
            }
            else
            {
                this.PushAsync(new AccountInfoPage());
            }
        }
    }
}