using System;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace Semio.ClientService.Data.Intelligence
{
    /// <summary>
    /// This class contains the locaiton detail information for the investigationalEntity
    /// </summary>
    [Serializable]
    [XmlTypeAttribute(AnonymousType = true)]
    [JsonObject(IsReference = true, MemberSerialization = MemberSerialization.OptOut)]
    public class InvestigationalEntityLocation
    {
        public InvestigationalEntityLocation()
        {
            Address = new Address();
            Region = new InvestigationalEntityRegion();
        }

        public Address Address { get; set; }

        /// <summary>
        /// Gets or sets the location ID.
        /// </summary>
        /// <value>The location ID.</value>
        [XmlAttributeAttribute]
        public string LocationId { get; set; }

        /// <summary>
        /// Gets or sets the country ID.
        /// </summary>
        /// <value>The country ID.</value>
        [XmlAttributeAttribute]
        //[Obsolete("Pass-through. Please use the Address property's attributes directly")]
        public string CountryId
        {
            get { return Address.Country.Id; }
            set { Address.Country.Id = value; }
        }

        /// <summary>
        /// Gets or sets the name of the country.
        /// </summary>
        /// <value>The name of the country.</value>
        [XmlAttributeAttribute]
        //[Obsolete("Pass-through. Please use the Address property's attributes directly")]
        public string CountryName
        {
            get { return Address.Country.Name; }
            set { Address.Country.Name = value; }
        }

        /// <summary>
        /// Gets or sets the iso code of the country.
        /// </summary>
        [XmlAttributeAttribute]
        //[Obsolete("Pass-through. Please use the Address property's attributes directly")]
        public string IsoCountryCode
        {
            get { return Address.Country.IsoCode; }
            set { Address.Country.IsoCode = value; }
        }

        /// <summary>
        /// Gets or sets the state.
        /// </summary>
        /// <value>The state.</value>
        [XmlAttributeAttribute]
        //[Obsolete("Pass-through. Please use the Address property's attributes directly")]
        public string State
        {
            get { return Address.StateOrProvince; }
            set { Address.StateOrProvince = value; }
        }

        /// <summary>
        /// Gets or sets the city.
        /// </summary>
        /// <value>The city.</value>
        [XmlAttributeAttribute]
        //[Obsolete("Pass-through. Please use the Address property's attributes directly")]
        public string City
        {
            get { return Address.City; }
            set { Address.City = value; }
        }

        /// <summary>
        /// Gets or sets the latitude.
        /// </summary>
        /// <value>The latitude.</value>
        [XmlAttributeAttribute]
        public string Latitude { get; set; }

        /// <summary>
        /// Gets or sets the longitude.
        /// </summary>
        /// <value>The longitude.</value>
        [XmlAttributeAttribute]
        public string Longitude { get; set; }

        [XmlElement]
        public InvestigationalEntityRegion Region { get; set; }
    }
}