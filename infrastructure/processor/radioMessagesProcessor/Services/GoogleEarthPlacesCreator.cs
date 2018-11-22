using RadioMessagesProcessor.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace radioMessagesProcessor.Services
{
    public class GoogleEarthPlacesCreator
    {
        public static void ToPlaceFile(RadioLocationMessageDto locationEvent)
        {
            var sb = new System.Text.StringBuilder();
            sb.Append(placemark_main.Replace("%%name%%", "GPS location")
                    .Replace("%%longitude%%", locationEvent.GpsLongitude.ToString())
                    .Replace("%%latitude%%", locationEvent.GpsLatitude.ToString()));
            foreach (var cell in locationEvent.Cells.Where(c => c.IsDecoded))
            {
                sb.AppendLine((cell.IsMain ? placemark_main_tower : placemark_tower)
                    .Replace("%%name%%", cell.ToFriendlyName())
                    .Replace("%%longitude%%", cell.Longitude.ToString())
                    .Replace("%%latitude%%", cell.Latitude.ToString()));
            }

            var fileNamePostFix = locationEvent.DeviceDate.ToLocalTime().ToString().Replace(":", "-").Replace(" ", "_");
            System.IO.File.WriteAllText($"c:\\temp\\kmls\\test-{fileNamePostFix}.kml", klm_file.Replace("%%placemarks%%", sb.ToString()));
        }

        static string klm_file = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<kml xmlns = ""http://www.opengis.net/kml/2.2"" xmlns:gx=""http://www.google.com/kml/ext/2.2"" xmlns:kml=""http://www.opengis.net/kml/2.2"" xmlns:atom=""http://www.w3.org/2005/Atom"">
<Document>
	<name>test.kml</name>
	<StyleMap id = ""msn_ylw-pushpin"">
        <Pair>
            <key>normal</key>
            <styleUrl>#sn_ylw-pushpin</styleUrl>
		</Pair>
        <Pair>
            <key> highlight </key>
            <styleUrl>#sh_ylw-pushpin</styleUrl>
		</Pair>
    </StyleMap>
    <Style id=""sn_ylw-pushpin"">
		<IconStyle>
			<scale>1.1</scale>
			<Icon>
				<href>http://maps.google.com/mapfiles/kml/pushpin/ylw-pushpin.png</href>
			</Icon>
			<hotSpot x = ""20"" y=""2"" xunits=""pixels"" yunits=""pixels""/>
		</IconStyle>
		<LabelStyle>
			<scale>0.9</scale>
		</LabelStyle>
	</Style>
	<Style id = ""sh_ylw-pushpin"">
        <IconStyle>
            <scale> 1.3 </scale>
            <Icon>
                <href> http://maps.google.com/mapfiles/kml/pushpin/ylw-pushpin.png</href>
			</Icon>
			<hotSpot x = ""20"" y=""2"" xunits=""pixels"" yunits=""pixels""/>
		</IconStyle>
		<LabelStyle>
			<scale>0.9</scale>
		</LabelStyle>
	</Style>
	<StyleMap id = ""msn_cell_tower_white"">
        <Pair>
            <key> normal </key>
            <styleUrl>#sn_cell_tower_white</styleUrl>
		</Pair>
        <Pair>
            <key>highlight</key>
            <styleUrl>#sh_cell_tower_white</styleUrl>
		</Pair>
    </StyleMap>
    <Style id=""sn_cell_tower_white"">
		<IconStyle>
			<scale>1.2</scale>
			<Icon>
				<href>cell_tower_white.png</href>
			</Icon>
		</IconStyle>
		<LabelStyle>
			<scale>0.9</scale>
		</LabelStyle>
	</Style>
	<Style id = ""sh_cell_tower_white"">
        <IconStyle>
            <scale>1.4</scale>
            <Icon>
                <href> cell_tower_white.png </href>
            </Icon>
        </IconStyle>
        <LabelStyle>
            <scale>0.9</scale>
        </LabelStyle>
    </Style>

    <StyleMap id = ""main_cell_tower"">
        <Pair>
            <key> normal</key>
            <styleUrl>#sn_main_tower</styleUrl>
		</Pair>
        <Pair>
            <key>highlight</key>
            <styleUrl>#sh_main_tower</styleUrl>
		</Pair>
    </StyleMap>
	
	<Style id = ""sn_main_tower"" >
        <IconStyle>
            <color>ff0554ff</color>
            <scale>1.2</scale>
            <Icon>
                <href> cell_tower_white.png </href>
            </Icon>
        </IconStyle>
        <LabelStyle>
            <scale> 0.9 </scale>
        </LabelStyle >
    </Style>

    <Style id = ""sh_main_tower"">
        <IconStyle>
		    <color>ff0554ff</color>
            <scale>1.4</scale>
            <Icon>
                <href>cell_tower_white.png</href>
            </Icon>
        </IconStyle>
        <LabelStyle>
            <scale>0.9</scale>
        </LabelStyle>
    </Style>



    <Folder>
        <name>My Places</name>
		<open>1</open>
		<Style>
			<ListStyle>
				<listItemType>check</listItemType>
				<ItemIcon>
					<state>open</state>
					<href>:/mysavedplaces_open.png</href>
				</ItemIcon>
				<ItemIcon>
					<state>closed</state>
					<href>:/mysavedplaces_closed.png</href>
				</ItemIcon>
				<bgColor>00ffffff</bgColor>
				<maxSnippetLines>2</maxSnippetLines>
			</ListStyle>
		</Style>
		%%placemarks%%
	</Folder>
</Document>
</kml>";


        static string placemark_main = @"<Placemark>
			<name>%%name%%</name>
			<LookAt>
				<longitude>%%longitude%%</longitude>
				<latitude>%%latitude%%</latitude>
				<altitude>0</altitude>
				<heading>0</heading>
				<tilt>0</tilt>
				<range>1600</range>
				<gx:altitudeMode>relativeToSeaFloor</gx:altitudeMode>
			</LookAt>
			<styleUrl>#msn_ylw-pushpin</styleUrl>
			<Point>
				<gx:drawOrder>1</gx:drawOrder>
				<coordinates>%%longitude%%,%%latitude%%,0</coordinates>
			</Point>
		</Placemark>";

        static string placemark_main_tower = @"
		<Placemark>
			<name>%%name%%</name>
			<styleUrl>#main_cell_tower</styleUrl>
			<Point>
				<gx:drawOrder>1</gx:drawOrder>
				<coordinates>%%longitude%%,%%latitude%%,0</coordinates>
			</Point>
		</Placemark>";

        static string placemark_tower = @"
		<Placemark>
			<name>%%name%%</name>
			<styleUrl>#msn_cell_tower_white</styleUrl>
			<Point>
				<gx:drawOrder>1</gx:drawOrder>
				<coordinates>%%longitude%%,%%latitude%%,0</coordinates>
			</Point>
		</Placemark>";
    }
}
