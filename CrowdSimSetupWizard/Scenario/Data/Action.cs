using System.Collections.Generic;

namespace CrowdSimSetupWizard
{
    public class Action
    {
        public string Name;
        public float Probability;
        public int Index;
        public List<Actor> Actors;
        public List<Blend> Blends;

        public Action()
        {
            Name = "";
            Probability = 0.0f;
            Index = -1;
            Actors = new List<Actor>();
            Blends = new List<Blend>();
        }

        public Action(string name, float probability, int index)
        {
            Name = name;
            Probability = probability;
            Index = index;
            Actors = new List<Actor>();
            Blends = new List<Blend>();
        }
    }
}
