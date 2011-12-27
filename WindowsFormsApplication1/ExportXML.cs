using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;

namespace BeachScouter
{
    public class ExportXML
    {



        private string xmlDocLocation;
        private XmlDocument xmlDoc;
        private Configuration configuration;
        private Game game;
        public ExportXML(String location, Game game)
        {
            this.xmlDocLocation = location;
            this.game = game;
            this.configuration = Program.getConfiguration();
            writeXml();
        }


        /********************************************************************************************************************/




        public void writeXml()
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
            XmlAttribute team_attribute = xmlDoc.CreateAttribute("Spieler_li");
            team_attribute.InnerText = configuration.Teama.Player1.Name;
            teamA.Attributes.Append(team_attribute);

            team_attribute = xmlDoc.CreateAttribute("Spieler_re");
            team_attribute.InnerText = configuration.Teama.Player2.Name;
            teamA.Attributes.Append(team_attribute);


            // Team 2 Element
            teamB = xmlDoc.CreateElement("Team2");
            team_attribute = xmlDoc.CreateAttribute("Spieler_li");
            team_attribute.InnerText = configuration.Teamb.Player1.Name;
            teamB.Attributes.Append(team_attribute);

            team_attribute = xmlDoc.CreateAttribute("Spieler_re");
            team_attribute.InnerText = configuration.Teamb.Player2.Name;
            teamB.Attributes.Append(team_attribute);

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
            
        }


        private XmlNode newPositionNode(Position position, string label)
        {
            XmlAttribute x, y, zeitpunkt;

            XmlNode newposition_label = xmlDoc.CreateElement(label);
            zeitpunkt = xmlDoc.CreateAttribute("Zeitpunkt");
            zeitpunkt.InnerText = position.Absolute_PositionTime.ToString(); 
            newposition_label.Attributes.Append(zeitpunkt);


            XmlNode position_element = xmlDoc.CreateElement("Position");
            x = xmlDoc.CreateAttribute("X");
            x.InnerText = position.WorldX.ToString();
            position_element.Attributes.Append(x);

            y = xmlDoc.CreateAttribute("Y");
            y.InnerText = position.WorldY.ToString();
            position_element.Attributes.Append(y);


            newposition_label.AppendChild(position_element);

            

            return newposition_label;
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
            spielzug_attribute.InnerText = rally.Absolute_start_time.ToString();
            newRally.Attributes.Append(spielzug_attribute);

            // Ende Attribute
            spielzug_attribute = xmlDoc.CreateAttribute("Ende");
            spielzug_attribute.InnerText = rally.Absolute_end_time.ToString();
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

            // Spieler_An Attribute
            spielzug_attribute = xmlDoc.CreateAttribute("Spieler_An");
            spielzug_attribute.InnerText = rally.Reception_pos.Player.Name;
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
            {
                XmlNode player = xmlDoc.CreateElement("Player");
                XmlAttribute player_attribute = xmlDoc.CreateAttribute("Name");
                player_attribute.InnerText = defenceplayers[i].Name;
                player.Attributes.Append(player_attribute);
                punktchance_aus_Defence.AppendChild(player);
            }
            newRally.AppendChild(punktchance_aus_Defence);

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

        public void addWinner(Team winner)
        {
            String teamname = winner.Name;

        }


        // Getter and Setter
        public string XmlDocLocation
        {
            get { return xmlDocLocation; }
            set { xmlDocLocation = value; }
        }
    }
}