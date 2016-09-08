﻿using System.Collections.Generic;

namespace CrowdSimSetupWizard
{
    class Level
    {
        public int Index;
        public List<Action> Actions;

        public Level()
        {
            Index = 0;
            Actions = new List<Action>();
        }

        public Level(int id)
        {
            Index = id;
            Actions = new List<Action>();
        }
    }
}
