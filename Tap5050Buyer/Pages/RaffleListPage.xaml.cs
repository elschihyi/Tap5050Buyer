﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xamarin.Forms;
using System.Threading.Tasks;

namespace Tap5050Buyer
{
    // TO DO: some pages need to be refactored to a better MVVM architecture
    public partial class RaffleListPage : ContentPage
    {
        public bool LocationDetected { get; set; }

        public bool IncludeSocialMedia { get; set; }

        private RaffleListViewModel _viewModel;

        public RaffleListPage(bool locationDetected, IList<RaffleLocation> raffleLocations, GeonamesCountrySubdivision countrySubdivision, bool includeSocialMedia)
        {
            InitializeComponent();
            NavigationPage.SetHasBackButton(this, false);

            LocationDetected = locationDetected;
            IncludeSocialMedia = includeSocialMedia;
            _viewModel = new RaffleListViewModel(raffleLocations, countrySubdivision);

            var locationPicker = new Picker();
            locationPicker.HorizontalOptions = LayoutOptions.Center;
            locationPicker.Title = "Pick a province";
            layout.Children.Add(locationPicker);

            if (locationDetected)
            {
                locationPicker.Items.Add(countrySubdivision.AdminName);
                locationPicker.SelectedIndex = 0;
                locationPicker.IsEnabled = false;

                var raffleLocation = _viewModel.MatchRaffleLocationWithCountrySubdivision(raffleLocations, countrySubdivision);

                if (raffleLocation == null)
                {
                    layout.Children.Add(new StackLayout
                        {
                            Children =
                            { new Label
                                {
                                    Text = "Sorry. There is no available raffle at your location.",
                                    HorizontalOptions = LayoutOptions.CenterAndExpand,
                                    VerticalOptions = LayoutOptions.CenterAndExpand,
                                }
                            },
                            Padding = new Thickness(20, 0, 20, 0),
                            VerticalOptions = LayoutOptions.CenterAndExpand,
                        }
                    );
                }
                else
                {
                    GetRaffleEventsAndCreateList(raffleLocation.Name);
                }
            }
            else
            {
                foreach (var location in raffleLocations)
                {
                    locationPicker.Items.Add(location.Name);
                }
                locationPicker.IsEnabled = true;
                locationPicker.SelectedIndexChanged += (sender, e) =>
                {
                    if (layout.Children.Count == 2)
                    {
                        layout.Children.RemoveAt(1);
                    }
                    GetRaffleEventsAndCreateList(locationPicker.Items[locationPicker.SelectedIndex]);
                };
            }
        }

        // Have to make this func because we can't have async ctor
        public async void GetRaffleEventsAndCreateList(string raffleLocationName)
        {
            var raffleEvents = await _viewModel.GetRaffleEventsAtLocation(raffleLocationName);
            CreateRaffleEventList(raffleEvents);
        }

        public void CreateRaffleEventList(IList<RaffleEvent> raffleEvents)
        {
            var raffleEventListView = new ListView();
            raffleEventListView.ItemsSource = raffleEvents;
            raffleEventListView.ItemTemplate = new DataTemplate(typeof(RaffleEventCell));
            raffleEventListView.ItemSelected += (sender, e) =>
            {
                if (e.SelectedItem != null)
                {
                    // PushAsync a new RaffleDetailsPage instead of creating one and reuse it: to workaround a bug in Carousel + TabbedPage in iOS
                    this.Navigation.PushAsync(new RaffleDetailsPage(LocationDetected, raffleEvents, ((RaffleEvent)e.SelectedItem).Id, IncludeSocialMedia));
                    raffleEventListView.SelectedItem = null;
                }
            };
            layout.Children.Add(raffleEventListView);
        }
    }

    public class RaffleEventCell : ViewCell
    {
        public RaffleEventCell()
        {
            var viewLayout = new StackLayout
            {
                HorizontalOptions = LayoutOptions.StartAndExpand,
                Orientation = StackOrientation.Horizontal,
            };
            
            var image = new Image
            {
                WidthRequest = 44,
                HeightRequest = 44,
            };
            image.SetBinding(Image.SourceProperty, new Binding("ImageUrl"));

            var labelLayout = new StackLayout
            {
                Padding = new Thickness(5, 0, 0, 0),
                VerticalOptions = LayoutOptions.CenterAndExpand,
                Orientation = StackOrientation.Vertical,
            };
            
            var eventNameLabel = new Label
            {
                YAlign = TextAlignment.Center,
                LineBreakMode = LineBreakMode.TailTruncation,
                FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label)),
            };
            eventNameLabel.SetBinding(Label.TextProperty, new Binding("Name"));

            // Organization name label
//            var organizationNameLable = new Label
//            {
//                YAlign = TextAlignment.Center,
//                LineBreakMode = LineBreakMode.TailTruncation,
//                FontSize = Device.GetNamedSize(NamedSize.Micro, typeof(Label)), 
//            };
//            organizationNameLable.SetBinding(Label.TextProperty, new Binding("Organization"));

            labelLayout.Children.Add(eventNameLabel);
//            labelLayout.Children.Add(organizationNameLable);

            viewLayout.Children.Add(image);
            viewLayout.Children.Add(labelLayout);

            View = viewLayout;
        }
    }
}