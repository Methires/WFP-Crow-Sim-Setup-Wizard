using System;
using System.Xml;
using System.IO;
using System.Linq;
using Xceed.Wpf.Toolkit;

namespace CrowdSimSetupWizard
{
    class ScenarioValidator
    {
        /* Things to check                                                          Exception:
         * 1. Main element name = scenario                                          ScenarioException("Invalid name for main markup.");
         * 2. Main element optional attribute                                       ScenarioException(""Scenario" can have only one attribute.")
         * 3. Main element attribute name = "name"                                  ScenarioException("Invalid attribute name for "scenario".")
         * 4. Main element childnodes != null                                       ScenarioException("Cannot use empty scenario.")
         * 5. Main element childnode name = "level"                                 ScenarioException("Invalid child node in "scenario".")
         * 6. Level mandatory attribute                                             ScenarioException((""Level" must have exactly one attribute. Check "level" number {i}.")
         * 7. Level attribute name = "id"                                           ScenarioException("Invalid attribute name in "level". Check "level" number {i}.")
         * 8. Level id attribute value same as order                                ScenarioException("Invalid attribute value. Check "level" number {i}."))
         * 9. Level childnodes != null                                              ScenarioException(""Level" cannot be empty. Check "level" number {i}."))
         * 10. Level childnodes name = "action"                                     ScenarioException("Invalid child node in "level". Check "level" number {i}."))
         * 11. Action three mandatory attributes                                    ScenarioException(""Action" must have exactly three attributes. Check "action" number {j} in "level" number {i}.")
         * 12. Action attributes names "prob", "name", "id"                         ScenarioException("Invalid attributes names in "action". Check "action" number {j} in "level" number {i}.");
         * 13. Action attribute id must have unique global value
         * 14. Action childnodes != null                                            ScenarioException(""Action" cannot be empty. Check "action" number {j} in "level" number {i}.")
         * 15. Action childnode name = "actor"                                      ScenarioException("Invalid element in "action". Check "action" number {j} in "level" number {i}.")
         * 16.a Actor one mandatory attribute if action name=walk                   ScenarioException("Invalid attribute in "actor\. Check "actor" number {k} in "action" number {j} in "level" number {i}.")
         * 16.b Actor two mandatory attribute else                                  ScenarioException("Invalid attributes in "actor\. Check "actor" number {k} in "action" number {j} in "level" number {i}.")
         * 17.a Actor attribute name="name"                                         ScenarioException("Invalid attribute name in "actor". Check "actor" number {k} in "action" number {j} in "level" number {i}.")
         * 17.b Actor attributes name "name", "mocapId"                             ScenarioException("Invalid attributes names in "actor". Check "actor" number {k} in "action" number {j} in "level" number {i}.")
         * 18.a If level id != 0, actor childnodes != null                          ScenarioException(""Actor" cannot be empty when "level" id is not 0. Check "actor" number {k} in "action " number {j} in "level" number {i}.")
         * 18.b If level id == 0, action childnodes == null                         ScenarioException(""Actor" must be empty when "level" id is 0. Check "actor" number {k} in "action" number {j} in "level" number {i}.", k, j, i))
         * 19. Actor childnode name="prev"                                          ScenarioException("Invalid child node in "actor". Check "actor" number {k} in "action" number {j} in "level" number {i}.")
         * 20. Prev element must have one attribute                                 
         * 21. Prev attribute name="id"
         * 22. Prev attribute id value must be equal to one of action elements id attribute value in previous level
         * 23. if !(action name=walk || name=run) Action name parameter value + actor mocapid parameter value = animationname
         * 24. if(level id != last id) each level must have action defined for all unique actors
         * 25. Total probability for unique actor in level must be equal to 1
         */
        public bool ValidateScenario(string path)
        {
            try
            {
                XmlDocument xml = LoadXmlFromFile(path);
                XmlElement scenario = xml.DocumentElement;

                if (!scenario.Name.ToLower().Equals("scenario"))
                {
                    throw new ScenarioException("Invalid name for main markup.");
                }

                if (!(scenario.Attributes.Count < 2))
                {
                    throw new ScenarioException("\"Scenario\" can have only one attribute.");
                }

                if (!scenario.HasAttribute("name"))
                {
                    throw new ScenarioException("Invalid attribute name for \"scenario\".");
                }

                if (scenario.ChildNodes.Count == 0)
                {
                    throw new ScenarioException("Cannot use empty scenario.");
                }

                foreach (XmlNode nodes in scenario.ChildNodes)
                {
                    if (!nodes.Name.ToLower().Equals("level"))
                    {
                        throw new ScenarioException("Invalid child node in \"scenario\".");
                    }
                }

                for (int i = 0; i < scenario.ChildNodes.Count; i++)
                {
                    var level = scenario.ChildNodes[i];
                    if (level.Attributes.Count != 1)
                    {
                        throw new ScenarioException(string.Format("\"Level\" must have exactly one attribute. Check \"level\" number {0}.", i));
                    }
                    if (!level.Attributes[0].Name.ToLower().Equals("id"))
                    {
                        throw new ScenarioException(string.Format("Invalid attribute name in \"level\". Check \"level\" number {0}.", i));
                    }
                    if (int.Parse(level.Attributes[0].Value) != i)
                    {
                        throw new ScenarioException(string.Format("Invalid attribute value. Check \"level\" number {0}.", i));
                    }
                    if (!level.HasChildNodes)
                    {
                        throw new ScenarioException(string.Format("\"Level\" cannot be empty. Check \"level\" number {0}.", i));
                    }
                    foreach (XmlNode nodes in level.ChildNodes)
                    {
                        if (!nodes.Name.ToLower().Equals("action"))
                        {
                            throw new ScenarioException(string.Format("Invalid child node in \"level\". Check \"level\" number {0}.", i));
                        }
                    }
                    for (int j = 0; j < level.ChildNodes.Count; j++ )
                    {
                        var action = level.ChildNodes[j];
                        if (action.Attributes.Count != 3 )
                        {
                            throw new ScenarioException(string.Format("\"Action\" must have exactly three attributes. Check \"action\" number {0} in \"level\" number {1}.", j, i));
                        }
                        int nameAttributeId, probAttributeId, idAttributeId;
                        if (!(FindAttributeAndIndex(action.Attributes,"name", out nameAttributeId) && FindAttributeAndIndex(action.Attributes, "prob", out probAttributeId) && FindAttributeAndIndex(action.Attributes, "id", out idAttributeId)))
                        {
                            throw new ScenarioException(string.Format("Invalid attribute name in \"action\". Check \"action\" number {0} in \"level\" number {1}.", j, i));
                        }

                        //TODO: Unique action id value check
                        //
                        //

                        if (action.ChildNodes.Count == 0)
                        {
                            throw new ScenarioException(string.Format("\"Action\" cannot be empty. Check \"action\" number {0} in \"level\" number {1}.", j, i));
                        }

                        foreach (XmlNode nodes in action.ChildNodes)
                        {
                            if (!nodes.Name.ToLower().Equals("actor"))
                            {
                                throw new ScenarioException(string.Format("Invalid child node in \"action\". Check \"action\" number {0} in \"level\" number {1}.", j, i));
                            }
                        }
                        for (int k = 0; k < action.ChildNodes.Count; k++)
                        {
                            var actor = action.ChildNodes[k];
                            int actorNameAttributeId, mocapIdAttributeId;
                            if (action.Attributes[nameAttributeId].Value.ToLower().Equals("walk") || action.Attributes[nameAttributeId].Value.ToLower().Equals("run"))
                            {
                                if (actor.Attributes.Count != 1)
                                {
                                    throw new ScenarioException(string.Format("Invalid attribute in \"actor\". Check \"actor\" number {0} in \"action\" number {1} in \"level\" number {2}.", k, j, i));
                                }
                                else if(!(FindAttributeAndIndex(actor.Attributes, "name", out actorNameAttributeId)))
                                {
                                    throw new ScenarioException(string.Format("Invalid attribute name in \"actor\". Check \"actor\" number {0} in \"action\" number {1} in \"level\" number {2}.", k, j, i));
                                }
                            }
                            else
                            {
                                if (actor.Attributes.Count != 2)
                                {
                                    throw new ScenarioException(string.Format("Invalid attributes in \"actor\". Check \"actor\" number {0} in \"action\" number {1} in \"level\" number {2}.", k, j, i));
                                }
                                else if(!(FindAttributeAndIndex(actor.Attributes, "name", out actorNameAttributeId) && FindAttributeAndIndex(actor.Attributes, "mocapId", out mocapIdAttributeId)))
                                {
                                    throw new ScenarioException(string.Format("Invalid attributes names in \"actor\". Check \"actor\" number {0} in \"action\" number {1} in \"level\" number {2}.", k, j, i));
                                }
                            }
                            if (i != 0 && actor.ChildNodes.Count == 0)
                            {
                                throw new ScenarioException(string.Format("\"Actor\" cannot be empty when \"level\" id is not 0. Check \"actor\" number {0} in \"action\" number {1} in \"level\" number {2}.", k, j, i));
                            }
                            else if (i == 0 && actor.ChildNodes.Count != 0)
                            {
                                throw new ScenarioException(string.Format("\"Actor\" must be empty when \"level\" id is 0. Check \"actor\" number {0} in \"action\" number {1} in \"level\" number {2}.", k, j, i));
                            }

                            if (i != 0)
                            {
                                foreach (XmlNode prev in actor.ChildNodes)
                                {
                                    if (!prev.Name.ToLower().Equals("prev"))
                                    {
                                        throw new ScenarioException(string.Format("Invalid child node in \"actor\". Check \"actor\" number {0} in \"action\" number {1} in \"level\" number {2}.", k, j, i));
                                    }
                                } 
                            }
                        }
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

        private bool FindAttributeAndIndex(XmlAttributeCollection attributes, string attributeName, out int index)
        {
            index = -1;
            for (int i = 0; i < attributes.Count; i++)
            {
                if (attributes.Item(i).Name.ToString().ToLower().Equals(attributeName.ToLower()))
                {
                    index = i;
                    return true;
                }
            }
            return false;
        }
    }
}
