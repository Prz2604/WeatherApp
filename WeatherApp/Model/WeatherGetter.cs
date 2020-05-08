﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace WeatherApp.Model
{
   public class WeatherGetter : INotifyPropertyChanged
    //facade
    {
        string URL;
        XDocument doc;

        string _city;
        string _days;
        public string City
        {
            get { return _city; }
            set 
            { 
                _city = value;
            }
        }

        public string Days
        {
            get { return _days; }
            set
            {
                _days = value;
            }
        }

        

        public void CreateHTTPRequestURL()
        {
           
            if(City.Equals("") || Days.Equals(""))
            {
                MessageBox.Show("You must provide value!");                
            }
            else if (!Regex.IsMatch(City, @"^[a-zA-Z]+$"))
            {
                MessageBox.Show("Use only latin letters in City field!");
            }
            else
            { 
                URL = string.Format(@"http://api.openweathermap.org/data/2.5/forecast?q="+ City + "&mode=xml&appid=2517431d46cd54e4f965409583890e1c&cnt=" + Days + "&units=metric");
            }
        }


        public void GetXMLData()
        {
            if(URL==null)
            {
               MessageBox.Show("Wrong URL passed!");                
            }
            else
            { 
                string html = string.Empty;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);

                try
                {
                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    using (Stream stream = response.GetResponseStream())

                    using (StreamReader reader = new StreamReader(stream))
                    {
                        html = reader.ReadToEnd();
                    }
                }
                catch(System.Net.WebException e)
                {
                    MessageBox.Show(e.Message);
                }

                             

                try
                {
                    doc= XDocument.Parse(html);
                }

                catch ( System.Xml.XmlException e)
                {
                    MessageBox.Show(e.Message);
                }
              
            }            
        }


        public List<DayWeather> PopulateDayWeatherList(List<DayWeather> passedList)
        {
            //XML data to linq querry anonymous class
            var days =
                from day
                in doc.Descendants("time")
                select new
                {
                    TemperatureValue = day.Element("temperature").Attribute("value").Value,
                    Clouds = day.Element("clouds").Attribute("value").Value,
                    Humidity = day.Element("humidity").Attribute("value").Value
                };


            //linq to list of DayWeather objects            
            foreach (var day in days)
            {
                passedList.Add(new DayWeather
                    (
                    Convert.ToDouble(day.TemperatureValue, System.Globalization.CultureInfo.InvariantCulture),
                    day.Clouds,
                    Convert.ToInt32(day.Humidity, System.Globalization.CultureInfo.InvariantCulture)));
            }
            return passedList;
        }

        public event PropertyChangedEventHandler PropertyChanged;   //event
        protected void OnPropertyChanged(string propertyName)   //raising
        {
            PropertyChangedEventHandler propertyChanged = PropertyChanged;
            if (propertyChanged != null)
                propertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}