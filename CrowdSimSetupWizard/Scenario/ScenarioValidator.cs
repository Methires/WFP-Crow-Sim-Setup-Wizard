using System;
using System.Xml;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Xceed.Wpf.Toolkit;
using System.Globalization;

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
                List<Level> levels = new List<Level>();

                if (!scenario.Name.ToLower().Equals("scenario"))
                {
                    throw new ScenarioException("Invalid name for main markup.");
                }
                else if (!(scenario.Attributes.Count < 2))
                {
                    throw new ScenarioException("\"Scenario\" can have only one attribute.");
                }
                else if (!scenario.HasAttribute("name"))
                {
                    throw new ScenarioException("Invalid attribute name for \"scenario\".");
                }
                else if (scenario.ChildNodes.Count == 0)
                {
                    throw new ScenarioException("Cannot use empty scenario.");
                }

                for (int i = 0; i < scenario.ChildNodes.Count; i++)
                {
                    var level = scenario.ChildNodes[i];
                    Level levelData = new Level();

                    if (!level.Name.ToLower().Equals("level"))
                    {
                        throw new ScenarioException("Invalid child node in \"scenario\".");
                    }
                    else if (level.Attributes.Count != 1)
                    {
                        throw new ScenarioException(string.Format("\"Level\" must have exactly one attribute. Check \"level\" number {0}.", i));
                    }
                    else if (!level.Attributes[0].Name.ToLower().Equals("id"))
                    {
                        throw new ScenarioException(string.Format("Invalid attribute name in \"level\". Check \"level\" number {0}.", i));
                    }
                    else if (int.Parse(level.Attributes[0].Value) != i)
                    {
                        throw new ScenarioException(string.Format("Invalid attribute value. Check \"level\" number {0}.", i));
                    }
                    else if (!level.HasChildNodes)
                    {
                        throw new ScenarioException(string.Format("\"Level\" cannot be empty. Check \"level\" number {0}.", i));
                    }

                    for (int j = 0; j < level.ChildNodes.Count; j++)
                    {
                        var action = level.ChildNodes[j];
                        int nameAttributeId, probAttributeId, idAttributeId;
                        Action actionData = new Action();

                        if (!action.Name.ToLower().Equals("action"))
                        {
                            throw new ScenarioException(string.Format("Invalid child node in \"level\". Check \"level\" number {0}.", i));
                        }
                        else if (action.Attributes.Count != 3)
                        {
                            throw new ScenarioException(string.Format("\"Action\" must have exactly three attributes. Check \"action\" number {0} in \"level\" number {1}.", j, i));
                        }
                        else if (!(FindAttributeAndIndex(action.Attributes, "name", out nameAttributeId) && FindAttributeAndIndex(action.Attributes, "prob", out probAttributeId) && FindAttributeAndIndex(action.Attributes, "id", out idAttributeId)))
                        {
                            throw new ScenarioException(string.Format("Invalid attribute name in \"action\". Check \"action\" number {0} in \"level\" number {1}.", j, i));
                        }
                        else if (action.ChildNodes.Count == 0)
                        {
                            throw new ScenarioException(string.Format("\"Action\" cannot be empty. Check \"action\" number {0} in \"level\" number {1}.", j, i));
                        }

                        for (int k = 0; k < action.ChildNodes.Count; k++)
                        {
                            var actor = action.ChildNodes[k];
                            int actorNameAttributeId, mocapIdAttributeId = -1;
                            Actor actorData = new Actor();

                            if (!actor.Name.ToLower().Equals("actor") && !actor.Name.ToLower().Equals("blend"))
                            {
                                throw new ScenarioException(string.Format("Invalid child node in \"action\". Check \"action\" number {0} in \"level\" number {1}.", j, i));
                            }

                            if (actor.Name.ToLower().Equals("blend"))
                            {
                                continue;
                            }

                            if (action.Attributes[nameAttributeId].Value.ToLower().Equals("walk") || action.Attributes[nameAttributeId].Value.ToLower().Equals("run"))
                            {
                                if (actor.Attributes.Count != 1)
                                {
                                    throw new ScenarioException(string.Format("Invalid attribute in \"actor\". Check \"actor\" number {0} in \"action\" number {1} in \"level\" number {2}.", k, j, i));
                                }
                                else if (!(FindAttributeAndIndex(actor.Attributes, "name", out actorNameAttributeId)))
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
                                else if (!(FindAttributeAndIndex(actor.Attributes, "name", out actorNameAttributeId) && FindAttributeAndIndex(actor.Attributes, "mocapId", out mocapIdAttributeId)))
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
                                List<int> previousActions = new List<int>();
                                for (int l = 0; l < actor.ChildNodes.Count; l++)
                                {
                                    var previous = actor.ChildNodes[l];
                                    int previousAttributeId;

                                    if (!previous.Name.ToLower().Equals("prev"))
                                    {
                                        throw new ScenarioException(string.Format("Invalid child node in \"actor\". Check \"actor\" number {0} in \"action\" number {1} in \"level\" number {2}.", k, j, i));
                                    }
                                    else if (previous.Attributes.Count != 1)
                                    {
                                        throw new ScenarioException(string.Format("Invalid number of attributes in \"previous\". Check \"previous\" number {0} in \"actor\" number {1} in \"action\" number {2} in \"level\" number {3}.", l, k, j, i));
                                    }
                                    else if (!(FindAttributeAndIndex(previous.Attributes, "id", out previousAttributeId)))
                                    {
                                        throw new ScenarioException(string.Format("Invalid attribute name in \"previous\". Check \"previous\" number {0} in \"actor\" number {1} in \"action\" number {2} in \"level\" number {3}.", l, k, j, i));
                                    }
                                    previousActions.Add(int.Parse(previous.Attributes[previousAttributeId].Value));
                                }
                                actorData.PreviousActivitiesIndexes = previousActions.ToArray();
                            }
                            actorData.Name = actor.Attributes[actorNameAttributeId].Value;
                            if (mocapIdAttributeId != -1)
                            {
                                actorData.MocapId = actor.Attributes[mocapIdAttributeId].Value;
                            }
                            actionData.Actors.Add(actorData);
                        }
                        actionData.Name = action.Attributes[nameAttributeId].Value;
                        actionData.Probability = float.Parse(action.Attributes[probAttributeId].Value.Replace(",", "."), CultureInfo.InvariantCulture.NumberFormat);
                        actionData.Index = int.Parse(action.Attributes[idAttributeId].Value);
                        levelData.Actions.Add(actionData);
                    }
                    levelData.Index = i;
                    levels.Add(levelData);
                }
                CheckActorsInLevels(levels, GetActorNames(levels[0]));
                CheckActionsId(levels);
                for (int i = 0; i < levels.Count; i++)
                {
                    foreach (string actor in GetActorNames(levels[0]))
                    {
                        if (i != 0)
                        {
                            CheckActionsForActor(GetActorsActions(levels[i], actor), levels[i - 1], actor, i);
                        }
                        else
                        {
                            CheckActionsForActor(GetActorsActions(levels[i], actor), null, actor, i);
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

        private List<string> GetActorNames(Level level)
        {
            HashSet<string> hashedActors = new HashSet<string>();
            foreach (Action activity in level.Actions)
            {
                foreach (Actor actor in activity.Actors)
                {
                    hashedActors.Add(actor.Name);
                }
            }
            List<string> actors = hashedActors.ToList();
            return actors;
        }

        private void CheckActorsInLevels(List<Level> levels, List<string> actorNames)
        {
            for (int i = 0; i < levels.Count; i++)
            {
                if (i == 0)
                {
                    continue;
                }
                else
                {
                    if (GetActorNames(levels[i]).Count != actorNames.Count)
                    {
                        throw new ScenarioException(string.Format("Level {0} contains previously undefined actor.", i));
                    }

                    bool[] actorChecker = new bool[actorNames.Count];
                    for (int j = 0; j < levels[i].Actions.Count; j++)
                    {
                        for (int k = 0; k < levels[i].Actions[j].Actors.Count; k++)
                        {
                            for (int l = 0; l < actorNames.Count; l++)
                            {
                                if (actorNames[l].Equals(levels[i].Actions[j].Actors[k].Name))
                                {
                                    actorChecker[l] = true;
                                    break;
                                }
                            }
                        }
                    }
                    for (int j = 0; j < actorChecker.Length; j++)
                    {
                        if (!actorChecker[j])
                        {
                            throw new ScenarioException(string.Format("Level {0} doesn't contain any action for {1}.", i, actorNames[j]));
                        }
                    }
                }
            }
        }

        private void CheckActionsId(List<Level> levels)
        {
            foreach (Level level in levels)
            {
                foreach (Action action in level.Actions)
                {
                    foreach (Level searchedLevel in levels)
                    {
                        foreach (Action searchedAction in searchedLevel.Actions)
                        {
                            if (action.Index == searchedAction.Index)
                            {
                                if (levels.IndexOf(level) != levels.IndexOf(searchedLevel))
                                {
                                    throw new ScenarioException(string.Format("Action must have unique index value. Check action number {0} in level number {1}.", searchedLevel.Actions.IndexOf(searchedAction), levels.IndexOf(searchedLevel)));
                                }
                                else if (searchedLevel.Actions.IndexOf(searchedAction) != level.Actions.IndexOf(action))
                                {
                                    throw new ScenarioException(string.Format("Action must have unique index value. Check action number {0} in level number {1}.", searchedLevel.Actions.IndexOf(searchedAction), levels.IndexOf(searchedLevel)));
                                }
                            }
                        }
                    }
                }
            }
        }

        private List<Action> GetActorsActions(Level level, string actorName)
        {
            List<Action> actions = new List<Action>();
            foreach (Action action in level.Actions)
            {
                foreach (Actor actor in action.Actors)
                {
                    if (actor.Name.Equals(actorName))
                    {
                        actions.Add(action);
                        break;
                    }
                }
            }

            return actions;
        }

        private void CheckActionsForActor(List<Action> actorsActions, Level previousLevel, string actorName, int levelId)
        {
            foreach (Action action in actorsActions)
            {
                foreach (Actor actor in action.Actors)
                {
                    if (actor.Name.Equals(actorName))
                    {
                        foreach (int id in actor.PreviousActivitiesIndexes)
                        {
                            if (!CheckPreviousId(id, previousLevel, actorName))
                            {
                                throw new ScenarioException(string.Format("Invalid previous action index value for {0} in action \"{1}\". Check level number {2}.", actorName, action.Name, levelId));
                            }
                        }
                        if (!CheckActionName(action.Name, actor.MocapId))
                        {
                            throw new ScenarioException(string.Format("Invalid action name and/or mocapId for {0} in action \"{1}\". Check level number {2}.", actorName, action.Name, levelId));
                        }
                    }
                }
            }
            CheckProbability(actorsActions, actorName, levelId);
        }

        private bool CheckPreviousId(int id, Level previousLevel, string actorName)
        {
            if (previousLevel == null)
            {
                return true;
            }

            foreach (Action action in previousLevel.Actions)
            {
                if (action.Index == id)
                {
                    foreach (Actor actor in action.Actors)
                    {
                        if (actor.Name.Equals(actorName))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private bool CheckActionName(string name, string mocapId)
        {
            if (name.Equals("walk") || name.Equals("run"))
            {
                return true;
            }
            string fullActionName = string.Format("{0}@{1}", mocapId, name);
            string animationsPath = WizardWindow.Project + "\\Assets\\Resources\\Animations\\";
            string[] animationFiles = Directory.GetFiles(animationsPath, "*.fbx", SearchOption.AllDirectories);
            for (int i = 0; i < animationFiles.Length; i++)
            {
                animationFiles[i] = Path.GetFileNameWithoutExtension(animationFiles[i]);
            }
            foreach (string animation in animationFiles)
            {
                if (animation.Equals(fullActionName))
                {
                    return true;
                }
            }
            return false;
        }

        private void CheckProbability(List<Action> actions, string actorName, int levelId)
        {
            HashSet<int> previousId = new HashSet<int>();
            foreach (Action action in actions)
            {
                foreach (Actor actor in action.Actors)
                {
                    if (actor.Name.Equals(actorName))
                    {
                        foreach (int id in actor.PreviousActivitiesIndexes)
                        {
                            previousId.Add(id);
                        }
                    }
                }
            }
            foreach (int id in previousId)
            {
                float probability = 0.0f;
                foreach (Action action in actions)
                {
                    foreach (Actor actor in action.Actors)
                    {
                        if (actor.Name.Equals(actorName))
                        {
                            foreach (int prevId in actor.PreviousActivitiesIndexes)
                            {
                                if (prevId == id)
                                {
                                    probability += action.Probability;
                                }
                            }
                        }
                    }
                }
                if (probability != 1.0f)
                {
                    throw new ScenarioException(string.Format("Actions' probability for {0} isn't equal 1. Check level number {1}.", actorName, levelId));
                }
            }
        }
    }
}
