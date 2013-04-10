﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace GameBuild
{
    class Particle
    {
        public Rectangle position;
        Vector2 velocity;

        float timer;
        float scale = 1;
        float scaleModifier;
        float rotation = 0;
        float rotationModifier;
        float elapsed;

        public Color color;

        bool rotationMultiplicatvie;
        bool scaleMultiplicative;
        bool hasGravity;
        public bool dead;

        public Particle(Rectangle position, Vector2 velocity, Color color, float timer, float scaleModifier, float rotationModifier,
            bool rotationMultiplicatvie, bool scaleMultiplicative, bool hasGravity)
        {
            this.position = position;
            this.velocity = velocity;
            this.hasGravity = hasGravity;
            this.rotationMultiplicatvie = rotationMultiplicatvie;
            this.rotationModifier = rotationModifier;
            this.scaleMultiplicative = scaleMultiplicative;
            this.scaleModifier = scaleModifier;
            this.timer = timer;
            this.color = color;
        }

        public void Update(GameTime gameTime)
        {
            position.X += (int)velocity.X;
            position.Y += (int)velocity.Y;
            
            elapsed += (float)gameTime.ElapsedGameTime.TotalSeconds;


            if (elapsed >= timer)
            {
                color *= 0.90f;
            }

            if (color.A <= 0)
            {
                dead = true;
            }

            if (hasGravity)
            {
                velocity.Y += (float)gameTime.ElapsedGameTime.TotalSeconds * 10;
                if (velocity.X > 0)
                {
                    velocity.X -= (float)gameTime.ElapsedGameTime.TotalMilliseconds / 70;
                }
                if (velocity.X < 0)
                {
                    velocity.X += (float)gameTime.ElapsedGameTime.TotalMilliseconds / 70;
                }
            }

            #region scale and rotation
            if (scaleMultiplicative)
            {
                scale *= scaleModifier;
            }
            else
            {
                scale += scaleModifier;
            }

            if (rotationMultiplicatvie)
            {
                rotation *= rotationModifier;
            }
            else
            {
                rotation += rotationModifier;
            }
            //position.Width += (int)scale;
            //position.Height += (int)scale;
            #endregion
        }
    }
}