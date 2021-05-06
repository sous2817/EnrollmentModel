using System;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace Semio.ClientService.Data.Intelligence
{
    [Serializable]
    [XmlTypeAttribute(AnonymousType = true)]
    [JsonObject(IsReference = true, MemberSerialization = MemberSerialization.OptOut)]
    public class InvestigationalEntityRegion
    {
        /// <summary>
        /// Gets or sets DataSourceOwner.
        /// </summary>
        /// <value>The Data Source Owner.</value>
        [XmlAttributeAttribute]
        public string RegionId { get; set; }

        /// <summary>
        /// Gets or sets RegionName.
        /// </summary>
        /// <value>The Data Source Name.</value>
        [XmlAttributeAttribute]
        public string RegionName { get; set; }

        /// <summary>
        /// Gets or sets RegionLongitude.
        /// </summary>
        /// <value>The Data Source Id.</value>
        [XmlAttributeAttribute]
        public string RegionLongitude { get; set; }

        /// <summary>
        /// Gets or sets RegionLatitude.
        /// </summary>
        /// <value>The Data Source Id.</value>
        [XmlAttributeAttribute]
        public string RegionLatitude { get; set; }
    }
}