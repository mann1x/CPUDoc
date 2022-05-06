using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel;
using System.Xml.Serialization;

namespace CPUDoc
{
    public class Event

    {
        public long recordId { get; set; }
        public int severity { get; set; }
        public string severityDesc { get; set; }
        public long eventId { get; set; }
        public DateTime timestamp { get; set; }
        public string gettimestamp
        {
            get => timestamp.ToString("dd/MM/yyyy H:mm");
        }
        public string reportedBy { get; set; }
        public string source { get; set; }
        public string type { get; set; }
        public string processor { get; set; }
        public string message { get; set; }
        public string errorSource { get; set; }
        public string apicId { get; set; }
        public string mcaBank { get; set; }
        public string mciStat { get; set; }
        public string mciAddr { get; set; }
        public string mciMisc { get; set; }
        public string errorType { get; set; }
        public string transactionType { get; set; }
        public string participation { get; set; }
        public string requestType { get; set; }
        public string memorIO { get; set; }
        public string memHierarchyLvl { get; set; }
        public string timeout { get; set; }
        public string operationType { get; set; }
        public string length { get; set; }
        public string rawData { get; set; }



    }
    [XmlRoot(ElementName = "Event")]
    public class XmlArray
    {
        [XmlArray("EventData")]
        [XmlArrayItem("Data", (typeof(XmlData)))]
        //[XmlArrayItem(typeof(XmlComplexData))]
        public XmlData[] EventData;
    }

    public class XmlData
    {
        [XmlAttribute("Name")]
        public string Name { get; set; }

        [XmlText]
        public string Value { get; set; }
    }

    /*
    public class XmlComplexData
    {
        [XmlAttribute]
        public string Name { get; set; }

        [XmlText(DataType = "hexBinary")]
        public byte[] Encoded { get; set; }
    }
    */
}
