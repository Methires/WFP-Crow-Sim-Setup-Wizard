using System;
using System.Xml;
using System.IO;
using Xceed.Wpf.Toolkit;

namespace CrowdSimSetupWizard
{
    class ScenarioValidator
    {
        public bool ValidateScenario(string path)
        {
            try
            {
                XmlDocument xml = LoadXmlFromFile(path);
                XmlElement scenario = xml.DocumentElement;

                if (!scenario.Name.ToLower().Equals("scenario"))
                {
                    throw new ScenarioException("Invalid name for main element.");
                    //throw new ScenarioException("ło kurwa! FAKO!");
                }

                if (!(scenario.Attributes.Count < 2))
                {
                    throw new ScenarioException("\"Scenario\" element can have only one attribute.");
                }

                if (!scenario.HasAttribute("name"))
                {
                    throw new ScenarioException("Invalid attribute name for \"scenario\" element.");
                }

                if (scenario.ChildNodes.Count == 0)
                {
                    throw new ScenarioException("Cannot use empty scenario.");
                }

                foreach (XmlNode nodes in scenario.ChildNodes)
                {
                    if (!nodes.Name.ToLower().Equals("level"))
                    {
                        throw new ScenarioException("Invalid elements in \"scenario\" element.");
                    }
                }
                int actionIndex = 0;
                for (int i = 0; i < scenario.ChildNodes.Count; i++)
                {
                    var level = scenario.ChildNodes.Item(i);
                    if (level.Attributes.Count != 1)
                    {
                        throw new ScenarioException(string.Format("\"Level\" must only one attribute. Check \"level\" element numer {0}.", i));
                    }
                    if (!level.Attributes.Item(0).Name.ToLower().Equals("id"))
                    {
                        throw new ScenarioException(string.Format("{1}.Invalid attribute name. Check \"level\" element numer {0}.", i, level.Attributes.Item(0).Name));
                    }
                    if (int.Parse(level.Attributes.Item(0).Value) != i)
                    {
                        throw new ScenarioException(string.Format("Invalid id attribute value. Check \"level\" element numer {0}.", i));
                    }

                }

                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return false;
            }
        }

        private XmlDocument LoadXmlFromFile(string path)
        {
            string xmlText = File.ReadAllText(path);
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(xmlText);

            return xml;
        }
    }
}
