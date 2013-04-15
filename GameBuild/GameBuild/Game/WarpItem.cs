﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameBuild.Game
{
    public class WarpItem
    {
        public Rectangle warpField;

        public string sourceMap;
        public string targetMap;
        public string key;

        public int targetX;
        public int targetY;

        public WarpItem(string sourceMap, int sourceX, int sourceY, int width, int height, string targetMap, int targetX, int targetY, string key)
        {
            this.sourceMap = sourceMap;
            this.targetMap = targetMap;
            this.key = key;
            this.targetX = targetX;
            this.targetY = targetY;
            warpField = new Rectangle(sourceX, sourceY, width, height);
        }
    }
}
