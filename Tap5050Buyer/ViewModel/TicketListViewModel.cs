﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace Tap5050Buyer
{
    public class TicketListViewModel
    {
        internal const string c_serverBaseAddress = "https://dev.tap5050.com/";
        internal const string c_ticketsApiAddress = "apex/tap5050_dev/Mobile_Web_Serv/tickets";
        internal const string c_userApiAddress = "apex/tap5050_dev/Mobile_Web_Serv/users";

        public List<Ticket> Tickets { get; set; }

        public IEnumerable<RaffleEventForTickets> EventForTicketsList { get; set; }

        public UserAccount UserAccount { get; set; }

        public TicketListViewModel()
        {
        }

        public async Task<List<Ticket>> GetTickets()
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(c_serverBaseAddress);

                HttpResponseMessage response = null;
                try // should be more fine grain maybe !!
                {
                    if (DatabaseManager.Token == null)
                    {
                        throw new Exception("Token is null while trying to retrieve tickets!");
                    }

                    var url = c_ticketsApiAddress + "?token_id=" + DatabaseManager.Token.Value;
                    response = await client.GetAsync(url);

                    var json = response.Content.ReadAsStringAsync().Result;

                    var obj = JsonConvert.DeserializeObject<JObject>(json);
                    var items = obj["items"];
                    if (items != null)
                    {
                        return JsonConvert.DeserializeObject<List<Ticket>>(items.ToString());
                    }
                    return null; 
                }
                catch (Exception e)
                {
                    throw new Exception("Error while retrieving account info:" + e.Message);
                }
            }
        }

        public async Task LoadData()
        {
            Tickets = await GetTickets();
            await GetAccountInfo(); // Not very good here!!

            if (Tickets != null)
            {
                var raffleEventForTicketsDic = new Dictionary<int, RaffleEventForTickets>();

                foreach (var ticket in Tickets)
                {
                    if (!raffleEventForTicketsDic.ContainsKey(ticket.EventId))
                    {
                        raffleEventForTicketsDic.Add(ticket.EventId, new RaffleEventForTickets
                            {
                                Name = ticket.RaffleName,
                                Id = ticket.EventId,
                                DrawDate = ticket.DrawDate,
                                LicenceNumber = ticket.LicenceNumber,
                                Type = ticket.RaffleType,
                                PrizeDescription = ticket.PrizeDescription,
                                JackpotTotal = ticket.JackpotTotal,
                                WinningTicket = ticket.WinningTicket,
                            });
                    }
                }

                EventForTicketsList = raffleEventForTicketsDic.Values;
            }
        }

        public async Task GetAccountInfo()
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(c_serverBaseAddress);

                HttpResponseMessage response = null;
                try // should be more fine grain maybe !!
                {
                    if (DatabaseManager.Token == null)
                    {
                        throw new Exception("Token is null while trying to retrieve data!");
                    }

                    var url = c_userApiAddress + "?token_id=" + DatabaseManager.Token.Value;
                    response = await client.GetAsync(url);

                    var json = response.Content.ReadAsStringAsync().Result;

                    UserAccount = JsonConvert.DeserializeObject<UserAccount>(json);

                    // Translate from code to full name
                    var country = DatabaseManager.DbConnection.Table<Country>().Where(x => x.CountryCode == UserAccount.CountryCode).FirstOrDefault();
                    if (country != null)
                    {
                        UserAccount.Country = country.CountryName;
                    }

                    var province = DatabaseManager.DbConnection.Table<Province>().Where(x => x.ProvinceAbbreviation == UserAccount.ProvinceAbbreviation).FirstOrDefault();
                    if (province != null)
                    {
                        UserAccount.Province = province.ProvinceName;
                    }
                }
                catch (Exception e)
                {
                    throw new Exception("Error while retrieving account info:" + e.Message);
                }
            }
        }
    }
}