﻿using System;

using Xamarin.Forms;

namespace Tap5050Buyer
{
    public class InviteFriendsNavigationPage : NavigationPage
    {
        public InviteFriendsNavigationPage()
        {
            Title = "Invite Friends";

            this.PushAsync(new RaffleListPage(LoadingLocationViewModel.IsLocationDetected, LoadingLocationViewModel.RaffleLocations, GeolocationManager.CountrySubdivision, true));
        }
    }
}