using Ducky.Model.Radio;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Linq;

namespace Ducky.Helpers.Radio
{
    public class StationFill
    {
        private string path { get; } = System.IO.Directory.GetCurrentDirectory();
        private ObservableCollection<Station> StationList { get; set; } = new ObservableCollection<Station>();

        public ObservableCollection<Station> RecordStationFill()
        {
            StationList.Clear();
            XDocument xDoc = XDocument.Load(path + @"/Data/RecordStations.xml");
            List<XElement> list = xDoc.Root.Elements().ToList();

            foreach (var name in list)
                StationList.Add(new Station() { RadioName = xDoc.Root.Element(name.Name.LocalName).Element("title").Value, });
            
            return StationList;
        }
        public ObservableCollection<Station> MoscowStationFill()
        {
            StationList.Clear();
            XDocument xDoc = XDocument.Load(path + @"/Data/MoscowRadioStations.xml");
            List<XElement> list = xDoc.Root.Elements().ToList();

            foreach (var name in list)
                StationList.Add(new Station() { RadioName = xDoc.Root.Element(name.Name.LocalName).Element("title").Value });

            return StationList;
        }
        public ObservableCollection<Station> BBCStationFill()
        {
            StationList.Clear();
            XDocument xDoc = XDocument.Load(path + @"/Data/BBCRadioStations.xml");
            List<XElement> list = xDoc.Root.Elements().ToList();

            foreach (var name in list)
                StationList.Add(new Station() { RadioName = xDoc.Root.Element(name.Name.LocalName).Element("title").Value });

            return StationList;
        }
    }
}
