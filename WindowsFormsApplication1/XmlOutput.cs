using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Windows.Forms;
using System.Drawing;

namespace BeachScouter
{
    public class XmlOutput
    {

        
        
        private string xmlDocLocation; // for the writing
        private XmlDocument xmlDoc; //for the writing
        private Configuration configuration;
        private Game game;

        public XmlOutput()
        {
            this.configuration = Program.getConfiguration();
        }


/******************************************* GETTER AND SETTER *****************************************************/
        public Configuration Configuration
        {
            get { return configuration; }
            set { configuration = value;
                  xmlDocLocation = configuration.XmlOutputFile;
                }
        }

        public void setGame(Game game) { this.game = game; }
/********************************************************************************************************************/




        public void readXml()
        {
            if (File.Exists(xmlDocLocation))
            {
                xmlDoc = new XmlDocument();
                xmlDoc.Load(xmlDocLocation);
            }

            else
            {
                xmlDoc = new XmlDocument();

                XmlNode docRoot;
                XmlNode spiel, freeText;
                XmlNode teamA, teamB;
                XmlNode ergebnis;

                

                docRoot = xmlDoc.CreateElement("File");
                xmlDoc.AppendChild(docRoot);


                // Spiel Element
                spiel = xmlDoc.CreateElement("Spiel");


                // ID Attribute
                XmlAttribute spiel_attribute = xmlDoc.CreateAttribute("ID");
                spiel_attribute.InnerText = configuration.GameID;
                spiel.Attributes.Append(spiel_attribute);

                // Datum Attribute
                spiel_attribute = xmlDoc.CreateAttribute("Datum");
                spiel_attribute.InnerText = DateTime.Now.ToShortDateString();
                spiel.Attributes.Append(spiel_attribute);

                // Ort Attribute
                spiel_attribute = xmlDoc.CreateAttribute("Ort");
                spiel_attribute.InnerText = configuration.Location;
                spiel.Attributes.Append(spiel_attribute);

                // Art Attribute
                spiel_attribute = xmlDoc.CreateAttribute("Art");
                spiel_attribute.InnerText = configuration.TournamentType;
                spiel.Attributes.Append(spiel_attribute);

                // Video URL Attribute
                spiel_attribute = xmlDoc.CreateAttribute("Video_Url");
                spiel_attribute.InnerText = configuration.Mediafolderpath;
                spiel.Attributes.Append(spiel_attribute);

               


                // Team 1 Element
                teamA = xmlDoc.CreateElement("Team1");
                XmlNode player = xmlDoc.CreateElement("Player1");
                player.AppendChild(newPlayerNode(configuration.Teama.Player1));
                teamA.AppendChild(player);

                player = xmlDoc.CreateElement("Player2");
                player.AppendChild(newPlayerNode(configuration.Teama.Player2));
                teamA.AppendChild(player);
     

                // Team 2 Element
                teamB = xmlDoc.CreateElement("Team2");
                player = xmlDoc.CreateElement("Player1");
                player.AppendChild(newPlayerNode(configuration.Teamb.Player1));
                teamB.AppendChild(player);

                player = xmlDoc.CreateElement("Player2");
                player.AppendChild(newPlayerNode(configuration.Teamb.Player2));
                teamB.AppendChild(player);


                // Kommentar Element
                freeText = xmlDoc.CreateElement("Kommentar");
                freeText.InnerText = configuration.FreeText;

                ergebnis = xmlDoc.CreateElement("Ergebnis");
                if (game.Sets.Count > 0)
                {
                    XmlNode satz = xmlDoc.CreateElement("Satz1");
                    satz.InnerText = game.Sets[0].TeamAScore + ":" + game.Sets[0].TeamBScore;
                    ergebnis.AppendChild(satz);
                }

                if (game.Sets.Count > 1)
                {
                    XmlNode satz = xmlDoc.CreateElement("Satz2");
                    satz.InnerText = game.Sets[1].TeamAScore + ":" + game.Sets[1].TeamBScore;
                    ergebnis.AppendChild(satz);
                }

                if (game.Sets.Count > 2)
                {
                    XmlNode satz = xmlDoc.CreateElement("Satz3");
                    satz.InnerText = game.Sets[2].TeamAScore + ":" + game.Sets[2].TeamBScore;
                    ergebnis.AppendChild(satz);
                }


                spiel.AppendChild(teamA);
                spiel.AppendChild(teamB);
                spiel.AppendChild(freeText);
                spiel.AppendChild(ergebnis);

                docRoot.AppendChild(spiel);
                xmlDoc.Save(xmlDocLocation);

                spiel.AppendChild(teamA);
                spiel.AppendChild(teamB);
                spiel.AppendChild(freeText);
                docRoot.AppendChild(spiel);

               // if (xmlDocLocation!=null)
                    xmlDoc.Save(xmlDocLocation);
            }
        }

        

        private XmlNode newPositionNode(Position position, string label)
        {
            XmlAttribute x, y, zeitpunkt;

            XmlNode newposition_label = xmlDoc.CreateElement(label);
            zeitpunkt = xmlDoc.CreateAttribute("ZeitpunktRelativ");
            zeitpunkt.InnerText = position.PositionTime.ToString();
            newposition_label.Attributes.Append(zeitpunkt);


            XmlNode position_element = xmlDoc.CreateElement("Position");
            x = xmlDoc.CreateAttribute("WX");
            x.InnerText = position.WorldX.ToString();
            position_element.Attributes.Append(x);

            x = xmlDoc.CreateAttribute("IX");
            x.InnerText = position.ImageX.ToString();
            position_element.Attributes.Append(x);

            y = xmlDoc.CreateAttribute("WY");
            y.InnerText = position.WorldY.ToString();
            position_element.Attributes.Append(y);

            y = xmlDoc.CreateAttribute("IY");
            y.InnerText = position.ImageY.ToString();
            position_element.Attributes.Append(y);




            newposition_label.AppendChild(position_element);


            return newposition_label;
        }


        private XmlNode newPlayerNode(Person player)
        {
            XmlNode playernode = xmlDoc.CreateElement("Player");
            
            XmlAttribute player_attribute = xmlDoc.CreateAttribute("Name");
            player_attribute.InnerText = player.Name;
            playernode.Attributes.Append(player_attribute);

            player_attribute = xmlDoc.CreateAttribute("DefaultPosition");
            player_attribute.InnerText = player.defaultposition.ToString();
            playernode.Attributes.Append(player_attribute);

            player_attribute = xmlDoc.CreateAttribute("CurrentPosition");
            player_attribute.InnerText = player.Current_position.ToString();
            playernode.Attributes.Append(player_attribute);

            
            return playernode;
        }

        private XmlNode newRallyNode(Rally rally)
        {
            XmlNode newRally;
            XmlNode punktchance_aus_Defence;


            newRally = xmlDoc.CreateElement("Spielzug");

            // ID Attribute
            XmlAttribute spielzug_attribute = xmlDoc.CreateAttribute("ID");
            spielzug_attribute.InnerText = rally.Start_time.ToString();
            newRally.Attributes.Append(spielzug_attribute);

            // Anfang Attribute
            spielzug_attribute = xmlDoc.CreateAttribute("Anfang");
            spielzug_attribute.InnerText = rally.Start_time.ToString();
            newRally.Attributes.Append(spielzug_attribute);


            // Erfolg Attribute
            spielzug_attribute = xmlDoc.CreateAttribute("Erfolg");
            spielzug_attribute.InnerText = rally.Kill.ToString();
            newRally.Attributes.Append(spielzug_attribute);

            // Smash Attribute
            spielzug_attribute = xmlDoc.CreateAttribute("Smash");
            spielzug_attribute.InnerText = rally.Smash.ToString();
            newRally.Attributes.Append(spielzug_attribute);

            // Drop Attribute
            spielzug_attribute = xmlDoc.CreateAttribute("Drop");
            spielzug_attribute.InnerText = rally.Drop.ToString();
            newRally.Attributes.Append(spielzug_attribute);


            // Satz Attribute
            spielzug_attribute = xmlDoc.CreateAttribute("Satz");
            spielzug_attribute.InnerText = rally.CorrespondingSetNr.ToString();
            newRally.Attributes.Append(spielzug_attribute);

            // Bigpoint Attribute
            spielzug_attribute = xmlDoc.CreateAttribute("Bigpoint");
            spielzug_attribute.InnerText = rally.BigPoint.ToString();
            newRally.Attributes.Append(spielzug_attribute);

            // Spielstand Attribute
            spielzug_attribute = xmlDoc.CreateAttribute("Spielstand");
            spielzug_attribute.InnerText = configuration.Teama_score + ":" + configuration.Teamb_score;
            newRally.Attributes.Append(spielzug_attribute);

            // Timeout Attribute
            spielzug_attribute = xmlDoc.CreateAttribute("Timeout");
            spielzug_attribute.InnerText = rally.Timeout.ToString();
            newRally.Attributes.Append(spielzug_attribute);

            // Standardseite Attribute
            spielzug_attribute = xmlDoc.CreateAttribute("Standardseite");
            spielzug_attribute.InnerText = rally.Standardposition.ToString();
            newRally.Attributes.Append(spielzug_attribute);



            newRally.AppendChild(newPositionNode(rally.Service_pos, "Aufschlag"));
            newRally.AppendChild(newPositionNode(rally.Reception_pos, "Annahme"));
            newRally.AppendChild(newPositionNode(rally.Set_pos, "Zuspiel"));
            newRally.AppendChild(newPositionNode(rally.Takeoff_pos, "Anlauf"));
            newRally.AppendChild(newPositionNode(rally.Approach_pos, "Angriff"));
            newRally.AppendChild(newPositionNode(rally.Defence_pos, "Abwehr"));


            punktchance_aus_Defence = xmlDoc.CreateElement("Punktchance_aus_Defence");
            List<Person> defenceplayers = rally.Reception_pos.Defenceplayers;
            for (int i = 0; i < defenceplayers.Count; i++)
                punktchance_aus_Defence.AppendChild(newPlayerNode(defenceplayers[i]));

            newRally.AppendChild(punktchance_aus_Defence);

            XmlNode receptionplayer = xmlDoc.CreateElement("ReceptionPlayer");
            receptionplayer.AppendChild(newPlayerNode(rally.Reception_pos.Player));
            newRally.AppendChild(receptionplayer);

            return newRally;
        }


        public void addRally(Rally rally)
        {
            string xpathExpr = "descendant::Spiel";

            XmlNode rootNode, newRally, rallyTree;

            // Get rallies tree for given set number
            rootNode = xmlDoc.DocumentElement;
            rallyTree = rootNode.SelectSingleNode(xpathExpr);

            // Construct new rally node
            newRally = newRallyNode(rally);

            //xmlDoc.InsertAfter(newRally, lastRally);
            rallyTree.AppendChild(newRally);

            // Write rally to xml output file
            xmlDoc.Save(xmlDocLocation);
        }

        

        // this method updates a live rally which got changed in review mode
        public void updateReviewRally(Rally toChangeRally)
        {
            XmlNode rootNode, locCurrentNode;
            string liveRallyStartTime = toChangeRally.Start_time.ToString();

            rootNode = xmlDoc.DocumentElement;

            string xpathExpr = "descendant::Spiel/Spielzug[@ID=" + liveRallyStartTime + "]";
            locCurrentNode = rootNode.SelectSingleNode(xpathExpr);

     

            if (locCurrentNode != null)
            {
                locCurrentNode.ParentNode.RemoveChild(locCurrentNode);
                addRally(toChangeRally);
            }

        }

        public void removeNode(long nodeid)
        {
            XmlNode rootNode, locCurrentNode;
            rootNode = xmlDoc.DocumentElement;

            string xpathExpr = "descendant::Spiel/Spielzug[@ID=" + nodeid.ToString() + "]";
            locCurrentNode = rootNode.SelectSingleNode(xpathExpr);



            if (locCurrentNode != null)
            {
                locCurrentNode.ParentNode.RemoveChild(locCurrentNode);
                xmlDoc.Save(xmlDocLocation);
            }
        }

/********************************************************************************************************************/
/************************************************ LOADING A XML FILE ************************************************/
        public void loadXML(String location)
        {
            this.xmlDocLocation = location;
            configuration.XmlOutputFile = xmlDocLocation;

            xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlDocLocation);
            loadConfiguration();
        }


        private Person readConfigurationPlayer(XmlNode rootNode, int team, int number)
        {
            Person player = new Person();
            String xpath = "descendant::Spiel/Team"+team+"/Player"+number+"/Player/";

            player.Name = rootNode.SelectSingleNode(xpath+"@Name").InnerText;
            if (rootNode.SelectSingleNode(xpath+"@DefaultPosition").InnerText == "Left")
                player.defaultposition = Person.DefaultPosition.Left;
            else
                player.defaultposition = Person.DefaultPosition.Right;

            if (rootNode.SelectSingleNode(xpath + "@CurrentPosition").InnerText == "lowerLeft")
                player.Current_position = Rally.PlayerPosition.lowerLeft;

            if (rootNode.SelectSingleNode(xpath + "@CurrentPosition").InnerText == "lowerRight")
                player.Current_position = Rally.PlayerPosition.lowerRight;

            if (rootNode.SelectSingleNode(xpath + "@CurrentPosition").InnerText == "upperLeft")
                player.Current_position = Rally.PlayerPosition.upperLeft;

            if (rootNode.SelectSingleNode(xpath + "@CurrentPosition").InnerText == "upperRight")
                player.Current_position = Rally.PlayerPosition.upperRight;

            return player;

        }

        private Person readPlayer(String playername)
        {
            if (playername == configuration.Teama.Player1.Name)
                return configuration.Teama.Player1;

            if (playername == configuration.Teama.Player2.Name)
                return configuration.Teama.Player2;

            if (playername == configuration.Teamb.Player1.Name)
                return configuration.Teamb.Player1;

            
           return configuration.Teamb.Player2;

        }


        private void loadConfiguration()
        {

            XmlNode rootNode;
            rootNode = xmlDoc.DocumentElement;
            configuration.Teama.Name = rootNode.SelectSingleNode("descendant::Spiel/Team1/Player1/Player/@Name").InnerText + "/" + rootNode.SelectSingleNode("descendant::Spiel/Team1/Player2/Player/@Name").InnerText;
            configuration.Teamb.Name = rootNode.SelectSingleNode("descendant::Spiel/Team2/Player1/Player/@Name").InnerText + "/" + rootNode.SelectSingleNode("descendant::Spiel/Team2/Player2/Player/@Name").InnerText;

            configuration.Teama.Player1 = readConfigurationPlayer(rootNode, 1, 1);
            configuration.Teama.Player2 = readConfigurationPlayer(rootNode, 1, 2);

            configuration.Teamb.Player1 = readConfigurationPlayer(rootNode, 2, 1);
            configuration.Teamb.Player2 = readConfigurationPlayer(rootNode, 2, 2);

            configuration.GameID = rootNode.SelectSingleNode("descendant::Spiel/@ID").InnerText;

            configuration.Mediafolderpath = rootNode.SelectSingleNode("descendant::Spiel/@Video_Url").InnerText;


            configuration.FreeText = rootNode.SelectSingleNode("descendant::Spiel/Kommentar").InnerText;

            
        }


        private Position getPosition(XmlNode xmlposition)
        {
            Position position = new Position();
            position.PositionTime = Convert.ToInt64(xmlposition.SelectSingleNode("@ZeitpunktRelativ").InnerText);
            XmlNode coordinates = xmlposition.SelectSingleNode("Position");
            position.ImageX = Convert.ToInt32(coordinates.SelectSingleNode("@IX").InnerText);
            position.ImageY = Convert.ToInt32(coordinates.SelectSingleNode("@IY").InnerText);
            position.WorldX = Convert.ToInt32(coordinates.SelectSingleNode("@WX").InnerText);
            position.WorldY = Convert.ToInt32(coordinates.SelectSingleNode("@WY").InnerText);

            return position;
        }


        public void loadRallies(Form_Main mainForm)
        {
            XmlNode rootNode;
            rootNode = xmlDoc.DocumentElement;

            List<long> list_timestamps = new List<long>();
            ImageList imageList_screenshots = new ImageList();
            imageList_screenshots.ImageSize = new Size(84, 68);

            mainForm.listView_screenshots.Items.Clear();


            List<Set> sets = new List<Set>();
            sets.Add(new Set(1));
            sets.Add(new Set(2));
            sets.Add(new Set(3));

            XmlNodeList rallies = xmlDoc.GetElementsByTagName("Spielzug");

            for (int i = 0; i < rallies.Count; i++)
            {
                int setnr = Convert.ToInt32(rallies[i].SelectSingleNode("@Satz").InnerText);
                long starttime = Convert.ToInt64(rallies[i].SelectSingleNode("@ID").InnerText);
                list_timestamps.Add(starttime);
                

                Rally rally = new Rally(starttime,setnr);


                rally.Kill = (rallies[i].SelectSingleNode("@Erfolg").InnerText == "True");
                rally.Smash = (rallies[i].SelectSingleNode("@Smash").InnerText == "True");
                rally.Drop = (rallies[i].SelectSingleNode("@Drop").InnerText == "True");
                rally.BigPoint = (rallies[i].SelectSingleNode("@Bigpoint").InnerText == "True");
                rally.Timeout = (rallies[i].SelectSingleNode("@Timeout").InnerText == "True");
                rally.Standardposition = (rallies[i].SelectSingleNode("@Standardseite").InnerText == "True");
 

                // Get All Positions
                XmlNode xmlposition = rallies[i].SelectSingleNode("Aufschlag");
                rally.Service_pos = getPosition(xmlposition);

                xmlposition = rallies[i].SelectSingleNode("Annahme");
                rally.Reception_pos = getPosition(xmlposition);

                rally.Reception_pos.Player = new Person();
                String playername = rallies[i].SelectSingleNode("ReceptionPlayer/Player/@Name").InnerText;
                rally.Reception_pos.Player = readPlayer(playername);


                XmlNodeList defenceplayers = rallies[i].SelectNodes("Punktchance_aus_Defence/Player");
                List<Person> defenceplayerslist = new List<Person>();
                for (int c = 0; c < defenceplayers.Count; c++)
                {
                    string name = defenceplayers[c].SelectSingleNode("@Name").InnerText;
                    defenceplayerslist.Add(readPlayer(name));
                }
                rally.Reception_pos.Defenceplayers = defenceplayerslist;



                xmlposition = rallies[i].SelectSingleNode("Zuspiel");
                rally.Set_pos = getPosition(xmlposition);

                xmlposition = rallies[i].SelectSingleNode("Anlauf");
                rally.Takeoff_pos = getPosition(xmlposition);

                xmlposition = rallies[i].SelectSingleNode("Angriff");
                rally.Approach_pos = getPosition(xmlposition);

                xmlposition = rallies[i].SelectSingleNode("Abwehr");
                rally.Defence_pos = getPosition(xmlposition);



                // Set Screenshot
                CaptureStream capture_stream = new CaptureStream();
                Bitmap screenshot = capture_stream.createScreenshot(starttime, false);
                if (screenshot != null)
                {

                    imageList_screenshots.Images.Add(screenshot);
                    mainForm.setImageListScreenshots(imageList_screenshots);

                    int screenshot_index = i;
                    ListViewItem screenshot_item = new ListViewItem("", screenshot_index);

                    mainForm.listView_screenshots.Items.Add(screenshot_item);
                    mainForm.listView_screenshots.LargeImageList = mainForm.getImageListScreenshots();
                    mainForm.listView_screenshots.Refresh();
                }


                

                sets[setnr - 1].Rallies.Add(rally);
                
            }

            mainForm.Game.Sets = sets;
            mainForm.List_timestamps = list_timestamps;
        }


       



        // Getter and Setter
        public string XmlDocLocation
        {
            get { return xmlDocLocation; }
            set { xmlDocLocation = value; }
        } 
    }
}